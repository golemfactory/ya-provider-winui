using System.Windows.Controls;
namespace GolemUI
{
    public class DashboardPageDescriptor
    {
        public UserControl View;
        ISavableLoadableDashboardPage ViewModel=null;
        public bool ShouldAutoLoad=false;
        public bool ShouldAutoSave=false;
        public DashboardPageDescriptor(UserControl view)
        {
            View = view;
        }
        public DashboardPageDescriptor(UserControl view, ISavableLoadableDashboardPage viewModel)
        {
            View = view;
            ViewModel = viewModel;
            ShouldAutoLoad = true;
            ShouldAutoLoad = true;
        }
        public void Unmount()
        {
            if (ShouldAutoSave && ViewModel != null)
            {
                ViewModel.SaveData();
            }
        }

        public void Mount()
        {
            if (ShouldAutoLoad && ViewModel != null)
            {
                ViewModel.LoadData();
            }
        }
    }
}