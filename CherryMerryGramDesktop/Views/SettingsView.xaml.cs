using System;
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
        }
    }
}
