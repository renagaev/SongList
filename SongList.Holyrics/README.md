# Holyrics Google Drive Sync (C# + Java Helper)

Этот репозиторий содержит реализацию синхронизации Holyrics через Google Drive.
По умолчанию работает в режиме чтения, но при необходимости может обновлять
или добавлять песни через write‑путь (см. ниже).

## Общая схема
1) Использование готового токена доступа Google Drive (при необходимости refresh).
2) Листинг файлов в Google Drive `appDataFolder`, где:
   - `appProperties.folder = "sync_holyrics_files"`.
3) Для каждой партиции `music_*.db` сравнивается `custom_md5`, при отличии файл скачивается.
4) Декодирование байтов в список песен (Java helper или собственный C# парсер).

## Важные константы и структура данных
- Пространство Drive: `appDataFolder`
- Фильтр: `appProperties has { key='folder' and value='sync_holyrics_files' }`
- Префикс партиций: `K8ABZWM4ZjYnxb1f5OXYZk9x0RbiRjlKwB33QrY9_`
- Партиции: `music_0.db` ... `music_15.db` (всего 16)
- Удаленные элементы: `deleted_items.dat`
- `syncId`: hex от внутреннего id песни (`Long.toString(id, 16)` в Holyrics)
- Выбор партиции: последний hex‑символ `syncId` (0..f)
- Детект изменений: `appProperties.custom_md5` (MD5 по `id + modifiedTime`)

## Обновление/добавление песен
Песни не хранятся отдельными файлами, они лежат внутри партиций `music_*.db`.
Чтобы обновить одну песню или добавить новую:
1) скачать нужную партицию;
2) изменить `SyncBytesList`;
3) пересчитать `custom_md5`;
4) залить файл обратно.

В этом репозитории для этого есть Java helper в режиме `--apply`
и C#‑обертка `HolyricsSyncWriter`.

## Где хранится токен Holyrics
Файл токена лежит в домашней папке пользователя:
`~/.holyrics/google_drive_auth_token-<keyMD5>.dat`

`keyMD5` = MD5 от строки:
```
key + "_" + PATH_FULL
```
где `key = "sync_holyrics_files"`, а:
```
PATH_FULL = абсолютный путь рабочей директории при запуске Holyrics
```

Файл токена — Java‑serialization + шифрование:
- AES/CBC/PKCS5Padding
- IV: `AAAAAAAAAAAAAAAA`
- Key: `getPass16("&p4ssS3sp")` (SHA‑256, первые 16 hex)

## Шифрование песен
Каждый `SyncBytesItem.bytes` внутри `music_*.db` зашифрован так:
- AES/CBC/PKCS5Padding
- IV: `AAAAAAAAAAAAAAAA`
- Key: `getPass16("$%#@keyOBJ*&¨123")`

После расшифровки — Java‑serialized объект `Music`.

## Что делает C# синхронизация
`HolyricsSyncEngine`:
- Листит файлы в `appDataFolder`.
- Фильтрует `music_*.db` и `deleted_items.dat`.
- Сравнивает `custom_md5` (или `md5Checksum`).
- Скачивает только измененные файлы (в память).
- Опционально декодирует через Java helper в `HolyricsSongsPayload`.

`HolyricsSyncWriter`:
- Применяет обновления/добавления песен к партиции.
- Пересчитывает `custom_md5`.
- Загружает обновленный файл обратно в Drive.

## Состав проекта
- `tools/holyrics-sync-csharp/`
  - `HolyricsAuthClient` (refresh access‑token)
  - `HolyricsDriveClient` (Drive appDataFolder list/download)
  - `HolyricsSyncEngine` (логика custom_md5)
  - `JavaSyncHelperDecoder` (передача байтов в Java helper через stdin)
  - `JavaSyncHelperApplier` (применение обновлений в партицию)
  - `HolyricsSyncWriter` (обновление/добавление песен)
  - `HolyricsSyncState` (хранение последнего custom_md5)
- `tools/holyrics-sync-helper/`
  - `HolyricsSyncHelper` (декодирование `music_*.db` в JSON)
  - `HolyricsTokenHelper` (декодирование `google_drive_auth_token-*.dat`)

## Java helper: сборка и запуск
Сборка helper:
```bash
mkdir -p holyrics-sync-helper/build
javac -d holyrics-sync-helper/build $(rg --files JavaHelper/src)
```

