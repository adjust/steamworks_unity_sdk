using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
            // Pass 'this' as the MonoBehaviour executor
            Adjust.InitSdk(AdjustAppToken, AdjustEnvironment, this, response =>
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
        var parameters = new Dictionary<string, object>
        {
            {"revenue", "149.99" },
            { "currency", "USD" },
            { "callback_params", new Dictionary<string, object>
                {
                    { "foo", "bar" },
                    { "master", "yoda" }
                }
            }
        };

        Adjust.TrackEvent("34vgg9", parameters, response =>
        {
            if (!string.IsNullOrEmpty(response))
            {
                Debug.Log("Event Response: " + response);
            }
            else
            {
                Debug.LogError("Failed to track event.");
            }
        });
    }
}
