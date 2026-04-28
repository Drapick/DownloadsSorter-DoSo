import shutil
import time
import random
from pathlib import Path
from plyer import notification

from win10toast import ToastNotifier
import multiprocessing


from tray_helper import resource_path
from phrases import ОТЧЕТЫ
from tray_helper import start_tray
import shared_data

# --- CONFIG (Настройки) ---
# Папка, которую бот мониторит и куда переносит файлы
SOURCE_DIR = Path("D:/_ЗАГРУЗКИ")
DEST_BASE_DIR = Path("D:/_ЗАГРУЗКИ")

toaster = ToastNotifier()

# Словарь-определитель. Ключ — имя папки, значение — список расширений.
FILE_CATEGORIES = {
    "Изображения": ['.jpg', '.jpeg', '.png', '.gif', '.bmp', '.svg', '.webp'],
    "Документы": ['.pdf', '.docx', '.doc', '.txt', '.rtf', '.xlsx', '.pptx', '.xls', '.csv'],
    "Видео": ['.mp4', '.mkv', '.avi', '.mov', '.wmv', '.flv'],
    "Аудиофайлы": ['.mp3', '.wav', '.flac', '.m4a', '.ogg', '.aac'],
    "Архивы": ['.zip', '.rar', '.7z', '.tar', '.gz'],
    "Программы": ['.exe', '.msi', '.dmg', '.iso'],
    "Остальное": []
}

# Список категорий, которые мы хотим дробить по месяцам (чтобы не копить 1000 файлов в одной папке)
DATE_SORTED_CATEGORIES = ["Изображения", "Видео", "Документы"]


def get_destination_path(file: Path, category: str) -> Path:
    """
    ШАГ 2: Определяем, куда именно положить файл.
    """
    # Начальный путь: D:/_ЗАГРУЗКИ/Images
    target_dir = DEST_BASE_DIR / category

    # Если категория в списке для сортировки по датам, добавляем папку месяца
    if category in DATE_SORTED_CATEGORIES:
        month_folder = time.strftime("%Y-%m")  # Получаем строку типа '2024-05'
        target_dir = target_dir / month_folder

    # Создаем папку, если её ещё нет (mkdir -p)
    target_dir.mkdir(parents=True, exist_ok=True)

    final_path = target_dir / file.name

    # ПРОВЕРКА НА ДУБЛИКАТ:
    # Если файл 'фото.jpg' уже существует, переименовываем в '171456789_фото.jpg'
    if final_path.exists():
        final_path = target_dir / f"{int(time.time())}_{file.name}"

    return final_path


def sort_files():
    """
    ШАГ 1: Проверяем наличие папки, сканируем и запускаем перемещение.
    """
    # ПРОВЕРКА КОРНЕВОЙ ПАПКИ:
    # Если D:/_ЗАГРУЗКИ нет, создаем её перед сканированием
    if not SOURCE_DIR.exists():
        try:
            SOURCE_DIR.mkdir(parents=True, exist_ok=True)
            print(f"[INFO] Создана отсутствующая папка: {SOURCE_DIR}")
        except Exception as e:
            print(f"[ERROR] Не удалось создать базу: {e}")
            return # Выходим, так как сканировать нечего

    moved_count = 0
    total_size = 0
    stats = {}

    # Берем всё из SOURCE_DIR, фильтруем: только файлы и игнорируем ярлыки (.lnk)
    # Это важно, чтобы бот не пытался переместить сам себя или системные ссылки
    items = [
        f for f in SOURCE_DIR.iterdir()
        if f.is_file()
           and not f.name.endswith('.lnk')
           and not f.suffix.lower() in ['.crdownload', '.part', '.tmp']
    ]

    for file in items:
        extension = file.suffix.lower()  # Берем расширение, например '.png'
        category = "Остальное"  # По умолчанию

        # Ищем категорию в словаре
        for cat, extensions in FILE_CATEGORIES.items():
            if extension in extensions:
                category = cat
                break

        # Получаем финальный путь через функцию выше
        dest_path = get_destination_path(file, category)
        file_size = file.stat().st_size

        try:
            # Перемещаем файл физически
            shutil.move(str(file), str(dest_path))

            # Накапливаем статистику для отчета
            total_size += file_size
            moved_count += 1
            stats[category] = stats.get(category, 0) + 1
            print(f"[OK] Moved {file.name} -> {category}")
        except Exception as e:
            print(f"[ERROR] Could not move {file.name}: {e}")

    # Если что-то переместили — кидаем уведомление
    if moved_count > 0:
        send_report(stats, moved_count, total_size)


def send_report(stats, count, size_bytes):
    """
    ШАГ 3: Красивый отчет пользователю.
    """
    size_mb = round(size_bytes / (1024 * 1024), 2)
    # Вычисляем, каких файлов было больше всего (для выбора фразы)
    top_category = max(stats, key=stats.get)

    # Выбираем рандомную фразу из твоего словаря
    title, message = random.choice(ОТЧЕТЫ.get(top_category, ОТЧЕТЫ["Остальное"]))

    print("ща будет картинка")

    toaster.show_toast(
        title,
        f"{message}\nПеренесено: {count} файлов ({size_mb} MB)",
        icon_path=resource_path("logo.ico"),
        duration=5,
        threaded=True  # Чтобы уведомление не вешало программу
    )

def manual_sort(icon, item):
    print("Принудительная сортировка запущена...")
    sort_files()



def main():
    """
    ТОЧКА ВХОДА
    """

    # Приветственное уведомление

    # СТАРОЕ УВЕДОМЛЕНИЕ НА КРАЙНЯК

    # notification.notify(
    #     title="DoSo Active",
    #     app_icon=resource_path("logo.ico"),
    #     message=f"Старожу {DEST_BASE_DIR}, дай бог комп не сгорит!",
    #     app_name='DoSo Sorter',
    #     timeout=5 #
    # )

    toaster.show_toast(
        title="DoSo Active",
        msg=f"Старожу {DEST_BASE_DIR}, дай бог комп не сгорит!",
        icon_path=resource_path("logo.ico"),
        duration=3,
        threaded=True
    )


    # Запуск иконки в трее (чтобы можно было выключить бота)
    start_tray("logo.png", manual_sort)


    # Бесконечный цикл с паузой в 5 минут
    while True:
        if not shared_data.is_paused:
            try:
                sort_files()
            except Exception as e:
                # Если что-то упало (например, диск D отключился), пишем в консоль
                print(f"Critical error: {e}")

        time.sleep(2)  # Спим 300 секунд


if __name__ == "__main__":
    multiprocessing.freeze_support()
    main()

# команда для создания приложения
# pyinstaller --onefile --noconsole --icon=logo.ico --add-data "logo.png;." --add-data "logo.ico;." --name="DoSo_Sorter" --hidden-import="plyer.platforms.win.notification" DoSo.py