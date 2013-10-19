using NUnit.Framework;
using Raven.Client;
using Raven.Tests.Helpers;

namespace nimbus.tests
{
	public class RavenMediatorTests : RavenTestBase
	{
		[Test]
		public void CanOrchestrate_UoW()
		{
			using (var store = NewDocumentStore())
			{
				var mediator = new Mediator();
				mediator.SubscribeScalar<RegisterElephant, string>(() =>
				{
					var session = store.OpenSession(); //per request scope
					return new ISubscribeFor<RegisterElephant>[] {new RavenZoo(session), new RavenUoWCommiter(session)};
				});

				var id = mediator.Send<RegisterElephant, string>(new RegisterElephant {Name = "Ellie"});

				using (var session = store.OpenSession())
				{
					var elephant = session.Load<Elephant>(id);
					Assert.AreEqual("Ellie", elephant.Name);
				}
			}
		}

		public class Elephant
		{
			public string Id { get; set; }
			public string Name { get; set; }
		}

		public class RegisterElephant
		{
			public string Name { get; set; }
		}

		public class RavenZoo : IHandle<RegisterElephant, string>
		{
			private readonly IDocumentSession _session;

			public RavenZoo(IDocumentSession session)
			{
				_session = session;
			}

			public string Handle(RegisterElephant message)
			{
				var paciderm = new Elephant {Name = message.Name};
				_session.Store(paciderm);
				return paciderm.Id;
			}
		}

		public class RavenUoWCommiter : IHandle<object>
		{
			private readonly IDocumentSession _session;

			public RavenUoWCommiter(IDocumentSession session)
			{
				_session = session;
			}

			public void Handle(object message)
			{
				_session.SaveChanges();
			}
		}
	}
}