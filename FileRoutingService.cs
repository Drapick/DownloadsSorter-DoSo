namespace DownloadsSorter_DoSo;

public class FileRoutingService
{
    // Словарь: расширение файла -> имя подпапки
    private readonly Dictionary<string, string> _rules = new(StringComparer.OrdinalIgnoreCase)
    {
        // Документы
        { ".pdf", "Документы" },
        { ".docx", "Документы" },
        { ".doc", "Документы" },
        { ".txt", "Документы" },
        { ".xlsx", "Документы" },
        { ".pptx", "Документы" },

        // Изображения
        { ".png", "Картинки" },
        { ".jpg", "Картинки" },
        { ".jpeg", "Картинки" },
        { ".gif", "Картинки" },
        { ".webp", "Картинки" },

        // Видео
        { ".mp4", "Видео" },
        { ".mkv", "Видео" },
        { ".avi", "Видео" },

        // Архивы и установщики
        { ".zip", "Архивы" },
        { ".rar", "Архивы" },
        { ".7z", "Архивы" },
        { ".exe", "Программы" },
        { ".msi", "Программы" }
    };

    // Метод определяет папку по расширению или отправляет в "Другое"
    public string GetDestinationFolder(string extension)
    {
        if (string.IsNullOrWhiteSpace(extension))
        {
            return "Другое";
        }

        return _rules.TryGetValue(extension, out var targetFolder) 
            ? targetFolder 
            : "Другое";
    }
}