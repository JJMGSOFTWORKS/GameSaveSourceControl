using GameSaveSourceControl.Model;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameSaveSourceControl.Managers
{
    public class MappingManager : IMappingManager
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly IFolderPathManager _folderPathManager;
        public MappingManager(IFolderPathManager folderPathManager)
        {
            _folderPathManager = folderPathManager;
        }

        private void WriteLocalMappingProfile(LocalMappingProfile gameGitRepoMapping)=>
            File.WriteAllText($"{_folderPathManager.ExePath}{Constants.SaveConfigFileName}.json", JsonConvert.SerializeObject(gameGitRepoMapping));


        public LocalMappingProfile ReadLocalMappingProfile()
        {
            var jsonData = File.ReadAllText($"{_folderPathManager.ExePath}{Constants.SaveConfigFileName}.json");
            return JsonConvert.DeserializeObject<LocalMappingProfile>(jsonData);
        }

        private void WriteSharedMappings(List<string> syncGames) =>
            File.WriteAllText($"{_folderPathManager.BaseRepoPath}{Constants.SharedSaveConfigFileName}.json", JsonConvert.SerializeObject(syncGames));

        public List<string> ReadSharedMappings()
        {
            var jsonData = File.ReadAllText($"{_folderPathManager.BaseRepoPath}{Constants.SharedSaveConfigFileName}.json");
            return JsonConvert.DeserializeObject<List<string>>(jsonData);
        }

        public void WriteLocalMappingData(List<LocalMapping> fileMappings, List<string> sharedMappings)
        {
            WriteLocalMappingProfile(new LocalMappingProfile { LocalMappings = fileMappings });
            WriteSharedMappings(sharedMappings);
        }

        public List<string> CompareLocalToSharedMappings(LocalMappingProfile localMapping, List<string> sharedMapping)
        {
            _logger.Info("comparing local to shared");
            List<string> output = new List<string>();

            foreach (var item in sharedMapping)
                if (!localMapping.LocalMappings.Any(g => g.FileName == item))
                    output.Add(item);

            return output;
        }
    }
}
