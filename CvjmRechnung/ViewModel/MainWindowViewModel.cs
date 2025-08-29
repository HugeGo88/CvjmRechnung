using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace CvjmRechnung.ViewModel
{
    partial class MainWindowViewModel : ObservableObject
    {
        [ObservableProperty]
        List<string> rents = new()
        {
            "Vereinsheim",
            "Gelände",
            "Vereinsheim und Gelände"
        };

        [ObservableProperty]
        string orderNumber = "";

        [ObservableProperty]
        string firstAndLastName = "";

        [ObservableProperty]
        string streetAndNumber = "";

        [ObservableProperty]
        string postalCodeAndCity = "";

        [ObservableProperty]
        string emailAddress = "";

        [ObservableProperty]
        DateTime? date = DateTime.Now;

        [ObservableProperty]
        bool isMember;

        [ObservableProperty]
        string rent = "Vereinsheim";

        [ObservableProperty]
        ObservableCollection<InvoiceRow> invoiceRows = new();

        [ObservableProperty]
        double totalAmount = 0;

        [RelayCommand]
        void AddRow()
        {
            InvoiceRows.Add(new InvoiceRow() { Position = InvoiceRows.Count + 1 });
            TotalAmount = InvoiceRows.Sum(x => x.TotalPrice);
        }

        [RelayCommand]
        void SetCurrentDate()
        {
            Date = DateTime.Now;
        }

        [RelayCommand]
        void SaveFile()
        {

        }
    }
}
