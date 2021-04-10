using System.Collections.Generic;

namespace GameSaveSourceControl.UI
{
    public interface IMessages
    {
        void ErrorMessageStatus();
        void InvalidConfigConfirmation();
        void InvalidFileNameConfirmation();
        void InvalidAddWarning();
        void NewSavesMessage(List<string> saves);
        void PullStatusDisplay(string pullStatus);
        void PushStatusDisplay(string pushStatus);
        void WaitingForActionMessage(string actionDescription);
        void WaitingToPull();
        void WaitingToPush();
        void ItemAddMessage(bool added);
        void SavesToTrackMessage(List<string> savesTracked);
        void FinishedTrackingMessage();
        void NoTrackableApplications();
    }
}