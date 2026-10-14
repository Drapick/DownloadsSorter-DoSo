using DownloadsSorter_DoSo.Utils;

namespace DownloadsSorter_DoSo;

public class FileWatcherService : IDisposable
{
    private readonly string _watchDirectory;
    private readonly FileRoutingService _routingService;
    private readonly FileMoverService _moverService;
    private FileSystemWatcher? _watcher;

    private readonly HashSet<string> _temporaryExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".tmp",
        ".temp",
        ".crdownload",
        ".part",
        ".download",
        ".opdownload"
    };

    public FileWatcherService(
        string watchDirectory, 
        FileRoutingService routingService, 
        FileMoverService moverService)
    {
        _watchDirectory = watchDirectory;
        _routingService = routingService;
        _moverService = moverService;
    }

    public void Start()
    {
        _watcher = new FileSystemWatcher(_watchDirectory)
        {
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime | NotifyFilters.LastWrite,
            IncludeSubdirectories = false,
            EnableRaisingEvents = true
        };
        
        _watcher.Created += OnFileCreated;
        _watcher.Renamed += OnFileRenamed;

        AppLogger.LogWatcher($"Мониторинг запущен для директории: {_watchDirectory}");
    }

    private void OnFileCreated(object sender, FileSystemEventArgs e) => ProcessFile(e.FullPath, e.Name);

    private void OnFileRenamed(object sender, RenamedEventArgs e)
    {
        AppLogger.LogWatcher($"Файл завершил скачивание / переименован: {e.OldName} -> {e.Name}");
        ProcessFile(e.FullPath, e.Name);
    }

    private void ProcessFile(string fullPath, string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName) || Directory.Exists(fullPath))
        {
            return;
        }

        var extension = Path.GetExtension(fileName);

        // Игнорируем временные файлы браузеров
        if (_temporaryExtensions.Contains(extension) ||
            fileName.EndsWith(".tmp", StringComparison.OrdinalIgnoreCase) ||
            fileName.EndsWith(".crdownload", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        AppLogger.LogWatcher($"Обработка входящего файла: {fileName}");

        // 1. Получаем категорию
        var category = _routingService.GetDestinationFolder(extension);
        var targetDirectory = Path.Combine(_watchDirectory, category);

        // 2. Делегируем безопасное перемещение сервису Mover
        _moverService.MoveFileSafely(fullPath, _watchDirectory, category);
    }

    public void Stop()
    {
        if (_watcher != null)
        {
            _watcher.EnableRaisingEvents = false;
            _watcher.Created -= OnFileCreated;
            _watcher.Renamed -= OnFileRenamed;
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
