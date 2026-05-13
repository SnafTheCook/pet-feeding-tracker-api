using System.Text.Json.Serialization;

namespace DidWeFeedTheCatToday.Shared.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum Roles
    {
        Admin,
        Parent,
        Child
    }
}
