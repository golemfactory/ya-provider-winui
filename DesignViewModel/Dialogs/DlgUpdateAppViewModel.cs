﻿using BetaMiner.Interfaces;
using BetaMiner.Utils;
using Nethereum.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BetaMiner.DesignViewModel.Dialogs
{
    public class DlgUpdateAppViewModel
    {
        public string Title => IsUpToDate ? "Your application is up to date" : "New version of application is available";
        public string UpdateLink => "http://google.com";
        public string CurrentVersion => System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        string NewVersion => "0.2.7.2";

        string AppCodeName => "CandyFlip";
        public string ChangeLog
        {
            get
            {
                if (Changes == null || Changes.Count == 0)
                    return "";
                return "New changes:\n" + String.Join("\n", Changes.Take(5).Select(x => "- " + x).ToArray());
            }
        }
        List<string> Changes = new List<string>() { "change1", "change2", "change3", "change4*" };
        public bool ShouldForceUpdate => false;
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
    }
}
