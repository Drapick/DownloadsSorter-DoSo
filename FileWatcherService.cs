using DownloadsSorter_DoSo.Utils;

namespace DownloadsSorter_DoSo;

public class FileWatcherService : IDisposable
{
    private readonly string _watchDirectory;
    private readonly FileRoutingService _routingService;
    private FileSystemWatcher? _watcher;

    public FileWatcherService(string watchDirectory, FileRoutingService routingService)
    {
        _watchDirectory = watchDirectory;
        _routingService = routingService;
    }

    public void Start()
    {
        _watcher = new FileSystemWatcher(_watchDirectory)
        {
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime,
            IncludeSubdirectories = false, // Слушаем только корень загрузок, не трогаем подпапки
            EnableRaisingEvents = true
        };

        // Подписываемся на событие создания нового файла
        _watcher.Created += OnFileCreated;

        AppLogger.LogWatcher($"Мониторинг запущен для директории: {_watchDirectory}");
    }

    private void OnFileCreated(object sender, FileSystemEventArgs e)
    {
        // Игнорируем папки, обрабатываем только файлы
        if (Directory.Exists(e.FullPath))
        {
            return;
        }

        var fileName = e.Name;
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        AppLogger.LogWatcher($"Обнаружен новый файл: {fileName}");

        try
        {
            var extension = Path.GetExtension(fileName);
            var category = _routingService.GetDestinationFolder(extension);

            // Формируем путь к целевой категории
            var targetDirectory = Path.Combine(_watchDirectory, category);

            // ДИНАМИЧЕСКАЯ ПРОВЕРКА: создаем папку только при реальной необходимости
            if (!Directory.Exists(targetDirectory))
            {
                AppLogger.LogSystem($"Категория отсутствует. Создаем: {category}");
                Directory.CreateDirectory(targetDirectory);
            }

            var targetFilePath = Path.Combine(targetDirectory, fileName);

            // Пока делаем базовое перемещение для проверки логики
            File.Move(e.FullPath, targetFilePath);
            AppLogger.LogSuccess($"Файл [{fileName}] успешно перемещен в -> [{category}]");
        }
        catch (IOException ex)
        {
            AppLogger.LogWarning($"Файл [{fileName}] пока занят другим процессом: {ex.Message}");
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"Не удалось переместить файл [{fileName}]", ex);
        }
    }

    public void Stop()
    {
        if (_watcher != null)
        {
            _watcher.EnableRaisingEvents = false;
            _watcher.Created -= OnFileCreated;
            _watcher.Dispose();
            _watcher = null;
        }

        AppLogger.LogWatcher("Служба мониторинга остановлена.");
    }

    public void Dispose()
    {
        Stop();
        GC.SuppressFinalize(this);
    }
}
