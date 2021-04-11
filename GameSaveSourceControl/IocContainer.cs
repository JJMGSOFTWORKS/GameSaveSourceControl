using GameSaveSourceControl.Managers;
using GameSaveSourceControl.UI;
using Microsoft.Extensions.DependencyInjection;

namespace GameSaveSourceControl
{
    public static class IocContainer
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private static ServiceProvider _serviceProvider;
        private static bool _initialised;

        public static void BuildDependancies()
        {
            _logger.Log(NLog.LogLevel.Info, "Preparing to build service provider for dependencies");
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddScoped<IGameSaveSourceControl, GameSaveSourceControl>();
            serviceCollection.AddTransient<IApplicationTrackingManager, ApplicationTrackingManager>();
            serviceCollection.AddScoped<IFolderPathManager, FolderPathManager>();
            serviceCollection.AddTransient<IMappingManager, MappingManager>();
            serviceCollection.AddTransient<ISharedRepoManager, SharedRepoManager>();
            serviceCollection.AddTransient<IMenus, Menus>();
            serviceCollection.AddTransient<IMessages, Messages>();

            _serviceProvider = serviceCollection.BuildServiceProvider();
            _initialised = true;
            _logger.Log(NLog.LogLevel.Info, "Service provider built, dependencies are injected");
        }

        public static T GetService<T>()
        {
            if (!_initialised)
                BuildDependancies();

            _logger.Log(NLog.LogLevel.Info, $"Service provider fetching Service of type {typeof(T)}");
            return _serviceProvider.GetRequiredService<T>();
        }
    }
}
