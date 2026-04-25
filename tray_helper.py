import pystray
from PIL import Image
import threading
import os
import sys
import shared_data
from plyer import notification
import time

auto_resume_timer = None


def resource_path(relative_path):
    """ Получает путь к файлу, который 'зашит' внутри EXE """
    try:
        # PyInstaller создает временную папку и хранит путь в _MEIPASS
        base_path = sys._MEIPASS
    except Exception:
        # Если запускаем просто как .py скрипт
        base_path = os.path.abspath(".")

    return os.path.join(base_path, relative_path)


def start_tray(icon_name="logo.png", sort_funk=None):
    def on_quit(icon, item):
        icon.stop()
        # Жесткое завершение всех процессов скрипта
        os._exit(0)

    def on_folder(icon, item):
        path = rf'D:/_ЗАГРУЗКИ/{item.text}'
        if os.path.exists(path):
            os.startfile(path)
        else:
            print(f"Папка не найдена: {path}")

    def wake_up(icon):
        shared_data.is_paused = False
        icon.update_menu()
        notification.notify(
            title="DoSo Active",
            app_icon=resource_path("logo.ico"),
            message="Я проснулся спустя 30 минут и снова за работой!",
            app_name='DoSo Sorter',
            timeout=5
        )

    def on_paused(icon, item):
        global auto_resume_timer
        shared_data.is_paused = not shared_data.is_paused
        print(f"Пауза: {shared_data.is_paused}")

        # Отменяем старый таймер, если он тикал
        if auto_resume_timer:
            auto_resume_timer.cancel()

        if shared_data.is_paused:
            # Запускаем новый на 1800 секунд (30 мин)
            auto_resume_timer = threading.Timer(1800, wake_up, args=(icon,))
            auto_resume_timer.start()
    try:
        # Находим реальный путь к иконке внутри EXE
        full_icon_path = resource_path(icon_name)
        image = Image.open(full_icon_path)

        icon = pystray.Icon(
            "DoSo_Sorter_Icon",
            image,
            "DoSo Sorter охраняет тебя",
            # Создаем меню со списком кнопок
            menu=pystray.Menu(
                pystray.MenuItem("Открыть категорию", pystray.Menu(
                    pystray.MenuItem("Остальное", on_folder),
                    pystray.MenuItem("Программы", on_folder),
                    pystray.MenuItem("Архивы", on_folder),
                    pystray.MenuItem("Аудиофайлы", on_folder),
                    pystray.MenuItem("Видео", on_folder),
                    pystray.MenuItem("Документы", on_folder),
                    pystray.MenuItem("Изображения", on_folder)
                )),
                pystray.Menu.SEPARATOR,

                pystray.MenuItem("Корневая папка", lambda: os.startfile(r'D:/_ЗАГРУЗКИ'), default=True),
                pystray.MenuItem("Сортировать сейчас", sort_funk),
                pystray.MenuItem("Пауза", on_paused, checked=lambda item: shared_data.is_paused),
                pystray.Menu.SEPARATOR,

                pystray.MenuItem("Выход", on_quit)
            )
        )

        # Запускаем в отдельном потоке (daemon=True, чтобы не вис при закрытии)
        threading.Thread(target=icon.run, daemon=True).start()

    except Exception as e:
        # Если иконка не загрузится, скрипт хотя бы не вылетит с ошибкой
        print(f"Ошибка загрузки иконки: {e}")

