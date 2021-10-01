using GolemUI.Interfaces;
using Nethereum.Util;
using Sentry;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GolemUI.ViewModel.Dialogs
{
    public class DlgGenericInformationViewModel
    {
        public DlgGenericInformationViewModel(string image, string title, string text, string closeButtonLabel)
        {
            _image = image;
            _title = title;
            _text = text;
            _buttonLabel = closeButtonLabel;
        }

        public BitmapImage Image
        {
            get
            {
                String basePath = "/UI/Icons/DefaultStyle/png/Dialogs/";

                string path = basePath + _image + ".png";

                return new BitmapImage(new Uri("pack://application:,,,/ThorgMiner;component" + path));
            }

        }
        private readonly string _buttonLabel = "";
        private readonly string _text = "";
        private readonly string _title = "";
        private readonly string _image = "shield";
        public string Title => _title;
        public string Line1 => getLine(0);
        public string Line2 => getLine(1);
        public string Line3 => getLine(2);
        public string ButtonText => _buttonLabel;

        string getLine(int i)
        {
            string[] lines = _text.Split('\n');
            return i < lines.Length ? lines[i] : "";
        }


    }
}
