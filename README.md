# Wild West Pioneers - Ukrainian Localization

Неофіційна повна українська локалізація гри **Wild West Pioneers**.

## Покриття

- 39 доменів локалізації
- 7036 ключів
- 100% поточних текстових ключів гри
- українська мова додається безпосередньо до списку мов у грі
- вибрана українська мова зберігається між запусками

## Вимоги

- Wild West Pioneers у Steam
- Steam App ID: `3222640`
- Windows x64
- BepInEx 6 IL2CPP x64

Перевірена збірка BepInEx:

`BepInEx-Unity.IL2CPP-win-x64-6.0.0-be.788+5b766a3`

## Встановлення

1. Встановіть BepInEx 6 IL2CPP x64 у кореневу папку гри.
2. Один раз запустіть гру, щоб BepInEx створив необхідні каталоги, після чого закрийте гру.
3. Розпакуйте релізний архів `WWP.Ukrainian_v1.0.0.zip` у кореневу папку Wild West Pioneers.
4. Запустіть гру.
5. Відкрийте налаштування мови та виберіть `Ukrainian`.

Після встановлення:

```text
Wild West Pioneers/
└── BepInEx/
    └── plugins/
        └── WWP.Ukrainian/
            ├── WWP.Ukrainian.dll
            └── uk-UA/
                ├── Achievement.json
                ├── ...
                └── UI_Satisfaction.json
```

## Оновлення

Замініть папку:

```text
BepInEx/plugins/WWP.Ukrainian
```

новою версією мода.

Конфіг користувача:

```text
BepInEx/config/games.trembita.wwp.ukrainian.cfg
```

до релізного архіву не входить і під час оновлення не перезаписується.

## Видалення

Видаліть:

```text
BepInEx/plugins/WWP.Ukrainian
```

За бажанням також можна видалити конфіг:

```text
BepInEx/config/games.trembita.wwp.ukrainian.cfg
```

## Діагностика

Якщо локалізація не працює, відкрийте:

```text
BepInEx/LogOutput.log
```

і знайдіть рядки з `WWP Ukrainian` або `Ukrainian`.

Плагін логує версію мода, версію гри, кількість знайдених JSON-файлів, кількість ключів і відсоток збігу з термінами гри.

Нові ключі після оновлення гри не блокують роботу мода: відсутні переклади залишаються мовою оригіналу, а покриття видно в логах.

## Структура репозиторію

```text
WWP.Ukrainian/
├── .github/
├── localization/
│   └── uk-UA/
├── scripts/
├── src/
│   └── WWP.Ukrainian/
├── CHANGELOG.md
├── CONTRIBUTING.md
├── README.md
└── manifest.json
```

## Збирання

Запустіть із кореня репозиторію:

```powershell
.\scripts\build-release.ps1
```

Якщо гра встановлена в іншому каталозі:

```powershell
.\scripts\build-release.ps1 -GameDir "E:\SteamLibrary\steamapps\common\Wild West Pioneers"
```

Можна також задати змінну середовища:

```powershell
$env:WWP_GAME_DIR = "E:\SteamLibrary\steamapps\common\Wild West Pioneers"
```

## Перевірка локалізації

```powershell
.\scripts\validate-localization.ps1
```

Скрипт перевіряє кількість файлів і ключів, відповідність доменів, дублікати, `null`-значення та заборонені довгі тире.

## Правові зауваження

Це неофіційна фанатська локалізація. Проєкт не пов'язаний із розробниками або видавцем Wild West Pioneers.

Репозиторій не містить DLL-файлів гри, оригінальних ресурсів гри або BepInEx.
