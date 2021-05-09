using GameSaveSourceControl.Model;
using System;
using System.Windows.Forms;

namespace GameSaveSourceControl.UI
{
    public class Menus : IMenus
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        public ConsoleKey MainMenuKeyChoice()
        {
            Console.WriteLine("\n");
            Console.WriteLine("___________Game Save Source Control___________");
            Console.WriteLine("------------------------------");
            Console.WriteLine("If you want to sync any of your saves when a game closes type T");
            Console.WriteLine("If you want to sync all of your saves manually type S");
            Console.WriteLine("If you want to add a game to the share library type A");
            Console.WriteLine("If you want to quit type ESC");
            Console.WriteLine("------------------------------");

            return Console.ReadKey(true).Key;
        }

        public string AddGameChooseNamePrompt()
        {
            Console.WriteLine("\n");
            Console.WriteLine("Answer the following to add a new save file to share");
            Console.WriteLine("What is the name of this shared save file");
            return Console.ReadLine();
        }

        public LocalMapping AddGameForm(string name)
        {
            var mapping = new LocalMapping();
            mapping.FileName = name;

            string file;
            Console.WriteLine($"\nGreat in what file can we find the {mapping.FileName} save");
            file = RunFileDialog("Select Save File");

            if (string.IsNullOrEmpty(file))
            {
                Console.WriteLine("Not a valid file");
                return null;
            }
            mapping.FilePathToTrack = file;

            Console.WriteLine("Okay!");

            Console.WriteLine($"\nWhere is the {mapping.FileName} application .exe located?");
            var exe = RunFileDialog("Select Game Executable");

            if (string.IsNullOrEmpty(exe))
            {
                Console.WriteLine("Invalid exe selected!");
                return null;
            }
            mapping.GameExePath = exe;

            if (ConfirmMapping(mapping))
            {
                return mapping;
            }
            else
                Console.WriteLine("New file was not added");

            return null;
        }

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

        //TODO refactor repeated code in following methods
        private static string RunFolderDialog(string dialogHeader)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = dialogHeader;
            dialog.UseDescriptionForTitle = true;
            bool resultFound = false;

            while (!resultFound)
            {
                var dialogResult = dialog.ShowDialog();
                if (dialogResult == DialogResult.OK)
                    return dialog.SelectedPath;
                if (dialogResult == DialogResult.Cancel)
                    resultFound = true;
            }

            return string.Empty;
        }

        private static string RunFileDialog(string dialogHeader)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = false;
            dialog.Title = dialogHeader;
            bool resultFound = false;

            while (!resultFound)
            {
                var dialogResult = dialog.ShowDialog();
                if (dialogResult == DialogResult.OK)
                    return dialog.FileName;
                if (dialogResult == DialogResult.Cancel)
                    resultFound = true;
            }

            return string.Empty;
        }
    }
}
