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
                if (!string.IsNullOrEmpty(response))
                {
                    Debug.Log("Adjust Initialization response: " + response);

                    // Call GetAttribution with a callback
                    Adjust.GetAttribution(response =>
                    {
                        if (!string.IsNullOrEmpty(response))
                        {
                            Debug.Log("Attribution Response: " + response);
                        }
                        else
                        {
                            Debug.LogError("GetAttribution failed or returned no response.");
                        }
                    });
                }
                else
                {
                    Debug.LogError("Failed to initialize the Sdk.");
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
        adjustEvent.AddCallbackParameter("purchase_type", "gold");

        // Add partner parameters
        adjustEvent.AddPartnerParameter("currency", "USD");
        adjustEvent.AddPartnerParameter("amount", "9.99");

        // Track an event
        Adjust.TrackEvent(adjustEvent, response =>
        {
            if (!string.IsNullOrEmpty(response))
            {
                Debug.Log("Event Tracking Response: " + response);
            }
            else
            {
                Debug.LogError("Failed to track event.");
            }
        });

    }
}
