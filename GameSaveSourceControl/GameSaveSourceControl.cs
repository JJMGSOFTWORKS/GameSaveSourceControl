using GameSaveSourceControl.Managers;
using GameSaveSourceControl.Model;
using GameSaveSourceControl.UI;
using GameSaveSourceControl.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameSaveSourceControl
{
    public class GameSaveSourceControl : IGameSaveSourceControl
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly IFolderPathManager _folderPathManager;
        private readonly IMappingManager _mappingManager;
        private readonly ISharedRepoManager _sharedRepoManager;
        private readonly IMenus _menus;
        private readonly IMessages _messages;
        private readonly IApplicationTrackingManager _applicationTrackingManager;

        static LocalMappingProfile _currentLocalMappingProfile;
        static List<string> _currentSharedMappings;

        public GameSaveSourceControl(IFolderPathManager folderPathManager, IMappingManager mappingManager, ISharedRepoManager sharedRepoManager, IMenus menus, IMessages messages, IApplicationTrackingManager applicationTrackingManager)
        {
            _folderPathManager = folderPathManager;
            _mappingManager = mappingManager;
            _sharedRepoManager = sharedRepoManager;
            _menus = menus;
            _messages = messages;
            _applicationTrackingManager = applicationTrackingManager;
        }
        public void ApplicationRun()
        {
            if (!_folderPathManager.IsAppConfigValid())
            {
                _logger.Info("Config was identified as invalid");
                _messages.InvalidConfigConfirmation();
                return;
            }
            var setRepoLocation = _folderPathManager.SetLocalRepoFolder(AppDomain.CurrentDomain.BaseDirectory);

            var localRepoLocation = _sharedRepoManager.CloneRepo(setRepoLocation);
            _folderPathManager.SetBaseFolder(localRepoLocation);

            _messages.WaitingToPull();
            _messages.PullStatusDisplay(_sharedRepoManager.PullWithStatus(localRepoLocation));

            try
            {
                _currentSharedMappings = _mappingManager.ReadSharedMappings();
                _currentLocalMappingProfile = _mappingManager.ReadLocalMappingProfile();
            }
            catch (Exception e)
            {
                _logger.Error(e,"Failed to get mappings");
                if (_currentSharedMappings != null)
                    if (!_currentSharedMappings.Any())
                        _currentSharedMappings = new List<string>();

                _currentLocalMappingProfile = new LocalMappingProfile();
            }

            MenuLoop();
        }

        private void MenuLoop()
        {
            ReadNewSaves();

            //TODO this may be better handled with something more readable than a key
            var choiceKey = _menus.MainMenuKeyChoice();

            switch (choiceKey)
            {
                case ConsoleKey.S:
                    _messages.WaitingToPush();
                    _messages.PushStatusDisplay(_sharedRepoManager.PushWithStatus(_currentLocalMappingProfile.LocalMappings, _folderPathManager.LocalRepoPath));
                    break;

                case ConsoleKey.A:
                    AddGameMenu();
                    break;

                case ConsoleKey.T:
                    try
                    {
                        if (_currentLocalMappingProfile == null)
                            _currentLocalMappingProfile = _mappingManager.ReadLocalMappingProfile();
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e,"The application cant find any games that could be tracked");
                        break;
                    }
                    if (_applicationTrackingManager.SetTrackingOnApplications(_currentLocalMappingProfile.LocalMappings, out var appsTracked))
                        _sharedRepoManager.PushWithStatus(appsTracked, _folderPathManager.LocalRepoPath);
                    break;

                case ConsoleKey.Escape:
                    return;

                default:
                    break;
            };
        }

        private void AddGameMenu()
        {
            var saveName = _menus.AddGameChooseNamePrompt();
            if (!ValidSaveName(saveName))
            {
                _logger.Info($"Name chosen was invalid: {saveName}");
                _messages.InvalidFileNameConfirmation();
                return;
            }
            var mapping = _menus.AddGameForm(saveName);

            if (mapping == null)
            {
                _logger.Info("Mapping was not added");
                _messages.ItemAddMessage(false);
                return;
            }    

            if (_currentLocalMappingProfile.LocalMappings == null)
                _currentLocalMappingProfile.LocalMappings = new List<LocalMapping>();

            _currentLocalMappingProfile.LocalMappings.Add(mapping);
            _mappingManager.WriteLocalMappingData(_currentLocalMappingProfile.LocalMappings);
            _messages.ItemAddMessage(true);
        }

        bool ValidSaveName(string nameToValidate)
        {
            if (!ValidGameGitRepoMapping())
            {
                _messages.InvalidAddWarning();
                return true;
            }

            return !_currentLocalMappingProfile.LocalMappings.Any(i => i.FileName == nameToValidate);
        }

        static bool ValidGameGitRepoMapping() =>
         (_currentLocalMappingProfile != null && _currentLocalMappingProfile.LocalMappings != null && _currentLocalMappingProfile.LocalMappings.Any()); 

        private void ReadNewSaves()
        {
            if (_currentSharedMappings == null)
                return;
            if (!MappingUtility.ValidateLocalMappingProfile(_currentLocalMappingProfile))
                _currentLocalMappingProfile = new LocalMappingProfile() { LocalMappings = new List<LocalMapping>() };

            var updatedMappings = _mappingManager.CompareLocalToSharedMappings(_currentLocalMappingProfile, _currentSharedMappings);

            if (!updatedMappings.Any())
                return;
            _messages.NewSavesMessage(updatedMappings);
        }
    }
}
