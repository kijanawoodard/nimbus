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
		public void Sanity()
		{
			var mediator = new Mediator();

			mediator.Register<ChangeUserName, string>(
				() => string.Empty, 
				() => new IHandleMarker<ChangeUserName>[] 
					{ new FakePersistance(), new GenericHook(), new ConsoleLogger() });

			var command = new ChangeUserName { Name = "Foo Bar" };
			var result = mediator.Send<ChangeUserName, string>(command);
			Console.WriteLine("Result: {0}", result);
			Assert.AreEqual("Foo Bar", result);
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

		public class FakePersistance : IHandleWithMediator<ChangeUserName, string>
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

		public class GenericHook : IHandle<object>
		{
			public void Handle(object message)
			{
				
			}
		}
    }
}
