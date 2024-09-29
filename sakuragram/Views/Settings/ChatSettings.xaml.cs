using System.Linq;
using Windows.Storage;
using Microsoft.UI.Xaml.Controls;

namespace sakuragram.Views.Settings;

public partial class ChatSettings : Page
{
    private ApplicationDataContainer _localSettings = ApplicationData.Current.LocalSettings;
    
    public ChatSettings()
    {
        InitializeComponent();
        
        if (_localSettings.Values["ChannelBottomButton"] != null)
        {
            var value = _localSettings.Values["ChannelBottomButton"];
            ComboBoxChannelBottomButton.SelectedItem = ComboBoxChannelBottomButton.Items
                .Cast<ComboBoxItem>().FirstOrDefault(x => x.Content == value);
        }
    }

    private void ComboBoxChannelBottomButton_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ComboBoxChannelBottomButton.SelectedValue == null) return;
        var value = ComboBoxChannelBottomButton.SelectedValue as ComboBoxItem;
        _localSettings.Values["ChannelBottomButton"] = value.Content;
    }
}