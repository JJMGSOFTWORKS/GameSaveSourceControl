using System;
using System.Collections.Generic;

namespace GameSaveSourceControl.UI
{
    public class Messages : IMessages
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        public void InvalidConfigConfirmation()
        {
            Console.WriteLine("Please be sure that your config is valid and restart this app");
            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }

        public void InvalidFileNameConfirmation() =>
        Console.WriteLine("Name is either a duplicate or invalid, try again");

        public void WaitingToPull() =>
            WaitingForActionMessage("Pulling new shared data");

        public void WaitingToPush() =>
            WaitingForActionMessage("Sending over data");

        public void WaitingForActionMessage(string actionDescription) =>
            Console.WriteLine($"Please Wait....{actionDescription}");

        public void PullStatusDisplay(string pullStatus)
        {
            var getSavesResult = $"Status retreiving latest data: {pullStatus}";
            Console.WriteLine(getSavesResult);
        }

        public void PushStatusDisplay(string pushStatus)
        {
            var getSavesResult = $"Status for sending data: {pushStatus}";
            Console.WriteLine(getSavesResult);
        }

        public void ErrorMessageStatus() =>
            Console.WriteLine("Error encountered, check the log file for more information");

        void IMessages.NewSavesMessage(List<string> saves)
        {
            Console.WriteLine("\n");
            Console.WriteLine("!!!!!!!There are new saves to sync with the repo!!!!!!!");
            Console.WriteLine("You will need to add new saves to the share library in the next menu that match the following headings exactly");
            saves.ForEach(s => Console.WriteLine(s));
        }

        void IMessages.InvalidAddWarning() => 
            Console.WriteLine("Adding your first save? if not exit now!");

        void IMessages.ItemAddMessage(bool added)
        {
            if(added)
                Console.WriteLine("\nNice, added the file to be shared! run a sync to share it");
            else
                Console.WriteLine("\nNew file was not added");
        }

        void IMessages.SavesToTrackMessage(List<string> savesTracked)
        {
            Console.WriteLine("\n Applications being tracked to sync the following saves:\n");
            savesTracked.ForEach(i => Console.WriteLine(i));
            Console.WriteLine("\nSaves will sync when all applications exit");
        }

        void IMessages.FinishedTrackingMessage() => Console.WriteLine("Tracked application exited sync will begin soon");

        void IMessages.NoTrackableApplications() => Console.WriteLine("There are no current applications that can be tracked in you shared saves");
    }
}
