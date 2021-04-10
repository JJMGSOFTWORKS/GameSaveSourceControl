using System;
using System.Configuration;
using System.IO;

namespace GameSaveSourceControl.Managers
{
    public class FolderPathManager : IFolderPathManager
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        public string BaseRepoPath { get { return _baseRepoPath; } }
        private string _baseRepoPath;

        public string ExePath { get { return _exePath; } }
        private string _exePath;

        public string LocalRepoPath { get { return $"{_exePath}{Constants.LocalRepoBasesFolderName}"; } }

        public string SetBaseFolder(string repoLocation)
        {
            _baseRepoPath = $"{repoLocation}\\{Constants.BaseFolderName}";
            _logger.Info($"Base repo path set as {_baseRepoPath}");
            return Directory.CreateDirectory(_baseRepoPath).FullName;
        }

        public string SetLocalRepoFolder(string currentExeLocation)
        {
            _exePath = currentExeLocation;
            Directory.CreateDirectory(LocalRepoPath);
            return LocalRepoPath;
        }
        public bool IsAppConfigValid()
        {
            try
            {
                if (string.IsNullOrEmpty(ConfigurationManager.AppSettings["GitEmail"]) ||
                    string.IsNullOrEmpty(ConfigurationManager.AppSettings["GitPassword"]) ||
                    string.IsNullOrEmpty(ConfigurationManager.AppSettings["GitRemoteUrl"]))
                {
                    _logger.Warn($"Found Config to be invalid");
                    return false;
                }
                return true;
            }
            catch (Exception e)
            {
                _logger.Error(e, "Config validation failed");
                return false;
            }
        }
    }
}
