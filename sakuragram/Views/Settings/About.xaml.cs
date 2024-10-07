using System;
using System.ComponentModel;
using System.IO;
using Windows.Storage;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace sakuragram.Views.Settings;

public partial class About : Page
{
    private readonly ApplicationDataContainer _localSettings = ApplicationData.Current.LocalSettings;
    private string _appName;
    private readonly string _appLatestVersion;
    private string _appLatestVersionLink;
    private static readonly Services.UpdateManager _updateManager = App.UpdateManager;
    
    public About()
    {
        InitializeComponent();
        
        System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
        System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
        _appName = assembly.GetName().Name;
        _appLatestVersion = fvi.FileVersion;
        _appLatestVersionLink = $"https://github.com/{Config.GitHubRepo}/releases/tag/{_appLatestVersion}";
        
        TextBlockVersionInfo.Text = $"Current version: {_appLatestVersion}, TdLib {Config.TdLibVersion}";
        
        _updateManager._asyncCompletedEventHandler += AsyncCompletedEventHandler;
        CheckForUpdates();
        
        if (File.Exists(AppContext.BaseDirectory + @"\sakuragram_Release_x64.msi"))
        {
            ButtonNewVersionAvailable.Content = "Install";
            ButtonNewVersionAvailable.IsEnabled = true;
            ButtonNewVersionAvailable.Click += ButtonNewVersionAvailable_OnUpdateDownloaded;
        }
    }
    
    private void AsyncCompletedEventHandler(object sender, AsyncCompletedEventArgs e)
    {
        if (_localSettings.Values["AutoUpdate"] != null && (bool)_localSettings.Values["AutoUpdate"])
        {
            ButtonNewVersionAvailable.Content = "Installing...";
            _updateManager.Update();
        }
        else
        {
            ButtonNewVersionAvailable.Content = "Install";
            ButtonNewVersionAvailable.IsEnabled = true;
            ButtonNewVersionAvailable.Click += ButtonNewVersionAvailable_OnUpdateDownloaded;
        }
    }

    private void ButtonCheckForUpdates_OnClick(object sender, RoutedEventArgs e)
    {
        CheckForUpdates();
    }
    
    private void ButtonNewVersionAvailable_OnUpdateDownloaded(object sender, RoutedEventArgs e)
    {
        _updateManager.Update();
    }

    private void CheckForUpdates()
    {
        try
        {
            ButtonCheckForUpdates.IsEnabled = false;
            CardCheckForUpdates.Description = "Checking for updates...";
            
            if (_updateManager.CheckForUpdates())
            {
                CardCheckForUpdates.Description = $"New version available: {ThisAssembly.Git.BaseTag}";
                CardNewVersionAvailable.Visibility = Visibility.Visible;
            }
            else
            {
                CardCheckForUpdates.Description = $"Current version: {_appLatestVersion}";
            }
            
            ButtonCheckForUpdates.IsEnabled = true;
        }
        catch (Exception e)
        {
            CardCheckForUpdates.Description = $"Error: {e.Message}";
            throw;
        }
    }
    
    private void ButtonNewVersionAvailable_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            _updateManager.DownloadUpdate();
            ButtonNewVersionAvailable.Content = "Downloading...";
            ButtonNewVersionAvailable.IsEnabled = false;
        }
        catch (Exception exception)
        {
            CardNewVersionAvailable.Description = $"Error: {exception.Message}";
            ButtonNewVersionAvailable.Content = "Download failed";
            throw;
        }
    }
}