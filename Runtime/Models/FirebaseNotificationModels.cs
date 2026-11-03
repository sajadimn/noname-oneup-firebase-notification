using System;
using System.Collections.Generic;

[Serializable]
public class FirebaseNotificationMessage
{
    public string from;
    public string messageId;
    public string title;
    public string body;
    public bool opened;
    public Dictionary<string, string> data = new Dictionary<string, string>();
}
