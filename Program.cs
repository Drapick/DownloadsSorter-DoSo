using DownloadsSorter_DoSo;
using DownloadsSorter_DoSo.Utils;

Console.WriteLine("=== Запуск DownloadsSorter-DoSo ===");

const string targetDirectory = @"D:\_DOWNLOADS";

var directoryInitializer = new DirectoryInitializationService();

// Проверяем готовность папки перед запуском основного цикла
if (!directoryInitializer.EnsureDirectoryExists(targetDirectory))
{
    AppLogger.LogError("Не удалось инициализировать рабочую папку. Завершение работы.");
    return;
}

AppLogger.LogSuccess("Система готова к отслеживанию файлов.");

// Сюда дальше подключим FileSystemWatcher...