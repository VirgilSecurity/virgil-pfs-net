
# Server Configuration
[Developer Account](#head1) | [Install SDK](#head2) | [Initialize SDK](#head3) | [Create Access Token](#head4) | [Approve & Publish Cards](#head5)

This guide helps you to set up your server and implement required mechanisms using Virgil Infrastructure.


## <a name="head1"></a> Developer Account

To use the Virgil SDK package, sign up for **Developer account** and create your first application. Make sure to save the **App ID**, the **App Key** and the **App Key Password**. If you did not create a Developer account yet, you can do so now by using this [link](https://developer.virgilsecurity.com/account/signup).


## <a name="head2"></a> Install SDK

The Virgil .NET SDK is provided as a package named Virgil.SDK. The package is distributed via NuGet package management system.

The package is available for .NET Framework 4.5 and newer.

Installing the package:

1. Use NuGet Package Manager (Tools -> Library Package Manager -> Package Manager Console)
2. Run PM> Install-Package Virgil.SDK


## <a name="head3"></a> Initialize SDK

To initialize the Virgil SDK on a server, you will need to sign up for a developer account and create your first application. Then, to initialize the SDK all you need are:

1. The **access token** for your application
2. The application **credentials** (the App ID, the App Key in a file, the App Key password).

```csharp
var virgil = new VirgilApi(new VirgilApiContext
{
  AccessToken = "[YOUR_ACCESS_TOKEN_HERE]",
  Credentials = new AppCredentials
  {
      AppId = "[YOUR_APP_ID_HERE]",
      AppKey = VirgilBuffer.FromFile("[YOUR_APP_KEY_FILEPATH_HERE]"),
      AppKeyPassword = "[YOUR_APP_KEY_PASSWORD_HERE]",
  }
});
```


## <a name="head4"></a> Create Access Token

When users want to start working with your Application in a browser or mobile device, Virgil can't trust them right away.  Virgil needs the developer to vouch for his users, so we can trust them too. You'll need to give your users an Access Token that tells Virgil who they are and what they can do. Thus, you need a service that will be responsible for an access token creation in your Virgil Developer Dashboard for users in case of their successful registration on your server.

You must decide, based on the token request that was sent to you, who the user is and what they should be allowed to do and then you have to transfer a recently created special access token to the client-side. To figure out who the user is, you might implement the users authentication mechanism by using the user's identity. Also, you can assign a temporary identity to the user if you don't care who he is.

```
// an example of an Access Token representation
AT.7652ee415726a1f43c7206e4b4bc67ac935b53781f5b43a92540e8aae5381b14
```


## <a name="head5"></a> Approve & Publish Cards

You have to transfer a recently created and signed users' **Virgil Cards** from the client-side to your server for further approving. When you receive the users' Virgil Cards from the client-side, import them, sign with your App Key using Virgil SDK and Publish to the **Virgil Services**. Thus, you need a service that will validate and sign the transferred users' Virgil Cards.

Use the following code to Import and Publish a Virgil Card to Virgil Services

```csharp
// import a Virgil Card from string
var importedCard = virgil.Cards.Import(exportedCard);

// publish a Virgil Card
await virgil.Cards.PublishAsync(importedCard);
```

At the Virgil Services, the Virgil Cards will be also signed. Developers can verify the signature of the Virgil Card owner, the Virgil Services, and the Application Server at any time.
