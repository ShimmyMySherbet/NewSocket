# NewSocket
**I don't know what to call this socket yet, but I don't want to keep it as 'NewSocket'**

### WIP Web Socket library
Features:
* Message Multi-casting (so one large message can't block other messages)
* System of custom 'Protocal Handlers' for different message types.

Current Message Types / Protocals:
* Object Transfer, Transmits an object over the socket, with a channel tag applied to it.
* Async RPC. Tests yeild around ~0.16ms total time per basic call.




