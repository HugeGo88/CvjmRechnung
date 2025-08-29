namespace CvjmRechnung.ViewModel
{
    internal class InvoiceRow
    {
        public int Position { get; set; } = 1;
        public int Quantity { get; set; } = 1;
        public string Description { get; set; } = "Vereinsheim Miete pro Tag";
        public double UnitPrice { get; set; } = 200;
        public double TotalPrice => Quantity * UnitPrice;
    }
}