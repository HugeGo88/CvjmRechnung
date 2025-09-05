using CommunityToolkit.Mvvm.ComponentModel;

namespace CvjmRechnung.Model
{
    internal partial class Invoice : ObservableObject
    {
        [ObservableProperty]
        List<string> rents = new()
        {
            "Vereinsheim",
            "Gelände",
            "Vereinsheim und Gelände"
        };

        [ObservableProperty]
        string orderNumber = "000";

        [ObservableProperty]
        string companyName = "Max Mustermann GmbH";

        [ObservableProperty]
        string firstAndLastName = "Max Mustermann";

        [ObservableProperty]
        string streetAndNumber = "Musterstraße 123";

        [ObservableProperty]
        string postalCodeAndCity = "12345 Muststadt";

        [ObservableProperty]
        string emailAddress = "max@musterman.de";

        [ObservableProperty]
        DateTime? date = DateTime.Now;

        [ObservableProperty]
        bool isMember;

        [ObservableProperty]
        string rent = "Vereinsheim";
    }
}
