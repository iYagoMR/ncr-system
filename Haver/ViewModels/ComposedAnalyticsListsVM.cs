namespace Haver.ViewModels
{
    public class ComposedAnalyticsListsVM
    {
        public List<PartsDefectiveVM> PagedData { get; set; }
        public List<NCRTimeListVM> NCRTPagedData { get; set; }
        public int NumbOfPages { get; set; }
        public int PageNum { get; set; }
    }
}
