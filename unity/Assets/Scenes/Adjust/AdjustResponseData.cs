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
    public AttributionData AttributionData { get; set; }

    public AdjustResponseData()
    {
        JsonResponse = new Dictionary<string, object>();
        AttributionData = new AttributionData();
    }

    public string GetSerializedJsonResponse()
    {
        return JsonConvert.SerializeObject(JsonResponse, Formatting.Indented);
    }
}

public class AttributionData
{
    [JsonProperty("tracker_token")]
    public string TrackerToken { get; set; }

    [JsonProperty("tracker_name")]
    public string TrackerName { get; set; }

    [JsonProperty("network")]
    public string Network { get; set; }
}

