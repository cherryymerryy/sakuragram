using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Newtonsoft.Json.Linq;

namespace CherryMerryGram.Views
{
    public sealed partial class SettingsView : Page
    {
        public SettingsView()
        {
            this.InitializeComponent();

            GetLatestVersion();
        }

        private async Task GetLatestVersion()
        {
            using var client = new HttpClient();
            const string requestUri = "https://api.github.com/repos/cherryymerryy/CherryMerryGram/releases/latest";
            var response = await client.GetAsync(requestUri);
            var content = await response.Content.ReadAsStringAsync();
            dynamic json = JObject.Parse(content);
            Version.Content = $"Version: {json.tag_name}";
            Version.NavigateUri = json.html_url;
        }
    }
}
