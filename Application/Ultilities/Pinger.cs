using System.Net.Http;

namespace Application.Ultilities
{
    public static class Pinger
    {
        static HttpClient _client = new HttpClient();
        public static void Ping()
        {
            _client.GetAsync("https://spotify97.azurewebsites.net/hangfire");
        }
    }
}
