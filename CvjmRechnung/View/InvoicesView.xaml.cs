using CvjmRechnung.Model;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace CvjmRechnung.View
{
    /// <summary>
    /// Interaction logic for InvoicesWindow.xaml
    /// </summary>
    public partial class InvoicesView : Window
    {
        // Public property to hold the selected item
        public object? SelectedEventItem { get; private set; }

        public ObservableCollection<EventDetails> AllEvents { get; set; } = new ObservableCollection<EventDetails>();
        private readonly string? _icsPath;

        public InvoicesView(string? icsPath = null)
        {
            InitializeComponent();
            _icsPath = icsPath;
            ListViewItems.ItemsSource = AllEvents;
            this.Loaded += OnWindowLoaded;
        }

        private async void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            // Load the new list of data
            Mouse.OverrideCursor = Cursors.Wait;
            List<EventDetails> newEvents = await EventDetails.GetEventDetails(_icsPath);
            Mouse.OverrideCursor = null;

            // 2. Clear the old collection and add the new items to the existing collection instance
            AllEvents.Clear();
            foreach (var eventDetail in newEvents)
            {
                AllEvents.Add(eventDetail);
            }
        }

        private void ListViewItems_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // 1. Get the currently selected item from the ListView
            object? selectedItem = ListViewItems.SelectedItem;

            // 2. Check if a valid item was actually selected (not null)
            if (selectedItem != null)
            {
                // 3. Store the selected item
                SelectedEventItem = selectedItem;

                // 4. Set DialogResult to true to close the modal window and return success
                this.DialogResult = true;
            }
        }
    }
}
