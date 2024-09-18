using InfinityNumerology.OpenAI;
using InfinityNumerology.Service.Text;
using Microsoft.AspNetCore.Mvc;

namespace InfinityNumerology.Service
{
    public class Service
    {
        private readonly OpenAIService _openAI;
        private readonly ILogger<Service> _logger;
        IConfiguration _configuration;
        public Service(OpenAIService openAI, ILogger<Service> logger, IConfiguration configuration)
        {
            _openAI = openAI;
            _logger = logger;
            _configuration = configuration;
        }
        public async Task<string> DistributorAsync(DateTime date, string command)
        {
            string? prompt = null;
            string systemHelp, assistantHelp;
            switch(command)
            {
                case "Расшифровка даты рождения":
                    prompt = TextHepler.DecodingBirthdayPrompt(date,out systemHelp, out assistantHelp);
                    break;
                case "Совместимость знаков зодиака":
                    prompt = TextHepler.ZodiakPrompt(date, out systemHelp, out assistantHelp);
                    break;
                case "Матрица судьбы":
                    prompt = TextHepler.MartixOfFate(date, out systemHelp, out assistantHelp);
                    break;
                default:
                    return "Неизвестная ошибка, попробуйте ещё раз";
            }
            try
            {
                if(prompt != null)
                {
                var resultMessage = await Message(date, prompt, systemHelp, assistantHelp);
                return resultMessage;
                }
                return "Ошибка обработки промпта";
            }
            catch (Exception exception)
            {
                return $"Ошибка запроса: {exception.Message}";
            }


        }
        
        public async Task<string> Message(DateTime date, string prompt, string systemHelp, string assistantHelp)
        {
            var result = await _openAI.MessageResponse(prompt, systemHelp, assistantHelp);
            return result;
        }

        public async Task<string> DistributorWithoutDateAsync(string command)
        {
            switch(command)
            {
                case "Как производится расшифрока?":
                    return TextHepler.DecodingInfo();
                case "Обратная связь":
                    return TextHepler.FeedBack();
                case "Информация":
                    return TextHepler.Information();
                default:
                    return "Неизвестная ошибка, попробуйте ещё раз";
            }
        }

    }
}
