using Application.Models;
using Domain;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;


namespace CompareBlazorApp.Services
{
    public class CompareService
    {
        private readonly HttpClient _httpClient;

        public CompareService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("Compare");
        }

        public async Task<string> CompareObjects(CompareRequest<Employe> request)
        {
            string requestUrl = _httpClient.BaseAddress + "api/Compare/compare";
            string jsonRequest = JsonSerializer.Serialize(request);
            Console.WriteLine($"Sending request to: {requestUrl}");
            Console.WriteLine($"Request Body: {jsonRequest}");

            var response = await _httpClient.PostAsJsonAsync("api/Compare/compare", request);

            string responseText = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Response Status: {response.StatusCode}");
            Console.WriteLine($"Response Body: {responseText}");

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
                return string.Join("\n", result.Select(kv => $"{kv.Key}: {kv.Value}"));
            }

            return $"Error: {responseText}";
        }

    }
}
