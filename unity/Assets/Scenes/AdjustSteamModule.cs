using System;
using System.Text;
using UnityEngine;
using Steamworks;
using Newtonsoft.Json;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

public class AdjustSteamModule
{
    private const string AdjustBaseUrl = "https://app.adjust.com";

    public string appToken { get; private set; }
    public string environment { get; private set; }
    private string steamId;
    private string steamUuid;

    private bool isInitialized = false;

    // Additional device-specific parameters
    private string deviceOSName;
    private string deviceOSVersion;
    private string deviceModel;
    private string appVersion;

    // Private MonoBehaviour reference for coroutine execution
    private readonly MonoBehaviour coroutineExecutor;

    public AdjustSteamModule(string appToken, string environment, MonoBehaviour executor)
    {
        if (isInitialized)
        {
            Debug.LogWarning("AdjustSteamModule is already initialized!");
            return;
        }

        if (executor == null)
        {
            Debug.LogError("MonoBehaviour executor cannot be null.");
            throw new ArgumentNullException(nameof(executor), "MonoBehaviour executor is required to run coroutines.");
        }

        this.appToken = appToken;
        this.environment = environment;
        this.coroutineExecutor = executor;

        ParseDeviceOSInfo(SystemInfo.operatingSystem);
        deviceModel = SystemInfo.deviceModel;
        appVersion = Application.version;

        isInitialized = true;

        Debug.Log("AdjustSteamModule initialized with AppToken and Environment.");
        InitializeSteamUserId();
        RetrieveOrGenerateSteamUuid();
    }

    private void ParseDeviceOSInfo(string operatingSystem)
    {
#if UNITY_STANDALONE_OSX
        deviceOSName = "macos";
#elif UNITY_STANDALONE_WIN
        deviceOSName = "windows";
#else
        deviceOSName = "unknown";
#endif

        var match = Regex.Match(operatingSystem, "^(?:\\S+\\s)?(?<version>.+)");
        deviceOSVersion = match.Success ? match.Groups["version"].Value : "unknown";

        Debug.Log($"Parsed OS Info - Name: {deviceOSName}, Version: {deviceOSVersion}");
    }

    public void Start(Action<string> onResponse)
    {
        Debug.Log("Starting AdjustSteamModule - Tracking session.");
        TrackSession(response =>
        {
            if (!string.IsNullOrEmpty(response))
            {
                onResponse?.Invoke(response);
            }
            else
            {
                Debug.LogError("TrackSession failed or returned no response.");
                onResponse?.Invoke(null);
            }
        });
    }

    private void InitializeSteamUserId()
    {
        if (SteamManager.Initialized)
        {
            CSteamID steamUserId = SteamUser.GetSteamID();
            steamId = steamUserId.ToString();
            Debug.Log("Steam User ID set: " + steamId);
        }
        else
        {
            Debug.LogError("Steamworks is not initialized. Could not retrieve Steam User ID");
        }
    }

    private void RetrieveOrGenerateSteamUuid()
    {
        if (PlayerPrefs.HasKey("SteamUuid"))
        {
            steamUuid = PlayerPrefs.GetString("SteamUuid");
            Debug.Log("Retrieved Steam Uuid from PlayerPrefs: " + steamUuid);
        }
        else
        {
            steamUuid = Guid.NewGuid().ToString();
            PlayerPrefs.SetString("SteamUuid", steamUuid);
            PlayerPrefs.Save();
            Debug.Log("Generated and saved new Steam Uuid: " + steamUuid);
        }
    }

    public void TrackSession(Action<string> onResponse)
    {
        if (string.IsNullOrEmpty(appToken))
        {
            Debug.LogError("AppToken is not set. Cannot track session.");
            return;
        }

        string url = $"{AdjustBaseUrl}/session";
        Dictionary<string, string> payload = GenerateCommonPayload();

        coroutineExecutor.StartCoroutine(SendGetRequest(url, payload, onResponse));
    }

    public void TrackEvent(string eventToken, Dictionary<string, object> parameters = null, Action<string> onResponse = null)
    {
        if (string.IsNullOrEmpty(appToken) || string.IsNullOrEmpty(eventToken))
        {
            Debug.LogError("AppToken or EventToken is not set. Cannot track event.");
            return;
        }

        string url = $"{AdjustBaseUrl}/event";
        Dictionary<string, string> payload = GenerateCommonPayload();
        payload.Add("event_token", eventToken);

        if (parameters != null)
        {
            foreach (var param in parameters)
            {
                if (param.Value is Dictionary<string, object>)
                {
                    var nestedDict = param.Value as Dictionary<string, object>;
                    payload.Add(param.Key, JsonConvert.SerializeObject(nestedDict));
                }
                else
                {
                    payload.Add(param.Key, param.Value?.ToString());
                }
            }
        }

        coroutineExecutor.StartCoroutine(SendGetRequest(url, payload, onResponse));
    }

    public void GetAttribution(Action<string> onResponse)
    {
        if (string.IsNullOrEmpty(appToken))
        {
            Debug.LogError("AppToken is not set. Cannot request attribution.");
            return;
        }

        string url = $"{AdjustBaseUrl}/attribution";
        Dictionary<string, string> payload = GenerateCommonPayload();

        coroutineExecutor.StartCoroutine(SendGetRequest(url, payload, onResponse));
    }

    private Dictionary<string, string> GenerateCommonPayload()
    {
        var payload = new Dictionary<string, string>
        {
            { "app_token", appToken },
            { "environment", environment },
            { "created_at", GetCurrentTimestamp() },
            { "os_name", deviceOSName },
            { "os_version", deviceOSVersion },
            { "device_type", deviceModel },
            { "app_version", appVersion }
        };

        if (!string.IsNullOrEmpty(steamId))
        {
            payload.Add("steam_id", steamId);
        }

        if (!string.IsNullOrEmpty(steamUuid))
        {
            payload.Add("steam_uuid", steamUuid);
        }

        return payload;
    }

    private string GetCurrentTimestamp()
    {
        DateTime now = DateTime.UtcNow;
        return now.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'Z");
    }

    private IEnumerator SendGetRequest(string url, Dictionary<string, string> payload, Action<string> onResponse)
    {
        string queryString = string.Join("&", payload.Select(kvp => UnityWebRequest.EscapeURL(kvp.Key) + "=" + UnityWebRequest.EscapeURL(kvp.Value)));
        string fullUrl = url + "?" + queryString;

        Debug.Log($"Sending GET request to {fullUrl}");

        using (UnityWebRequest request = UnityWebRequest.Get(fullUrl))
        {
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Client-Sdk", "steam_unity0.0.1");

            yield return request.SendWebRequest();

            Debug.Log("HTTP Status Code: " + request.responseCode);
            Debug.Log("HTTP request result: " + request.result);

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("HTTP request completed successfully: " + request.downloadHandler.text);
                onResponse?.Invoke(request.downloadHandler.text);
            }
            else
            {
                Debug.LogError($"HTTP request failed: {request.error}, Response: {request.downloadHandler.text}");
                onResponse?.Invoke(null);
            }
        }
    }
}
