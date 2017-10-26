# Virgil .NET/C# PFS SDK

[Installation](#installation) | [Initialization](#initialization)  | [PFS Chat Example](# PFS Chat Example) | [Documentation](#documentation) | [Support](#support)

[Virgil Security](https://virgilsecurity.com) provides a set of APIs for adding security to any application.

Using [Perfect Forward Secrecy](https://developer.virgilsecurity.com/docs/references/perfect-forward-secrecy) (PFS) for Encrypted Communication allows you to protect previously intercepted traffic from being decrypted even if the main Private Key is compromised.


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

Use Virgil PFS SDK is suitable only for client side. If you need .NET/C# SDK for Server Side take a look at this [repository](https://github.com/VirgilSecurity/virgil-sdk-net/tree/v4-docs-review)


## PFS Chat Example

With our SDK you may create unique Virgil Cards for your all users and devices. With users' Virgil Cards, you can easily initialize PFS chat and encrypt any data at Client Side.

In order to begin communicating, each user must run the initialization:

```cs
var secureChatPreferences = new SecureChatPreferences(
    "[CRYPTO]",
        // User's 
		"[BOB_IDENTITY_CARD]",
		"[BOB_PRIVATE_KEY]",
		"[YOUR_ACCESS_TOKEN_HERE]");

this.SecureChat = new SecureChat(secureChatPreferences);
    try
    {
        await this.SecureChat.RotateKeysAsync(100);
    }
    catch(Exception){
    //...
    }
```

Then Sender establishes a secure PFS conversation with Receiver, encrypts and sends the message:

```cs
public void SendMessage(User receiver, string message) {
    // get an active session by receiver's card id
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

__Next:__ Take a look at out [Get Started](/documentation/get-started/pfs-encrypted-communication.md) guide to see the whole scenario of the PFS encrypted communication.


## Documentation

Virgil Security has a powerful set of APIs and the documentation to help you get started:

* Get Started
  * [PFS Encrypted Сommunication](/documentation/get-started/pfs-encrypted-communication.md)
* Guides
  * [Virgil Cards](/documentation/guides/virgil-card)
  * [Virgil Keys](/documentation/guides/virgil-key)
* [Configuration](/documentation/guides/configuration)
  * [Set Up PFS Client Side](/documentation/guides/configuration/client-pfs.md)
  * [Set Up Server Side](/documentation/guides/configuration/server.md)


## License

This library is released under the [3-clause BSD License](LICENSE.md).

## Support

Our developer support team is here to help you. You can find us on [Twitter](https://twitter.com/virgilsecurity) and [email][support].

[support]: mailto:support@virgilsecurity.com
