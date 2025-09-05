using CommunityToolkit.Mvvm.ComponentModel;

namespace CvjmRechnung.ViewModel
{
    public partial class InvoiceRow : ObservableObject
    {
        [ObservableProperty]
        public int position = 1;
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TotalPrice))]
        public int quantity = 1;
        [ObservableProperty]
        public string description = "Vereinsheim Miete pro Tag";
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TotalPrice))]
        public double unitPrice = 200;
        [ObservableProperty]
        public string unit = "Tage";
        public double TotalPrice => Quantity * UnitPrice;
    }
}