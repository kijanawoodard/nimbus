#What is nimbus?

>A circle, or disk, or any indication of radiant light...
>
> -- <cite>[morewords.com]</cite>

Or

>A [nimble] message bus

Or

>.Net In-Memory Bus


##Summary
An in-memory bus, inspired by [ShortBus]. 

Nimbus does not use an IoC container. Instead, you subscribe the handlers you want to process each message explicitly. For the price of a little more typing, you get a very discoverable bus configuration and no magic. You always know exactly what handlers will run.

With explicit configuration, you can have handlers that are in "test" without having to worry about them getting executed. Multi-tenancy is as simple as an if statement or whatever other normal programming construct you want to use. You don't have to learn the intricacies of a container.

Messages are ordinary classes. There are no special interfaces to attach. 

Nimbus does not have an opinion about Commands vs Queries or whether a response should be returned for either. Your application controls those semantics.

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
		
##License
		
MIT License
2013 Kijana Woodard

[morewords.com]: http://www.morewords.com/word/nimbus/
[nimble]: http://www.merriam-webster.com/dictionary/nimble
[ShortBus]: https://github.com/mhinze/ShortBus
