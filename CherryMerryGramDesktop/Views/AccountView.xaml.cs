using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using TdLib;
using WinRT;

namespace CherryMerryGramDesktop.Views
{
	public sealed partial class AccountView : Page
	{
		private static readonly TdClient _client = App._client;
        
        private Window _mWindow;
        
        private string _firstName;
        private string _lastName;
        private string _username;
        private string _bio;
        private string _phoneNumber;

		public AccountView()
		{
			this.InitializeComponent();
            
            InitializeAllVariables();
        }

        private async void InitializeAllVariables()
        {
            var currentUser = await GetCurrentUser();
            var userFullInfo = await _client.GetUserFullInfoAsync(currentUser.Id);
            GetProfilePhoto(currentUser);
            
            _firstName = $"{currentUser.FirstName}";
            _lastName = $"{currentUser.LastName}";
            _username = $"@{currentUser.Usernames?.ActiveUsernames[0]}";
            _bio = $"{userFullInfo.Bio.Text}";
            _phoneNumber = $"+{currentUser.PhoneNumber}";

            TextBoxUsername.Text = _username; 
            TextBoxFirstName.Text = _firstName;
            TextBoxLastName.Text = _lastName;
            TextBoxBio.Text = _bio;
            TextBoxPhoneNumber.Content = _phoneNumber;
        }

        private async void GetProfilePhoto(TdApi.User user)
        {
            try
            {
                var profilePhoto = await _client.ExecuteAsync(new TdApi.DownloadFile
                {
                    FileId = user.ProfilePhoto.Big.Id,
                    Priority = 1
                });
                
                if (!Directory.Exists(profilePhoto.Local.Path)) return;
                
                ImageProfilePicture.ImageSource = new BitmapImage(new Uri(profilePhoto.Local.Path));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        
        private static async Task<TdApi.User> GetCurrentUser()
        {
            return await _client.ExecuteAsync(new TdApi.GetMe());
        }

        private async void Button_LogOut_OnClick(object sender, RoutedEventArgs e)
        {
            await _client.ExecuteAsync(new TdApi.LogOut());
            
            _mWindow = new LoginView();
            _mWindow.Activate();
            
            var window = (Application.Current as App)?._mWindow as MainWindow;
            if (window == null) return;
            window.AppWindow.Destroy();
        }

        private void Button_Apply_OnClick(object sender, RoutedEventArgs e)
        {
            if (TextBoxUsername.Text != _username)
            {
                _username = TextBoxUsername.Text;
                
                _client.ExecuteAsync(new TdApi.SetUsername
                {
                    Username = _username
                });
            }

            if (TextBoxFirstName.Text != _firstName || TextBoxLastName.Text != _lastName)
            {
                _firstName = TextBoxFirstName.Text;
                _lastName = TextBoxLastName.Text;
                
                _client.ExecuteAsync(new TdApi.SetName
                {
                    FirstName = _firstName,
                    LastName = _lastName
                });
            }

            if (TextBoxBio.Text != _bio)
            {
                _bio = TextBoxBio.Text;
                
                _client.ExecuteAsync(new TdApi.SetBio
                {
                    Bio = _bio
                });
            }
        }
    }
}
