using GameSaveSourceControl.Model;
using GameSaveSourceControl.UI;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;

namespace GameSaveSourceControl.Managers
{
    public class ApplicationTrackingManager : IApplicationTrackingManager
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly IMessages _messages;

        List<AppTrackingProfile> _currentTrackingProfiles = new List<AppTrackingProfile>();

        public ApplicationTrackingManager(IMessages messages)
        {
            _messages = messages;
        }

        public bool SetTrackingOnApplications(List<LocalMapping> applicationMappings, out List<LocalMapping> trackedApps)
        {
            _logger.Info("About to search for apps to track");
            GetCurrentTrackingProfiles(applicationMappings);
            if (_currentTrackingProfiles != null)
                if (_currentTrackingProfiles.Any())
                {
                    var savesTracked = _currentTrackingProfiles.Select(i => i.LinkedMapping.FileName).ToList();
                    _messages.SavesToTrackMessage(savesTracked);

                    RunProfileTrackingLoop();
                    _messages.FinishedTrackingMessage();

                    trackedApps = _currentTrackingProfiles.Select(i => i.LinkedMapping).ToList(); 
                    return true;
                }

            _logger.Info("No trackable apps found");
            _messages.NoTrackableApplications();
            trackedApps = null;
            return false;
        }

        void RunProfileTrackingLoop()
        {
            bool stopTracking = false;
            _logger.Info("Track on apps");
            while (!stopTracking)
            {
                foreach (var item in _currentTrackingProfiles)
                    if (item.ProcessesIdsNamed.All(i => i.Value.HasExited))
                        item.ApplicationClosed = true;

                _currentTrackingProfiles.RemoveAll(i => i.ApplicationClosed);

                if (!_currentTrackingProfiles.Any())
                    stopTracking = true;
            }
            _logger.Info("Track on apps complete");
        }

        void GetCurrentTrackingProfiles(List<LocalMapping> games)
        {
            _logger.Info("Preparing to query all running system applications, hold on tight!!!");
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
                                foreach (var trackProfile in _currentTrackingProfiles)
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
                                    _currentTrackingProfiles.Add(newProfile);
                                }
                            }
            }
            _logger.Info($"We have queried all the applications finding {_currentTrackingProfiles.Count} results");
        }

    }
}
