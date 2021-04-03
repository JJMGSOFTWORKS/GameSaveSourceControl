using GameSaveSourceControl.Managers;
using GameSaveSourceControl.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

//TODO Refactor this moving views and separating concerns
namespace GameSaveSourceControl
{
    public class Program
    {
        [DllImport("Kernel32")]
        public static extern void AllocConsole();

        [DllImport("Kernel32")]
        public static extern void FreeConsole();

        private static LocalFolderManager _localFolderManager;
        private static SharedRepoManager _sharedRepoManager;
        private static ApplicationTrackingManager _applicationTrackingManager;

        static LocalMappingProfile _currentLocalMappingProfile;
        static List<string> _currentSharedMappings;
        private static string _localRepoLocation;

        [STAThread]
        static void Main(string[] args)
        {
            AllocConsole();
            _localFolderManager = new LocalFolderManager();
            _sharedRepoManager = new SharedRepoManager();
            _applicationTrackingManager = new ApplicationTrackingManager();

            if (!_localFolderManager.IsAppConfigValid())
            {
                Console.WriteLine("please be sure that your config is valid and restart this app");
                Console.WriteLine("press any key to exit");
                Console.ReadKey();
                FreeConsole();
                Environment.Exit(0);
            }
            var setRepoLocation = _localFolderManager.SetLocalRepoFolder(AppDomain.CurrentDomain.BaseDirectory);

            _localRepoLocation = _sharedRepoManager.CloneRepo(setRepoLocation);
            _localFolderManager.SetBaseFolder(_localRepoLocation);

            Console.WriteLine("Pulling any new save data");
            var getSavesResult = $"result for pulling new shared saves: {_sharedRepoManager.PullWithStatus(_localRepoLocation)}";
            Console.WriteLine(getSavesResult);

            try
            {
                _currentSharedMappings = _localFolderManager.ReadSharedMappings();
                _currentLocalMappingProfile = _localFolderManager.ReadLocalMappingProfile();
            }
            catch (Exception)
            {
                if (_currentSharedMappings != null)
                    if (!_currentSharedMappings.Any())
                        _currentSharedMappings = new List<string>();

                _currentLocalMappingProfile = new LocalMappingProfile();
            }

            MainMenu();
        }

        private static void MainMenu()
        {
            try
            {
                ReadNewSaves();

                Console.WriteLine("\n");
                Console.WriteLine("___________Game Save Sync___________");
                Console.WriteLine("------------------------------");
                Console.WriteLine("If you want to sync any of your saves when a game closes type T");
                Console.WriteLine("If you want to sync all of your saves manually type S");
                Console.WriteLine("If you want to add a game to the share library type A");
                Console.WriteLine("If you want to quit type ESC");
                Console.WriteLine("------------------------------");

                var key = Console.ReadKey(true).Key;

                switch (key)
                {
                    case ConsoleKey.S:
                        Console.WriteLine("about to sync data");
                        Console.WriteLine($"result of sync is = {_sharedRepoManager.PushWithStatus(_currentLocalMappingProfile.LocalMappings, _localRepoLocation)}");
                        break;

                    case ConsoleKey.A:
                        AddGameMenu();
                        break;

                    case ConsoleKey.T:
                        try
                        {
                            if (_currentLocalMappingProfile == null)
                                _currentLocalMappingProfile = _localFolderManager.ReadLocalMappingProfile();
                        }
                        catch (Exception)
                        {
                            Console.WriteLine("Log :: The application cant find any games that could be tracked");
                            break;
                        }
                        if (_applicationTrackingManager.ReadApplicationsRunning(_currentLocalMappingProfile.LocalMappings))
                            _sharedRepoManager.PushWithStatus(_currentLocalMappingProfile.LocalMappings, _localRepoLocation);
                        break;

                    case ConsoleKey.Escape:
                        FreeConsole();
                        return;

                    default:
                        break;
                };
            }
            catch (Exception e)
            {
                Console.WriteLine("WARNING!!! The app has encountered a catastrophic failure and can not continue. Press any key to refresh the menu or Close and reopen the application");
                Console.WriteLine(e.Message);
                Console.ReadKey();
            }
            MainMenu();
        }

