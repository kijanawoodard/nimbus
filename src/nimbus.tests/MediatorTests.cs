using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace nimbus.tests
{
	public class MediatorTests
	{
		[Test]
		public void Throws_If_Message_Not_Subscribed()
		{
			var mediator = new Mediator();
			Assert.Throws<ApplicationException>(
				() => mediator.Send<ChangeUserName, string>(new ChangeUserName()));
		}

		[Test]
		public void CanGetResult()
		{
			var mediator = new Mediator();

			mediator.Subscribe<ChangeUserName, string>(
				() => new ISubscribeFor<ChangeUserName>[] {new ReturnsName()},
				() => string.Empty);

			var command = new ChangeUserName {Name = "Foo Bar"};
			var result = mediator.Send<ChangeUserName, string>(command);
			Assert.AreEqual("Foo Bar", result);
		}

		[Test]
		public void CanGetResultWithASecondVoidHandler()
		{
			var mediator = new Mediator();

			mediator.Subscribe<ChangeUserName, string>(
				() => new ISubscribeFor<ChangeUserName>[] {new ReturnsName(), new ConsoleLogger()},
				() => string.Empty);

			var command = new ChangeUserName {Name = "Foo Bar"};
			var result = mediator.Send<ChangeUserName, string>(command);
			Assert.AreEqual("Foo Bar", result);
		}

		[Test]
		public void OrderOfResultAndVoidHandlersDoesntMatter()
		{
			var mediator = new Mediator();

			mediator.Subscribe<ChangeUserName, string>(
				() => new ISubscribeFor<ChangeUserName>[] {new ConsoleLogger(), new ReturnsName()},
				() => string.Empty);

			var command = new ChangeUserName {Name = "Foo Bar"};
			var result = mediator.Send<ChangeUserName, string>(command);
			Assert.AreEqual("Foo Bar", result);
		}

		[Test]
		public void CanHaveAContravariantHandler()
		{
			var mediator = new Mediator();

			mediator.Subscribe<ChangeUserName, string>(
				() => new ISubscribeFor<ChangeUserName>[] {new ReturnsName(), new GenericHook(), new ConsoleLogger()},
				() => string.Empty);

			var command = new ChangeUserName {Name = "Foo Bar"};
			var result = mediator.Send<ChangeUserName, string>(command);
			Console.WriteLine("Result: {0}", result);
			Assert.AreEqual("Foo Bar", result);
		}

		[Test]
		public void CanSendWithoutResult()
		{
			var mediator = new Mediator();

			var counter = new Counter();
			mediator.Subscribe<ChangeUserName, string>(
				() => new ISubscribeFor<ChangeUserName>[] {counter},
				() => string.Empty);

			var command = new ChangeUserName {Name = "Foo Bar"};
			mediator.Send<ChangeUserName>(command);
			Assert.AreEqual(1, counter.Count);
		}

		[Test]
		public void CanSubscribeClassWithoutFunc()
		{
			var mediator = new Mediator();

			mediator.Subscribe<GetUserName, NameViewModel>(() => new[] {new SomeRepository()});

			var result = mediator.Send<GetUserName, NameViewModel>(new GetUserName());
			Assert.AreEqual("Some Name", result.Name);
		}

		[Test]
		public void CanSubscribeAndSupplyParameterToResultClass()
		{
			var mediator = new Mediator();

			mediator.Subscribe<GetUserName, NameViewModel>(
				() => new[] {new SomeRepository()},
				() => new NameViewModel("some value"));

			var result = mediator.Send<GetUserName, NameViewModel>(new GetUserName());
			Assert.AreEqual("Some Name", result.Name);
		}

		[Test]
		public void CanSubscribeAScalarResultWithoutAFunc()
		{
			var mediator = new Mediator();

			mediator.SubscribeScalar<ChangeUserName, string>(
				() => new ISubscribeFor<ChangeUserName>[] {new ReturnsName()});

			var command = new ChangeUserName {Name = "Foo Bar"};
			var result = mediator.Send<ChangeUserName, string>(command);
			Assert.AreEqual("Foo Bar", result);
		}

		[Test]
		public void CanSubscribeAIntResultWithoutAFunc()
		{
			var mediator = new Mediator();

			mediator.SubscribeScalar<ChangeUserName, int>(
				() => new ISubscribeFor<ChangeUserName, int>[] {new Returns42()});

			var command = new ChangeUserName {Name = "Foo Bar"};
			var result = mediator.Send<ChangeUserName, int>(command);
			Assert.AreEqual(42, result);
		}

		[Test]
		public void CanSubscribeWithoutAResult()
		{
			var mediator = new Mediator();

			var counter = new Counter();
			mediator.Subscribe<ChangeUserName>(
				() => new ISubscribeFor<ChangeUserName>[] {counter});

			var command = new ChangeUserName {Name = "Foo Bar"};
			mediator.Send<ChangeUserName>(command);
			Assert.AreEqual(1, counter.Count);
		}

		[Test]
		public void CanSubscribeAndSendWithoutGenericArgs()
		{
			var mediator = new Mediator();
			mediator.Subscribe(() => new[] {new NamePersistor()});
			mediator.Send(new ChangeUserName {Name = "Foo Bar"});
		}

		[Test]
		public void SecondSubscription_Throws()
		{
			var mediator = new Mediator();
			mediator.Subscribe(() => new[] {new NamePersistor()});
			Assert.Throws<ArgumentException>(() => mediator.Subscribe(() => new[] {new Returns42()}));
		}

		[Test]
		public void CanUseMediatorWithinHandler()
		{
			var mediator = new Mediator();
			mediator.Subscribe<ProcessAccount>(() => new[] {new AccountExpiditer()});
			mediator.Subscribe<GetAccount, Account>(() => new[] {new AccountRepository()});

			mediator.Send(new ProcessAccount());
		}

		public class ChangeUserName
		{
			public string Name { get; set; }
		}

		public class NamePersistor : IHandle<ChangeUserName>
		{
			public void Handle(ChangeUserName message)
			{
				//do persistence
			}
		}

		public class ReturnsName : IHandle<ChangeUserName, string>
		{
			public string Handle(ChangeUserName message, string result)
			{
				return message.Name;
			}
		}

		public class ConsoleLogger : IHandle<ChangeUserName>
		{
			public void Handle(ChangeUserName message)
			{
				Console.WriteLine(message.Name);
			}
		}

		public class Counter : IHandle<ChangeUserName>
		{
			public int Count { get; set; }

			public void Handle(ChangeUserName message)
			{
				Count++;
			}
		}

		public class Returns42 : IHandle<ChangeUserName, int>
		{
			public int Handle(ChangeUserName message, int result)
			{
				return 42;
			}
		}

		public class GenericHook : IHandle<object>
		{
			public void Handle(object message)
			{

			}
		}

		public class GetUserName
		{

		}

		public class NameViewModel
		{
			public NameViewModel()
			{
			}

			public NameViewModel(string someParam)
			{
			}

			public string Name { get; set; }
		}

		public class SomeRepository : IHandle<GetUserName, NameViewModel>
		{
			public SomeRepository()
			{
			}

			public SomeRepository(string someParam)
			{
			}

			public NameViewModel Handle(GetUserName message, NameViewModel result)
			{
				result.Name = "Some Name";
				return result;
			}
		}

		public class Account
		{
			public void Process()
			{
				Console.WriteLine("account processed");
			}
		}

		public class ProcessAccount
		{
			/*account id*/
		}

		public class GetAccount
		{
			/*account id*/
		}

		public class AccountRepository : IHandle<GetAccount, Account>
		{
			public Account Handle(GetAccount message, Account result)
			{
				return new Account(); //return account 
			}
		}

		public class AccountExpiditer : IHandleWithMediator<ProcessAccount>
		{
			public void Handle(IMediator mediator, ProcessAccount message)
			{
				var account = mediator.Send<GetAccount, Account>(new GetAccount());
				account.Process();
			}
		}
	}
}
