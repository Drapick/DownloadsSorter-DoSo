namespace DownloadsSorter_DoSo;

public class FileRoutingService
{
    private const string DefaultCategory = "Остальное";
    // Словарь: расширение файла -> имя подпапки
    private readonly Dictionary<string, string> _rules = new(StringComparer.OrdinalIgnoreCase)
    {
        // Изображения
        { ".jpg", "Изображения" },
        { ".jpeg", "Изображения" },
        { ".png", "Изображения" },
        { ".gif", "Изображения" },
        { ".bmp", "Изображения" },
        { ".svg", "Изображения" },
        { ".webp", "Изображения" },

        // Документы
        { ".pdf", "Документы" },
        { ".docx", "Документы" },
        { ".doc", "Документы" },
        { ".txt", "Документы" },
        { ".rtf", "Документы" },
        { ".xlsx", "Документы" },
        { ".pptx", "Документы" },
        { ".xls", "Документы" },
        { ".csv", "Документы" },
        { ".odt", "Документы" },
        { ".ods", "Документы" },
        { ".odp", "Документы" },
        { ".odg", "Документы" },
        { ".odf", "Документы" },
        { ".odb", "Документы" },
        { ".ott", "Документы" },
        { ".ots", "Документы" },
        { ".otp", "Документы" },
        { ".otg", "Документы" },
        { ".odm", "Документы" },
        { ".oth", "Документы" },

        // Видео
        { ".mp4", "Видео" },
        { ".mkv", "Видео" },
        { ".avi", "Видео" },
        { ".mov", "Видео" },
        { ".wmv", "Видео" },
        { ".flv", "Видео" },

        // Аудиофайлы
        { ".mp3", "Аудиофайлы" },
        { ".wav", "Аудиофайлы" },
        { ".flac", "Аудиофайлы" },
        { ".m4a", "Аудиофайлы" },
        { ".ogg", "Аудиофайлы" },
        { ".aac", "Аудиофайлы" },

        // Архивы
        { ".zip", "Архивы" },
        { ".rar", "Архивы" },
        { ".7z", "Архивы" },
        { ".tar", "Архивы" },
        { ".gz", "Архивы" },

        // Программы и образы
        { ".exe", "Программы" },
        { ".msi", "Программы" },
        { ".dmg", "Программы" },
        { ".iso", "Программы" }
    };

    // Метод определяет папку по расширению или отправляет в "Другое"
    public string GetDestinationFolder(string extension)
    {
        if (string.IsNullOrWhiteSpace(extension)) return DefaultCategory;

        return _rules.TryGetValue(extension, out var targetFolder) ? targetFolder : DefaultCategory;
    }
}