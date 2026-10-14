using System.Timers;
using DownloadsSorter_DoSo.Utils;
using Timer = System.Timers.Timer;

namespace DownloadsSorter_DoSo;

public class FileWatcherService : IDisposable
{
    private readonly string _watchDirectory;
    private readonly FileRoutingService _routingService;
    private readonly FileMoverService _moverService;
    private readonly FileBatchStatisticsService _statsService;
    private readonly PersonalityPhraseService _phraseService;
    private readonly Action<string, string> _notifyAction;

    private FileSystemWatcher? _watcher;
    private readonly Timer _debounceTimer;
    private readonly object _timerLock = new();

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
        FileMoverService moverService,
        FileBatchStatisticsService statsService,
        PersonalityPhraseService phraseService,
        Action<string, string> notifyAction)
    {
        _watchDirectory = watchDirectory;
        _routingService = routingService;
        _moverService = moverService;
        _statsService = statsService;
        _phraseService = phraseService;
        _notifyAction = notifyAction;
        
        _debounceTimer = new Timer(300)
        {
            AutoReset = false
        };
        _debounceTimer.Elapsed += OnBatchCompleted;
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

        if (_temporaryExtensions.Contains(extension) ||
            fileName.EndsWith(".tmp", StringComparison.OrdinalIgnoreCase) ||
            fileName.EndsWith(".crdownload", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        AppLogger.LogWatcher($"Обработка входящего файла: {fileName}");

        var category = _routingService.GetDestinationFolder(extension);

        // Получаем размер до перемещения (на случай удаления исходника)
        long fileSize = 0;
        try
        {
            var fileInfo = new FileInfo(fullPath);
            if (fileInfo.Exists)
            {
                fileSize = fileInfo.Length;
            }
        }
        catch
        {
            // Игнорируем ошибку чтения метаданных
        }

        // Физическое безопасное перемещение
        var moveSucceeded = _moverService.MoveFileSafely(fullPath, _watchDirectory, category);

        if (moveSucceeded)
        {
            // 1. УЧЕТ В СТАТИСТИКЕ: фиксируем успешный файл
            _statsService.RecordFile(category, fileSize);

            // 2. ПЕРЕЗАПУСК ТАЙМЕРА: сбрасываем ожидание тишины
            lock (_timerLock)
            {
                _debounceTimer.Stop();
                _debounceTimer.Start();
            }
        }
    }

    private void OnBatchCompleted(object? sender, ElapsedEventArgs e)
    {
        lock (_timerLock)
        {
            if (_statsService.TotalFiles == 0)
            {
                return;
            }
            
            // Получаем преобладающую категорию и богатырскую реплику
            var dominantCategory = _statsService.GetDominantCategory();
            var (title, description) = _phraseService.GetRandomPhrase(dominantCategory);

            var totalFiles = _statsService.TotalFiles;
            var formattedSize = _statsService.GetFormattedTotalSize();
            
            var message = $"{description}\nПеренесено: {totalFiles} файлов ({formattedSize})";

            _notifyAction(title, message);

            // Сбрасываем счетчик для следующей пачки
            _statsService.Reset();
        }
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

        _debounceTimer.Stop();
        _debounceTimer.Dispose();

        AppLogger.LogWatcher("Служба мониторинга остановлена.");
    }

    public void Dispose()
    {
        Stop();
        GC.SuppressFinalize(this);
    }
}
