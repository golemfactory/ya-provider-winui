using GolemUI.Controllers;

namespace GolemUI
{
    public delegate void PageChangeRequestedEvent(DashboardPages page);
    public interface ISavableLoadableDashboardPage
    {
        public event PageChangeRequestedEvent PageChangeRequested;
        public void LoadData();
        public void SaveData();

    }
}
