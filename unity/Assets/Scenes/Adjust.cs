using System;
using System.Text;
using UnityEngine;
using Steamworks;
using Newtonsoft.Json;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class Adjust
{
    private const string AdjustBaseUrl = "https://app.adjust.com";

    private static Adjust defaultInstance;
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

    private Adjust(string appToken, string environment, MonoBehaviour monoBehavior)
    {
        this.appToken = appToken;
        this.environment = environment;
        this.steamId = GetSteamId();
        this.steamUuid = GetSteamUuid();
        this.osName = GetOsName();
        this.osVersion = GetOsVersion();
        this.deviceModel = SystemInfo.deviceModel;
        this.appVersion = Application.version;
        this.monoBehavior = monoBehavior;
    }

    #region Public API
    public static void InitSdk(string appToken, string environment, MonoBehaviour monoBehavior, Action<string> onResponse)
    {
        if (defaultInstance != null)
        {
            onResponse?.Invoke("Adjust SDK already initialized");
            Debug.LogError("[Adjust]: Adjust SDK already initialized");
            return;
        }
        if (IsAppTokenValid(appToken) == false)
        {
            onResponse?.Invoke("App token is not valid");
            Debug.LogError("[Adjust]: App token is not valid");
            return;
        }
        if (IsEnvironmentValid(environment) == false)
        {
            onResponse?.Invoke("Environment is not valid");
            Debug.LogError("[Adjust]: Environment is not valid");
            return;
        }
        if (IsMonoBehaviorValid(monoBehavior) == false)
        {
            onResponse?.Invoke("MonoBehaviour instance is not valid");
            Debug.LogError("[Adjust]: MonoBehaviour instance is not valid");
            return;
        }

        defaultInstance = new Adjust(appToken, environment, monoBehavior);
        defaultInstance.TrackSessionInternal(onResponse);
    }

    public static void TrackEvent(string eventToken, Dictionary<string, object> parameters, Action<string> onResponse)
    {
        if (defaultInstance == null)
        {
            onResponse?.Invoke("Adjust SDK not initialized - call InitSdk first");
            Debug.LogError("[Adjust]: Adjust SDK not initialized - call InitSdk first");
            return;
        }

        if (IsEventTokenValid(eventToken) == false)
        {
            onResponse?.Invoke("Event token is not valid");
            Debug.LogError("[Adjust]: Event token is not valid");
            return;
        }

        defaultInstance.TrackEventInternal(eventToken, parameters, onResponse);
    }

    public static void GetAttribution(Action<string> onResponse)
    {
        if (defaultInstance == null)
        {
            onResponse?.Invoke("Adjust SDK not initialized - call InitSdk first");
            Debug.LogError("[Adjust]: Adjust SDK not initialized - call InitSdk first");
            return;
        }

        defaultInstance.GetAttributionInternal(onResponse);
    }
    #endregion

    #region SDK logic
    private void TrackSessionInternal(Action<string> onResponse)
    {
        string url = $"{AdjustBaseUrl}/session";
        monoBehavior.StartCoroutine(SendRequest(url, null, onResponse));
    }

    private void TrackEventInternal(string eventToken, Dictionary<string, object> parameters, Action<string> onResponse)
    {
        string url = $"{AdjustBaseUrl}/event";

        Dictionary<string, object> eventParameters = new Dictionary<string, object>();
        eventParameters["event_token"] = eventToken;

        if (parameters != null)
        {
            foreach (var param in parameters)
            {
                if (param.Value is Dictionary<string, object>)
                {
                    var nestedDict = param.Value as Dictionary<string, object>;
                    eventParameters.Add(param.Key, JsonConvert.SerializeObject(nestedDict));
                }
                else
                {
                    eventParameters.Add(param.Key, param.Value?.ToString());
                }
            }
        }

        monoBehavior.StartCoroutine(SendRequest(url, eventParameters, onResponse));
    }

    private void GetAttributionInternal(Action<string> onResponse)
    {
        string url = $"{AdjustBaseUrl}/attribution";
        monoBehavior.StartCoroutine(SendRequest(url, null, onResponse, false));
    }

    private IEnumerator SendRequest(string url, Dictionary<string, object> payload, Action<string> onResponse, bool isPost = true)
    {
        Dictionary<string, string> commonPayload = GenerateCommonPayload();
        Dictionary<string, object> mergedPayload = new Dictionary<string, object>();

        foreach (var kvp in commonPayload)
        {
            mergedPayload[kvp.Key] = kvp.Value;
        }

        if (payload != null)
        {
            foreach (var kvp in payload)
            {
                mergedPayload[kvp.Key] = kvp.Value;
            }
        }
        UnityWebRequest request = isPost ? CreatePostRequest(url, mergedPayload) : CreateGetRequest(url, mergedPayload);
        request.SetRequestHeader("Client-Sdk", "steam_unity0.0.1");
        yield return request.SendWebRequest();
        HandleRequestResponse(request, onResponse);
    }

    private UnityWebRequest CreatePostRequest(string url, Dictionary<string, object> payload)
    {
        WWWForm form = new WWWForm();
        foreach (var kvp in payload)
        {
            form.AddField(kvp.Key, kvp.Value.ToString());
        }

        UnityWebRequest request = UnityWebRequest.Post(url, form);
        request.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
        Debug.Log($"Adjust]: Created POST request to {url} with payload: {JsonConvert.SerializeObject(payload)}");
        return request;
    }

    private UnityWebRequest CreateGetRequest(string url, Dictionary<string, object> payload)
    {
        var queryString = new StringBuilder();
        foreach (var kvp in payload)
        {
            if (queryString.Length > 0)
            {
                queryString.Append("&");
            }
            queryString.AppendFormat("{0}={1}", UnityWebRequest.EscapeURL(kvp.Key), UnityWebRequest.EscapeURL(kvp.Value.ToString()));
        }

        string fullUrl = url + "?" + queryString.ToString();
        UnityWebRequest request = UnityWebRequest.Get(fullUrl);
        request.SetRequestHeader("Content-Type", "application/json");
        Debug.Log($"Adjust]: Created GET request to {fullUrl}");
        return request;
    }

    private void HandleRequestResponse(UnityWebRequest request, Action<string> onResponse)
    {
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

    private Dictionary<string, string> GenerateCommonPayload()
    {
        var payload = new Dictionary<string, string>
        {
            { "app_token", this.appToken },
            { "environment", this.environment },
            { "created_at", DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'") },
            { "os_name", this.osName },
            { "os_version", this.osVersion },
            { "device_type", this.deviceModel },
            { "app_version", this.appVersion },
            { "sent_at", DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'") },
        };

        if (!string.IsNullOrEmpty(this.steamId))
        {
            payload.Add("steam_id", steamId);
        }
        else
        {
            payload.Add("steam_id", GetSteamId());
        }

        if (!string.IsNullOrEmpty(steamUuid))
        {
            payload.Add("steam_uuid", steamUuid);
        }
        else
        {
            payload.Add("steam_uuid", GetSteamUuid());
        }

        return payload;
    }
    #endregion

    #region Device info
    private string GetSteamId()
    {
        if (SteamManager.Initialized)
        {
            CSteamID steamId = SteamUser.GetSteamID();
            Debug.Log("[Adjust]: steam_id read: " + steamId.ToString());
            return steamId.ToString();
        }
        else
        {
            Debug.LogError("[Adjust]: SteamworksManager not initialized. Could not retrieve steam_id");
            return null;
        }
    }

    private string GetSteamUuid()
    {
        if (PlayerPrefs.HasKey("AdjustSteamUuid"))
        {
            string steamUuid = PlayerPrefs.GetString("AdjustSteamUuid");
            Debug.Log("[Adjust]: Retrieved steam_uuid from PlayerPrefs: " + steamUuid);
            return steamUuid;
        }
        else
        {
            string steamUuid = Guid.NewGuid().ToString();
            PlayerPrefs.SetString("AdjustSteamUuid", steamUuid);
            PlayerPrefs.Save();
            Debug.Log("[Adjust]: Generated and saved new steam_uuid to PlayerPrefs: " + steamUuid);
            return steamUuid;
        }
    }

    private string GetOsName()
    {
#if UNITY_STANDALONE_OSX
        return "macos";
#elif UNITY_STANDALONE_WIN
        return "windows";
#elif UNITY_STANDALONE_LINUX
        return "linux";
#else
        return "unknown";
#endif
    }

    private string GetOsVersion()
    {
        var match = Regex.Match(SystemInfo.operatingSystem, @"\d+(\.\d+)+");
        return match.Success ? match.Value : "unknown";
    }
    #endregion

    #region Helper methods
    private static bool IsAppTokenValid(string appToken)
    {
        if (string.IsNullOrEmpty(appToken))
        {
            return false;
        }
        if (appToken.Length != 12)
        {
            return false;
        }
        return true;
    }

    private static bool IsEnvironmentValid(string environment)
    {
        if (string.IsNullOrEmpty(environment))
        {
            return false;
        }
        if (environment != "sandbox" && environment != "production")
        {
            return false;
        }
        return true;
    }

    private static bool IsMonoBehaviorValid(MonoBehaviour monoBehavior)
    {
        if (monoBehavior == null)
        {
            return false;
        }
        return true;
    }

    private static bool IsEventTokenValid(string eventToken)
    {
        if (string.IsNullOrEmpty(eventToken))
        {
            return false;
        }
        return true;
    }
    #endregion
}