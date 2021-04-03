using System.Collections.Generic;
using System.Diagnostics;

namespace GameSaveSourceControl.Model
{
    class AppTrackingProfile
    {
        public LocalMapping LinkedMapping { get; set; }
        //some tasks can run many processes, we watch for all to close
        public Dictionary<int,Process> ProcessesIdsNamed { get; set; }
        public bool ApplicationClosed { get; set; }
    }
}
