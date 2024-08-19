using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using TdLib;
using TdLib.Bindings;
using CherryMerryGramDesktop;

namespace CherryMerryGramDesktop
{
	public sealed partial class MainWindow : Window
	{
		private NavigationViewItem _lastItem;
		private static TdClient _client;
		
		public MainWindow()
		{
			this.InitializeComponent();

            Window window = this;
            window.ExtendsContentIntoTitleBar = true;;
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
			var view = Assembly.GetExecutingAssembly().GetType($"CherryMerryGramDesktop.Views.{clickedView}");

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
