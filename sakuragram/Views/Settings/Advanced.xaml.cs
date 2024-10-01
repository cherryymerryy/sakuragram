using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace sakuragram.Views.Settings;

public partial class Advanced : Page
{
    private DispatcherTimer _timer;

    public Advanced()
    {
        InitializeComponent();
    
        _timer = new DispatcherTimer();
        _timer.Interval = TimeSpan.FromSeconds(5);
        _timer.Tick += UpdateDCStatus;
        _timer.Start();
    }

    private async void UpdateDCStatus(object sender, object e)
    {
        string[] dcServers = new string[] {
            "149.154.175.50:443", // DC1
            "149.154.167.50:443",// DC2
            "149.154.175.100:443",// DC3
            "149.154.167.91:443",// DC4
            "91.108.56.100:443" // DC5
        };

        for (int i = 0; i < dcServers.Length; i++)
        {
            try
            {
                var dcServer = dcServers[i];
                var stopwatch = Stopwatch.StartNew();

                using (var client = new TcpClient())
                {
                    var parts = dcServer.Split(':');
                    var ipAddress = IPAddress.Parse(parts[0]);
                    var port = int.Parse(parts[1]);

                    await client.ConnectAsync(ipAddress, port);

                    stopwatch.Stop();

                    switch (i)
                    {
                        case 0: TextBlockDC1Ping.Text = $"{stopwatch.ElapsedMilliseconds} ms"; break;
                        case 1: TextBlockDC2Ping.Text = $"{stopwatch.ElapsedMilliseconds} ms"; break;
                        case 2: TextBlockDC3Ping.Text = $"{stopwatch.ElapsedMilliseconds} ms"; break;
                        case 3: TextBlockDC4Ping.Text = $"{stopwatch.ElapsedMilliseconds} ms"; break;
                        case 4: TextBlockDC5Ping.Text = $"{stopwatch.ElapsedMilliseconds} ms"; break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}