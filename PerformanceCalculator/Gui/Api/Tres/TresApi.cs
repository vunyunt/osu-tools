using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PerformanceCalculator.Gui.Api.Tres
{
    public struct TresBeatmap {
        public string beatmap_id;
        public double new_rating;
    }

    public class TresApi
    {
        private HttpClient client;
        private string baseUrl;

        public TresApi() : this("http://127.0.0.1:3000")
        {
        }

        public TresApi(string baseUrl)
        {
            this.baseUrl = baseUrl;
            client = new HttpClient();
            client.BaseAddress = new System.Uri(this.baseUrl);
        }

        public async Task<string[]> getBeatmapIds(uint limit = 10)
        {
            HttpRequestMessage message = new HttpRequestMessage(
                HttpMethod.Get,
                $"/beatmaps?filter={{\"limit\":{limit}}}");
            HttpResponseMessage response = client.Send(message);
            string responseString = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<TresBeatmap[]>(responseString).Select(b => b.beatmap_id).ToArray();
        }

        public async Task<TresBeatmap[]> getBeatmaps(uint limit = 10)
        {
            HttpRequestMessage message = new HttpRequestMessage(
                HttpMethod.Get,
                $"/beatmaps?filter={{\"limit\":{limit}}}");
            HttpResponseMessage response = client.Send(message);
            string responseString = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<TresBeatmap[]>(responseString);
        }
    }
}