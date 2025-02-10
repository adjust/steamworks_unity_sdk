using System;
using System.Collections.Generic;
using Newtonsoft.Json;

public class AdjustResponse
{
    public long ResponseCode { get; set; }
    public string ResponseText { get; set; }
    public string Message { get; set; }
    public string Timestamp { get; set; }
    public string Adid { get; set; }
    public string Error { get; set; }
    public Dictionary<string, object> JsonResponse { get; set; }

    public string GetSerializedJsonResponse()
    {
        return JsonConvert.SerializeObject(JsonResponse, Formatting.Indented);
    }

}