using GameSaveSourceControl.Model;
using System.Collections.Generic;

namespace GameSaveSourceControl.Managers
{
    public interface IMappingManager
    {
        List<string> CompareLocalToSharedMappings(LocalMappingProfile localMapping, List<string> sharedMapping);
        LocalMappingProfile ReadLocalMappingProfile();
        List<string> ReadSharedMappings();
        void WriteLocalMappingData(List<LocalMapping> fileMappings);
    }
}