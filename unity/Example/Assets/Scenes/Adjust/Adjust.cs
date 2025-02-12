using System;
using System.Text;
using UnityEngine;
using Steamworks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

    private Adjust(AdjustConfig adjustConfig)
    {
        this.appToken = adjustConfig.AppToken;
        this.environment = adjustConfig.Environment;
        this.steamId = GetSteamId();
        this.steamUuid = GetSteamUuid();
        this.osName = GetOsName();
        this.osVersion = GetOsVersion();
        this.deviceModel = SystemInfo.deviceModel;
        this.appVersion = Application.version;
        this.monoBehavior = adjustConfig.MonoBehaviour;
    }

    #region Public API
    public static void InitSdk(AdjustConfig adjustConfig, Action<AdjustResponseData> onResponse)
    {
        if (defaultInstance != null)
        {
            Debug.LogError("[Adjust]: Adjust SDK already initialized");
            return;
        }
        if (IsAdjustConfigValid(adjustConfig) == false)
        {
            Debug.LogError("[Adjust]: Adjust Config is not valid");
            return;
        }

        defaultInstance = new Adjust(adjustConfig);
        defaultInstance.TrackSessionInternal(onResponse);
    }

    public static void TrackEvent(AdjustEvent adjustEvent, Action<AdjustResponseData> onResponse)
    {
        if (defaultInstance == null)
        {
            Debug.LogError("[Adjust]: Adjust SDK not initialized - call InitSdk first");
            return;
        }

        defaultInstance.TrackEventInternal(adjustEvent, onResponse);
    }

    public static void GetAttribution(Action<AdjustResponseData> onResponse)
    {
        if (defaultInstance == null)
        {
            Debug.LogError("[Adjust]: Adjust SDK not initialized - call InitSdk first");
            return;
        }

        defaultInstance.GetAttributionInternal(onResponse);
    }
    #endregion

    #region SDK logic
    private void TrackSessionInternal(Action<AdjustResponseData> onResponse)
    {
        string url = $"{AdjustBaseUrl}/session";
        monoBehavior.StartCoroutine(SendRequest(url, null, onResponse));
    }

    private void TrackEventInternal(AdjustEvent adjustEvent, Action<AdjustResponseData> onResponse)
    {
        if (IsAdjustEventValid(adjustEvent) == false)
        {
            Debug.LogError("[Adjust]: AdjustEvent cannot be null.");
            return;
        }

        string url = $"{AdjustBaseUrl}/event";

        Dictionary<string, object> eventDictionary = adjustEvent.GetEventDictionary();
        var eventParameters = new Dictionary<string, object>();

        if (eventDictionary != null)
        {
            foreach (var param in eventDictionary)
            {
                if (param.Value is Dictionary<string, object>)
                {
                    var nestedDict = param.Value as Dictionary<string, object>;
                    eventParameters[param.Key] = JsonConvert.SerializeObject(nestedDict);
                }
                else
                {
                    eventParameters[param.Key] = param.Value;
                }
            }
        }

        monoBehavior.StartCoroutine(SendRequest(url, eventParameters, onResponse));
    }

    private void GetAttributionInternal(Action<AdjustResponseData> onResponse)
    {
        string url = $"{AdjustBaseUrl}/attribution";
        monoBehavior.StartCoroutine(SendRequest(url, null, onResponse, false));
    }

    private IEnumerator SendRequest(string url, Dictionary<string, object> payload, Action<AdjustResponseData> onResponse, bool isPost = true)
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
        Debug.Log($"[Adjust]: Created POST request to {url} with payload: {JsonConvert.SerializeObject(payload)}");
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
        Debug.Log($"[Adjust]: Created GET request to {fullUrl}");
        return request;
    }

    private void HandleRequestResponse(UnityWebRequest request, Action<AdjustResponseData> onResponse)
    {
        AdjustResponseData adjustResponse = ParseResponse(request);
        if (adjustResponse != null)
        {
            Debug.Log("[Adjust]: HTTP request completed successfully: " + request.downloadHandler.text);
            onResponse?.Invoke(adjustResponse);
        }
        else
        {
            Debug.LogError($"[Adjust]: HTTP request failed: {request.error}, Response: {request.downloadHandler.text}");
            onResponse?.Invoke(null);
        }
    }

    private AdjustResponseData ParseResponse(UnityWebRequest request)
    {
        long responseCode = request.responseCode;
        string responseBody = request.downloadHandler.text;

        AdjustResponseData adjustResponse = new AdjustResponseData
        {
            ResponseCode = responseCode,
            ResponseBody = responseBody
        };

        if (!string.IsNullOrEmpty(responseBody))
        {
            try
            {
                adjustResponse.JsonResponse = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseBody);
                if (adjustResponse.JsonResponse.TryGetValue("message", out var message))
                {
                    adjustResponse.Message = message.ToString();
                }
                if (adjustResponse.JsonResponse.TryGetValue("timestamp", out var timestamp))
                {
                    adjustResponse.Timestamp = timestamp.ToString();
                }
                if (adjustResponse.JsonResponse.TryGetValue("adid", out var adid))
                {
                    adjustResponse.Adid = adid.ToString();
                }
                if (adjustResponse.JsonResponse.TryGetValue("error", out var error))
                {
                    adjustResponse.Error = error.ToString();
                }
                if (adjustResponse.JsonResponse.TryGetValue("attribution", out var attributionObj))
                {
                    if (attributionObj is JObject attributionJson)
                    {
                        adjustResponse.AttributionData = attributionJson.ToObject<AdjustAttributionData>();
                    }
                    else
                    {
                        Debug.LogError("[Adjust]: Attribution object is not of type JObject.");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[Adjust]: Failed to parse Adjust response JSON: " + ex.Message);
                return null;
            }
        }

        return adjustResponse;
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

        if (!string.IsNullOrEmpty(steamId))
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
    private static bool IsAdjustEventValid(AdjustEvent adjustEvent)
    {
        return adjustEvent?.EventToken != null;
    }

    private static bool IsAdjustConfigValid(AdjustConfig adjustConfig)
    {
        return adjustConfig?.AppToken != null;
    }
    #endregion
}