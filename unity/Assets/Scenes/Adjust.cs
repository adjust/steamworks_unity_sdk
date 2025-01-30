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

public class Adjust
{
    private const string AdjustBaseUrl = "https://app.adjust.com";
    private static Adjust instance;

    private string appToken;
    private string environment;
    private string steamId;
    private string steamUuid;

    // Additional device-specific parameters
    private string osName;
    private string osVersion;
    private string deviceModel;
    private string appVersion;

    // Private MonoBehaviour reference for coroutine execution
    private MonoBehaviour monoBehavior;

    public static void InitSdk(string appToken, string environment, MonoBehaviour monoBehavior, Action<string> onResponse)
    {
        if (instance == null)
        {
            instance = new Adjust(appToken, environment, monoBehavior);
        }

        instance.TrackSession(onResponse);
    }

    private Adjust(string appToken, string environment, MonoBehaviour monoBehavior)
    {
        if (monoBehavior == null)
        {
            Debug.LogError("MonoBehaviour executor cannot be null.");
            throw new ArgumentNullException(nameof(monoBehavior), "MonoBehaviour executor is required to run coroutines.");
        }

        this.appToken = appToken;
        this.environment = environment;
        this.monoBehavior = monoBehavior;

        InitializeDeviceInfo();
        InitializeSteamInfo();
    }

    private void InitializeDeviceInfo()
    {
        ParseDeviceOSInfo(SystemInfo.operatingSystem);
        deviceModel = SystemInfo.deviceModel;
        appVersion = Application.version;
    }

    private void InitializeSteamInfo()
    {
        InitializeSteamUserId();
        RetrieveOrGenerateSteamUuid();
    }

    private void ParseDeviceOSInfo(string operatingSystem)
    {
#if UNITY_STANDALONE_OSX
        osName = "macos";
#elif UNITY_STANDALONE_WIN
        osName = "windows";
#else
        osName = "unsupported";
#endif

        var match = Regex.Match(operatingSystem, @"\d+(\.\d+)+");
        osVersion = match.Success ? match.Value : "Unknown";

        Debug.Log($"Parsed OS Info - Name: {osName}, Version: {osVersion}");
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

    private void TrackSession(Action<string> onResponse)
    {
        if (instance == null)
        {
            Debug.LogError("Adjust instance is not initialized. Call InitSdk first.");
            return;
        }

        string url = $"{AdjustBaseUrl}/session";
        Dictionary<string, string> payload = GenerateCommonPayload();

        monoBehavior.StartCoroutine(SendGetRequest(url, payload, onResponse));
    }

    public static void TrackEvent(string eventToken, Dictionary<string, object> parameters, Action<string> onResponse)
    {
        if (instance == null)
        {
            Debug.LogError("Adjust instance is not initialized. Call InitSdk first.");
            return;
        }

        instance.InternalTrackEvent(eventToken, parameters, onResponse);
    }

    private void InternalTrackEvent(string eventToken, Dictionary<string, object> parameters, Action<string> onResponse)
    {
        if (string.IsNullOrEmpty(eventToken))
        {
            Debug.LogError("EventToken is missing. Cannot track event.");
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

        monoBehavior.StartCoroutine(SendGetRequest(url, payload, onResponse));
    }

    public static void GetAttribution(Action<string> onResponse)
    {
        if (instance == null)
        {
            Debug.LogError("Adjust instance is not initialized. Call InitSdk first.");
            return;
        }

        instance.InternalGetAttribution(onResponse);
    }

    private void InternalGetAttribution(Action<string> onResponse)
    {
        string url = $"{AdjustBaseUrl}/attribution";
        Dictionary<string, string> payload = GenerateCommonPayload();

        monoBehavior.StartCoroutine(SendGetRequest(url, payload, onResponse));
    }

    private Dictionary<string, string> GenerateCommonPayload()
    {
        var payload = new Dictionary<string, string>
        {
            { "app_token", appToken },
            { "environment", environment },
            { "created_at", GetCurrentTimestamp() },
            { "os_name", osName },
            { "os_version", osVersion },
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
        return now.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'");
    }

    private IEnumerator SendGetRequest(string url, Dictionary<string, string> payload, Action<string> onResponse)
    {
        var escapedPayload = payload.Select(kvp => 
        {
            var escapedKey = UnityWebRequest.EscapeURL(kvp.Key);
            var escapedValue = UnityWebRequest.EscapeURL(kvp.Value);
            return $"{escapedKey}={escapedValue}";
        });
        string queryString = string.Join("&", escapedPayload);
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
