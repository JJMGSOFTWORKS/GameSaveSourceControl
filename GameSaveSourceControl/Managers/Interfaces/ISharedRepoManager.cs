using GameSaveSourceControl.Model;
using System.Collections.Generic;

namespace GameSaveSourceControl.Managers
{
    public interface ISharedRepoManager
    {
        string CloneRepo(string repoFolderPath);
        string PullWithStatus(string repoFolderPath);
        string PushWithStatus(List<LocalMapping> fileMappings, string repoFolderPath);
        void TransforLocalToShared(string localFilePath, string sharedLocation, string saveName);
        void TransforSharedToLocal(string sharedLocation, string localFilePath, string saveName);
    }
}