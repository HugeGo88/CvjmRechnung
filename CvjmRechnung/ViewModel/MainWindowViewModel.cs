using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CvjmRechnung.ViewModel
{
    partial class MainWindowViewModel : ObservableObject
    {
        [ObservableProperty]
        string firstAndLastName = "";

        [ObservableProperty]
        string streetAndNumber = "";

        [ObservableProperty]
        string postalCodeAndCity = "";

        [ObservableProperty]
        string emailAddress = "";

        [ObservableProperty]
        List<InvoiceRow> invoiceRows = new();

        [RelayCommand]
        void AddRow()
        {
            InvoiceRows.Add(new InvoiceRow());
        }
    }
}
