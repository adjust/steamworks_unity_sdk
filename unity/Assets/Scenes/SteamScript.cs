using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SteamScript : MonoBehaviour
{
    public Button eventButton;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        eventButton.onClick.AddListener(TrackEventOnClick);

        if (SteamManager.Initialized)
        {
            // Pass 'this' as the MonoBehaviour executor
            Adjust.InitSdk("2fm9gkqubvpc", "sandbox", this, response =>
            {
                if (!string.IsNullOrEmpty(response))
                {
                    Debug.Log("Session Tracked Successfully with response: " + response);

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
                    Debug.LogError("Failed to track session.");
                }
            });
        }
    }

    void TrackEventOnClick()
    {
        var parameters = new Dictionary<string, object>
        {
            { "custom_key", "custom_value" },
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
