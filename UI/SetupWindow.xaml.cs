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

            Sentry.Setup.Log("> Setup Window");


           

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
            Sentry.Setup.Log("> Setup Window > App Minimized");
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
            Sentry.Setup.Log("> SetupWindow > WantToLearn_Click");
            Model!.GoToNoobFlow();

        }

        private void ExpertMode_Click(object sender, RoutedEventArgs e)
        {
            Sentry.Setup.Log("> SetupWindow > ExpertMode_Click");
            Model!.GoToExpertMode();

        }


        private void OnWTLStep1(object sender, RoutedEventArgs e)
        {
            Sentry.Setup.Log("> SetupWindow > OnWTLStep1");
            Model!.NoobStep = 1;

        }

        // Genetate Seed
        private void OnWTLStep2(object sender, RoutedEventArgs e)
        {
            Sentry.Setup.Log("> SetupWindow > OnWTLStep2");
            Model!.GenerateSeed();

        }

        private void OnWTLStep3Print(object sender, RoutedEventArgs e)
        {
            Sentry.Setup.Log("> SetupWindow > OnWTLStep3Print");
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
            printDlg.PrintDocument(idpSource.DocumentPaginator, "Wallet Recovery Sheet");
        }

        private /*async*/ void OnWTLStep3Next(object sender, RoutedEventArgs e)
        {
            Sentry.Setup.Log("> SetupWindow > ActivateHdWallet");
            Model!.ActivateHdWallet();
        }

        private void OnWTLStep4Next(object sender, RoutedEventArgs e)
        {
            SentrySdk.AddBreadcrumb(message: "Nodename set ", data: new Dictionary<string, string>() { { "NodeName", Model.NodeName } }, category: "property set", level: BreadcrumbLevel.Info);
            Sentry.Setup.Log("> SetupWindow > OnWTLStep4Next");

            Model!.NoobStep = 4;
            int defaultBenchmarkStep = (int)PerformanceThrottlingEnum.High;
            Model!.BenchmarkService.StartBenchmark("", defaultBenchmarkStep.ToString(), "", "", null);
        }

        private void OnCancelNoobFlow(object sender, RoutedEventArgs e)
        {
            Sentry.Setup.Log("> SetupWindow > OnCancelNoobFlow");
            Model!.Flow = 0;
        }

        private void OnNoobFinish(object sender, RoutedEventArgs e)
        {
            Sentry.Setup.Log("> SetupWindow > OnNoobFinish");
            Model!.Save();
            var wnd = _serviceProvider.GetService(typeof(GolemUI.Dashboard)) as GolemUI.Dashboard;
            wnd?.Show();
            Close();
        }

        private void OnChooseNewWallet(object sender, RoutedEventArgs e)
        {
            Sentry.Setup.Log("> SetupWindow > OnChooseNewWallet");
            Model!.NoobStep = (int)ViewModel.SetupViewModel.NoobSteps.Prepare;
            Model!.Flow = (int)ViewModel.SetupViewModel.FlowSteps.Noob;
        }

        private void OnChooseOwnWallet(object sender, RoutedEventArgs e)
        {
            Sentry.Setup.Log("> SetupWindow > OnChooseOwnWallet");
            Model!.Flow = (int)ViewModel.SetupViewModel.FlowSteps.OwnWallet;
        }

        private void OnEMWalletStepDone(object sender, RoutedEventArgs e)
        {
            SentrySdk.AddBreadcrumb(message: "Address set ", data: new Dictionary<string, string>() { { "Address", Model.Address } }, category: "property set", level: BreadcrumbLevel.Info);
            Sentry.Setup.Log("> SetupWindow > OnEMWalletStepDone");
            Model!.ExpertStep = (int)ViewModel.SetupViewModel.ExpertSteps.Name;
        }

        private void OnEMNameStepDone(object sender, RoutedEventArgs e)
        {
            SentrySdk.AddBreadcrumb(message: "Nodename set ", data: new Dictionary<string, string>() { { "NodeName", Model.NodeName } }, category: "property set", level: BreadcrumbLevel.Info);
            Sentry.Setup.Log("> SetupWindow > OnEMNameStepDone");

            Model!.ExpertStep = (int)ViewModel.SetupViewModel.ExpertSteps.Benchmark;
            int defaultBenchmarkStep = (int)PerformanceThrottlingEnum.High;
            Model!.BenchmarkService.StartBenchmark("", defaultBenchmarkStep.ToString(), "", "", null);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Sentry.Setup.Log("> SetupWindow > Window_Loaded");
            EnableBlur();
        }

       
    }
}
