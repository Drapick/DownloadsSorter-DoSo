using DownloadsSorter_DoSo.Utils;

namespace DownloadsSorter_DoSo;

public class DirectoryInitializationService
{
    public bool EnsureDirectoryExists(string directoryPath)
    {
        if (string.IsNullOrWhiteSpace(directoryPath))
        {
            AppLogger.LogError("Путь к директории не может быть пустым.");
            return false;
        }

        try
        {
            var root = Path.GetPathRoot(directoryPath);
            if (!string.IsNullOrEmpty(root))
            {
                var drive = new DriveInfo(root);
                if (!drive.IsReady)
                {
                    AppLogger.LogError($"Диск {root} недоступен или не готов к работе.");
                    return false;
                }
            }

            if (Directory.Exists(directoryPath))
            {
                AppLogger.LogSystem($"Рабочая директория найдена: {directoryPath}");
                return true;
            }

            AppLogger.LogSystem($"Директория отсутствует. Создаем: {directoryPath}");
            Directory.CreateDirectory(directoryPath);
            AppLogger.LogSuccess("Директория успешно создана.");
            return true;
        }
        catch (UnauthorizedAccessException)
        {
            AppLogger.LogError($"Нет прав доступа для создания директории: {directoryPath}");
            return false;
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"Сбой при инициализации директории: {ex.Message}", ex);
            return false;
        }
    }
}