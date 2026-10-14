namespace DownloadsSorter_DoSo;

public class DateFolderRoutingService
{
    // Категории, внутри которых имеет смысл вести хронологию
    private readonly HashSet<string> _categoriesWithDateSubfolders = new(StringComparer.OrdinalIgnoreCase)
    {
        "Документы",
        "Изображения",
        "Видео"
    };

    /// <summary>
    /// Вычисляет путь к подпапке с учетом даты (месяц и день).
    /// Если для категории сортировка по дате отключена, возвращает базовую папку категории.
    /// </summary>
    public string GetDatedDestinationDirectory(string baseCategoryDirectory, string categoryName, DateTime? fileDate = null)
    {
        if (!_categoriesWithDateSubfolders.Contains(categoryName))
        {
            return baseCategoryDirectory;
        }

        var date = fileDate ?? DateTime.Now;

        // Формируем структуру: Категория -> 2026-10
        var monthFolder = date.ToString("yyyy-MM");

        return Path.Combine(baseCategoryDirectory, monthFolder);
    }
}