namespace GameSaveSourceControl.Managers
{
    public interface IFolderPathManager
    {
        string BaseRepoPath { get; }
        string ExePath { get; }
        string LocalRepoPath { get; }

        bool IsAppConfigValid();
        string SetBaseFolder(string repoLocation);
        string SetLocalRepoFolder(string currentExeLocation);
    }
}