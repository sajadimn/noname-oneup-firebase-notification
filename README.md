Firebase Notification Unity Package
===

## Description
Firebase Cloud Messaging (push notifications) for Android and iOS. It does not need Firebase Analytics: it starts Firebase itself.

- `FirebaseNotification.Instance.Init()`: call it once at boot. It is safe to call twice and does nothing harmful when Firebase or its config file is missing (it logs a warning). In the Editor it does not start Firebase, because Firebase Messaging only runs on devices.
- `RequestPermission(granted => …)`: asks for notification permission (iOS, Android 13+). Your project decides when.
- `Subscribe(topic)` / `Unsubscribe(topic)`: kept until Firebase is ready.
- `Token`, `TokenReceived`, `MessageReceived` (`FirebaseNotificationMessage`: from, messageId, title, body, opened, data), `IsReady`, `IsInitStarted`.
- Android: the package never edits a manifest in your project. It writes its own `Assets/Plugins/Android/FirebaseNotification.androidlib` from the config (default icon, color, channel), and at the end of each Android build it makes `com.google.firebase.MessagingUnityPlayerActivity` the launcher in Unity's generated Gradle copy, so any manifest you swap in before a build works with no Firebase lines in it. Intent-filters on your launcher (for example a payment return scheme) stay on it. A manifest that already declares `MessagingUnityPlayerActivity` is left as it is. Firebase's Messaging library declares `MessageForwardingService`.

## Dependencies
- [noname-oneup-singleton](https://github.com/sajadimn/noname-oneup-singleton). Add it to `Packages/manifest.json` before this package.
- Firebase App and Messaging. The package installs them from Google's registry into `LocalPackages` (with the External Dependency Manager) unless Firebase is already in `Assets/Firebase`. `OneUp/FirebaseNotification/Update SDKs` updates them. Use the same Firebase version as any other Firebase package in the project.
- Your Firebase config files (`google-services.json`, `GoogleService-Info.plist`) stay in your project.

## How to use
1. Add this package. It creates `Assets/Settings/FirebaseNotification/Resources/FirebaseNotificationConfig.asset` and the SDK versions config.
2. In the config, set the default icon (a drawable name), color, channel, topics and whether to ask for permission at `Init`.
3. Call `FirebaseNotification.Instance.Init();` at boot.
