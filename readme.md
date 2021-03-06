#What is nimbus?

>A circle, or disk, or any indication of radiant light...
>
> -- <cite>[morewords.com]</cite>

Send messages in, get messages back.

>A [nimble] message bus

Use normal programming techniques to configure your application.

>.Net In-Memory Bus

Happens now. Want something to happen later, code it in a handler.

##Summary
An in-memory bus, inspired by [ShortBus], Greg Young's [8 lines of code], Ayende's [limit your abstractions] series, and Rob Conery's [massive]. 

Nimbus does not use an IoC container. Instead, you subscribe the handlers you want to process each message explicitly. For the price of a little more typing, you get a very discoverable bus configuration and no magic. You always know exactly what handlers will run.

With explicit configuration, you can have handlers that are in "test" without having to worry about them getting executed. Multi-tenancy is as simple as an if statement or whatever other normal programming construct you want to use. You don't have to learn the intricacies of a container.

Messages are ordinary classes. There are no special interfaces to attach. 

Nimbus does not have an opinion about Commands vs Queries or whether a response should be returned for either. Your application controls those semantics.

This is _not_ a framework. Copy/paste deploy nimbus into your solution. Modify to taste. It all fits in [one file].

##Why
Minimize dependencies. If a class only depends on a mediator and/or messages, dependency implementations can change easily and as radically as necessary. Superfluous abstractions and implementation specific interfaces, such as IFooService, can disappear. Changing persistence technology is straightforward: handle persistence related messages with the new technology. Persistence tech can be mixed and matched in whatever combination is appropriate.

##Usage

###Declare Message
	
	public class ChangeUserName
	{
		public string Name { get; set; }
	}
	
###Declare Handler

	public class NamePersistor : IHandle<ChangeUserName>
	{
		public void Handle(ChangeUserName message)
		{
			//do persistence
		}
	}

###Subscribe Handlers
	
	//On app startup
	var mediator = new Mediator();
	mediator.Subscribe(() => new [] { new NamePersistor() });
	
###Send Message
	
	//somewhere in app
	mediator.Send(new ChangeUserName { Name = "Foo Bar" });

###Populate a ViewModel that has an empty constructor

	mediator.Subscribe<GetUserName, NameViewModel>(
		() => new [] { new SomeRepository() }); 
	
	//later...
	var result = mediator.Send<GetUserName, NameViewModel>(new GetUserName());
	
###Explicit ViewModel creation (the generic args are optional here)
	
	mediator.Subscribe<GetUserName, NameViewModel>(
		() => new[] { new SomeRepository() },
		() => new NameViewModel("some value"));

###Declare Chain of Handlers

	mediator.Subscribe<ChangeUserName>(
		() => new [] { new ReturnsName(), new GenericHook(), new ConsoleLogger() });
				
###Be explicit to handle contra-variant handlers

	public class GenericHook : IHandle<object>
	{
		public void Handle(object message) { }
	}
		
	mediator.Subscribe(
		() => new ISubscribeFor<ChangeUserName>[] { new ReturnsName(), new GenericHook(), new ConsoleLogger() });

###Use Mediator in handler		

	//handler
	public class AccountExpiditer : IHandle<ProcessAccount>
	{
		private readonly IMediator _mediator;

		public AccountExpiditer(IMediator mediator)
		{
			_mediator = mediator;
		}

		public void Handle(ProcessAccount message)
		{
			var account = _mediator.Send<GetAccount, Account>(new GetAccount());
			account.Process();
		}
	}
	
	//setup
	var mediator = new Mediator();
	mediator.Subscribe<ProcessAccount>(() => new[] {new AccountExpiditer(mediator)});
	mediator.Subscribe<GetAccount, Account>(() => new[] {new AccountRepository()});
	
	//send
	mediator.Send(new ProcessAccount());

###Orchestrate a Unit of Work

	using (var store = NewDocumentStore())
	{
		var mediator = new Mediator();
		mediator.SubscribeScalar<RegisterElephant, string>(() =>
		{
			var session = store.OpenSession(); //per request scope
			return new ISubscribeFor<RegisterElephant>[] {new RavenZoo(session), new RavenUoWCommitter(session)};
		});

		var id = mediator.Send<RegisterElephant, string>(new RegisterElephant {Name = "Ellie"});

		using (var session = store.OpenSession())
		{
			var elephant = session.Load<Elephant>(id);
			Assert.AreEqual("Ellie", elephant.Name);
		}
	}

[Full source for UoW example][uow]
	
##Why not ...?

[NServiceBus] - I love NServiceBus, but sometimes I just want an in memory bus. [NServiceBus 4.0 has one][nsb in memory], but it's semantics are restricted to publishing events. Nimbus is more concerned with mediating messages without regard to command/query semantics.

[Mass Transit] - I've never used it, but I've met some of the team members and it looks great. Like NServiceBus, it's main goal is distributed messaging, and that isn't a goal of nimbus.

[ShortBus] - The main difference is nimbus makes no usage of an IoC container. Another minor difference is no separation between commands and queries.

Microsoft Azure/Windows [Service Bus][microsoft service bus] - More than what I wanted for nimbus obviously. 

[MemBus] - Needs an IoC container.

[Reactor]

[RockBus]

[esb.net]

[morewords.com]: http://www.morewords.com/word/nimbus/
[nimble]: http://www.merriam-webster.com/dictionary/nimble
[ShortBus]: https://github.com/mhinze/ShortBus
[8 lines of code]: http://www.infoq.com/presentations/8-lines-code-refactoring
[limit your abstractions]: http://ayende.com/blog/154081/limit-your-abstractions-you-only-get-six-to-a-dozen-in-the-entire-app
[massive]: https://github.com/robconery/massive
[one file]: https://github.com/kijanawoodard/nimbus/blob/master/src/mediator.cs
[uow]: https://github.com/kijanawoodard/nimbus/blob/ab7ff533da2f44e3c2f5ed1c6daaf36e907deb7e/src/nimbus.tests/RavenMediatorTests.cs#L21
[NServiceBus]: http://particular.net/NServiceBus
[nsb in memory]: http://particular.net/articles/using-the-in-memory-bus
[Mass Transit]: http://masstransit-project.com/
[microsoft service bus]: http://www.windowsazure.com/en-us/documentation/services/service-bus/
[MemBus]: https://github.com/flq/MemBus
[Reactor]: http://reactorplatform.codeplex.com/wikipage?title=Reactor%20Service%20Bus
[RockBus]: http://rockbus.codeplex.com/
[esb.net]: http://keystrokeesbnet.codeplex.com/