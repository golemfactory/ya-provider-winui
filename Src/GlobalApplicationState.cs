#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GolemUI
{
    public class GlobalApplicationState
    {
        ProcessController _processController;

        public ProcessController ProcessController { get { return _processController; } }

        public Dashboard? Dashboard { get; set; }
        public DebugWindow? DebugWindow { get; set; }

        private GlobalApplicationState()
        {
            _processController = new ProcessController();
        }

        void StartDebugWindow()
        {

        }

        /***
         * static methods 
         * (GlobalApplicationState is singleton like class initialized at the beggining and finishing at the end of application)
         */
        public static GlobalApplicationState Instance { get; private set; }

        public static void Initialize()
        {
            if (Instance != null)
            {
                throw new Exception("Initialize at the program start");
            }
            Instance = new GlobalApplicationState();
        }
        public static void Finish()
        {
            if (Instance == null)
            {
                throw new Exception("Finalizing unitialized GlobalApplicationState");
            }
        }
    }
}
