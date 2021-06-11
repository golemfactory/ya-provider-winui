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
    /// Interaction logic for GolemUISettingsWindow.xaml
    /// </summary>
    public partial class GolemUISettingsWindow : Window
    {
        NameGen _gen;
        public GolemUISettingsWindow()
        {
            _gen = new NameGen();
            InitializeComponent();
            txNodeName.Text = _gen.GenerateElvenName() + "-" + _gen.GenerateElvenName();
        }
    }
}
