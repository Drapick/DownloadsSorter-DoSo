using System.Text;
using System.Windows.Forms;
using DownloadsSorter_DoSo;
using DownloadsSorter_DoSo.Utils;

Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

ApplicationConfiguration.Initialize();

AppLogger.LogSystem("=== Инициализация DownloadsSorter-DoSo ===");

const string watchDirectory = @"D:\_DOWNLOADS";
var iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "logo.ico");

// 1. Проверяем и создаем рабочую папку, если ее нет
var initializer = new DirectoryInitializationService();
if (!initializer.EnsureDirectoryExists(watchDirectory))
{
    AppLogger.LogError("Критическая ошибка запуска. Завершение работы.");
    return;
}

// 2. Инициализируем сервисы статистики и фраз
var routingService = new FileRoutingService();
var dateRoutingService = new DateFolderRoutingService();
var moverService = new FileMoverService(dateRoutingService);
var statsService = new FileBatchStatisticsService();
var phraseService = new PersonalityPhraseService();

using var trayManager = new TrayIconManager(watchDirectory, iconPath);

// 3. Передаем лямбду с вызовом трея
using var watcherService = new FileWatcherService(
    watchDirectory, 
    routingService, 
    moverService, 
    statsService, 
    phraseService,
    (title, msg) => trayManager.ShowNotification(title, msg, ToolTipIcon.None)
);

trayManager.OnExitRequested += () =>
{
    watcherService.Stop();
};

// 4. Запускаем слежение
watcherService.Start();

trayManager.ShowNotification(
    "DoSo", 
    $"Старожу {watchDirectory}, дай бог комп не сгорит!", 
    ToolTipIcon.None
);

Application.Run();