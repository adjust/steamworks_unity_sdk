using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public class AdjustEvent
{
    public string EventToken { get; private set; }
    public double? Revenue { get; private set; }
    public string Currency { get; private set; }

    private Dictionary<string, object> innerCallbackParameters;
    private Dictionary<string, object> innerPartnerParameters;

    #region Public API
    public AdjustEvent(string eventToken)
    {
        EventToken = eventToken;
        innerCallbackParameters = new Dictionary<string, object>();
        innerPartnerParameters = new Dictionary<string, object>();
    }

    public void SetRevenue(double revenue, string currency)
    {
        if (revenue <= 0)
        {
            Debug.LogError("[Adjust]: Revenue value is invalid.");
            return;
        }

        if (string.IsNullOrEmpty(currency))
        {
            Debug.LogError("[Adjust]: Currency must be set with revenue.");
            return;
        }

        Revenue = revenue;
        Currency = currency;
    }

    public void AddCallbackParameter(string key, string value)
    {
        if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
        {
            innerCallbackParameters[key] = value;
        }
        else
        {
            Debug.LogError("[Adjust]: Callback parameter key or value cannot be null or empty.");
            return;
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
            Debug.LogError("[Adjust]: Partner parameter key or value cannot be null or empty.");
            return;
        }
    }

    public bool IsValid()
    {
        return !string.IsNullOrEmpty(EventToken);
    }

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
    #endregion
}
