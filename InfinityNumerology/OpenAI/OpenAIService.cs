using InfinityNumerology.OpenAI.Model;
using Newtonsoft.Json;
using System.Text;

namespace InfinityNumerology.OpenAI
{
    public class OpenAIService
    {
        private readonly HttpClient client = new HttpClient();
        private readonly string apiUrl = "https://api.proxyapi.ru/openai/v1/chat/completions";
        private readonly string apiKey = "Your_Token";
        private readonly IConfiguration _configuration;

        public OpenAIService(IConfiguration configuration)
        {
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            _configuration = configuration;
        }

        public async Task<string> MessageResponse(string content,string systemHelp, string assisnantHelp)
        {
            
            var requestData = new
            {
                model = "gpt-3.5-turbo-0125",
                messages = new List<Message>
                {
                    new Message {role = "system", content = systemHelp},
                    new Message { role = "user", content = content },
                    new Message {role = "assistant", content = assisnantHelp}
                }
            };

            string jsonData = JsonConvert.SerializeObject(requestData);
            var requestContent = new StringContent(jsonData, Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage response = await client.PostAsync(apiUrl, requestContent);
                response.EnsureSuccessStatusCode();  

                string responseBody = await response.Content.ReadAsStringAsync();

                ChatResponse chatResponse = JsonConvert.DeserializeObject<ChatResponse>(responseBody);

                return $"Ваша расшифровка: {chatResponse.choices[0].message.content}";
            }
            catch (HttpRequestException e)
            {
                return $"Ошибка запроса: {e.Message}";
            }
        }
    }
}
