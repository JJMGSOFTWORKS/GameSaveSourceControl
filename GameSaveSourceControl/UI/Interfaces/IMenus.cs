using GameSaveSourceControl.Model;
using System;

namespace GameSaveSourceControl.UI
{
    public interface IMenus
    {
        string AddGameChooseNamePrompt();
        LocalMapping AddGameForm(string name);
        ConsoleKey MainMenuKeyChoice();
    }
}