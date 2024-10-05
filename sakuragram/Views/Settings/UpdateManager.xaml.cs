using Windows.Storage;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace sakuragram.Views.Settings;

public partial class UpdateManager : Page
{
    private readonly ApplicationDataContainer _localSettings = ApplicationData.Current.LocalSettings;
    private string _appName;
    private string _appLatestVersion;
    private string _appLatestVersionLink;
    
    public UpdateManager()
    {
        InitializeComponent();
        
        System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
        System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
        _appName = assembly.GetName().Name;
        _appLatestVersion = fvi.FileVersion;
        _appLatestVersionLink = $"https://github.com/cherryymerryy/sakuragram/releases/tag/{_appLatestVersion}";
        
        TextBlockVersionInfo.Text = $"Current version: {_appLatestVersion}, TDLib 1.8.29";

        #region Settings

        if (_localSettings != null)
        {
            if (_localSettings.Values["AutoUpdate"] != null)
            {
                bool autoUpdateValue = (bool)_localSettings.Values["AutoUpdate"];
                ToggleSwitchAutoUpdate.IsOn = autoUpdateValue;
            }
            else
            {
                ToggleSwitchAutoUpdate.IsOn = true;
                _localSettings.Values["AutoUpdate"] = true;
            }
        
            if (_localSettings.Values["InstallBeta"] != null)
            {
                bool installBetaValue = (bool)_localSettings.Values["InstallBeta"];
                ToggleSwitchInstallBeta.IsOn = installBetaValue;
            }
            else
            {
                ToggleSwitchInstallBeta.IsOn = false;
                _localSettings.Values["InstallBeta"] = false;
            }
        }

        #endregion
    }

    private void ButtonCheckForUpdates_OnClick(object sender, RoutedEventArgs e)
    {
    }

    #region setting parameters

    private void ToggleSwitch_OnToggled(object sender, RoutedEventArgs e)
    {
        _localSettings.Values["InstallBeta"] = ToggleSwitchInstallBeta.IsOn;
    }

    private void ToggleSwitchAutoUpdate_OnToggled(object sender, RoutedEventArgs e)
    {
        _localSettings.Values["AutoUpdate"] = ToggleSwitchAutoUpdate.IsOn;
    }

    #endregion
}