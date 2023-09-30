using OpenCvSharp;
using SpawnDev.BlazorJS.JSObjects;

namespace SpawnDev.BlazorJS.OpenCVSharp4.Services
{

    public class OpenCVService : IDisposable
    {
        HttpClient _httpClient;

        public OpenCVService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<CascadeClassifier> LoadCascadeClassifier(string url)
        {
            var text = await _httpClient.GetStringAsync(url);
            System.IO.File.WriteAllText("tmp.xml", text);
            var cascadeClassifier = new CascadeClassifier("tmp.xml");
            return cascadeClassifier;
        }

        public void Dispose()
        {
 
        }
    }
}
