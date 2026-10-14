namespace DownloadsSorter_DoSo;

public class FileBatchStatisticsService
{
    private readonly object _lock = new();
    private readonly Dictionary<string, int> _categoryCounts = new(StringComparer.OrdinalIgnoreCase);
    private long _totalBytes;
    private int _totalFiles;

    /// <summary>
    /// Фиксирует успешно перемещенный файл.
    /// </summary>
    public void RecordFile(string category, long fileSizeBytes)
    {
        lock (_lock)
        {
            _totalFiles++;
            _totalBytes += fileSizeBytes;

            if (_categoryCounts.ContainsKey(category))
            {
                _categoryCounts[category]++;
            }
            else
            {
                _categoryCounts[category] = 1;
            }
        }
    }

    /// <summary>
    /// Определяет категорию, файлов которой было перенесено больше всего.
    /// </summary>
    public string GetDominantCategory()
    {
        lock (_lock)
        {
            if (_categoryCounts.Count == 0)
            {
                return "Остальное";
            }

            return _categoryCounts
                .OrderByDescending(pair => pair.Value)
                .First()
                .Key;
        }
    }

    public int TotalFiles
    {
        get { lock (_lock) return _totalFiles; }
    }

    /// <summary>
    /// Форматирует байты в аккуратную строку (0.11 MB, 4.52 GB).
    /// </summary>
    public string GetFormattedTotalSize()
    {
        lock (_lock)
        {
            double bytes = _totalBytes;
            string[] suffixes = ["B", "KB", "MB", "GB", "TB"];
            var suffixIndex = 0;

            while (bytes >= 1024 && suffixIndex < suffixes.Length - 1)
            {
                bytes /= 1024;
                suffixIndex++;
            }

            return $"{bytes:F2} {suffixes[suffixIndex]}";
        }
    }

    /// <summary>
    /// Сбрасывает счетчики после показа отчета.
    /// </summary>
    public void Reset()
    {
        lock (_lock)
        {
            _categoryCounts.Clear();
            _totalBytes = 0;
            _totalFiles = 0;
        }
    }
}
