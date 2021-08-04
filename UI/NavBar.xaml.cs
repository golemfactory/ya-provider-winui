using GolemUI.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GolemUI.UI
{
    /// <summary>
    /// Interaction logic for NavBar.xaml
    /// </summary>
    public partial class NavBar : UserControl
    {

        public static readonly DependencyProperty StepProperty = DependencyProperty.Register("Step", typeof(int), typeof(NavBar), new FrameworkPropertyMetadata(
                                0,
                                new PropertyChangedCallback(OnStepChanged)));

        public static readonly DependencyProperty ItemsProperty = DependencyProperty.Register("Items", typeof(Model.NavBarItems), typeof(NavBar), new FrameworkPropertyMetadata(
                        null,
                        new PropertyChangedCallback(OnItemsChanged)));


        private Model.NavBar _model = new Model.NavBar();

        private static void OnStepChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NavBar ctrl = (NavBar)d;
            var model = ctrl._model;
            if (ctrl.Items != null && model.Items != ctrl.Items)
            {
                model.Items = ctrl.Items;
            }
            model.Step = (int)e.NewValue;
            ctrl.Items?.UpdateItems(model.Step);

            //ctrl.SetValue(ItemsProperty, ctrl!.Model.Items);
        }

        private static void OnItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NavBar ctrl = (NavBar)d;

            ctrl.Items?.UpdateItems(ctrl.Step);

            //ctrl.Model!.Items = (ObservableCollection<Model.NavBarItem>)e.NewValue;
        }


        [Bindable(true)]
        public Model.NavBarItems? Items => (Model.NavBarItems)GetValue(ItemsProperty);

        [Bindable(true)]
        public int Step
        {
            get => (int)GetValue(StepProperty);
            set
            {
                SetValue(StepProperty, value);
            }
        }

        public NavBar()
        {
            if (Items == null)
            {
                SetValue(ItemsProperty, new Model.NavBarItems());
            }
            InitializeComponent();
        }

        private void OnReady(object sender, RoutedEventArgs e)
        {
            Items?.UpdateItems(Step);
        }

        private void NavButtons_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var item = ItemsControl.ContainerFromElement(sender as ListBox, e.OriginalSource as DependencyObject) as ListBoxItem;
            if (item != null)
            {
                if (item.DataContext is NavBarItem clickItem)
                {
                    if (clickItem.Status == NavBarItem.ItemStatus.Realized)
                        this.Step = clickItem.StepIndex ?? clickItem.Index - 1 ?? 0;
                }
            }
        }
    }
}
