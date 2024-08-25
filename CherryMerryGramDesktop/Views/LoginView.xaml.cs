using System.Reflection;
using System.Threading.Tasks;
using CherryMerryGramDesktop.Views.Auth;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using TdLib;

namespace CherryMerryGramDesktop.Views
{
	public sealed partial class LoginView : Window
	{
		private static TdClient _client = App._client;
		private int _loginState = 0;
		private Window _mWindow;
		private string _passwordHint;

		public LoginView()
		{
			InitializeComponent();
			Title = "CherryMerryGram : Login";
			Window window = this;
			window.ExtendsContentIntoTitleBar = true;
			TrySetDesktopAcrylicBackdrop();
			
			_client.UpdateReceived += async (_, update) => { await ProcessUpdates(update); };
			
			TextBlockCurrentAuthState.Text = "Your phone";
			TextBlockCurrentAuthStateDescription.Text = "Please confirm your country code and enter your phone number.";
		}

		private async Task ProcessUpdates(TdApi.Update update)
		{
			switch (update)
			{
				case TdApi.Update.UpdateAuthorizationState updateAuthorizationState:
				{
					switch (updateAuthorizationState.AuthorizationState)
					{
						case TdApi.AuthorizationState.AuthorizationStateWaitPassword password:
						{
							_passwordHint = password.PasswordHint;
							break;
						}
					}
					break;
				}
			}
		}

		private bool TrySetDesktopAcrylicBackdrop()
		{
			if (Microsoft.UI.Composition.SystemBackdrops.DesktopAcrylicController.IsSupported())
			{
				Microsoft.UI.Xaml.Media.DesktopAcrylicBackdrop DesktopAcrylicBackdrop = new Microsoft.UI.Xaml.Media.DesktopAcrylicBackdrop();
				this.SystemBackdrop = DesktopAcrylicBackdrop;

				return true;
			}

			return false;
		}
		
		private async void button_Next_Click(object sender, RoutedEventArgs e)
		{
			switch (_loginState)
			{
				case 0:
					if (TextBoxPhoneNumber.Text == "") return;
					ButtonNext.IsEnabled = false;
					TextBoxPhoneNumber.IsEnabled = false;
					LoginProgress.Visibility = Visibility.Visible;
					await _client.ExecuteAsync(new TdApi.SetAuthenticationPhoneNumber
					{
						PhoneNumber = TextBoxPhoneNumber.Text,
					});
					LoginProgress.Visibility = Visibility.Collapsed;
					_loginState++;
					TextBlockCurrentAuthState.Text = "Phone verification";
					TextBlockCurrentAuthStateDescription.Text = "We've sent the code to the Telegram app on your other device.";
					TextBoxPhoneNumber.Visibility = Visibility.Collapsed;
					TextBoxCode.Visibility = Visibility.Visible;
					ButtonNext.IsEnabled = true;
					break;
				case 1:
					if (TextBoxCode.Text == "") return;
					ButtonNext.IsEnabled = false;
					TextBoxCode.IsEnabled = false;
					LoginProgress.Visibility = Visibility.Visible;
                    await _client.ExecuteAsync(new TdApi.CheckAuthenticationCode
					{
						Code = TextBoxCode.Text
					});
					LoginProgress.Visibility = Visibility.Collapsed;
					TextBoxCode.Visibility = Visibility.Collapsed;
					ButtonNext.IsEnabled = true;
					if (App._passwordNeeded)
					{
						_loginState++;
						TextBoxPassword.Visibility = Visibility.Visible;
						ForgotPassword.Visibility = Visibility.Visible;
						TextBlockCurrentAuthState.Text = "Password";
						TextBlockCurrentAuthStateDescription.Text = 
							"You have Two-Step Verification enabled, so your account is protected with an additional password.";
						TextBoxPassword.PlaceholderText = $"Hint: {_passwordHint}";
					}
					else
					{
						_mWindow = new MainWindow();
						_mWindow.Activate();
						Close();
					}
					break;
				case 2:
					if (TextBoxPassword.Password == "") return;
					ButtonNext.IsEnabled = false;
					TextBoxPassword.IsEnabled = false;
					LoginProgress.Visibility = Visibility.Visible;
                    await _client.ExecuteAsync(new TdApi.CheckAuthenticationPassword
					{
						Password = TextBoxPassword.Password
					});
					LoginProgress.Visibility = Visibility.Collapsed;
					_loginState = 0;
					ButtonNext.IsEnabled = true;
					_mWindow = new MainWindow();
					_mWindow.Activate();
					Close();
					break;
			}
		}

		private void ForgotPassword_OnClick(object sender, RoutedEventArgs e)
		{
			var window = new Auth_ForgotPassword();
			window.Activate();
		}
	}
}
