using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Steamworks;

public class SteamScript : MonoBehaviour
{
    public Button eventButton;
    private const string AdjustAppToken = "2fm9gkqubvpc";
    private const string AdjustEnvironment = "sandbox";

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        eventButton.onClick.AddListener(TrackEventOnClick);

        if (SteamManager.Initialized)
        {
            // Create AdjustConfig with app token, environment, and the current MonoBehaviour instance
            AdjustConfig adjustConfig = new AdjustConfig(AdjustAppToken, AdjustEnvironment, this);

            // Adjust SDK initialization
            Adjust.InitSdk(adjustConfig, response =>
            {
                if (response != null)
                {
                    Debug.Log($"Adjust SDK Initialized. Response Code: {response.ResponseCode}, Response: {response.ResponseBody}, JsonResponse: {response.GetSerializedJsonResponse()}");
                }
                else
                {
                    Debug.LogError("Adjust SDK initialization failed.");
                }
            });

            // Call GetAttribution with a callback
            Adjust.GetAttribution(response =>
            {
                if (response != null)
                {
                    Debug.Log($"Attribution Response: Response Code: {response.ResponseCode}, Response: {response.ResponseBody}, JsonResponse: {response.GetSerializedJsonResponse()}");

                    // Extract and log attribution data
                    if (response.Attribution != null)
                    {
                        Debug.Log($"Tracker Token: {response.Attribution.TrackerToken}");
                        Debug.Log($"Tracker Name: {response.Attribution.TrackerName}");
                        Debug.Log($"Network: {response.Attribution.Network}");
                    }
                }
                else
                {
                    Debug.LogError("GetAttribution failed or returned no response.");
                }
            });
        }
    }

    void TrackEventOnClick()
    {
        // Create an AdjustEvent object with an event token
        AdjustEvent adjustEvent = new AdjustEvent("34vgg9");

        // Add Revenue and Currency
        adjustEvent.SetRevenue(150, "USD");

        // Add custom callback parameters
        adjustEvent.AddCallbackParameter("player_id", "123456");

        // Add partner parameters
        adjustEvent.AddPartnerParameter("foo", "bar");

        // Track an event
        Adjust.TrackEvent(adjustEvent, response =>
        {
            if (response != null)
            {
                Debug.Log($"Event Tracking Response: Response Code: {response.ResponseCode}, Response: {response.ResponseBody}, JsonResponse: {response.GetSerializedJsonResponse()}");
            }
            else
            {
                Debug.LogError("Event tracking failed or returned no response.");
            }
        });

    }
}
