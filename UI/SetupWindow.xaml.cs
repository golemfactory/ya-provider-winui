using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using Sentry;

namespace GolemUI.UI
{


    internal enum AccentState
    {
        ACCENT_DISABLED = 1,
        ACCENT_ENABLE_GRADIENT = 0,
        ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
        ACCENT_ENABLE_BLURBEHIND = 3,
        ACCENT_INVALID_STATE = 4
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct AccentPolicy
    {
        public AccentState AccentState;
        public int AccentFlags;
        public int GradientColor;
        public int AnimationId;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct WindowCompositionAttributeData
    {
        public WindowCompositionAttribute Attribute;
        public IntPtr Data;
        public int SizeOfData;
    }

    internal enum WindowCompositionAttribute
    {
        // ...
        WCA_ACCENT_POLICY = 19
        // ...
    }
    /// <summary>
    /// Interaction logic for SetupWindow.xaml
    /// </summary>
    public partial class SetupWindow : Window
    {
        [DllImport("user32.dll")]
        internal static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

        private readonly IServiceProvider _serviceProvider;

        protected ViewModel.SetupViewModel? Model => DataContext as ViewModel.SetupViewModel;

        public SetupWindow(ViewModel.SetupViewModel model, IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            InitializeComponent();
            DataContext = model;

        }



        internal void EnableBlur()
        {
            var windowHelper = new WindowInteropHelper(this);

            var accent = new AccentPolicy();
            accent.AccentState = AccentState.ACCENT_ENABLE_BLURBEHIND;

            var accentStructSize = Marshal.SizeOf(accent);

            var accentPtr = Marshal.AllocHGlobal(accentStructSize);
            Marshal.StructureToPtr(accent, accentPtr, false);

            var data = new WindowCompositionAttributeData();
            data.Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY;
            data.SizeOfData = accentStructSize;
            data.Data = accentPtr;

            SetWindowCompositionAttribute(windowHelper.Handle, ref data);

            Marshal.FreeHGlobal(accentPtr);
        }
        private void CloseApp(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MinimizeApp(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void WantToLearn_Click(object sender, RoutedEventArgs e)
        {
            Model!.GoToNoobFlow();
        }

        private void ExpertMode_Click(object sender, RoutedEventArgs e)
        {
            Model!.GoToExpertMode();
        }


        private void OnWTLStep1(object sender, RoutedEventArgs e)
        {
            Model!.NoobStep = 1;
        }

        // Genetate Seed
        private void OnWTLStep2(object sender, RoutedEventArgs e)
        {
            Model!.GenerateSeed();
        }

        private void OnWTLStep3Print(object sender, RoutedEventArgs e)
        {
            var printDlg = new PrintDialog();
            var table = new Table();
            var rg = new TableRowGroup();
            var _words = Model?.MnemonicWords;
            if (_words == null)
            {
                return;
            }
            var row = new TableRow();
            for (var i = 0; i < _words.Length; ++i)
            {
                var cell = new TableCell(new Paragraph(new Run($"{i + 1}. {_words[i]}")));
                row.Cells.Add(cell);
                if (row.Cells.Count >= 3)
                {
                    rg.Rows.Add(row);
                    row = new TableRow();
                }
            }
            rg.Rows.Add(row);
            table.RowGroups.Add(rg);

            FlowDocument doc = new FlowDocument(new Paragraph(new Run("Wallet Recovery Words")) { FontWeight = FontWeights.Bold });
            doc.Blocks.Add(table);
            doc.Name = "RecoveryDoc";
            // Create IDocumentPaginatorSource from FlowDocument  
            IDocumentPaginatorSource idpSource = doc;
            // Call PrintDocument method to send document to printer  
            bool? result = printDlg.ShowDialog();
            if (result == true)
                printDlg.PrintDocument(idpSource.DocumentPaginator, "Wallet Recovery Sheet");
        }

        private async void OnWTLStep3Next(object sender, RoutedEventArgs e)
        {
            BtnSeedPhaseGotThemAll.IsEnabled = false;
            await Model!.ActivateHdWallet();
            BtnSeedPhaseGotThemAll.IsEnabled = true;
        }

        private void OnWTLStep4Next(object sender, RoutedEventArgs e)
        {
            Model!.NoobStep = 4;
            int defaultBenchmarkStep = (int)PerformanceThrottlingEnumConverter.Default;
            Model!.BenchmarkService.StartBenchmark("", defaultBenchmarkStep.ToString(), "", "", null);
        }

        private void OnCancelNoobFlow(object sender, RoutedEventArgs e)
        {
            Model!.Flow = 0;
        }

        private void OnNoobFinish(object sender, RoutedEventArgs e)
        {
            Model!.Save();
            if (App.Current is App app)
                app.GetOrCreateDashboardWindow()?.Show();
            Close();
        }

        private void OnChooseNewWallet(object sender, RoutedEventArgs e)
        {
            Model!.NoobStep = (int)ViewModel.SetupViewModel.NoobSteps.Prepare;
            Model!.Flow = (int)ViewModel.SetupViewModel.FlowSteps.Noob;
        }

        private void OnChooseOwnWallet(object sender, RoutedEventArgs e)
        {
            Model!.Flow = (int)ViewModel.SetupViewModel.FlowSteps.OwnWallet;
        }



        private void OnEMNameStepDone(object sender, RoutedEventArgs e)
        {

            Model!.ExpertStep = (int)ViewModel.SetupViewModel.ExpertSteps.Benchmark;
            int defaultBenchmarkStep = (int)PerformanceThrottlingEnumConverter.Default;
            Model!.BenchmarkService.StartBenchmark("", defaultBenchmarkStep.ToString(), "", "", null);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            EnableBlur();
        }

        private void ConfirmAddress_Click(object sender, RoutedEventArgs e)
        {
            if (Model == null || Model.Address == null)
            {
                return;
            }
            var dlg = new UI.Dialogs.DlgConfirmAddress(new ViewModel.Dialogs.DlgConfirmAddressViewModel(Model.Address));
            dlg.Owner = Window.GetWindow(this);
            RectBlack.Visibility = Visibility.Visible;
            bool? result = dlg?.ShowDialog();
            RectBlack.Visibility = Visibility.Hidden;
            if (result == true) Model!.ExpertStep = (int)ViewModel.SetupViewModel.ExpertSteps.Name;
        }

        private void BtnBackToMainScreen_Click(object sender, RoutedEventArgs e)
        {
            if (Model != null)
            {
                Model.GoToStart();

                Model.NoobStep = 0;
                Model.ExpertStep = 0;
                if (Model.BenchmarkIsRunning) Model.BenchmarkService.StopBenchmark();
            }
        }



        private void BtnNoGpuContinue_Click(object sender, RoutedEventArgs e)
        {
            Model!.Save();
            if (App.Current is App app)
                app.GetOrCreateDashboardWindow()?.Show();
            Close();
        }
    }
}
