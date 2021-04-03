using GameSaveSourceControl.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;

namespace GameSaveSourceControl.Managers
{
    public class LocalFolderManager
    {
        public string BaseRepoPath { get { return _baseRepoPath; } }
        private string _baseRepoPath;

        public string ExePath { get { return _exePath; } }
        private string _exePath;

        public string SetBaseFolder(string repoLocation)
        {
            _baseRepoPath = $"{repoLocation}\\{Constants.BaseFolderName}";
            return Directory.CreateDirectory(_baseRepoPath).FullName;
        }

        public string SetLocalRepoFolder(string currentExeLocation)
        {
            _exePath = currentExeLocation;

            var localRepo = $"{currentExeLocation}{Constants.LocalRepoBasesFolderName}";
              Directory.CreateDirectory(localRepo);
            return localRepo;
        }

        private void WriteLocalMappingProfile(LocalMappingProfile gameGitRepoMapping)
        {
            File.WriteAllText($"{_exePath}{Constants.SaveConfigFileName}.json", JsonConvert.SerializeObject(gameGitRepoMapping));
        }

        public LocalMappingProfile ReadLocalMappingProfile()
        {
            var jsonData = File.ReadAllText($"{_exePath}{Constants.SaveConfigFileName}.json");
            return JsonConvert.DeserializeObject<LocalMappingProfile>(jsonData);
        }

        private void WriteSharedMappings(List<string> syncGames)
        {
            File.WriteAllText($"{_baseRepoPath}{Constants.SharedSaveConfigFileName}.json", JsonConvert.SerializeObject(syncGames));
        }

        public List<string> ReadSharedMappings()
        {
            var jsonData = File.ReadAllText($"{_baseRepoPath}{Constants.SharedSaveConfigFileName}.json");
            return JsonConvert.DeserializeObject<List<string>>(jsonData);
        }

        public void WriteLocalMappingData(List<LocalMapping> fileMappings)
        {
            WriteLocalMappingProfile(new LocalMappingProfile { LocalMappings = fileMappings});
            WriteSharedMappings(fileMappings.Select(m => m.FileName).ToList());
        }

        public List<string> CompareLocalToSharedMappings(LocalMappingProfile localMapping, List<string> sharedMapping)
        {
            List<string> output = new List<string>();

                foreach (var item in sharedMapping)
                if (!localMapping.LocalMappings.Any(g => g.FileName == item))
                    output.Add(item);

            return output;
        }

        public bool IsAppConfigValid()
        {
            try
            {
                if (string.IsNullOrEmpty(ConfigurationManager.AppSettings["GitEmail"]) ||
                    string.IsNullOrEmpty(ConfigurationManager.AppSettings["GitPassword"]) ||
                    string.IsNullOrEmpty(ConfigurationManager.AppSettings["GitRemoteUrl"]))
                {
                    return false;
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
