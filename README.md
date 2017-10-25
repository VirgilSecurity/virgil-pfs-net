# Virgil .NET/C# PFS SDK

[Installation](#installation) | [Initialization](#initialization)  |[Documentation](#documentation) | [Support](#support)

[Virgil Security](https://virgilsecurity.com) provides a set of APIs for adding security to any application.

Using [Perfect Forward Secrecy](https://developer.virgilsecurity.com/docs/references/perfect-forward-secrecy) (PFS) for Encrypted Communication allows you to protect previously intercepted traffic from being decrypted even if the main Private Key is compromised.

The Virgil .NET PFS is provided as a package named Virgil.PFS. The package is distributed via NuGet package management system.

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


To initialize the SDK at the __Server Side__ you need the application credentials (__Access Token__, __App ID__, __App Key__ and __App Key Password__) you got during Application registration at the [Dev Portal](https://developer.virgilsecurity.com/account/signin).

```csharp
var context = new VirgilApiContext
{
    AccessToken = "[YOUR_ACCESS_TOKEN_HERE]",
    Credentials = new AppCredentials
    {
        AppId = "[YOUR_APP_ID_HERE]",
        AppKeyData = VirgilBuffer.FromFile("[YOUR_APP_KEY_PATH_HERE]"),
        AppKeyPassword = "[YOUR_APP_KEY_PASSWORD_HERE]"
    }
};

var virgil = new VirgilApi(context);
```


__Next:__ You can add PFS to your application for secure communication just in few minutes, our [get started guide](/documentation/get-started/pfs-encrypted-communication.md)) provides more details.


## Documentation

Virgil Security has a powerful set of APIs and the documentation to help you get started:

* Get Started
  * [PFS Encrypted communication](/documentation/get-started/pfs-encrypted-communication.md)
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
