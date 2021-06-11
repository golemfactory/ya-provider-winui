﻿using GolemUI.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GolemUI
{
    /// <summary>
    /// Interaction logic for DebugWindow.xaml
    /// </summary>
    public partial class DebugWindow : Window
    {
        public DebugWindow(IProcessControler pc)
        {
            InitializeComponent();
#if DEBUG
            pc.LineHandler += LogLine;
            NameGen g = new NameGen();
            for (int i = 0; i < 20; i++)
            {
                txtR.Text += g.GenerateElvenName() + "-" + g.GenerateDwarvenName() + "\n";
            }
#endif
        }
        void LogLine(string logger, string line)
        {
#if DEBUG
            this.Dispatcher.Invoke(() =>
            {
                txtR.Text += $"{line}\n";
            });

#endif
        }
    }


}
