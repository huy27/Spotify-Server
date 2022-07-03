using System.Net.Http;

namespace Application.Ultilities
{
    public static class Pinger
    {
        static HttpClient _client = new HttpClient();
        public static void Ping()
        {
            _client.GetAsync("https://bsite.net/huy27297/hangfire");
        }
    }
}
