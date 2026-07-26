# 1WantToPiglet site + JournalTrace fork

## Страницы
- `/home/`
- `/filters/`
- `/forks/`
- `/otchet/`

## Сборка JournalTrace
Откройте PowerShell в корне репозитория и выполните:

```powershell
Set-ExecutionPolicy -Scope Process Bypass
.\Build-JournalTrace.ps1
```

Скрипт собирает Release и копирует файл в `downloads/JournalTrace.exe`, после чего ссылка на странице `/forks/` начинает работать.
