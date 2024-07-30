using Microsoft.UI.Xaml;
using System;
using System.IO;
using System.Threading.Tasks;
using TdLib.Bindings;
using TdLib;
using System.Threading;
using static TdLib.TdApi;

namespace CherryMerryGram
{
    public sealed partial class MainWindow : Window
    {
        private const int ApiId = 1959060;
        private const string ApiHash = "315144c05dd280c6ea4bf9d353b89a08";

        private const string ApplicationVersion = "1.0.0";

        private static TdClient _client;
        private static readonly ManualResetEventSlim ReadyToAuthenticate = new();

        private static bool _authNeeded;
        private static bool _passwordNeeded;

        private void PrepareTelegramApi()
        {
            using var jsonClient = new TdJsonClient();

            var json = "";
            double timeout = 1.0;

            jsonClient.Send(json);
            var result = jsonClient.Receive(timeout);

            _client = new TdClient();
            _client.Bindings.SetLogVerbosityLevel(TdLogLevel.Fatal);

            _client.UpdateReceived += async (_, update) => { await ProcessUpdates(update); };

            ReadyToAuthenticate.Wait();
        }

        private static async Task ProcessUpdates(TdApi.Update update)
        {
            switch (update)
            {
                case TdApi.Update.UpdateAuthorizationState { AuthorizationState: TdApi.AuthorizationState.AuthorizationStateWaitTdlibParameters }:
                    var filesLocation = Path.Combine(AppContext.BaseDirectory, "db");
                    await _client.ExecuteAsync(new TdApi.SetTdlibParameters
                    {
                        ApiId = ApiId,
                        ApiHash = ApiHash,
                        DeviceModel = "PC",
                        SystemLanguageCode = "en",
                        ApplicationVersion = ApplicationVersion,
                        DatabaseDirectory = filesLocation,
                        FilesDirectory = filesLocation,
                    });
                    break;

                case TdApi.Update.UpdateAuthorizationState { AuthorizationState: TdApi.AuthorizationState.AuthorizationStateWaitPhoneNumber }:
                case TdApi.Update.UpdateAuthorizationState { AuthorizationState: TdApi.AuthorizationState.AuthorizationStateWaitCode }:
                    _authNeeded = true;
                    ReadyToAuthenticate.Set();
                    break;

                case TdApi.Update.UpdateAuthorizationState { AuthorizationState: TdApi.AuthorizationState.AuthorizationStateWaitPassword }:
                    _authNeeded = true;
                    _passwordNeeded = true;
                    ReadyToAuthenticate.Set();
                    break;

                case TdApi.Update.UpdateUser:
                    ReadyToAuthenticate.Set();
                    break;

                case TdApi.Update.UpdateConnectionState { State: TdApi.ConnectionState.ConnectionStateReady }:
                    break;

                default:
                    // ReSharper disable once EmptyStatement
                    ;
                    // Add a breakpoint here to see other events
                    break;
            }
        }

        public MainWindow()
        {
            this.InitializeComponent();
            
            PrepareTelegramApi();

            //button_EnterCode.Visibility = Visibility.Collapsed;
            //textBox_Code.Visibility = Visibility.Collapsed;
            //button_EnterPassword.Visibility = Visibility.Collapsed;
            //textBox_Password.Visibility = Visibility.Collapsed;
            //SendMessageList.Visibility = Visibility.Collapsed;
        }

        private void ContentFrame_NavigationFailed() 
        { 
        }

        private async void buttonLogInClickAsync(object sender, RoutedEventArgs e)
        {
            if (sender is null)
            {
                throw new ArgumentNullException(nameof(sender));
            }

            await _client.ExecuteAsync(new TdApi.SetAuthenticationPhoneNumber
            {
                //PhoneNumber = textBox_PhoneNumber.Text,
            });

            //button_EnterCode.Visibility = Visibility.Visible;
            //textBox_Code.Visibility = Visibility.Visible;
        }

        private async void button_EnterCode_Click(object sender, RoutedEventArgs e)
        {
            if (sender is null)
            {
                throw new ArgumentNullException(nameof(sender));
            }

            await _client.ExecuteAsync(new TdApi.CheckAuthenticationCode
            {
                //Code = textBox_Code.Text
            });

            if (_passwordNeeded)
            {
                //button_EnterPassword.Visibility = Visibility.Visible;
                //textBox_Password.Visibility = Visibility.Visible;
            }
        }

        private async void button_EnterPassword_Click(object sender, RoutedEventArgs e)
        {
            await _client.ExecuteAsync(new TdApi.CheckAuthenticationPassword
            {
                Password = textBox_Password.Text
            });

            Auth.Visibility = Visibility.Collapsed;
            SendMessageList.Visibility = Visibility.Visible;
        }

        private async void button_SendMessage_Click(object sender, RoutedEventArgs e)
        {
            await _client.ExecuteAsync(new TdApi.SendMessage
            {
                ChatId = -1002111703440, 
                InputMessageContent = new TdApi.InputMessageContent.InputMessageText()
            });
        }
    }
}
