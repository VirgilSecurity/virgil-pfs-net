# Virgil .NET/C# PFS SDK

[Installation](#installation) | [Initialization](#initialization) | [Chat Example](#chat-example) | [Documentation](#documentation) | [Support](#support)

[Virgil Security](https://virgilsecurity.com) provides a set of APIs for adding security to any application.

[Perfect Forward Secrecy](https://developer.virgilsecurity.com/docs/references/perfect-forward-secrecy) (PFS) for Encrypted Communication allows you to protect previously intercepted traffic from being decrypted even if the main Private Key is compromised.

Virgil __.NET/C# PFS SDK__ contains dependent Virgil [.NET/C# SDK](https://github.com/VirgilSecurity/virgil-sdk-net/tree/v4) package.


To initialize and use Virgil PFS SDK, you need to have [Developer Account](https://developer.virgilsecurity.com/account/signin).

## Installation

The package is available for .NET Framework 4.5 and newer.

Installing the package using Package Manager Console

```
PM> Install-Package Virgil.PFS -Version 1.0.3-alpha
```

For more details about the Nuget Package Manager installation take a look at [this guide](https://docs.microsoft.com/en-us/nuget/quickstart/use-a-package).

## Initialization

Be sure that you have already registered at the [Dev Portal](https://developer.virgilsecurity.com/account/signin) and created your application.

To initialize the PFS SDK at the __Client Side__ you need only the __Access Token__ created for a client at [Dev Portal](https://developer.virgilsecurity.com/account/signin). The Access Token helps to authenticate client's requests.

```cs
var virgil = new VirgilApi("[YOUR_ACCESS_TOKEN_HERE]");
```

Virgil .NET/C# PFS SDK is suitable only for Client Side. If you need .NET/C# SDK for Server Side take a look at this [repository](https://github.com/VirgilSecurity/virgil-sdk-net/tree/v4-docs-review).


In Virgil every user has own Private Virgil Key and is represented with a Virgil Card which contains user's Public Key and all necessary information to identify him, take a look [here](#register-users) to see more details on how create user's Virgil Card.. 


 
## Chat Example

Bofore chat initialization each user must have own Virgil Card, you can easily create it with our [guide](#register-users).

In order to begin communicating, each user must run the initialization:

```cs
// initialize Virgil crypto instance
var crypto = new VirgilCrypto();
// enter User's credentials to create OTC and LTC Cards
var preferences = new SecureChatPreferences(
    crypto, 
    "[BOB_IDENTITY_CARD]",
    "[BOB_PRIVATE_KEY]",
    "[YOUR_ACCESS_TOKEN_HERE]");

// TODO: Объяснить какие задачи выполняет этот класс
var chat = new SecureChat(preferences);

// TODO: Объяснить зачем этот метод нужгл вызывать время от времени
await this.SecureChat.RotateKeysAsync(100);
```

Then Sender establishes a secure PFS conversation with Receiver, encrypts and sends the message:

```cs
public void SendMessage(User receiver, string message) {
    // get an active session by receiver's Virgil Card ID
    var session = this.Chat.ActiveSession(receiver.Card.Id);
    if (session == null)
    {
        // start new session with recipient if session wasn't initialized yet
        try
        {
	       	session = await this.chat.StartNewSessionWithAsync(receiver.Card);
       	}
       	catch{
    	   	// Error handling
       	}
    }
    this.SendMessage(receiver, session, message);
}

public void SendMessage(User receiver, SecureSession session, string message) {
    string ciphertext;
    try
    {
        // encrypt the message using previously initialized session
        ciphertext = session.Encrypt(message);
    }
    catch (Exception) {
        // Error handling
    }

    // send a cipher message to recipient using your messaging service
    this.Messenger.SendMessage(receiver.Name, ciphertext)
}
```

Receiver decrypts the incoming message using the conversation he has just created:

```cs
public void MessageReceived(string senderName, string message) {
    var sender = this.Users.Where(x => x.Name == senderName).FirstOrDefault();
    if (sender == null){
       return;
    }

    this.ReceiveMessage(sender, message);
}

public void ReceiveMessage(User sender, string message) {
    try
    {
        var session = this.Chat.LoadUpSession(sender.Card, message);

        // decrypt message using established session
        var plaintext = session.Decrypt(message);

        // show a message to the user
        Print(plaintext);
    }
    catch (Exception){
        // Error handling
    }
}
```

With the open session, which works in both directions, Sender and Receiver can continue PFS encrypted communication.

__Next:__ Take a look at our [Get Started](/documentation/get-started/pfs-encrypted-communication.md) guide to see the whole scenario of the PFS encrypted communication.


## Documentation

Virgil Security has a powerful set of APIs and the documentation to help you get started:

* Get Started
  * [PFS Encrypted Сommunication](/documentation/get-started/pfs-encrypted-communication.md)
* [Configuration](/documentation/guides/configuration)
  * [Set Up PFS Client Side](/documentation/guides/configuration/client-pfs.md)
  * [Set Up Server Side](/documentation/guides/configuration/server.md)


## Register Users
In Virgil every user has own Private Virgil Key and is represented with a Virgil Card. Using this Identity Cards we generates special one-time (OTC) and long-time (LTC) cards that have own life-time. Then you can use for each session new OTC and delete it after session finished. 

In order to create user's Identity Virgil Cards use the following code:

```cs
// generate a new Virgil Key for Alice
var aliceKey = virgil.Keys.Generate()

// save the Alice's Virgil Key into the storage at her device
aliceKey.Save("[KEY_NAME]", "[KEY_PASSWORD]");

// create a Alice's Virgil Card
var aliceCard = virgil.Cards.Create("alice", aliceKey);

// export a Virgil Card to string
var exportedAliceCard = aliceCard.Export();
```
after Virgil Card creation it is necessary to sign and publish it with Application Private Virgil Key at the server side.

```cs
// import a Alice's Virgil Card from string
var aliceCard = virgil.Cards.Import(exportedAliceCard);

// publish a Virgil Card at Virgil Services
await virgil.Cards.PublishAsync(aliceCard);
```
Now you have User's Virgil Cards and ready to initialize a PFS Chat. During initialization you create OTC and LTC Cards.

__Next:__ Take a look at our [guides](/documentation/get-started/pfs-encrypted-communication.md) to see more examples. 

## License

This library is released under the [3-clause BSD License](LICENSE.md).

## Support

Our developer support team is here to help you. You can find us on [Twitter](https://twitter.com/virgilsecurity) and [email][support].

[support]: mailto:support@virgilsecurity.com
