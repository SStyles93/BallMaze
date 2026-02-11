using System;
using UnityEngine;

#if UNITY_ANDROID
using UnityEngine.Android;
using Unity.Notifications.Android;
#endif

#if UNITY_IOS
using Unity.Notifications.iOS;
#endif

public class NotificationManager : MonoBehaviour
{
    public static NotificationManager Instance { get; private set; }

    const string permission = "android.permission.POST_NOTIFICATIONS";
    public bool HasPermission { get; private set; } = false;

    private const string HEART_CHANNEL_ID = "hearts_channel";
    private const string REENGAGE_CHANNEL_ID = "reengage_channel";

    private int lastHeartNotificationId = -1;
    private int lastReengageNotificationId = -1;



    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Initialize();
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            NotificationManager.Instance.ScheduleReengagementNotification(24);
        }
        else
        {
            NotificationManager.Instance.CancelReengagementNotification();
        }
    }

    private void Initialize()
    {
#if UNITY_ANDROID

        CheckAndroidPermission();

        RegisterAndroidNotifications();

#endif

#if UNITY_IOS
        iOSNotificationCenter.RequestAuthorization(
            AuthorizationOption.Alert | AuthorizationOption.Badge | AuthorizationOption.Sound,
            true
        );
#endif
    }
    

    private void CheckAndroidPermission()
    {
#if UNITY_ANDROID
        if (!Permission.HasUserAuthorizedPermission(permission))
        {
            Permission.RequestUserPermission(permission);
            HasPermission = false;
        }
        else
        {
            HasPermission = true;
        }
#endif
    }


    private void RegisterAndroidNotifications()
    {
        // HEART CHANNEL
        var heartChannel = new AndroidNotificationChannel()
        {
            Id = HEART_CHANNEL_ID,
            Name = "Hearts",
            Description = "Heart refill notifications",
            Importance = Importance.Default,
            EnableVibration = true
        };

        AndroidNotificationCenter.RegisterNotificationChannel(heartChannel);

        // RE-ENGAGEMENT CHANNEL
        var reengageChannel = new AndroidNotificationChannel()
        {
            Id = REENGAGE_CHANNEL_ID,
            Name = "Come Back Reminder",
            Description = "Reminder if player has not played",
            Importance = Importance.Default,
            EnableVibration = true
        };

        AndroidNotificationCenter.RegisterNotificationChannel(reengageChannel);
    }

    // =========================================================
    // HEART NOTIFICATION
    // =========================================================
    public void ScheduleHeartNotification(DateTime nextHeartTimeUtc)
    {
        HasPermission = Permission.HasUserAuthorizedPermission(permission);

        if (!HasPermission)
            return;

        CancelHeartNotification();

        DateTime localTime = nextHeartTimeUtc.ToLocalTime();

#if UNITY_ANDROID
        var notification = new AndroidNotification
        {
            Title = "❤️ Heart Refilled!",
            Text = "Full ❤️! Ready to play?",
            FireTime = localTime,
            SmallIcon = "icon_0",
            LargeIcon = "icon_1"
        };

        lastHeartNotificationId =
            AndroidNotificationCenter.SendNotification(notification, HEART_CHANNEL_ID);
#endif

#if UNITY_IOS
        TimeSpan delay = localTime - DateTime.Now;
        if (delay.TotalSeconds <= 0)
            return;

        var trigger = new iOSNotificationTimeIntervalTrigger()
        {
            TimeInterval = delay,
            Repeats = false
        };

        var notification = new iOSNotification()
        {
            Identifier = "heart_notification",
            Title = "❤️ Heart Refilled!",
            Body = "Full ❤️! Ready to play?",
            ShowInForeground = true,
            ForegroundPresentationOption = PresentationOption.Alert | PresentationOption.Sound,
            Trigger = trigger
        };

        iOSNotificationCenter.ScheduleNotification(notification);
#endif
    }

    public void CancelHeartNotification()
    {
#if UNITY_ANDROID
        if (lastHeartNotificationId != -1)
            AndroidNotificationCenter.CancelNotification(lastHeartNotificationId);
#endif

#if UNITY_IOS
        iOSNotificationCenter.RemoveScheduledNotification("heart_notification");
#endif
    }

    // =========================================================
    // RE-ENGAGEMENT NOTIFICATION
    // =========================================================
    public void ScheduleReengagementNotification(int hoursDelay)
    {
        HasPermission = Permission.HasUserAuthorizedPermission(permission);

        if (!HasPermission)
            return;

        CancelReengagementNotification();

        DateTime fireTime = DateTime.Now.AddHours(hoursDelay);

#if UNITY_ANDROID
        var notification = new AndroidNotification
        {
            Title = "Will you survive the Maze?",
            Text = "Come back and beat the Maze!",
            FireTime = fireTime,
            SmallIcon = "icon_0",
            LargeIcon = "icon_1"
        };

        lastReengageNotificationId =
            AndroidNotificationCenter.SendNotification(notification, REENGAGE_CHANNEL_ID);
#endif

#if UNITY_IOS
        var trigger = new iOSNotificationTimeIntervalTrigger()
        {
            TimeInterval = TimeSpan.FromHours(hoursDelay),
            Repeats = false
        };

        var notification = new iOSNotification()
        {
            Identifier = "reengage_notification",
            Title = "🎮 We miss you!",
            Body = "Come back and collect your rewards!",
            ShowInForeground = true,
            ForegroundPresentationOption = PresentationOption.Alert | PresentationOption.Sound,
            Trigger = trigger
        };

        iOSNotificationCenter.ScheduleNotification(notification);
#endif
    }

    public void CancelReengagementNotification()
    {
#if UNITY_ANDROID
        if (lastReengageNotificationId != -1)
            AndroidNotificationCenter.CancelNotification(lastReengageNotificationId);
#endif

#if UNITY_IOS
        iOSNotificationCenter.RemoveScheduledNotification("reengage_notification");
#endif
    }
}
