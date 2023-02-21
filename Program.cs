using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace ChatGPTAPI
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine($"Current directory: {Directory.GetCurrentDirectory()}");
            IConfigurationRoot configuration;

            try
            {
                var configBuilder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json");

                configuration = configBuilder.Build();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading configuration: {ex.Message}");
                return;
            }

            string apiKey = configuration.GetSection("OpenAI")["ApiKey"];

            Console.WriteLine("Enter your prompt:");
            string prompt = Console.ReadLine();

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                var values = new
                {
                    prompt = prompt,
                    max_tokens = 512
                };

                var content = new StringContent(JsonConvert.SerializeObject(values), System.Text.Encoding.UTF8, "application/json");

                var response = await client.PostAsync("https://api.openai.com/v1/engines/davinci-codex/completions", content);

                if (response.IsSuccessStatusCode)
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();

                    dynamic responseObj = JsonConvert.DeserializeObject(apiResponse);

                    if (responseObj != null && responseObj.choices != null && responseObj.choices.Count > 0 && responseObj.choices[0].text != null)
                    {
                        Console.WriteLine(responseObj.choices[0].text);
                    }
                    else
                    {
                        Console.WriteLine("Error: Invalid response from API");
                    }
                }
                else
                {
                    Console.WriteLine($"Error: {response.ReasonPhrase}");
                }
            }
        }
    }
}
