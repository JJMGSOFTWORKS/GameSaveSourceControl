using GameSaveSourceControl.Model;
using System.Linq;

namespace GameSaveSourceControl.Utilities
{
    public static class MappingUtility
    {
        public static bool ValidateLocalMappingProfile(LocalMappingProfile localMappingProfile) =>        
            (localMappingProfile != null && localMappingProfile.LocalMappings != null && localMappingProfile.LocalMappings.Any());        
    }
}
