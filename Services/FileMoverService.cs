using DownloadsSorter_DoSo.Utils;

namespace DownloadsSorter_DoSo;

public class FileMoverService
{
    private const int MaxRetryAttempts = 10;
    private const int RetryDelayMilliseconds = 500;
    
    private readonly DateFolderRoutingService _dateRoutingService;
    
    public FileMoverService(DateFolderRoutingService dateRoutingService)
    {
        _dateRoutingService = dateRoutingService;
    }

    /// <summary>
    /// Безопасно перемещает файл с ожиданием снятия блокировки и защитой от перезаписи дубликатов.
    /// </summary>
    public bool MoveFileSafely(string sourcePath, string baseWatchDirectory, string category)
    {
        if (!File.Exists(sourcePath))
        {
            return false;
        }

        var fileName = Path.GetFileName(sourcePath);

        // 1. Ожидаем, пока внешний процесс (браузер/антивирус) освободит дескриптор файла
        if (!WaitForFileUnlocked(sourcePath))
        {
            AppLogger.LogError($"Файл [{fileName}] заблокирован другим процессом слишком долго. Перемещение отменено.");
            return false;
        }

        try
        {
            var fileCreationDate = File.GetCreationTime(sourcePath);
            
            var categoryDirectory = Path.Combine(baseWatchDirectory, category);
            
            var finalTargetDirectory = _dateRoutingService.GetDatedDestinationDirectory(
                categoryDirectory, 
                category, 
                fileCreationDate
            );

            // 2. ДИНАМИЧЕСКАЯ ПРОВЕРКА ПАПКИ: создаем категорию только при необходимости
            if (!Directory.Exists(finalTargetDirectory))
            {
                Directory.CreateDirectory(finalTargetDirectory);
                var relativeCreatedPath = Path.GetRelativePath(baseWatchDirectory, finalTargetDirectory);
                AppLogger.LogSystem($"Создана категория / папка: {relativeCreatedPath}");
            }

            // 3. ОБРАБОТКА ДУБЛИКАТОВ: вычисляем уникальное имя (например, report (1).pdf)
            var destinationFilePath = GetUniqueDestinationPath(finalTargetDirectory, fileName);
            var finalFileName = Path.GetFileName(destinationFilePath);

            File.Move(sourcePath, destinationFilePath);

            if (!string.Equals(fileName, finalFileName, StringComparison.OrdinalIgnoreCase))
            {
                AppLogger.LogWarning($"Обнаружен дубликат. Файл переименован: [{fileName}] -> [{finalFileName}]");
            }
            
            var displayPath = Path.GetRelativePath(baseWatchDirectory, destinationFilePath);
            AppLogger.LogSuccess($"Файл [{finalFileName}] успешно перемещен в -> [{displayPath}]");
            return true;
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"Сбой перемещения файла [{fileName}]", ex);
            return false;
        }
    }

    /// <summary>
    /// Цикл опроса дескриптора: проверяет, закончил ли браузер запись файла.
    /// </summary>
    private static bool WaitForFileUnlocked(string filePath)
    {
        for (var attempt = 1; attempt <= MaxRetryAttempts; attempt++)
        {
            try
            {
                // Пытаемся монопольно открыть файл на чтение/запись. 
                // Если браузер еще пишет в него, ОС выбросит IOException.
                using var stream = File.Open(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                if (stream.Length > 0 || attempt > 1)
                {
                    return true;
                }
            }
            catch (IOException)
            {
                // Файл занят внешним процессом — ждем и повторяем попытку
                Thread.Sleep(RetryDelayMilliseconds);
            }
            catch (UnauthorizedAccessException)
            {
                Thread.Sleep(RetryDelayMilliseconds);
            }
        }

        return false;
    }

    /// <summary>
    /// Генерирует уникальный путь, добавляя индекс (1), (2) к имени при коллизиях.
    /// </summary>
    private static string GetUniqueDestinationPath(string directory, string originalFileName)
    {
        var destinationPath = Path.Combine(directory, originalFileName);

        if (!File.Exists(destinationPath))
        {
            return destinationPath;
        }

        var fileNameWithoutExt = Path.GetFileNameWithoutExtension(originalFileName);
        var extension = Path.GetExtension(originalFileName);
        var counter = 1;

        // Инкрементируем счетчик, пока не найдем свободное имя
        while (File.Exists(destinationPath))
        {
            var uniqueFileName = $"{fileNameWithoutExt} ({counter}){extension}";
            destinationPath = Path.Combine(directory, uniqueFileName);
            counter++;
        }

        return destinationPath;
    }
}
