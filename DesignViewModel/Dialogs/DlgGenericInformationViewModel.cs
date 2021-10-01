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

namespace GolemUI.DesignViewModel.Dialogs
{
    public class DlgGenericInformationViewModel
    {



        string _image = "shield";

        public BitmapImage Image
        {
            get
            {
                String basePath = "/UI/Icons/DefaultStyle/png/Dialogs/";

                string path = basePath + _image + ".png";

                return new BitmapImage(new Uri("pack://application:,,,/ThorgMiner;component" + path));
            }

        }
        string _buttonText = "Close";
        string _text = "Thorg is most suited for addresses that the user has custody of, compared to some other individual other than the Thorg user(such as a centralized exchange) having custody of the address.\nMost exchanges do not support L2 payments like Polygon that Thorg uses.Please change your wallet address to one that you're in control of, such as MetaMask.";
        public string Title => "Don't use address from an exchange";
        public string Line1 => getLine(0);
        public string Line2 => getLine(1);
        public string Line3 => getLine(2);
        public string ButtonText => _buttonText;

        string getLine(int i)
        {
            string[] lines = _text.Split('\n');
            return i < lines.Length ? lines[i] : "";
        }
    }
}
