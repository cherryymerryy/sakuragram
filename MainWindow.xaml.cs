using Microsoft.UI.Xaml;
using System;
using System.IO;
using System.Threading.Tasks;
using TdLib.Bindings;
using TdLib;
using System.Threading;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Controls;
using System.Reflection;
using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.UI.Xaml.Media.Animation;

namespace CherryMerryGram
{
	public sealed partial class MainWindow : Window
	{
		private NavigationViewItem _lastItem;
		private static Config.Config _config;

		public static TdClient _client;
		private static readonly ManualResetEventSlim ReadyToAuthenticate = new();

		public static bool _authNeeded;
        public static bool _passwordNeeded;
        
		private void PrepareTelegramApi()
		{
			using var jsonClient = new TdJsonClient();

			var json = "";
			double timeout = 1.0;

			jsonClient.Send(json);
			var result = jsonClient.Receive(timeout);
			
			_config = new Config.Config();
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
						ApiId = Config.Config.ApiId,
						ApiHash = Config.Config.ApiHash,
						UseFileDatabase = true,
						UseChatInfoDatabase = true,
						UseMessageDatabase = true,
						UseSecretChats = true,
						DeviceModel = "PC",
						SystemLanguageCode = "en",
						ApplicationVersion = Config.Config.ApplicationVersion,
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

		private void CheckAuth()
		{
			if (_authNeeded)
			{ 
				NavigateToView("LoginView");
				NavViewLogin.IsEnabled = true;
				NavViewAccount.IsEnabled = false;
				NavViewChats.IsEnabled = false;
				NavViewSettings.IsEnabled = false;
				NavViewHelp.IsEnabled = false;
			}
			else
			{ 
				NavigateToView("ChatsView");
				NavViewLogin.IsEnabled = false;
				NavViewAccount.IsEnabled = true;
				NavViewChats.IsEnabled = true;
				NavViewSettings.IsEnabled = true;
				NavViewHelp.IsEnabled = true;
			}
		}
		
		public void UpdateWindow()
		{
			CheckAuth();
		}
		
		public MainWindow()
		{
			this.InitializeComponent();

            Window window = this;
            window.ExtendsContentIntoTitleBar = true;

            PrepareTelegramApi();
			UpdateWindow();
		}

		private void ContentFrame_NavigationFailed(object sender, NavigationFailedEventArgs e) 
		{

		}

		private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
		{
			var item = args.InvokedItemContainer as NavigationViewItem;
			if (item == null || item == _lastItem)
				return;

			var clickedView = item.Tag.ToString() ?? "SettingsView";

			if (!NavigateToView(clickedView)) return;
			_lastItem = item;
		}

		public bool NavigateToView(string clickedView)
		{
			var view = Assembly.GetExecutingAssembly().GetType($"CherryMerryGram.Views.{clickedView}");

			if (string.IsNullOrEmpty(clickedView) || view == null)
				return false;

			ContentFrame.Navigate(view, null, new EntranceNavigationTransitionInfo());
			return true;
        }

        private void NavView_Loaded(object sender, RoutedEventArgs e)
		{
			
		}

		private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
		{

		}
    }
}
