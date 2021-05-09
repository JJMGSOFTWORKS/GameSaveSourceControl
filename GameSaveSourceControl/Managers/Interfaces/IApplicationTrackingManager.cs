using GameSaveSourceControl.Model;
using System.Collections.Generic;

namespace GameSaveSourceControl.Managers
{
    public interface IApplicationTrackingManager
    {
        bool SetTrackingOnApplications(List<LocalMapping> applicationMappings, out List<string> trackedApps);
    }
}