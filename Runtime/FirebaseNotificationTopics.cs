using System;
using System.Collections.Generic;

public sealed class FirebaseNotificationTopics
{
    private readonly Action<string, bool> apply;
    private readonly List<KeyValuePair<string, bool>> pending = new List<KeyValuePair<string, bool>>();
    private bool ready;

    public FirebaseNotificationTopics(Action<string, bool> apply)
    {
        this.apply = apply;
    }

    public int Pending => pending.Count;

    public void Request(string topic, bool subscribe)
    {
        if (string.IsNullOrEmpty(topic))
            return;

        if (ready)
        {
            apply(topic, subscribe);
            return;
        }

        pending.Add(new KeyValuePair<string, bool>(topic, subscribe));
    }

    public void MarkReady()
    {
        ready = true;
        foreach (KeyValuePair<string, bool> request in pending)
            apply(request.Key, request.Value);

        pending.Clear();
    }
}
