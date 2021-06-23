using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GolemUI
{

    public enum GlobalApplicationStateAction
    {
        emptyAction,
        yagnaAppStarting,
        yagnaAppStarted,
        yagnaAppStopped
    }

    public class GlobalApplicationStateEventArgs
    {
        public GlobalApplicationStateAction action { get; set; }
    }


    public class GlobalApplicationState
    {
        private static GlobalApplicationState? _instance = null;

        public static void Initialize()
        {
            if (_instance != null)
            {
                throw new Exception("Initialize at the program start");
            }
            _instance = new GlobalApplicationState();
        }
        public static void Finish()
        {
            if (_instance == null)
            {
                throw new Exception("Finalizing unitialized GlobalApplicationState");
            }
        }

        public static GlobalApplicationState Instance
        {
            get
            {
                if (_instance == null)
                {
                    throw new Exception("GlobalApplicationState not initialized");
                }
                return _instance;
            }
        }

        public void NotifyApplicationStateChanged(object sender)
        {
            if (ApplicationStateChanged != null)
            {
                ApplicationStateChanged(sender, null);
            }
        }

        public void NotifyApplicationStateChanged(object sender, GlobalApplicationStateEventArgs args)
        {
            if (ApplicationStateChanged != null)
            {
                ApplicationStateChanged(sender, args);
            }
        }

        public void NotifyApplicationStateChanged(object sender, GlobalApplicationStateAction action)
        {
            if (ApplicationStateChanged != null)
            {
                var args = new GlobalApplicationStateEventArgs();
                args.action = action;
                ApplicationStateChanged(sender, args);
            }
        }

        public delegate void ApplicationStateChangedDelegate(object sender, GlobalApplicationStateEventArgs? args);
        public ApplicationStateChangedDelegate? ApplicationStateChanged { get; set; }


    }


}
