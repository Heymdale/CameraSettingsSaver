# Camera Settings Saver

A utility for Windows designed to save and restore webcam settings. Supports a graphical interface, console mode, and automatic execution via a Windows Service.

Differs from most similar tools by saving settings such as "Power Line Frequency" and "Low Light Compensation".

## 🚀 Features

- **Save/Apply:** Export current camera parameters to JSON and subsequently restore them.
- **Operating Modes:**
  - **GUI:** Convenient interface for manual control.
  - **Console:** Command line support for scripts.
  - **Windows Service:** Automatic application of settings at intervals.
- **Logging:** Detailed operation logs with export capability.
- **Multilingual:** Support for Russian and English languages.

## 📖 Quick Start (GUI)

1. Launch `WebCamSettings.exe`.
2. Go to the **Cameras** tab.
3. Select the desired camera from the list. Use the **Preview** and **Settings** buttons to adjust the image.
4. Check the boxes for the configured cameras.
5. Click **Save Settings** to export parameters to `web_conf.json`.
6. Click **Apply Settings** to restore parameters from the JSON file.

## 💻 Console Mode

Run the application with command line arguments for automation or headless operation.

```
WebCamSettings.exe [PARAMS]
```

### Main Parameters

| Parameter | Description |
|----------|----------|
| `/c` | Console mode (output to console) |
| `/ni`, `/no-interface` | No interface (background mode, no UI) |
| `/p`, `/profile FILE` | Specify configuration file (default: `web_conf.json`) |
| `/l`, `/log [FILE]` | Enable logging to file |
| `/lang {ru\|en}` | Set output language |
| `/?`, `/h`, `/help` | Show help |

### Examples

```
# Console mode with custom config
WebCamSettings.exe /c /p myconfig.json

# Quiet background mode with logging
WebCamSettings.exe /ni /p settings.json /l app.log

# Console mode with Russian language
WebCamSettings.exe /c /lang ru /p settings.json
```

### Exit Codes

| Code | Description |
|-----|----------|
| `0` | Success (at least one camera configured) |
| `1` | General error |
| `2` | Configuration file not found |
| `3` | Cameras not found or not configured |

## ⚙️ Windows Service

Automate setting application by installing a Windows Service.

### Installation and Management

- **Install:** Configure parameters and click **Install Service** in the GUI.
- **Start/Stop:** Manual control of the service state.
- **Uninstall:** Remove the service from the system.
- **SC Commands:** You can also manage directly via the Windows built-in `sc` command. The service file `CameraSettingsService.exe` is located in the `bin` folder.

> ⚠️ **Administrator Rights Required:** You must run the application as Administrator to manage the service.

### Service Parameters

| Parameter | Description |
|----------|----------|
| `/profile`, `/p FILE` | Path to configuration file (default: `web_conf.json`) |
| `/lang {en\|ru}` | Logging language (default: `en`) |
| `/interval`, `/i N` | Execution interval in seconds (default: 20) |
| `/log`, `/l [FILE]` | Path to log file (default: `service.log`) |
| `/console` | Run in console mode (for debugging) |

### SC Commands (Run as Administrator)

```
sc create CameraSettingsService binPath= "C:\Program Files\CameraSettings\bin\CameraSettingsService.exe /p web_conf.json" start= auto
sc start CameraSettingsService
sc stop CameraSettingsService
sc delete CameraSettingsService
```

> 💡 **Easier:** Use the WebCamSettings application (Service Management tab) to install, start, stop, and uninstall the service via the graphical interface.

## 📁 Configuration File

- **Format:** JSON
- **Default Name:** `web_conf.json`
- **Content:** Stores camera monikers and property values (VideoProcAmp, CameraControl).

## 🛠 Troubleshooting

- **Camera not displaying?** Click **Refresh List**.
- **Settings not applying?** Some cameras do not support all properties. Check the logs.
- **Service issues?** Ensure administrator rights are present, try specifying a log file.
- **Errors?** Refer to the **Application Logs** tab or saved log files.

---

# Camera Settings Saver

Утилита для Windows, предназначенная для сохранения и восстановления настроек веб-камер. Поддерживает графический интерфейс, консольный режим и автоматическое выполнение через службу Windows.

Отличается от большинства аналогов сохранением таких настроек как "Частота линии питания" и "Компенсация низкой освещённости".

## 🚀 Возможности

