namespace Haver.ViewModels
{
    public class PartsDefectiveVM
    {
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public int PartsDefectiveAmount { get; set; }
        public bool IsChangePositive { get; set; }
        public bool EnoughData { get; set; }
        public double? PeriodChange { get; set; }
        public int PartNumber {  get; set; }
        public string PartDescription { get; set; }
        public string SupplierSummary { get; set; }
        public string SupplierName { get; set; }
    }
}
