using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class AdjustEvent
{
    public string EventToken { get; private set; }
    public double? Revenue { get; private set; }
    public string Currency { get; private set; }

    private Dictionary<string, object> innerCallbackParameters;
    private Dictionary<string, object> innerPartnerParameters;

    public AdjustEvent(string eventToken)
    {
        if (IsEventTokenValid(eventToken) == false)
        {
            Debug.LogError("[Adjust]: Event token cannot be null or empty.");
            return;
        }

        EventToken = eventToken;
        innerCallbackParameters = new Dictionary<string, object>();
        innerPartnerParameters = new Dictionary<string, object>();
    }

    public void AddCallbackParameter(string key, string value)
    {
        if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
        {
            innerCallbackParameters[key] = value;
        }
        else
        {
            throw new ArgumentException("[Adjust]: Callback parameter key or value cannot be null or empty.");
        }
    }

    public void AddPartnerParameter(string key, string value)
    {
        if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
        {
            innerPartnerParameters[key] = value;
        }
        else
        {
            throw new ArgumentException("[Adjust]: Partner parameter key or value cannot be null or empty.");
        }
    }

    public void SetRevenue(double revenue, string currency)
    {
        Revenue = revenue;
        Currency = currency;
    }

    #region Helper methods
    public Dictionary<string, object> GetEventDictionary()
    {
        var eventData = new Dictionary<string, object>
        {
            { "event_token", EventToken },
            { "callback_params", innerCallbackParameters },
            { "partner_params", innerPartnerParameters }
        };

        if (Revenue.HasValue && !string.IsNullOrEmpty(Currency))
        {
            eventData["revenue"] = Revenue.Value;
            eventData["currency"] = Currency;
        }

        return eventData;
    }

    private static bool IsEventTokenValid(string eventToken)
    {
        if (string.IsNullOrEmpty(eventToken))
        {
            return false;
        }
        return true;
    }

    public string ToJson()
    {
        return JsonConvert.SerializeObject(GetEventDictionary());
    }
    #endregion
}
