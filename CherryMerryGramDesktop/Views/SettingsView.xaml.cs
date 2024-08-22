using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Core;
using Microsoft.UI.Xaml.Controls;

namespace CherryMerryGramDesktop.Views
{
    public sealed partial class SettingsView : Page
    {
        public SettingsView()
        {
            this.InitializeComponent();
            
            Version.Content = $"Latest version: {ThisAssembly.Git.BaseTag}";
            Version.NavigateUri = new Uri($"https://github.com/cherryymerryy/CherryMerryGram/releases/tag/{ThisAssembly.Git.BaseTag}");

            //StartUpdatingDcStatus();
            //UpdateDcStatus(Dc1Status, 1);
        }

        private async void StartUpdatingDcStatus()
        {
            await UpdateDcStatus(Dc1Status, 1);
            await UpdateDcStatus(Dc2Status, 2);
            await UpdateDcStatus(Dc3Status, 3);
            await UpdateDcStatus(Dc4Status, 4);
            await UpdateDcStatus(Dc5Status, 5);
        }
        
        private Task UpdateDcStatus(TextBlock textStatus, int serverIndex)
        {
            string[] dcServers = new string[] {
                "149.154.175.50:443", // DC1
                "149.154.167.50:443",// DC2
                "149.154.175.100:443",// DC3
                "149.154.167.91:443",// DC4
                "91.108.56.100:443" // DC5
            };
            
            textStatus.Text = $"Checking DC{serverIndex}";
            
            Stopwatch stopwatch = Stopwatch.StartNew();

            try
            {
                TcpClient client = new TcpClient();

                var parts = dcServers[serverIndex].Split(':');
                var ipAddress = IPAddress.Parse(parts[0]);
                var port = int.Parse(parts[1]);

                var endPoint = new IPEndPoint(ipAddress, port);

                client.Connect(endPoint);
                
                textStatus.Text = $"DC{serverIndex} Available, ping {stopwatch.ElapsedMilliseconds}ms";
                
                client.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        
            Thread.Sleep(5000);
            
            return Task.CompletedTask;
        }
    }
}
