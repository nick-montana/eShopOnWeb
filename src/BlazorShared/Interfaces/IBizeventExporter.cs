using System.Collections.Generic;
using Dynatrace.OpenKit.Util.Json.Objects;


namespace BlazorShared.Interfaces;

public interface IBizeventExporter
{
    void sendBizEvent(string eventName, Dictionary<string, JsonValue> data);
}