        private static void ReadNewSaves()
        {
            if (_currentSharedMappings == null)
                return;
            if (!ValidGameGitRepoMapping())
                _currentLocalMappingProfile = new LocalMappingProfile() { LocalMappings = new List<LocalMapping>() };

            var updatedMappings = _localFolderManager.CompareLocalToSharedMappings(_currentLocalMappingProfile, _currentSharedMappings);

            if (!updatedMappings.Any())
                return;

            Console.WriteLine("\n");
            Console.WriteLine("!!!!!!!There are new saves to sync with the repo!!!!!!!");
            Console.WriteLine("You will need to add new saves to the share library in the next menu that match the following headings");
            foreach (var item in updatedMappings)
                Console.WriteLine(item);
        }

        private static void AddGameMenu()
        {
            try
            {
                var mapping = new LocalMapping();
                Console.WriteLine("\n");
                Console.WriteLine("Answer the following to add a new save file to share");
                Console.WriteLine("What is the name of this shared save file");

                mapping.FileName = Console.ReadLine();

                if (!ValidSaveName(mapping.FileName))
                {
                    Console.WriteLine("Name is either a duplicate or invalid, try again");
                    return;
                }
                string file;

                //Console.WriteLine("Is the save a file or a folder?");
                //Console.WriteLine("Type F for a file type any other key for a folder?");

                //if (Console.ReadKey(true).Key == ConsoleKey.F)
                //{
                Console.WriteLine($"\nGreat in what file can we find the {mapping.FileName} save");
                file = RunFileDialog();
                //}
                //else
                //{
                //    Console.WriteLine($"Great in what folder can we find the {mapping.FileName} save");
                //    file = RunFolderdialog();
                //}

                if (string.IsNullOrEmpty(file))
                {
                    Console.WriteLine("Not a valid file");
                    return;
                }
                mapping.FilePathToTrack = file;

                Console.WriteLine("Okay!");

                Console.WriteLine($"\nWhere is the {mapping.FileName} application .exe located?");
                var exe = RunFileDialog();

                if (string.IsNullOrEmpty(exe))
                {
                    Console.WriteLine("Invalid exe selected!");
                    return;
                }
                mapping.GameExePath = exe;

                if (ConfirmMapping(mapping))
                {
                    if (_currentLocalMappingProfile.LocalMappings == null)
                        _currentLocalMappingProfile.LocalMappings = new List<LocalMapping>();

                    _currentLocalMappingProfile.LocalMappings.Add(mapping);


                    _localFolderManager.WriteLocalMappingData(_currentLocalMappingProfile.LocalMappings);
                    Console.WriteLine("\nNice, added the file to be shared! run a sync to share it to the shared repository");
                }
                else
                    Console.WriteLine("New file was not added");
            }
            catch (Exception)
            {
                throw;
            }
        }

        static bool ValidSaveName(string nameToValidate)
        {
            if (_currentSharedMappings.Any(i => i == nameToValidate))
                return false;

            if (!ValidGameGitRepoMapping())
            {
                Console.WriteLine("!!!If this is not your first save exit now, something is wrong");
                return true;
            }

            return !_currentLocalMappingProfile.LocalMappings.Any(i => i.FileName == nameToValidate);
        }

        static bool ValidGameGitRepoMapping()
        { return (_currentLocalMappingProfile != null && _currentLocalMappingProfile.LocalMappings != null && _currentLocalMappingProfile.LocalMappings.Any()); }

        private static bool ConfirmMapping(LocalMapping mapping)
        {
            Console.WriteLine("\n");
            Console.WriteLine("You have created the following mapping");
            Console.WriteLine($"Name: {mapping.FileName}");
            Console.WriteLine($"Save Location: {mapping.FilePathToTrack}");
            Console.WriteLine($"Exe: {mapping.GameExePath}");
            Console.WriteLine("\nType C to confirm the mapping is correct, any other key to return to menu without saving");

            if (Console.ReadKey(true).Key == ConsoleKey.C)
                return true;

            return false;
        }

        //TODO ensure folder file sync conflicts are managed
        private static string RunFolderdialog()
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
                return dialog.SelectedPath;

            return string.Empty;
        }

        private static string RunFileDialog()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
                return dialog.FileName;

            return string.Empty;
        }
    }
}
