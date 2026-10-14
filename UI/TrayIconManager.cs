using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using DownloadsSorter_DoSo.Utils;

namespace DownloadsSorter_DoSo;

public class TrayIconManager : IDisposable
{
    private readonly string _rootDirectory;
    private readonly NotifyIcon _notifyIcon;
    private readonly ContextMenuStrip _contextMenu;
    private CancellationTokenSource? _dismissCts;
    private readonly object _lock = new();

    public event Action? OnExitRequested;

    public TrayIconManager(string rootDirectory, string iconPath)
    {
        _rootDirectory = rootDirectory;

        _contextMenu = new ContextMenuStrip();

        var rootFolderItem = new ToolStripMenuItem("Корневая папка", null, (_, _) => OpenFolder(_rootDirectory))
        {
            Font = new Font(_contextMenu.Font, FontStyle.Bold)
        };

        var trashItem = new ToolStripMenuItem("Корзина", null, (_, _) => OpenTrash());
        var separator = new ToolStripSeparator();
        var exitItem = new ToolStripMenuItem("Выход", null, (_, _) => ExitApplication());

        _contextMenu.Items.Add(rootFolderItem);
        _contextMenu.Items.Add(trashItem);
        _contextMenu.Items.Add(separator);
        _contextMenu.Items.Add(exitItem);

        _notifyIcon = new NotifyIcon
        {
            Text = "DownloadsSorter DoSo",
            ContextMenuStrip = _contextMenu,
            Visible = true
        };

        if (File.Exists(iconPath))
        {
            _notifyIcon.Icon = new Icon(iconPath);
        }
        else
        {
            _notifyIcon.Icon = SystemIcons.Application;
            AppLogger.LogWarning($"Файл иконки не найден: {iconPath}. Применена системная иконка.");
        }

        _notifyIcon.MouseClick += (_, e) =>
        {
            if (e.Button == MouseButtons.Left)
            {
                OpenFolder(_rootDirectory);
            }
        };
    }

    /// <summary>
    /// Показывает нативное уведомление на 5 секунд и принудительно закрывает его, не засоряя Центр уведомлений.
    /// </summary>
    public void ShowNotification(string title, string message, ToolTipIcon icon = ToolTipIcon.None, int durationMilliseconds = 5000)
    {
        lock (_lock)
        {
            // Отменяем предыдущий таймер автозакрытия, если уведомления пришли подряд
            _dismissCts?.Cancel();
            _dismissCts?.Dispose();
            _dismissCts = new CancellationTokenSource();
            var token = _dismissCts.Token;
            
            _notifyIcon.ShowBalloonTip(durationMilliseconds, title, message, icon);

            // Асинхронно через 5 секунд гасим уведомление в ОС
            Task.Delay(durationMilliseconds, token).ContinueWith(_ =>
            {
                if (!token.IsCancellationRequested)
                {
                    DismissBalloonTip();
                }
            }, TaskScheduler.Default);
        }
    }

    /// <summary>
    /// Принудительно отзывает текущий BalloonTip из оболочки Windows
    /// </summary>
    private void DismissBalloonTip()
    {
        try
        {
            // Сброс подсказки пустой строкой сбрасывает активный тост в оболочке проводника
            _notifyIcon.BalloonTipText = string.Empty;
            _notifyIcon.BalloonTipTitle = string.Empty;
            
            // Быстрое переключение видимости гарантирует, что тост удалится из Action Center
            _notifyIcon.Visible = false;
            _notifyIcon.Visible = true;
        }
        catch
        {
            // Игнорируем возможные ошибки при закрытии программы
        }
    }

    private static void OpenFolder(string path)
    {
        try
        {
            if (Directory.Exists(path))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = path,
                    UseShellExecute = true
                });
            }
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"Не удалось открыть папку: {path}", ex);
        }
    }

    private static void OpenTrash()
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = "shell:RecycleBinFolder",
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            AppLogger.LogError("Не удалось открыть Корзину", ex);
        }
    }

    private void ExitApplication()
    {
        AppLogger.LogSystem("Получен сигнал завершения от пользователя из трея.");
        OnExitRequested?.Invoke();
        Application.Exit();
    }

    public void Dispose()
    {
        lock (_lock)
        {
            _dismissCts?.Cancel();
            _dismissCts?.Dispose();
        }

        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
        _contextMenu.Dispose();
        GC.SuppressFinalize(this);
    }
}
