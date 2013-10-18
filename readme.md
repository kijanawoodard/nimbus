#What is nimbus?

>A circle, or disk, or any indication of radiant light...
>
> -- <cite>[morewords.com]</cite>

>A nimble message bus
>
> -- <cite>Kijana Woodard</cite>

>.Net In-Memory Bus
>
> -- <cite>Kijana Woodard</cite>

An in-memory bus, inspired by [ShortBus]. 

Nimbus does not use an IoC. Instead, you subscribe the handlers you want to process each message explicitly. For the price of a little more typing, you get a very discoverable bus configuration and no magic. You always know exactly what handlers will run.

With explicit configuration, you can have handlers that are in "test" without having to worry about them getting executed. Multi-tenancy is as simple as an if statement or whatever other normal programming construct you want to use. You don't have to learn the intricacies of a container.



MIT License
2013 Kijana Woodard

[morewords.com]: http://www.morewords.com/word/nimbus/
[ShortBus]: https://github.com/mhinze/ShortBus