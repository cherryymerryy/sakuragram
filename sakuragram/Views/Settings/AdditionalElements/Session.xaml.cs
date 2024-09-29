using System;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using TdLib;

namespace sakuragram.Views.Settings.AdditionalElements;

public partial class Session : Button
{
    private static readonly TdClient _client = App._client;
    private TdApi.Session _session;
    
    public Session()
    {
        InitializeComponent();
    }

    public void Update(TdApi.Session session)
    {
        _session = session;

        if (session.IsCurrent)
        {
            ContentDialogSessionInfo.IsPrimaryButtonEnabled = false;
            ContentDialogSessionInfo.PrimaryButtonText = string.Empty;
        }
        else
        {
            ContentDialogSessionInfo.IsPrimaryButtonEnabled = true;
            ContentDialogSessionInfo.PrimaryButtonText = "Terminate";
        }
        
        
        TextBlockApplicationName.Text = session.ApplicationName + " " + session.ApplicationVersion;
        TextBlockPlatformAndVersion.Text = session.DeviceModel;
        
        DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
        dateTime = dateTime.AddSeconds(session.LogInDate).ToLocalTime();
        string loginDate = dateTime.ToShortTimeString();
        
        TextBlockLocationAndDate.Text = session.Location + ", " + loginDate;

        BorderAppColor.Background = session.ApplicationName switch
        {
            "Telegram Desktop" => new SolidColorBrush(Colors.Aqua),
            "Telegram Web" => new SolidColorBrush(Colors.Blue),
            "Telegram Android" => new SolidColorBrush(Colors.Green),
            "Telegram iOS" => new SolidColorBrush(Colors.Orange),
            "Swiftgram" => new SolidColorBrush(Colors.Red),
            "Unigram" => new SolidColorBrush(Colors.Coral),
            "sakuragram" => new SolidColorBrush(Colors.HotPink),
            _ => BorderAppColor.Background
        };
    }

    private void ContentDialogSessionInfo_OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        _client.TerminateSessionAsync(_session.Id);
    }

    private void ContentDialogSessionInfo_OnOpened(ContentDialog sender, ContentDialogOpenedEventArgs args)
    {
        ContentDialogSessionInfo.Title = _session.DeviceModel;
        CardAppliction.Description = _session.ApplicationName;
        CardSystemVersion.Description = _session.DeviceModel;
        CardLocation.Description = _session.Location;
    }

    private void Session_OnClick(object sender, RoutedEventArgs e)
    {
        ContentDialogSessionInfo.ShowAsync();
    }
}