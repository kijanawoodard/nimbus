#What is nimbus?

>A circle, or disk, or any indication of radiant light...
>
> -- <cite>[morewords.com]</cite>

Send messages in, get messages back.

>A [nimble] message bus

Use normal programming techniques to configure your application.

>.Net In-Memory Bus

Happens now. WWant something to happen later, code it in a handler.

##Summary
An in-memory bus, inspired by [ShortBus]. 

Nimbus does not use an IoC container. Instead, you subscribe the handlers you want to process each message explicitly. For the price of a little more typing, you get a very discoverable bus configuration and no magic. You always know exactly what handlers will run.

With explicit configuration, you can have handlers that are in "test" without having to worry about them getting executed. Multi-tenancy is as simple as an if statement or whatever other normal programming construct you want to use. You don't have to learn the intricacies of a container.

Messages are ordinary classes. There are no special interfaces to attach. 

Nimbus does not have an opinion about Commands vs Queries or whether a response should be returned for either. Your application controls those semantics.

##Why
Minimize dependencies. If a class only depends on a mediator and on messages, implementations can change easily. Superfluous abstractions, like implementation specific interfaces [IFooService] can disappear.

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
	mediator.Subscribe(
		() => new ISubscribeFor<ChangeUserName>[] { new ReturnsName(), new GenericHook(), new ConsoleLogger() });

###Convenience methods

	mediator.Subscribe<ChangeUserName, NamePersistor>();
	//TODO: could add many of such helpers
	
###UnitOfWork
###Use Mediator in handler		

##Why not ...?

[NServiceBus] - I love NServiceBus, but sometimes I just want an in memory bus. NServiceBus 4.0 has an [in memory bus][nsb in memory], but it's semantics are restricted to publishing events. Nimbus is more concerned with mediating messages without regard to command/query semantics.

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
[NServiceBus]: http://particular.net/NServiceBus
[nsb in memory]: http://particular.net/articles/using-the-in-memory-bus
[Mass Transit]: http://masstransit-project.com/
[microsoft service bus]: http://www.windowsazure.com/en-us/documentation/services/service-bus/
[MemBus]: https://github.com/flq/MemBus
[Reactor]: http://reactorplatform.codeplex.com/wikipage?title=Reactor%20Service%20Bus
[RockBus]: http://rockbus.codeplex.com/
[esb.net]: http://keystrokeesbnet.codeplex.com/