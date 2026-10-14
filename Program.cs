using DownloadsSorter_DoSo;
using DownloadsSorter_DoSo.Utils;

AppLogger.LogSystem("=== Инициализация DownloadsSorter-DoSo ===");

const string watchDirectory = @"D:\_DOWNLOADS";

// 1. Проверяем и создаем рабочую папку, если ее нет
var initializer = new DirectoryInitializationService();
if (!initializer.EnsureDirectoryExists(watchDirectory))
{
    AppLogger.LogError("Критическая ошибка запуска. Завершение работы.");
    return;
}

// 2. Инициализируем маршрутизатор и слушатель событий
var routingService = new FileRoutingService();
using var watcherService = new FileWatcherService(watchDirectory, routingService);

// 3. Запускаем слежение
watcherService.Start();

AppLogger.LogSuccess("Программа готова к работе. Нажмите [Enter] для завершения...");
Console.ReadLine();

watcherService.Stop();