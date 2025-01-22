using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Steamworks;

public class SteamScript : MonoBehaviour
{
    private AdjustSteamModule adjustSteamModule;
    public Button eventButton;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        eventButton.onClick.AddListener(TrackEventOnClick);

        if (SteamManager.Initialized)
        {
            string name = SteamFriends.GetPersonaName();
            Debug.Log(name);
            // Initialize the AdjustSteamModule
            // Pass 'this' as the MonoBehaviour executor
            adjustSteamModule = new AdjustSteamModule("2fm9gkqubvpc", "sandbox", this);

            // Initialize the AdjustSteamModule
            adjustSteamModule.Start(jsonResponse =>
            {
                if (jsonResponse != null && jsonResponse.ContainsKey("ask_in"))
                {
                    int askIn = jsonResponse.TryGetValue("ask_in", out var value) && int.TryParse(value.ToString(), out int parsedValue) ? parsedValue : 0;
                    Debug.Log($"ask_in value: {askIn} ms");

                    // Call GetAttribution with a callback
                    adjustSteamModule.GetAttribution(response =>
                    {
                        if (!string.IsNullOrEmpty(response))
                        {
                            Debug.Log("GetAttribution Response Parsed Successfully with response: " + response);
                        }
                        else
                        {
                            Debug.LogError("GetAttribution failed or returned no response.");
                        }
                    }, askIn);
                }
                else
                {
                    Debug.LogError("Failed to parse Adjust init response or response was null.");
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

        adjustSteamModule.TrackEvent("34vgg9", parameters, response =>
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

    // Update is called once per frame
    void Update()
    {

    }

}
