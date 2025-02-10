using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic; // Added for List<T>

public class ApiService
{
    private readonly HttpClient _httpClient;

    public ApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<T> GetAsync<T>(string url)
    {
        var response = await _httpClient.GetStringAsync(url);
        return JsonConvert.DeserializeObject<T>(response);
    }

    public async Task<List<T>> GetListAsync<T>(string url) // Added Component 1
    {
        var response = await _httpClient.GetStringAsync(url);
        return JsonConvert.DeserializeObject<List<T>>(response);
    }

    public async Task<HttpResponseMessage> PostAsync<T>(string url, T data) // Added Component 2
    {
        var json = JsonConvert.SerializeObject(data);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        return await _httpClient.PostAsync(url, content);
    }

    public async Task<HttpResponseMessage> PutAsync<T>(string url, T data) // Added Component 3
    {
        var json = JsonConvert.SerializeObject(data);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        return await _httpClient.PutAsync(url, content);
    }

    public async Task<HttpResponseMessage> DeleteAsync(string url) // Added Component 4
    {
        return await _httpClient.DeleteAsync(url);
    }

    public async Task<string> GetRawAsync(string url) // Added Component 5
    {
        return await _httpClient.GetStringAsync(url);
    }
}
