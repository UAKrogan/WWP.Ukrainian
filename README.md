# Wild West Pioneers - Ukrainian Localization

Неофіційна українська локалізація гри **Wild West Pioneers**.

## Покриття

- 39 доменів локалізації
- 7036 ключів
- 100% поточних текстових ключів гри
- українська мова додається безпосередньо до списку мов у грі
- вибрана українська мова зберігається між запусками

## Вимоги для гравців

- Wild West Pioneers у Steam
- Steam App ID: `3222640`
- Windows x64
- BepInEx 6 IL2CPP x64

### BepInEx

Сторінка збірок BepInEx:

https://builds.bepinex.dev/projects/bepinex_be

Локалізація перевірена з цією збіркою:

[BepInEx-Unity.IL2CPP-win-x64-6.0.0-be.788+5b766a3](https://builds.bepinex.dev/projects/bepinex_be/788/BepInEx-Unity.IL2CPP-win-x64-6.0.0-be.788%2B5b766a3.zip)

Рекомендується використовувати саме цю версію, оскільки робота локалізації з іншими збірками BepInEx не перевірялася.

## Встановлення

1. Завантажте перевірену версію [BepInEx 6 IL2CPP x64](https://builds.bepinex.dev/projects/bepinex_be/788/BepInEx-Unity.IL2CPP-win-x64-6.0.0-be.788%2B5b766a3.zip).
2. Розпакуйте BepInEx у кореневу папку Wild West Pioneers.
3. Один раз запустіть гру, щоб BepInEx створив необхідні файли та IL2CPP interop assemblies, після чого закрийте гру.
4. Розпакуйте релізний архів `WWP.Ukrainian_v1.0.0.zip` у кореневу папку Wild West Pioneers.
5. Запустіть гру.
6. Відкрийте налаштування мови та виберіть `Ukrainian`.

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

## Розробка

Для компіляції плагіна потрібні BepInEx та IL2CPP interop assemblies, згенеровані для Wild West Pioneers.

Ці DLL не зберігаються в репозиторії. Частина з них належить BepInEx, а частина генерується з assemblies гри. Тому перед першою збіркою розробнику потрібно один раз підготувати локальні build references.

### Підготовка середовища розробки

1. Встановіть BepInEx у Wild West Pioneers.
2. Один раз запустіть гру, щоб BepInEx згенерував папку `BepInEx/interop`.
3. Запустіть із кореня репозиторію:

```powershell
.\scripts\setup-dev.cmd -GameDir "D:\SteamLibrary\steamapps\common\Wild West Pioneers"
```

Скрипт скопіює потрібні DLL у локальну папку:

```text
lib/
```

Папка `lib/` ігнорується Git і не потрапляє до репозиторію.

Цю операцію потрібно повторити лише після оновлення гри, BepInEx або IL2CPP interop assemblies.

### Перевірка локалізації

```powershell
.\scripts\validate-localization.cmd
```

Скрипт перевіряє:

- наявність усіх 39 доменів;
- загальну кількість 7036 ключів;
- відповідність ключа домену файла;
- дублікати;
- `null`-значення;
- заборонені довгі тире.

### Збирання релізу

Після одноразового виконання `setup-dev.ps1` шлях до встановленої гри для звичайної збірки більше не потрібен.

Запустіть:

```powershell
.\scripts\build-release.cmd
```

Готовий архів буде створено тут:

```text
dist/WWP.Ukrainian_v1.0.0.zip
```

Архів містить тільки файли, необхідні користувачу:

```text
BepInEx/
└── plugins/
    └── WWP.Ukrainian/
        ├── WWP.Ukrainian.dll
        └── uk-UA/
```

## Структура репозиторію

```text
WWP.Ukrainian/
├── .github/
├── localization/
│   └── uk-UA/
├── scripts/
│   ├── build-release.cmd
│   ├── build-release.ps1
│   ├── setup-dev.cmd
│   ├── setup-dev.ps1
│   ├── validate-localization.cmd
│   └── validate-localization.ps1
├── src/
│   └── WWP.Ukrainian/
│       ├── Plugin.cs
│       └── WWP.Ukrainian.csproj
├── CHANGELOG.md
├── CONTRIBUTING.md
├── README.md
└── manifest.json
```

Локальна папка `lib/`, створена `setup-dev.ps1`, до Git не додається.

## Правові зауваження

Це неофіційна фанатська локалізація. Проєкт не пов'язаний із розробниками або видавцем Wild West Pioneers.

Репозиторій не містить DLL-файлів гри, оригінальних ресурсів гри або BepInEx.
