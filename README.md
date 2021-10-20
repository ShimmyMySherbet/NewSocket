# NewSocket
**I don't know what to call this socket yet, but I don't want to keep it as 'NewSocket'**

This page needs to be filled out. In the mean time, see <a href="https://github.com/ShimmyMySherbet/NewSocket/blob/master/SocketTest/Client.cs">Client</a> and <a href="https://github.com/ShimmyMySherbet/NewSocket/blob/master/SocketTest/Server.cs">Server</a> examples.

### WIP Web Socket library

##  Features:
### Message Multi-casting
Transmit multiple messages at a time over a single connection. Prevents a single large message from blocking other messages.

### Message Formats
Works using custom message formats or 'protocals', allowing you to implement custom message formats.
For an example, see <a href="https://github.com/ShimmyMySherbet/NewSocket/blob/master/NewSocket/Protocals/OTP/ObjectTransferProtocal.cs">Object Transfer</a>

## RPC:
* Invoking/Querying a remote RPC is asyncronous, however the RPC handler doesn't have to be async.
* Handlers can be instance based or static.
* All objects are automatically serialized/deserialized. Allowing for class and struct return types and parameters.
* RPC handlers can be manually assigned, or automatically assigned using attributes.
* Bind remote RPC to delegate.

### Binding remote RPC to delegate
* Creates a new instance of the specified delegate type and binds it to the remote RPC.
* All parameters passed to it are used when invoking the remote RPC handler.
* The delegate type can run the query async *(Task<object.>)* or blocking *(object.)*. It is recomended to run queries async.
* The remote RPC name is attached to the delegate type using the RPC attribute.
* The remote RPC name can also be supplied using `.GetRPC<DelegateType>("RemoteMethodName")`
```cs
[RPC("Login")]
public delegate Task<bool> LoginRPC(string username, string password);

var Login = Client.RPC.GetRPC<LoginRPC>();

// login
var loggedIn = await Login("Username", "Password");
if (loggedIn)
{
  Console.WriteLine("Logged in!");
}
```
This allows remote RPCs to be stored as a variable and invoked like a normal method.



See below for remote method.


### Assign Automatically
* If the RPC name is omitted in the RPC attribute, the method name will be selected instead.
```cs
Client.RPC.RegisterFrom(this);

[RPC("Login")]
public async Task<bool> Login(string username, string password)
{
    var result = await Auth.LoginAsync(username, password);
    IsLoggedIn = result.LoggedIn;
    return result.LoggedIn;    
}

[RPC]
public string GetName()
{
    return ClientUsername;
}

[RPC]
public DateTime GetTime() => DateTime.Now;
```

#### Assign Manually:
* You can pass any delegate type to `Subscribe()` 
```cs
Client.RPC.Subscribe("GetLoginState", GetLoginState);

public async Task<bool> Login(string username, string password)
{
    var result = await Auth.LoginAsync(username, password);
    IsLoggedIn = result.LoggedIn;
    return result.LoggedIn;    
}
```
#### Querying Remote RPC
* An alternate from binding an RPC to a delegate
```cs
var result = await Client.RPC.QueryAsync<bool>("Login", "Username", "Password");
```

For more examples, see <a href="https://github.com/ShimmyMySherbet/NewSocket/blob/master/SocketTest/Client.cs">Client.cs</a> and <a href="https://github.com/ShimmyMySherbet/NewSocket/blob/master/SocketTest/Server.cs">Server.cs</a> examples.

### Performance
Querying `GetTime` from the auto assign example on average took 0.18ms.

This is the time from querying and reciving the response, including the minimal network time in testing.

# Get Involved

<a href="https://github.com/ShimmyMySherbet/NewSocket/discussions/categories/ideas">Submit feature requests/ideas</a>

<a href="https://github.com/ShimmyMySherbet/NewSocket/discussions/categories/q-a">Ask a question</a>

My <a href="https://discord.shimmymysherbet.com/">Discord</a>
