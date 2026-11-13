using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UnityConsent;
using Unity.Services.Core;
using Unity.Services.PushNotifications;

public class TestNotification : MonoBehaviour
{
    private bool isInitialized;

    private async void Start()
    {
        try
        {
            // Step 1: Initialize Unity Services
            await UnityServices.InitializeAsync();
            isInitialized = true;

            Debug.Log("Unity Services Initialized!");

#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
            // Step 2: Listen for incoming notifications
            PushNotificationsService.Instance
                .OnRemoteNotificationReceived += OnNotificationReceived;
#endif
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    // Step 3: Set Analytics consent from your privacy UI
    public void SetAnalyticsConsent(bool userGaveConsent)
    {
        ConsentState consent = EndUserConsent.GetConsentState();

        consent.AnalyticsIntent = userGaveConsent
            ? ConsentStatus.Granted
            : ConsentStatus.Denied;

        EndUserConsent.SetConsentState(consent);
    }

    // Step 4: Register the device for push notifications
    public async void RegisterPushNotifications()
    {
        if (!isInitialized)
        {
            Debug.LogWarning("Unity Services not ready.");
            return;
        }
        
        if (EndUserConsent.GetConsentState().AnalyticsIntent
            != ConsentStatus.Granted)
        {
            Debug.LogWarning("Analytics consent required.");
            
            return;
        }

#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
        try
        {
            string pushToken = await PushNotificationsService
                .Instance.RegisterForPushNotificationsAsync();

            // For testing only
            Debug.Log("Push Token: " + pushToken);

        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
#else
        Debug.Log("Push Notifications require a mobile build.");
#endif
    }

#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
    private void OnNotificationReceived(
        Dictionary<string, object> notificationData)
    {
        Debug.Log("Notification Received!");
    }
#endif

    private void OnDestroy()
    {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
        if (isInitialized)
        {
            PushNotificationsService.Instance
                .OnRemoteNotificationReceived -= OnNotificationReceived;
        }
#endif
    }
}
