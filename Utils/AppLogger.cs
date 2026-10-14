using System.Text;

namespace DownloadsSorter_DoSo.Utils;

public static class AppLogger
{
    private static readonly object LockObject = new();
    private static readonly string LogFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "doso.log");

    static AppLogger()
    {
        var logDir = Path.GetDirectoryName(LogFilePath);
        if (!string.IsNullOrEmpty(logDir) && !Directory.Exists(logDir))
        {
            Directory.CreateDirectory(logDir);
        }
    }

    // Методы-обертки подсистем проекта
    public static void LogSystem(string message)  => Write("SYSTEM", ConsoleColor.DarkGray, message);
    public static void LogWatcher(string message) => Write("WATCHER", ConsoleColor.Cyan, message);
    public static void LogRouter(string message)  => Write("ROUTER", ConsoleColor.Magenta, message);
    public static void LogMover(string message)   => Write("MOVER", ConsoleColor.Blue, message);
    public static void LogSuccess(string message) => Write("SUCCESS", ConsoleColor.Green, message);
    public static void LogWarning(string message) => Write("WARN", ConsoleColor.Yellow, message);
    public static void LogError(string message, Exception? ex = null)
    {
        var formattedMessage = ex != null ? $"{message} | Детали: {ex.Message}" : message;
        Write("ERROR", ConsoleColor.Red, formattedMessage);
    }
    
    private static void Write(string tag, ConsoleColor color, string message)
    {
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        var logLine = $"[{timestamp}] [{tag}] {message}";

        // Блокировка гарантирует, что потоки FileSystemWatcher не перемешают строки
        lock (LockObject)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($"[{timestamp}] ");
            
            Console.ForegroundColor = color;
            Console.Write($"[{tag,-7}] ");
            
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(message);
            Console.ResetColor();

            // Дублирование в текстовый файл логов
            try
            {
                File.AppendAllText(LogFilePath, logLine + Environment.NewLine, Encoding.UTF8);
            }
            catch
            {
                // Игнорируем ошибку диска при записи самого лога, чтобы программа не падала
            }
        }
    }
}
