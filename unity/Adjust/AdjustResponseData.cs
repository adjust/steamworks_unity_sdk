using System;
using System.Collections.Generic;
using Newtonsoft.Json;

public class AdjustResponseData
{
    public long ResponseCode { get; set; }
    public string ResponseBody { get; set; }
    public string Message { get; set; }
    public string Timestamp { get; set; }
    public string Adid { get; set; }
    public string Error { get; set; }
    public Dictionary<string, object> JsonResponse { get; set; }
    public AdjustAttributionData AttributionData { get; set; }

    public AdjustResponseData()
    {
        JsonResponse = new Dictionary<string, object>();
        AttributionData = new AdjustAttributionData();
    }

    public string GetSerializedJsonResponse()
    {
        return JsonConvert.SerializeObject(JsonResponse, Formatting.Indented);
    }
}

public class AdjustAttributionData
{
    [JsonProperty("tracker_token")]
    public string TrackerToken { get; set; }

    [JsonProperty("tracker_name")]
    public string TrackerName { get; set; }

    [JsonProperty("network")]
    public string Network { get; set; }

    [JsonProperty("campaign")]
    public string Campaign { get; set; }

    [JsonProperty("adgroup")]
    public string Adgroup { get; set; }

    [JsonProperty("creative")]
    public string Creative { get; set; }

    [JsonProperty("click_label")]
    public string ClickLabel { get; set; }

    [JsonProperty("cost_type")]
    public string CostType { get; set; }

    [JsonProperty("cost_amount")]
    public double CostAmount { get; set; }

    [JsonProperty("cost_currency")]
    public string CostCurrency { get; set; }

    [JsonProperty("fb_install_referrer")]
    public string FbInstallReferrer { get; set; }
}