Декодирование из stdin (использует C#; helper читает только stdin):
Бинарный формат:
```
int32 dataLen (big-endian)
data bytes
```
Запуск:
```bash
java -cp holyrics-sync-helper/build \
  com.holyrics.sync.HolyricsSyncHelper \
  --stdin
```

Декодирование deleted_items.dat (отдельная команда):
Формат stdin:
```
int32 dataLen (big-endian)
data bytes
```
Запуск:
```bash
java -cp holyrics-sync-helper/build \
  com.holyrics.sync.HolyricsSyncHelper \
  --deleted --stdin
```

Применение обновлений (stdin, режим `--apply`):
Формат:
```
int32 fileCount
repeat fileCount:
  int32 dataLen
  data bytes
int32 updateCount
repeat updateCount:
  uint16 syncIdLen
  syncId bytes (UTF-8, пустая строка => вычислить от id)
  int64 id
  int64 modifiedTime (0 => now)
  int32 titleLen   (-1 => без изменения)
  title bytes
  int32 artistLen  (-1 => без изменения)
  artist bytes
  int32 lyricsLen  (-1 => без изменения)
  lyrics bytes
  int32 authorLen  (-1 => без изменения)
  author bytes
  int32 noteLen    (-1 => без изменения)
  note bytes
```
Запуск:
```bash
java -cp holyrics-sync-helper/build \
  com.holyrics.sync.HolyricsSyncHelper \
  --apply --stdin
```

## C# пример (in‑memory sync)
```csharp
using HolyricsSync;

var http = new HttpClient();
// Токен берется из файла Holyrics (через Java token helper) или передается вручную
// var token = ...
// Если access_token истек, можно обновить:
// var auth = new HolyricsAuthClient(http);
// token = await auth.RefreshAsync(token.RefreshToken, ct);

var drive = new HolyricsDriveClient(http, _ => Task.FromResult(token.AccessToken));
var state = await HolyricsSyncState.LoadAsync("sync-state.json", ct);
var decoder = new JavaSyncHelperDecoder("holyrics-sync-helper/build");
var engine = new HolyricsSyncEngine(drive, state, decoder);

var result = await engine.SyncMusicAsync(ct);
await state.SaveAsync("sync-state.json", ct);
```

## C# пример (обновление/добавление песни)
```csharp
using HolyricsSync;

var http = new HttpClient();
var drive = new HolyricsDriveClient(http, _ => Task.FromResult(token.AccessToken));
var applier = new JavaSyncHelperApplier("holyrics-sync-helper/build");
var writer = new HolyricsSyncWriter(drive, applier);

var updates = new[]
{
    new HolyricsSongUpdate
    {
        Id = 0,                 // 0 => будет сгенерирован новый id
        Title = "Новая песня",
        Artist = "Исполнитель",
        Lyrics = "Текст...",
        Author = "",
        Note = ""
    }
};

await writer.ApplySongUpdatesAsync(updates, ct);
```

## Token helper
Где взять токен:
- Файл лежит в `~/.holyrics` и называется `google_drive_auth_token-<keyMD5>.dat`.
- Если не знаете `keyMD5`, возьмите самый свежий файл `google_drive_auth_token-*.dat`.

Десериализация токена через Java helper:
Сначала собери Java классы:
```bash
javac -d holyrics-sync-helper/build $(find JavaHelper/src -type f -name "*.java")
```
Запуск:
```bash
java -cp holyrics-sync-helper/build \
  com.holyrics.sync.HolyricsTokenHelper \
  --file ~/.holyrics/google_drive_auth_token-XXXX.dat
```

Если путь неизвестен, можно указать только папку:
```bash
java -cp holyrics-sync-helper/build \
  com.holyrics.sync.HolyricsTokenHelper \
  --holyrics-home ~/.holyrics
```

## Частота синхронизации (как в Holyrics)
- Первый check: примерно через 5 секунд после старта.
- Обычный polling: около 30 секунд.
- Если включен webhook‑режим, интервал может быть около 600 секунд.

## Troubleshooting
- Если токен протух, обновите его через Holyrics и заново извлеките.
- Если `appDataFolder` пустой, проверьте авторизацию и `project_name=sync`.

## IKVM / in-process Java
IKVM или embedding JVM возможны, но здесь не реализованы. Отдельный Java helper
проще и надежнее.