- **Сохранение/Применение:** Экспорт текущих параметров камеры в JSON и их последующее восстановление.
- **Режимы работы:**
  - **GUI:** Удобный интерфейс для ручного управления.
  - **Консоль:** Поддержка командной строки для скриптов.
  - **Служба Windows:** Автоматическое применение настроек по интервалу.
- **Логирование:** Подробные логи операций с возможностью экспорта.
- **Мультиязычность:** Поддержка русского и английского языков.

## 📖 Быстрый старт (GUI)

1. Запустите `WebCamSettings.exe`.
2. Перейдите на вкладку **Камеры**.
3. Выделите нужную камеру из списка. С помощью кнопок **Превью** и **Настройки** настройте изображение.
4. Выделите галочками настроенные камеры.
5. Нажмите **Сохранить настройки**, чтобы экспортировать параметры в `web_conf.json`.
6. Нажмите **Применить настройки**, чтобы восстановить параметры из JSON-файла.

## 💻 Консольный режим

Запускайте приложение с аргументами командной строки для автоматизации или работы без интерфейса.

```
WebCamSettings.exe [ПАРАМЕТРЫ]
```

### Основные параметры

| Параметр | Описание |
|----------|----------|
| `/c` | Консольный режим (вывод в консоль) |
| `/ni`, `/no-interface` | Без интерфейса (фоновый режим, без UI) |
| `/p`, `/profile ФАЙЛ` | Указать файл конфигурации (по умолчанию: `web_conf.json`) |
| `/l`, `/log [ФАЙЛ]` | Включить логирование в файл |
| `/lang {ru\|en}` | Установить язык вывода |
| `/?`, `/h`, `/help` | Показать справку |

### Примеры

```
# Консольный режим с кастомным конфигом
WebCamSettings.exe /c /p myconfig.json

# Тихий фоновый режим с логированием
WebCamSettings.exe /ni /p settings.json /l app.log

# Консольный режим с русским языком
WebCamSettings.exe /c /lang ru /p settings.json
```

### Коды выхода

| Код | Описание |
|-----|----------|
| `0` | Успех (хотя бы одна камера настроена) |
| `1` | Общая ошибка |
| `2` | Файл конфигурации не найден |
| `3` | Камеры не найдены или не настроены |

## ⚙️ Служба Windows

Автоматизируйте применение настроек, установив службу Windows.

### Установка и управление

- **Установить:** Настройте параметры и нажмите **Установить службу** в GUI.
- **Запуск/Остановка:** Ручное управление состоянием службы.
- **Удалить:** Удаление службы из системы.
- **SC Команды:** Также можно управлять напрямую через встроенную в Windows команду `sc`, файл службы `CameraSettingsService.exe` находится в папке `bin`.

> ⚠️ **Требуются права администратора:** Для управления службой необходимо запускать приложение от имени администратора.

### Параметры службы

| Параметр | Описание |
|----------|----------|
| `/profile`, `/p ФАЙЛ` | Путь к файлу конфигурации (по умолчанию: `web_conf.json`) |
| `/lang {en\|ru}` | Язык логирования (по умолчанию: `en`) |
| `/interval`, `/i N` | Интервал выполнения в секундах (по умолчанию: 20) |
| `/log`, `/l [ФАЙЛ]` | Путь к лог-файлу (по умолчанию: `service.log`) |
| `/console` | Запуск в консольном режиме (для отладки) |

### SC Команды (Запуск от администратора)

```
sc create CameraSettingsService binPath= "C:\Program Files\CameraSettings\bin\CameraSettingsService.exe /p web_conf.json" start= auto
sc start CameraSettingsService
sc stop CameraSettingsService
sc delete CameraSettingsService
```

> 💡 **Проще:** Используйте приложение WebCamSettings (вкладка "Управление службой") для установки, запуска, остановки и удаления службы через графический интерфейс.

## 📁 Файл конфигурации

- **Формат:** JSON
- **Имя по умолчанию:** `web_conf.json`
- **Содержимое:** Хранит моникеры камер и значения свойств (VideoProcAmp, CameraControl).

## 🛠 Устранение проблем

- **Камера не отображается?** Нажмите **Обновить список**.
- **Настройки не применяются?** Некоторые камеры не поддерживают все свойства. Проверьте логи.
- **Проблемы со службой?** Убедитесь в наличии прав администратора, попробуйте указать файл для логов.
- **Ошибки?** Обратитесь к вкладке **Логи приложения** или сохранённым лог-файлам.