using UnityEngine;
using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using Firebase.Messaging;

/// <summary>
///  Pushwoosh sample class
/// </summary>
public class PushNotificator : MonoBehaviour
{
    public UnityEngine.UI.Text hwidUIText;
    string hwidString = "null";
    public UnityEngine.UI.Text tokenUIText;
    string tokenString = "Unsubscribed";
    public UnityEngine.UI.Text notificationUIText;
    string notificationString = "{}";
    public UnityEngine.UI.Text launchNotificationUIText;
    string launchNotificationString = "{}";

    public UnityEngine.UI.Text postEventKeyUIText;
    public UnityEngine.UI.Text postEventAttributeUIText;
    public UnityEngine.UI.Button sendEventButton;

    public GameObject AndroidSpecific;

    // Use this for initialization
    private void Start()
    {
        FirebaseMessaging.TokenReceived += OnTokenReceived;
        FirebaseMessaging.MessageReceived += OnMessageReceived;

        Pushwoosh.ApplicationCode = "B65BD-9711E";
        Pushwoosh.FcmProjectNumber = "101783084614";
        Pushwoosh.Instance.OnRegisteredForPushNotifications += OnRegisteredForPushNotifications;
        Pushwoosh.Instance.OnFailedToRegisteredForPushNotifications += OnFailedToRegisteredForPushNotifications;
        Pushwoosh.Instance.OnPushNotificationsReceived += OnPushNotificationsReceived;
        Pushwoosh.Instance.OnPushNotificationsOpened += OnPushNotificationsOpened;
        Pushwoosh.Instance.RegisterForPushNotifications();

        Pushwoosh.Instance.SetStringTag("UserName", "Alex");
        Pushwoosh.Instance.SetIntTag("Age", 42);
        Pushwoosh.Instance.SetListTag("Hobbies", new List<object>(new[] { "Football", "Tennis", "Fishing" }));

        Pushwoosh.Instance.SetBadgeNumber(0);

        Pushwoosh.Instance.SendPurchase("com.pushwoosh.Developer", 49.95, "USD");
        NotificationSettings notificationSettings = Pushwoosh.Instance.GetRemoteNotificationStatus();
        if (notificationSettings != null)
        {
            Debug.Log("qqq Notification status enabled: " + notificationSettings.enabled
#if UNITY_IPHONE && !UNITY_EDITOR
                      + " alert: " + notificationSettings.pushAlert
                      + " badge: " + notificationSettings.pushBadge
                      + " sound: " + notificationSettings.pushSound
#endif
            );
        }

        Pushwoosh.Instance.SetUserId("%userId%");

        Dictionary<string, object> attributes = new Dictionary<string, object>()
        {
            { "attribute", "value" },
        };

        Pushwoosh.Instance.PostEvent("applicationOpened", attributes);

        AndroidSpecific.SetActive(true);
        string launchNotification = Pushwoosh.Instance.GetLaunchNotification();
        if (launchNotification == null)
            launchNotificationString = "No launch notification";
        else
            launchNotificationString = launchNotification;

#if UNITY_ANDROID
        Dictionary<string, string> parameters = new Dictionary<string, string>()
        {
            { "l", "https://www.pushwoosh.com/" },
            { "u", "custom data" }
        };

        Pushwoosh.Instance.ScheduleLocalNotification("Hello, Android!", 5, parameters);
        Pushwoosh.Instance.SetNotificationChannelDelegate(new MyNotificationChannelDelegate());
#endif
    }

    private void OnTokenReceived(object sender, TokenReceivedEventArgs token)
    {
        Debug.Log($"{nameof(OnTokenReceived)}: {token} = {token.Token}");
    }

    private void OnMessageReceived(object sender, MessageReceivedEventArgs e)
    {
        Debug.Log($"{nameof(OnMessageReceived)}: Received a new message from: " + e.Message.From);
    }

    public void Update()
    {
        tokenUIText.text = tokenString;
        hwidUIText.text = hwidString;
        notificationUIText.text = notificationString;
        launchNotificationUIText.text = launchNotificationString;
    }

    public void OnSubscribe()
    {
        Pushwoosh.Instance.RegisterForPushNotifications();
    }

    public void OnUnsubscribe()
    {
        tokenString = "Unsubscribed";
        Pushwoosh.Instance.UnregisterForPushNotifications();
    }

    public void OnStartLocationTracking()
    {
        Pushwoosh.Instance.StartTrackingGeoPushes();
    }

    public void OnStopLocationTracking()
    {
        Pushwoosh.Instance.StopTrackingGeoPushes();
    }

    public void OnSendPostEvent()
    {
        Debug.Log(
            "On Send post event key: " + postEventKeyUIText.text + "; attribute: " + postEventAttributeUIText.text);
        Dictionary<string, object> parameters = new Dictionary<string, object>();
        parameters.Add(postEventKeyUIText.text, postEventAttributeUIText.text);
        Pushwoosh.Instance.PostEvent(postEventKeyUIText.text, parameters);
    }

    public void OnFailedToRegisteredForPushNotifications(string error)
    {
        tokenString = "Error ocurred while registering to push notifications: \n" + error;

        Debug.Log(tokenString);
    }

    public void OnPushNotificationsReceived(string payload)
    {
        notificationString = "NotificationReceived: " + payload;

        Debug.Log("qqq NotificationReceived: " + payload);
    }

    public void OnPushNotificationsOpened(string payload)
    {
        notificationString = "NotificationOpened: " + payload;
        Debug.Log("qqq NotificationOpened: " + payload);
        ResetPushWooshBadge();
    }

    private void OnRegisteredForPushNotifications(string token)
    {
        tokenString = token;
        hwidString = Pushwoosh.Instance.HWID;

        Debug.Log("qqq OnRegisteredForPushNotifications");
        Debug.Log("qqq token: " + token);
        Debug.Log("qqq HWID: " + Pushwoosh.Instance.HWID);
        Debug.Log("qqq PushToken: " + Pushwoosh.Instance.PushToken);

        Pushwoosh.Instance.GetTags((tags, error) =>
        {
            string json = PushwooshUtils.DictionaryToJson(tags);
            Debug.Log("qqq Tags: " + json);
        });
        Debug.Log(Pushwoosh.Instance.GetRemoteNotificationStatus());
    }

    private void ResetPushWooshBadge()
    {
        Debug.Log("qqq Notification cleared");

        Pushwoosh.Instance.SetBadgeNumber(0);
        Pushwoosh.Instance.ClearNotificationCenter();
    }

    private class MyNotificationChannelDelegate : NotificationChannelDelegate
    {
        public override string ChannelDescription(string channelName)
        {
            // Implement your channel description customization logic here
            return "Hello World";
        }

        public override string ChannelName(string channelName)
        {
            // Implement your channel name customization logic here
            return channelName;
        }
    }
}