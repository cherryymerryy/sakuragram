using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CherryMerryGramDesktop.Services;
using CherryMerryGramDesktop.Views;
using Microsoft.UI.Xaml;
using TdLib;
using TdLib.Bindings;
using TdLogLevel = TdLib.Bindings.TdLogLevel;

namespace CherryMerryGramDesktop
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }
        
        public static TdClient _client;
        private static Config.Config _config;
		private static readonly ManualResetEventSlim ReadyToAuthenticate = new();

		public static bool _authNeeded;
        public static bool _passwordNeeded;
        
		private void PrepareTelegramApi()
		{
			using var jsonClient = new TdJsonClient();

			const string json = "";
			const double timeout = 1.0;

			jsonClient.Send(json);
			var result = jsonClient.Receive(timeout);
			
			_config = new Config.Config();
			_client = new TdClient();
			_client.Bindings.SetLogVerbosityLevel(TdLogLevel.Fatal);

			_client.UpdateReceived += async (_, update) => { await ProcessUpdates(update); };

            ReadyToAuthenticate.Wait();
		}
		
		private async Task ProcessUpdates(TdApi.Update update)
		{
			switch (update)
			{
				case TdApi.Update.UpdateAuthorizationState { AuthorizationState: TdApi.AuthorizationState.AuthorizationStateWaitTdlibParameters }:
					var filesLocation = Path.Combine(AppContext.BaseDirectory, "db");
					await _client.ExecuteAsync(new TdApi.SetTdlibParameters
					{
						ApiId = Config.Config.ApiId,
						ApiHash = Config.Config.ApiHash,
						UseFileDatabase = true,
						UseChatInfoDatabase = true,
						UseMessageDatabase = true,
						UseSecretChats = true,
						DeviceModel = "Desktop",
						SystemLanguageCode = "en",
						ApplicationVersion = ThisAssembly.Git.BaseTag,
						DatabaseDirectory = filesLocation,
						FilesDirectory = filesLocation,
					});
					break;

				case TdApi.Update.UpdateAuthorizationState { AuthorizationState: TdApi.AuthorizationState.AuthorizationStateWaitPhoneNumber }:
					_authNeeded = true;
					ReadyToAuthenticate.Set();
					break;
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
        
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            PrepareTelegramApi();

            if (_authNeeded)
            {
	            _loginWindow = new LoginView();
	            _loginWindow.Activate();
            }
            else
            {
	            _mWindow = new MainWindow();
	            _mWindow.Activate();
            }
        }

        public Window _mWindow;
        private Window _loginWindow;
    }
}
