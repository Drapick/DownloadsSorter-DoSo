import pystray
from PIL import Image
import threading
import os
import sys


def resource_path(relative_path):
    """ Получает путь к файлу, который 'зашит' внутри EXE """
    try:
        # PyInstaller создает временную папку и хранит путь в _MEIPASS
        base_path = sys._MEIPASS
    except Exception:
        # Если запускаем просто как .py скрипт
        base_path = os.path.abspath(".")

    return os.path.join(base_path, relative_path)


def start_tray(icon_name="logo.png"):
    def on_quit(icon, item):
        icon.stop()
        # Жесткое завершение всех процессов скрипта
        os._exit(0)

    try:
        # Находим реальный путь к иконке внутри EXE
        full_icon_path = resource_path(icon_name)
        image = Image.open(full_icon_path)

        icon = pystray.Icon(
            "DoSo_Sorter_Icon",
            image,
            "DoSo Sorter работает",
            menu=pystray.Menu(pystray.MenuItem("Выход", on_quit))
        )

        # Запускаем в отдельном потоке (daemon=True, чтобы не вис при закрытии)
        threading.Thread(target=icon.run, daemon=True).start()

    except Exception as e:
        # Если иконка не загрузится, скрипт хотя бы не вылетит с ошибкой
        print(f"Ошибка загрузки иконки: {e}")

