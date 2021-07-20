﻿using System;
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

namespace GolemUI.UI
{
    /// <summary>
    /// Interaction logic for SetupWindow.xaml
    /// </summary>
    public partial class SetupWindow : Window
    {
        protected ViewModel.SetupViewModel? Model => DataContext as ViewModel.SetupViewModel;

        public SetupWindow(ViewModel.SetupViewModel model)
        {
            InitializeComponent();
            DataContext = model;
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
            printDlg.PrintDocument(idpSource.DocumentPaginator, "Wallet Recovery Sheet");
        }

        private void OnWTLStep3Next(object sender, RoutedEventArgs e)
        {
            Model!.NoobStep = 3;
        }

        private void OnWTLStep4Next(object sender, RoutedEventArgs e)
        {
            Model!.BenchmarkService.StartBenchmark();
            Model!.NoobStep = 4;
        }

        private void OnCancelNoobFlow(object sender, RoutedEventArgs e)
        {
            Model!.Flow = 0;
        }
    }
}
