using System;
using Microsoft.UI.Xaml.Controls;
using TdLib;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;

namespace CherryMerryGram.Views
{
	public sealed partial class AccountView : Page
	{
		private static readonly TdClient _client = MainWindow._client;
        
        private string _firstName;
        private string _lastName;
        private string _username;
        private string _bio;
        private string _phoneNumber;
        private TdApi.ProfilePhoto _profilePicture;

		public AccountView()
		{
			this.InitializeComponent();
            
            InitializeAllVariables();
        }

        private async void InitializeAllVariables()
        {
            var currentUser = await GetCurrentUser();
            GetProfilePhoto(currentUser);
            
            _firstName = $"{currentUser.FirstName}";
            _lastName = $"{currentUser.LastName}";
            _username = $"@{currentUser.Usernames?.ActiveUsernames[0]}";
            //_bio = $"{}"
            _phoneNumber = $"+{currentUser.PhoneNumber}";

            textBlock_Username.Text = _username; 
            textBlock_FirstName.Text = _firstName;
            textBlock_LastName.Text = _lastName;
            textBlock_Bio.Text = "я не ебу как достать био, чес слово";
            textBlock_PhoneNumber.Content = _phoneNumber;
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
            
                image_ProfilePicture.ImageSource = new BitmapImage(new Uri(profilePhoto.Local.Path));
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
        }

        private void TextBlock_PhoneNumber_OnClick(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Button_Apply_OnClick(object sender, RoutedEventArgs e)
        {
            //_client.ExecuteAsync(new TdApi.SetProfilePhoto
            //{
            //  Photo = _profilePicture
            //});
            
            _client.ExecuteAsync(new TdApi.SetName
            {
                FirstName = _firstName,
                LastName = _lastName
            });

            //if (_bio != GetCurrentUser().Result.Extra.)
            //{
            //    _client.ExecuteAsync(new TdApi.SetBio
            //    {
            //        Bio = _bio
            //    });
            //}

            if (_username != GetCurrentUser().Result.Usernames.ActiveUsernames[0])
            {
                _client.ExecuteAsync(new TdApi.SetUsername
                {
                    Username = _username
                });
            }
        }
    }
}
