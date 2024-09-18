using Microsoft.UI.Xaml.Controls;
using TdLib;

namespace CherryMerryGramDesktop.Views.Settings;

public partial class UpdateManager : Page
{
    private static TdClient _client = App._client;
    
    public UpdateManager()
    {
        InitializeComponent();
        
        TextBlockVersionInfo.Text = $"v{ThisAssembly.Git.BaseTag}, TdLib 1.8.29";
    }
}