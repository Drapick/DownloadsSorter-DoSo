<img width="251" height="251" alt="logo" src="https://github.com/user-attachments/assets/14978d67-18a4-47d4-b076-44fe32992160" />

------------------------------------------
## DownloadsSorter-DoSo (v1.0.0)
## RU:
DownloadsSorter-DoSo - это сортировщик файлов с характером русского Богатыря. Он берет на себя рутину по наведению порядка в ваших скачанных файлах сортируя их по категориям и датам.

Ключевые особенности:
- Создает рабочую директорию `_DOWNLOADS` на диске D
- Все файлы, попадающие в основную папку, автоматически распределяются по категориям.
- После завершения работы DoSo присылает уведомление с кратким отчетом.
- Текст отчета адаптивен: Богатырь выделит ту категорию файлов, которой было больше всего.

Важные условия:
⚠️ Обязательная настройка: Вам необходимо изменить стандартную папку загрузок в вашем браузере на `_DOWNLOADS`.

## Архитектура
C# (.NET), разделение на независимые сервисы (Services, UI, Utils):
- Отслеживание файловой системы в реальном времени с помощью `FileSystemWatcher`.
- Фильтрация временных файлов браузера (`.tmp`, `.crdownload`) и debounce-таймер на 1.5 сек для предотвращения повторных триггеров.
- Маршрутизация файлов (`FileRoutingService`) по категориям (словари расширений для 43+ типов данных) с последующей динамической сортировкой по месяцам (`yyyy-MM`) в `DateFolderRoutingService`.
- Менеджер управления через системный трей на базе `NotifyIcon` (открытие целевой папки по клику, быстрый доступ к корзине) и механизм всплывающих уведомлений.

## Готовая сборка (Для пользователей)
Вы можете скачать утилиту в один клик:
1. Перейдите в раздел [Releases](https://github.com/Drapick/DownloadsSorter-DoSo/releases).
2. Скачайте `DownloadsSorter-DoSo.exe`
3. Запустите. Приложение свернется в системный трей.

---
## EN:
DownloadsSorter-DoSo is a file organizer with the spirit of a legendary Russian Bogatyr (epic warrior). It takes full charge of the routine chore of tidying up your downloaded files, sorting them neatly by categories and dates.

Key Features:
- Creates a dedicated `_DOWNLOADS` working directory on your D: drive.
- All files entering the main folder are automatically distributed into their respective categories.
- Once the sorting is complete, DoSo sends a desktop notification with a brief report.
- Dynamic report text: The Bogatyr will dynamically highlight the file category that made up the bulk of the download session.

Important Note:
⚠️ Required Setup: You must change the default download folder in your web browser settings to point to `_DOWNLOADS`.

## Architecture
C# (.NET), with clean separation into decoupled layers (Services, UI, Utils):
- Real-time file system tracking utilizing `FileSystemWatcher`.
- Native filtering for temporary browser files (`.tmp`, `.crdownload`) combined with a 1.5-second debounce timer to prevent duplicate triggers.
- Advanced file routing (`FileRoutingService`) by categories using extension dictionaries for 43+ data types, followed by dynamic chronological sorting (`yyyy-MM`) inside `DateFolderRoutingService`.
- System tray management layer built on `NotifyIcon` (click to open the target folder, quick access to the recycle bin) paired with a custom desktop notification engine.

## Production Build (For Users)
You can download the utility in just one click:
1. Go to the [Releases](https://github.com/Drapick/DownloadsSorter-DoSo/releases) section.
2. Download `DownloadsSorter-DoSo.exe`.
3. Launch it. The application will immediately minimize to your system tray.
