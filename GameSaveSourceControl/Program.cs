using System;
using System.Runtime.InteropServices;

namespace GameSaveSourceControl
{
    public class Program
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        [DllImport("Kernel32")]
        public static extern void AllocConsole();

        [DllImport("Kernel32")]
        public static extern void FreeConsole();

        [STAThread]
        static void Main(string[] args)
        {
            _logger.Log(NLog.LogLevel.Info, "Application started pre console allocation");

            AllocConsole();

            IocContainer.BuildDependancies();

            _logger.Log(NLog.LogLevel.Info, "Calling GSSC about to start");
            var gameSaveSourControl = IocContainer.GetService<IGameSaveSourceControl>();
            gameSaveSourControl.ApplicationRun();
            _logger.Log(NLog.LogLevel.Info, "GSSC is closed");

            FreeConsole();
            _logger.Log(NLog.LogLevel.Info, "Application closed console is free");
        }


    }
}
