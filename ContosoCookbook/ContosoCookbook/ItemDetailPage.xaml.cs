using Callisto.Controls;
using ContosoCookbook.Common;
using ContosoCookbook.Data;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Streams;
using Windows.UI.Notifications;
using Windows.UI.Popups;
using Windows.UI.StartScreen;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Item Detail Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234232

namespace ContosoCookbook
{
    /// <summary>
    /// A page that displays details for a single item within a group while allowing gestures to
    /// flip through other items belonging to the same group.
    /// </summary>
    public sealed partial class ItemDetailPage : ContosoCookbook.Common.LayoutAwarePage
    {
        public ItemDetailPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="navigationParameter">The parameter value passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        /// </param>
        /// <param name="pageState">A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.</param>
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            // Allow saved page state to override the initial item to display
            if (pageState != null && pageState.ContainsKey("SelectedItem"))
            {
                navigationParameter = pageState["SelectedItem"];
            }

            // TODO: Create an appropriate data model for your problem domain to replace the sample data
            var item = RecipeDataSource.GetItem((String)navigationParameter);
            this.DefaultViewModel["Group"] = item.Group;
            this.DefaultViewModel["Items"] = item.Group.Items;
            this.flipView.SelectedItem = item;

            DataTransferManager.GetForCurrentView().DataRequested += OnDataRequested;
        }

        private void OnDataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            var request = args.Request;
            var item = (RecipeDataItem)this.flipView.SelectedItem;
            request.Data.Properties.Title = item.Title;
            request.Data.Properties.Description = "Recipe ingredients and directions";

            // Share recipe text
            var recipe = "\r\nINGREDIENTS\r\n";
            recipe += String.Join("\r\n", item.Ingredients);
            recipe += ("\r\n\r\nDIRECTIONS\r\n" + item.Directions);
            request.Data.SetText(recipe);

            // Share recipe image
            var reference = RandomAccessStreamReference.CreateFromUri(new Uri(item.ImagePath.AbsoluteUri));
            request.Data.Properties.Thumbnail = reference;
            request.Data.SetBitmap(reference);
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="pageState">An empty dictionary to be populated with serializable state.</param>
        protected override void SaveState(Dictionary<String, Object> pageState)
        {
            var selectedItem = (RecipeDataItem)this.flipView.SelectedItem;
            pageState["SelectedItem"] = selectedItem.UniqueId;

            DataTransferManager.GetForCurrentView().DataRequested -= OnDataRequested;
        }

        private void OnBragButtonClicked(object sender, RoutedEventArgs e)
        {
            // Create a menu containing two items
            var menu = new Menu();
            var item1 = new MenuItem { Text = "Photo" };
            item1.Tapped += OnCapturePhoto;
            menu.Items.Add(item1);
            var item2 = new MenuItem { Text = "Video" };
            item2.Tapped += OnCaptureVideo;
            menu.Items.Add(item2);

            // Show the menu in a Flyout anchored to the Brag button
            var flyout = new Flyout();
            flyout.Placement = PlacementMode.Top;
            flyout.HorizontalAlignment = HorizontalAlignment.Left;
            flyout.HorizontalContentAlignment = HorizontalAlignment.Left;
            flyout.PlacementTarget = BragButton;
            flyout.Content = menu;
            flyout.IsOpen = true;
        }

        private void OnCapturePhoto(object sender, TappedRoutedEventArgs e)
        {
            DataTransferManager.ShowShareUI();
        }

        private void OnCaptureVideo(object sender, TappedRoutedEventArgs e)
        {
            DataTransferManager.ShowShareUI();
        }

        private async void OnPinRecipeButtonClicked(object sender, RoutedEventArgs e)
        {
            var item = (RecipeDataItem)this.flipView.SelectedItem;
            var uri = new Uri(item.TileImagePath.AbsoluteUri);

            var tile = new SecondaryTile(
                    item.UniqueId,              // Tile ID
                    item.ShortTitle,            // Tile short name
                    item.Title,                 // Tile display name
                    item.UniqueId,              // Activation argument
                    TileOptions.ShowNameOnLogo, // Tile options
                    uri                         // Tile logo URI
                );

            await tile.RequestCreateAsync();
        }

        private async void OnReminderButtonClicked(object sender, RoutedEventArgs e)
        {
            var notifier = ToastNotificationManager.CreateToastNotifier();

            // Make sure notifications are enabled
            if (notifier.Setting != NotificationSetting.Enabled)
            {
                var dialog = new MessageDialog("Notifications are currently disabled");
                await dialog.ShowAsync();
                return;
            }

            // Get a toast template and insert a text node containing a message
            var template = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText01);
            var element = template.GetElementsByTagName("text")[0];
            element.AppendChild(template.CreateTextNode("Reminder!"));

            // Schedule the toast to appear 30 seconds from now
            var date = DateTimeOffset.Now.AddSeconds(30);
            var stn = new ScheduledToastNotification(template, date);
            notifier.AddToSchedule(stn);
        }
    }
}
