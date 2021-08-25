using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GolemUI.DesignViewModel.Dialogs
{
    public class DlgAppInfoViewModel
    {
        public string UserName
        {
            get => "Paawełek";
            set { }
        }
        public bool ShouldAttachLogs
        {
            get => true;
            set
            {

            }
        }
        public string UserEmail
        {
            get => "pawelek@mail.??";
            set { }
        }
        public string UserComment
        {
            get => "FirstLine\nSecondLine";
            set { }
        }
        public string AppVersion => "0.1.2";
        public string NodeName => "Magical Node";
    }
}
