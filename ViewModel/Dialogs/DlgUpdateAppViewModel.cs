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
        public string Title => IsUpToDate ? "Thorg Miner is up to date." : "Thorg Miner update available.";
        public string UpdateLink { private set; get; }
        public string CurrentVersion { private set; get; }
        private string _newVersion { set; get; }
        private string _appCodeName { set; get; }
        public string ChangeLog => _changes.Count == 0 ? "" : "New changes:\n" + String.Join("\n", _changes.Take(5).Select(x => "- " + x).ToArray());
        private List<string> _changes = new List<string>();
        public bool ShouldForceUpdate { private set; get; }
        public bool IsUpToDate => VersionUtil.AreVersionsEqual(CurrentVersion, _newVersion);
        public string NewVersionDisplayString => String.IsNullOrEmpty(_appCodeName) ? _newVersion : $"{_newVersion} [{_appCodeName}]";


        public DlgUpdateAppViewModel(string updateLink, string currentVersion, string newVersion, string appCodeName, List<string> changes, bool shouldForceUpdate)
        {
            UpdateLink = updateLink;
            CurrentVersion = currentVersion;
            _newVersion = newVersion;
            _appCodeName = appCodeName;
            _changes = changes;
            ShouldForceUpdate = shouldForceUpdate;
        }
    }

}
