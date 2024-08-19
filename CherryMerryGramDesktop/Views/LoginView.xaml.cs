using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using TdLib;

namespace CherryMerryGramDesktop.Views
{
	public sealed partial class LoginView : Window
	{
		private static TdClient _client = App._client;
		private int _loginState = 0;
		private Window _mWindow;

		public LoginView()
		{
			this.InitializeComponent();
		}

		private async void button_Next_Click(object sender, RoutedEventArgs e)
		{
			switch (_loginState)
			{
				case 0:
					button_Next.IsEnabled = false;
					textBox_PhoneNumber.IsEnabled = false;
					await _client.ExecuteAsync(new TdApi.SetAuthenticationPhoneNumber
					{
						PhoneNumber = textBox_PhoneNumber.Text,
					});
					_loginState++;
					textBox_PhoneNumber.Visibility = Visibility.Collapsed;
					textBox_Code.Visibility = Visibility.Visible;
					button_Next.IsEnabled = true;
					break;
				case 1:
					button_Next.IsEnabled = false;
					textBox_Code.IsEnabled = false;
                    await _client.ExecuteAsync(new TdApi.CheckAuthenticationCode
					{
						Code = textBox_Code.Text
					});
					textBox_Code.Visibility = Visibility.Collapsed;
					button_Next.IsEnabled = true;
					if (App._passwordNeeded)
					{
						_loginState++;
						textBox_Password.Visibility = Visibility.Visible; 
					}
					else
					{
						_mWindow = new MainWindow();
						_mWindow.Activate();
						Close();
					}
					break;
				case 2:
					button_Next.IsEnabled = false;
					textBox_Password.IsEnabled = false;
                    await _client.ExecuteAsync(new TdApi.CheckAuthenticationPassword
					{
						Password = textBox_Password.Password
					});
					_loginState = 0;
					button_Next.IsEnabled = true;
					_mWindow = new MainWindow();
					_mWindow.Activate();
					Close();
					break;
			}
		}
	}
}
