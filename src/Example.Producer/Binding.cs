using System.Text.Json.Serialization;

namespace Example.Producer
{
    public class Binding
    {
        [JsonPropertyName("source")]
        public string Source { get; set; }
        [JsonPropertyName("destination")]
        public string Destination { get; set; }
        public string DestinationType { get; set; }
        public string RoutingKey { get; set; }
        // "source": "Example.Contracts:TestMessage",
        // "vhost": "vhost",
        // "destination": "country_agnostic",
        // "destination_type": "exchange",
        // "routing_key": "",
        // "arguments": {},
        // "properties_key": "~"
    }
}