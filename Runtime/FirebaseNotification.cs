using System;
using System.Collections.Generic;
using UnityEngine;
#if ONEUP_FIREBASE_MESSAGING
using System.Threading.Tasks;
using Firebase;
using Firebase.Extensions;
using Firebase.Messaging;
#endif

public class FirebaseNotification : Singleton<FirebaseNotification>
{
    public event Action<string> TokenReceived;
    public event Action<FirebaseNotificationMessage> MessageReceived;

    private readonly FirebaseNotificationTopics topics;
    private readonly List<Action<bool>> permissionCallbacks = new List<Action<bool>>();
    private FirebaseNotificationConfig config;
    private bool failed;
    private bool permissionRequested;

    public FirebaseNotification()
    {
        topics = new FirebaseNotificationTopics(ApplyTopic);
    }

    public string Token { get; private set; }
    public bool IsReady { get; private set; }
    public bool IsInitStarted { get; private set; }

    public void Init()
    {
        if (IsInitStarted)
            return;

        IsInitStarted = true;
        config = FirebaseNotificationConfig.Load();
        foreach (string topic in config.topics)
            Subscribe(topic);

        if (config.requestPermissionOnInit)
            permissionRequested = true;

        if (Application.isEditor)
        {
            Fail("Firebase Messaging does not run in the Editor.");
            return;
        }

#if ONEUP_FIREBASE_MESSAGING
        try
        {
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(OnDependencies);
        }
        catch (Exception exception)
        {
            Fail(exception.GetBaseException().Message);
        }
#else
        Fail("Firebase Messaging SDK is not installed.");
#endif
    }

    public void RequestPermission(Action<bool> done)
    {
        if (failed)
        {
            done?.Invoke(false);
            return;
        }

        permissionRequested = true;
        if (done != null)
            permissionCallbacks.Add(done);

        if (IsReady)
            AskPermission();
    }

    public void Subscribe(string topic)
    {
        topics.Request(topic, true);
    }

    public void Unsubscribe(string topic)
    {
        topics.Request(topic, false);
    }

    private void Fail(string reason)
    {
        failed = true;
        Debug.LogWarning($"FirebaseNotification - Not started: {reason}");
        FinishPermission(false);
    }

    private void FinishPermission(bool granted)
    {
        permissionRequested = false;
        List<Action<bool>> callbacks = new List<Action<bool>>(permissionCallbacks);
        permissionCallbacks.Clear();
        foreach (Action<bool> callback in callbacks)
            callback(granted);
    }

#if ONEUP_FIREBASE_MESSAGING
    private void OnDependencies(Task<DependencyStatus> task)
    {
        if (task.IsFaulted || task.IsCanceled)
        {
            Fail(task.Exception != null ? task.Exception.GetBaseException().Message : "Firebase check was canceled.");
            return;
        }

        if (task.Result != DependencyStatus.Available)
        {
            Fail($"Firebase dependencies: {task.Result}");
            return;
        }

        try
        {
            FirebaseMessaging.TokenReceived += OnTokenReceived;
            FirebaseMessaging.MessageReceived += OnMessageReceived;
        }
        catch (Exception exception)
        {
            Fail(exception.GetBaseException().Message);
            return;
        }

        IsReady = true;
        topics.MarkReady();
        if (permissionRequested)
            AskPermission();
    }

    private void AskPermission()
    {
        FirebaseMessaging.RequestPermissionAsync().ContinueWithOnMainThread(task => FinishPermission(!task.IsFaulted && !task.IsCanceled));
    }

    private void ApplyTopic(string topic, bool subscribe)
    {
        Task task = subscribe ? FirebaseMessaging.SubscribeAsync(topic) : FirebaseMessaging.UnsubscribeAsync(topic);
        task.ContinueWithOnMainThread(result =>
        {
            if (result.IsFaulted)
                Debug.LogWarning($"FirebaseNotification - Topic {topic} failed: {result.Exception?.GetBaseException().Message}");
        });
    }

    private void OnTokenReceived(object sender, TokenReceivedEventArgs args)
    {
        Token = args.Token;
        Debug.Log($"FirebaseNotification - Token: {Token}");
        TokenReceived?.Invoke(Token);
    }

    private void OnMessageReceived(object sender, MessageReceivedEventArgs args)
    {
        FirebaseMessage message = args.Message;
        FirebaseNotificationMessage received = new FirebaseNotificationMessage
        {
            from = message.From,
            messageId = message.MessageId,
            title = message.Notification?.Title,
            body = message.Notification?.Body,
            opened = message.NotificationOpened,
        };

        if (message.Data != null)
        {
            foreach (KeyValuePair<string, string> pair in message.Data)
                received.data[pair.Key] = pair.Value;
        }

        MessageReceived?.Invoke(received);
    }
#else
    private void AskPermission()
    {
        FinishPermission(false);
    }

    private void ApplyTopic(string topic, bool subscribe)
    {
    }
#endif
}
