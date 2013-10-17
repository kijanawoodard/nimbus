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
		public void CanGetResult()
		{
			var mediator = new Mediator();

			mediator.Register<ChangeUserName, string>(
				() => string.Empty,
				() => new IHandleMarker<ChangeUserName>[] { new ReturnsName() });

			var command = new ChangeUserName { Name = "Foo Bar" };
			var result = mediator.Send<ChangeUserName, string>(command);
			Assert.AreEqual("Foo Bar", result);
		}

		[Test]
		public void CanGetResultWithASecondVoidHandler()
		{
			var mediator = new Mediator();

			mediator.Register<ChangeUserName, string>(
				() => string.Empty,
				() => new IHandleMarker<ChangeUserName>[] { new ReturnsName(), new ConsoleLogger() });

			var command = new ChangeUserName { Name = "Foo Bar" };
			var result = mediator.Send<ChangeUserName, string>(command);
			Assert.AreEqual("Foo Bar", result);
		}

		[Test]
		public void OrderOfResultAndVoidHandlersDoesntMatter()
		{
			var mediator = new Mediator();

			mediator.Register<ChangeUserName, string>(
				() => string.Empty,
				() => new IHandleMarker<ChangeUserName>[] { new ConsoleLogger(), new ReturnsName() });

			var command = new ChangeUserName { Name = "Foo Bar" };
			var result = mediator.Send<ChangeUserName, string>(command);
			Assert.AreEqual("Foo Bar", result);
		}

		[Test]
		public void CanHaveAContravariantHandler()
		{
			var mediator = new Mediator();

			mediator.Register<ChangeUserName, string>(
				() => string.Empty, 
				() => new IHandleMarker<ChangeUserName>[] 
					{ new ReturnsName(), new GenericHook(), new ConsoleLogger() });

			var command = new ChangeUserName { Name = "Foo Bar" };
			var result = mediator.Send<ChangeUserName, string>(command);
			Console.WriteLine("Result: {0}", result);
			Assert.AreEqual("Foo Bar", result);
		}

		[Test]
		public void CanSendWithoutResult()
		{
			var mediator = new Mediator();

			var counter = new Counter();
			mediator.Register<ChangeUserName, string>(
				() => string.Empty,
				() => new IHandleMarker<ChangeUserName>[] { counter });

			var command = new ChangeUserName { Name = "Foo Bar" };
			mediator.Send<ChangeUserName>(command);
			Assert.AreEqual(1, counter.Count);
		}

		[Test]
		public void CanRegisterClassWithoutFunc()
		{
			var mediator = new Mediator();

			mediator.Register<GetUserName, NameViewModel>(
				() => new IHandleMarker<GetUserName>[] { new JimNameRepository() });

			var result = mediator.Send<GetUserName, NameViewModel>(new GetUserName());
			Assert.AreEqual("Jim", result.Name);
		}

		[Test]
		public void Throws_If_Message_Not_Registered()
		{
			var mediator = new Mediator();
			Assert.Throws<ApplicationException>(
				() => mediator.Send<ChangeUserName, string>(new ChangeUserName()));
		}


		public class ChangeUserName
		{
			public string Name { get; set; }
		}

		public class ReturnsName : IHandleWithMediator<ChangeUserName, string>
		{
			public string Handle(IMediator mediator, string result, ChangeUserName message)
			{
				return message.Name;
			}
		}

		public class ConsoleLogger : IHandleWithMediator<ChangeUserName>
		{
			public void Handle(IMediator mediator, ChangeUserName message)
			{
				Console.WriteLine(message.Name);
			}
		}

		public class Counter : IHandleWithMediator<ChangeUserName>
		{
			public int Count { get; set; }

			public void Handle(IMediator mediator, ChangeUserName message)
			{
				Count++;
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
			public string Name { get; set; }
		}

		public class JimNameRepository : IHandle<GetUserName, NameViewModel>
		{
			public NameViewModel Handle(NameViewModel result, GetUserName message)
			{
				result.Name = "Jim";
				return result;
			}
		}

    }
}
