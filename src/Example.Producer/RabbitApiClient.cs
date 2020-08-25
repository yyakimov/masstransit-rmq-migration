using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Example.Producer
{
    public class RabbitApiClient
    {
        private readonly HttpClient _client;
        // todo: тут vhost тоже надо задавать
        private string _vhost = "vhost";
        public RabbitApiClient(string host, string userName, string password)
        {
            _client = GetRabbitMqApiClient(host, userName, password);
        }

        public async Task AddTmpTopicExchange<T>()
        {
            var exchangeName = GetExchangeName(typeof(T));
            var tmpExchangeName = exchangeName + "Tmp";
            
            // создаём новый exchange
            await CreateTopicExchange(tmpExchangeName);
            // прописываем новые bindings с ключами
            await CreateDuplicateBindings(exchangeName, tmpExchangeName);
            // биндинг между старым и временным exchange'ами
            await CreateBinding(exchangeName, tmpExchangeName);
        }

        public async Task RecreateExchange<T>()
        {
            var exchangeName = GetExchangeName(typeof(T));
            var tmpExchangeName = exchangeName + "Tmp";
            await _client.DeleteAsync($"/api/exchanges/{_vhost}/{exchangeName}");
            await CreateTopicExchange(exchangeName);
            await CreateDuplicateBindings(tmpExchangeName, exchangeName);
        }

        private async Task CreateBinding(string exchangeName, string tmpExchangeName)
        {
            var content =
                new
                {
                    routing_key = "#"
                };
            var contentStr = new StringContent(JsonSerializer.Serialize(content), Encoding.UTF8, "application/json");

            await _client.PostAsync($"/api/bindings/{_vhost}/e/{exchangeName}/e/{tmpExchangeName}", contentStr);
        }

        private async Task CreateDuplicateBindings(string fstExchangeName, string sndExchangeName)
        {
            Console.WriteLine($"Create duplicate bindings from {fstExchangeName} to {sndExchangeName}");
            var getBindings = await _client
                .GetAsync($"/api/exchanges/{_vhost}/{fstExchangeName}/bindings/source");

            if (getBindings.IsSuccessStatusCode)
            {
                var result = await getBindings.Content.ReadAsStringAsync();
                var bindings = JsonSerializer.Deserialize<Binding[]>(result);
                foreach (var binding in bindings)
                {
                    var routingKey = binding.Destination.Split('_')[1];
                    if (routingKey.Length > 2)
                        routingKey = "#";
                    var content =
                        new
                        {
                            routing_key = routingKey
                        };
                    var contentStr = new StringContent(JsonSerializer.Serialize(content), Encoding.UTF8, "application/json");
                    try
                    {
                        var response = await _client.PostAsync(
                            $"/api/bindings/{_vhost}/e/{sndExchangeName}/e/{binding.Destination}",
                            contentStr);
                        if (!response.IsSuccessStatusCode)
                        {
                            Console.WriteLine(response.ReasonPhrase);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
            else
            {
                Console.WriteLine(getBindings.ReasonPhrase);
            }
        }

        private async Task CreateTopicExchange(string exchangeName)
        {
            // {"type":"direct","auto_delete":false,"durable":true,"internal":false,"arguments":{}}
            var content =
                new
                {
                    type = "topic",
                    auto_delete = "false",
                    durable = "true"
                };
            var contentStr = new StringContent(JsonSerializer.Serialize(content), Encoding.UTF8, "application/json");
            await _client.PutAsync($"/api/exchanges/{_vhost}/{exchangeName}", contentStr);
        }

        private static HttpClient GetRabbitMqApiClient(string host, string userName, string password)
        {
            var uriBuilder = new UriBuilder(host);

            var authentication =
                Convert.ToBase64String(
                    Encoding.ASCII.GetBytes($"{userName}:{password}"));

            var client = new HttpClient {BaseAddress = uriBuilder.Uri};
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authentication);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            
            return client;
        }
        
        private static string GetExchangeName(Type type)
        {
            return type.Namespace + ":" + type.Name;
        }
    }
}