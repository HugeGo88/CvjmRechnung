using CvjmRechnung.Model;
using CvjmRechnung.Services;
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
        public ObservableCollection<EventDetails> PastEvents { get; set; } = new ObservableCollection<EventDetails>();
        private readonly string? _icsPath;
        private readonly string? _eventsFolder;

        public InvoicesView(string? icsPath = null, string? eventsFolder = null)
        {
            InitializeComponent();
            _icsPath = icsPath;
            _eventsFolder = eventsFolder;
            ListViewItems.ItemsSource = AllEvents;
            PastListViewItems.ItemsSource = PastEvents;
            this.Loaded += OnWindowLoaded;
        }

        private async void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            // Load the new list of data from ICS
            Mouse.OverrideCursor = Cursors.Wait;
            List<EventDetails> newEvents = await EventDetails.GetEventDetails(_icsPath);
            Mouse.OverrideCursor = null;

            // Update the current events collection
            AllEvents.Clear();
            foreach (var eventDetail in newEvents)
            {
                AllEvents.Add(eventDetail);
            }

            // Store fetched events for later use and load past events
            if (!string.IsNullOrWhiteSpace(_eventsFolder))
            {
                EventStorageService.MergeAndSave(newEvents, _eventsFolder);

                var pastEvents = EventStorageService.LoadPastEvents(_eventsFolder);
                PastEvents.Clear();
                foreach (var eventDetail in pastEvents)
                {
                    PastEvents.Add(eventDetail);
                }
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

        private void PastListViewItems_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            object? selectedItem = PastListViewItems.SelectedItem;

            if (selectedItem != null)
            {
                SelectedEventItem = selectedItem;
                this.DialogResult = true;
            }
        }
    }
}
