using BetaMiner.ViewModel;

namespace BetaMiner
{
    public delegate void PageChangeRequestedEvent(DashboardViewModel.DashboardPages page);
    public interface ISavableLoadableDashboardPage
    {
        public event PageChangeRequestedEvent? PageChangeRequested;
        public void LoadData();
        public void SaveData();
    }
}
