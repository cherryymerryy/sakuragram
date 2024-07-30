using Microsoft.UI.Xaml.Controls;
using TdLib;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;

namespace CherryMerryGram.Views
{
	public sealed partial class AccountView : Page
	{
		private static readonly TdClient _client = MainWindow._client;
        
        private string _displayName;
        private string _username;
        private TdApi.ProfilePhoto _profilePicture;

		public AccountView()
		{
			this.InitializeComponent();
            
            InitializeAllVariables();
        }

        private async void InitializeAllVariables()
        {
            var currentUser = await GetCurrentUser();
            _displayName = $"{currentUser.FirstName} {currentUser.LastName}";
            _username = $"{currentUser.Usernames?.ActiveUsernames[0]}";
            _profilePicture = currentUser.ProfilePhoto;

            textBlock_Username.Text = _username;
            textBlock_DisplayName.Text = _displayName;
        }

        private static async Task<TdApi.User> GetCurrentUser()
        {
            return await _client.ExecuteAsync(new TdApi.GetMe());
        }

        private async void Button_LogOut_OnClick(object sender, RoutedEventArgs e)
        {
            await _client.ExecuteAsync(new TdApi.LogOut());
        }
    }
}
