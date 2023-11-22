using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace api_CDEK;

public class ClientCDEK
{
    private readonly string _account;
    private readonly string _securePassword;
    private string _accessToken = "";
    
    public ClientCDEK(string account, string securePassword)
    {
        _account = account;
        _securePassword = securePassword; 
        RenewAccessToken().Wait();
    }

    
    
    private async Task RenewAccessToken()
    {
        string urlRequest = "https://api.cdek.ru/v2/oauth/token?parameters";
        
        try
        {
            using (HttpClient client = new HttpClient())
            {
                var formData = new Dictionary<string, string>
                {
                    { "grant_type", "client_credentials"},
                    { "client_id", _account },
                    { "client_secret", _securePassword },
                };
                HttpContent content = new FormUrlEncodedContent(formData);
                HttpResponseMessage response = await client.PostAsync(urlRequest, content);
                
                if (response.IsSuccessStatusCode)
                {
                    string pattern = "\"access_token\":\"(.*?)\"";
                    var match = Regex.Match(response.Content.ReadAsStringAsync().Result, pattern);
                    _accessToken = match.Groups[1].Value;
                    return;
                }
                
                Console.WriteLine("Error: " + response.StatusCode);

            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        
    }

    public async Task<string?> GetCodeCity(string fullNameCity)
    {
        string urlRequest = "https://api.cdek.ru/v2/location/cities?";
        urlRequest += $"city={fullNameCity}";
        
        try
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + _accessToken);
                HttpResponseMessage response = await client.GetAsync(urlRequest);
                
                if (response.IsSuccessStatusCode)
                {
                    string pattern = "\"code\":(\\d+)";
                    var match = Regex.Match(response.Content.ReadAsStringAsync().Result, pattern);
                    return match.Groups[1].Value;
                }
                
                Console.WriteLine("Error: " + response.StatusCode);
                
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
            
        }

        return null;
    }
    
    public async Task<string?> CalculatePrice(string cityArrival, double mass, double volume)
    {
        string urlRequest = "https://api.cdek.ru/v2/calculator/tariff";
        string tarifCode = "136";   // код тарифа
        string fromCodeCity = "563";    //код города отправления - Чайковский 
        if (cityArrival.Contains(','))
        {
            cityArrival = cityArrival.Split(',')[0];
            Console.WriteLine(cityArrival);
        }
        string? toCodeCity = await GetCodeCity(cityArrival);
        string massStr = (mass * 1000).ToString(CultureInfo.InvariantCulture);
        string volumeStr = volume.ToString(CultureInfo.InvariantCulture);
        
        string jsonRequestData = $"{{\n    \"tariff_code\": \"{tarifCode}\",\n    " +
                     $"\"from_location\": {{\n        \"code\": {fromCodeCity}\n    }},\n    \"to_location\": {{\n        " +
                     $"\"code\": {toCodeCity}\n    }},\n    \"packages\": [\n        {{\n            \"height\": 0,\n           " +
                     $" \"length\": 0,\n            \"weight\": {massStr},\n            \"width\": 0\n        }}\n    ]\n}}";
        
        try
        {
            using (HttpClient client = new HttpClient())
            {
                StringContent content = new StringContent(jsonRequestData, Encoding.UTF8, "application/json");
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + _accessToken);
                HttpResponseMessage response = await client.PostAsync(urlRequest, content);
                
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine(response.Content.ReadAsStringAsync().Result);
                    string pattern =  "\"delivery_sum\":(\\d+(\\.\\d+)?)";
                    var match = Regex.Match(response.Content.ReadAsStringAsync().Result, pattern);
                    
                    return match.Groups[1].Value;
                }
                
                Console.WriteLine("Error: " + response.StatusCode);

            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        return null;
    }

}