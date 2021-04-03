using GameSaveSourceControl.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;

namespace GameSaveSourceControl.Managers
{
    public class ApplicationTrackingManager
    {
        List<AppTrackingProfile> currentTrackingProfiles = new List<AppTrackingProfile>();

        public bool ReadApplicationsRunning(List<LocalMapping> games)
        {
            GetCurrentTrackingProfiles(games);
            if (currentTrackingProfiles != null)
                if (currentTrackingProfiles.Any())
                {
                    Console.WriteLine("\n Application being watched to close");
                    foreach (var item in currentTrackingProfiles)
                        Console.WriteLine($"{item.LinkedMapping.FileName}");
                    Console.WriteLine("Saves will sync when all applications exit");

                    RunProfileTrackingLoop();
                    Console.WriteLine("Tracked application exited sync will begin soon");
                    return true;
                }

            Console.WriteLine("There are no current applications that can be tracked in you shared saves");
            return false;
        }

        void RunProfileTrackingLoop()
        {
            bool stopTracking = false;

            while (!stopTracking)
            {
                foreach (var item in currentTrackingProfiles)
                    if (item.ProcessesIdsNamed.All(i => i.Value.HasExited))
                        item.ApplicationClosed = true;

                currentTrackingProfiles.RemoveAll(i => i.ApplicationClosed);

                if (!currentTrackingProfiles.Any())
                    stopTracking = true;
            }
        }

        void GetCurrentTrackingProfiles(List<LocalMapping> games)
        {
            var wmiQueryString = "SELECT ProcessId, ExecutablePath, CommandLine FROM Win32_Process";
            using (var searcher = new ManagementObjectSearcher(wmiQueryString))
            using (var results = searcher.Get())
            {
                var query = from p in Process.GetProcesses()
                            join mo in results.Cast<ManagementObject>()
                            on p.Id equals (int)(uint)mo["ProcessId"]
                            select new
                            {
                                Process = p,
                                Path = (string)mo["ExecutablePath"],
                                CommandLine = (string)mo["CommandLine"],
                            };

                foreach (var process in query)
                    if (!string.IsNullOrEmpty(process.Path))
                        foreach (var game in games)
                            if (game.GameExePath.ToLower() == process.Path.ToLower())
                            {
                                bool alreadyTracked = false;
                                foreach (var trackProfile in currentTrackingProfiles)
                                    if (trackProfile.LinkedMapping.FileName == game.FileName)
                                    {
                                        trackProfile.ProcessesIdsNamed.Add(process.Process.Id, process.Process);
                                        alreadyTracked = true;
                                    }

                                if (!alreadyTracked)
                                {
                                    var newProfile = new AppTrackingProfile()
                                    {
                                        LinkedMapping = game,
                                        ProcessesIdsNamed = new Dictionary<int, Process>()
                                    };
                                    newProfile.ProcessesIdsNamed.Add(process.Process.Id, process.Process);
                                    currentTrackingProfiles.Add(newProfile);
                                }
                            }
            }
        }

    }
}
