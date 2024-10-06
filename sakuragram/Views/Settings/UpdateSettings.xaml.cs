using System;
using System.ComponentModel;
using Windows.Storage;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace sakuragram.Views.Settings;

public partial class UpdateSettings : Page
{
    private readonly ApplicationDataContainer _localSettings = ApplicationData.Current.LocalSettings;
    private string _appName;
    private readonly string _appLatestVersion;
    private string _appLatestVersionLink;
    private static readonly Services.UpdateManager _updateManager = App.UpdateManager;
    
    public UpdateSettings()
    {
        InitializeComponent();
        
        System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
        System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
        _appName = assembly.GetName().Name;
        _appLatestVersion = fvi.FileVersion;
        _appLatestVersionLink = $"https://github.com/{Config.GitHubRepo}/releases/tag/{_appLatestVersion}";
        
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