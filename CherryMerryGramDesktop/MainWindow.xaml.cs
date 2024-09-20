using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using TdLib;
using CherryMerryGramDesktop.Services;
using Microsoft.UI.Dispatching;

namespace CherryMerryGramDesktop
{
	public sealed partial class MainWindow : Window
	{
		private NavigationViewItem _lastItem;
		private static TdClient _client = App._client;
		private static TdApi.User _user;
		private NotificationService _notificationService;
		
		private int _totalUnreadCount = 0;
		
		public MainWindow()
		{
			InitializeComponent();
			#if DEBUG
			{
				Title = "CherryMerryGram: DEBUG";
			}
			#else
			{
				Title = "CherryMerryGram";
			}
			#endif
			
            ExtendsContentIntoTitleBar = true;
            NavigateToView("ChatsView");
            TrySetDesktopAcrylicBackdrop();
            
            _notificationService = new NotificationService();
            
            var chatsIds = _client.ExecuteAsync(new TdApi.GetChats{Limit = 100}).Result.ChatIds;
            foreach (var chatId in chatsIds)
            {
	            var chat = _client.ExecuteAsync(new TdApi.GetChat {ChatId = chatId}).Result;
	            _totalUnreadCount += chat.UnreadCount;
            }
            
			UnreadMessagesCount.Value = _totalUnreadCount;
			
			_user = _client.GetMeAsync().Result;
			NavigationView.PaneTitle = $"{_user.FirstName} ({_totalUnreadCount})";
            _client.UpdateReceived += async (_, update) => { await ProcessUpdates(update); };
        }

		private async Task ProcessUpdates(TdApi.Update update)
		{
			switch (update)
			{
				case TdApi.Update.UpdateNewMessage updateNewMessage:
				{
					//_notificationService.SendNotification(updateNewMessage.Message);
					_totalUnreadCount += 1;
					NavigationView.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.High,
						() => NavigationView.PaneTitle = $"{_user.FirstName} ({_totalUnreadCount})");
					UnreadMessagesCount.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.High,
						() => UnreadMessagesCount.Value = _totalUnreadCount);
					break;
				}
				case TdApi.Update.UpdateConnectionState updateConnectionState:
				{
					DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal, () =>
					{
						NavigationView.PaneTitle = updateConnectionState.State switch
						{
							TdApi.ConnectionState.ConnectionStateReady => $"{_user.FirstName} ({_totalUnreadCount})",
							TdApi.ConnectionState.ConnectionStateUpdating => "Updating...",
							TdApi.ConnectionState.ConnectionStateConnecting => "Connecting...",
							TdApi.ConnectionState.ConnectionStateWaitingForNetwork => "Waiting for network...",
							TdApi.ConnectionState.ConnectionStateConnectingToProxy => "Connecting to proxy...",
							_ => "CherryMerryGram"
						};
					});
					break;
				}
			}
		}

		private bool TrySetDesktopAcrylicBackdrop()
		{
			if (Microsoft.UI.Composition.SystemBackdrops.DesktopAcrylicController.IsSupported())
			{
				Microsoft.UI.Xaml.Media.DesktopAcrylicBackdrop DesktopAcrylicBackdrop = new Microsoft.UI.Xaml.Media.DesktopAcrylicBackdrop();
				SystemBackdrop = DesktopAcrylicBackdrop;

				return true;
			}

			return false;
		}
		
		private void ContentFrame_NavigationFailed(object sender, NavigationFailedEventArgs e) 
		{

		}

		private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
		{
			var item = args.InvokedItemContainer as NavigationViewItem;
			if (item == null || item == _lastItem)
				return;

			var clickedView = item.Tag.ToString();

			if (!NavigateToView(clickedView)) return;
			_lastItem = item;
		}

		private bool NavigateToView(string clickedView)
		{
			var view = Assembly.GetExecutingAssembly().GetType($"CherryMerryGramDesktop.Views.{clickedView}");

			if (string.IsNullOrEmpty(clickedView) || view == null)
				return false;

			ContentFrame.Navigate(view, null, new EntranceNavigationTransitionInfo());

			NavigationView.PaneDisplayMode = clickedView switch
			{
				"SettingsView" => NavigationViewPaneDisplayMode.LeftCompact,
				"ChatsView" => NavigationViewPaneDisplayMode.Left,
				_ => NavigationView.PaneDisplayMode
			};

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
