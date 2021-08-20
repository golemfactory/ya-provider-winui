using GolemUI.Interfaces;
using GolemUI.Utils;
using Nethereum.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GolemUI.ViewModel.Dialogs
{

    public class DlgUpdateAppViewModel
    {
        public string Title => IsUpToDate ? "Your application is up to date" : "New version of application is available";
        public string UpdateLink { private set; get; }
        public string CurrentVersion { private set; get; }
        string NewVersion { set; get; }
        private string AppCodeName { set; get; }
        public string ChangeLog
        {
            get
            {
                if (Changes == null || Changes.Count == 0)
                    return "";
                return "New changes:\n" + String.Join("\n", Changes.Take(5).Select(x => "- " + x).ToArray());
            }
        }
        List<string> Changes = null;
        public bool ShouldForceUpdate { private set; get; }
        public bool IsUpToDate => VersionUtil.AreVersionsEqual(CurrentVersion, NewVersion);
        public string NewVersionDisplayString
        {
            get
            {
                if (String.IsNullOrEmpty(AppCodeName))
                    return NewVersion;
                return $"{NewVersion} [{AppCodeName}]";
            }
        }

        public DlgUpdateAppViewModel(string updateLink, string currentVersion, string newVersion, string appCodeName, List<string> changes, bool shouldForceUpdate)
        {
            UpdateLink = updateLink;
            CurrentVersion = currentVersion;
            NewVersion = newVersion;
            AppCodeName = appCodeName;
            Changes = changes;
            ShouldForceUpdate = shouldForceUpdate;
        }
    }

}
