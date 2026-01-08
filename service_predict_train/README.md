# IsServiceTrain Detection Project

Модель и пайплайн для определения, был ли показ песни на служении (`IsServiceTrain`) по логам показов слайдов.

## Данные
- `song_shows.csv`: выгрузка из `SongShows` (`id, start_at, end_at, length_sec, is_service_train`). Метка `IsServiceTrain` — целевая; строки с пустой меткой не попадают в датасет.
- `slides.csv`: выгрузка из `SlideHistory` (`slide_number, total_slides, showed_at, hidden_at, show_id`).
- Размер: ~1090 показов, доля сервисов ~0.606.

## Экспорт из Postgres
- Скрипт `export_postgres.py` пишет `song_shows.csv` и `slides.csv` в папку `service_predict_train`.
- Строка подключения берётся из `--connection`, переменных `DB_CONNECTION_STRING`/`SONGLIST_DB` или `SongList.Web/appsettings.json` (ключ `DbConnectionString`).
- Поддерживаются строки Npgsql (`Host=...;Port=...;Database=...;User Id=...;Password=...`) и стандартный libpq/URL.
- Требуется пакет `psycopg` или `psycopg2-binary` в виртуальном окружении.

## Предобработка слайдов
- Сортировка по `show_id, showed_at`.
- Дропаeм первый слайд (техэкран).
- Дубликаты `slide_number` → оставляем последний показ.
- Разрывы >10 минут делят на сегменты, берём крупнейший сегмент.
- Длительность каждого слайда клипим 600 секунд (исправление зависаний).

## Признаки (без future-инфы)
В `service_model.py`/`feature_order` (см. `onnx_meta.json`):
- Время: `dow_pg, hour, delta_to_11m, delta_to_19m, is_sunday, is_saturday`.
- Расстояния до расписания: `dist_to_service_min` (мин. до Wed 19, Fri 19, Sun 11/19), `dist_to_rehearsal_min` (мин. до Sat 20, Sun 9:30, Sun 20:30).
- Контекст показа: `gap_prev` (предыдущий конец → начало текущего), `length_sec`.
- Статы слайдов: `n_slides, unique_slides, shown_ratio=unique/total_slides, total_time, total_over_length=sum(dur)/length_sec, mean/median/std/min/max_dur, cv_dur, span, repeat_ratio, unique_ratio, first/last_dur, max_pos/_norm, long_tail_mean/ratio`.
- Future-фичи `gap_next, shows_in_day, order_in_day` убраны для realtime.
- Пост-правило: если `unique_slides<=2`, принудительно `False`.

## Модель
- `GradientBoostingClassifier` (sklearn): 100 деревьев, глубина 3, learning_rate=0.1, random_state=42.
- Порог 0.5 (после пост-правила 1–2 слайда → False).
- ONNX: `model.onnx`, вход `input` (float[*, 29]), выход `probabilities`, `label` (zipmap off). Метаданные — `onnx_meta.json` (порядок фич, пост-правило).

## Качество (последняя разметка)
- Holdout 20%, seed=42: acc ≈ 0.986, P ≈ 0.992, R ≈ 0.985, F1 ≈ 0.989, conf [[85,1],[2,130]].
- По 10 seed (0–9): acc mean ≈ 0.992, std ≈ 0.005; ошибок 0–3 на сплит.
- Частые крайние случаи: пятница/суббота с очень малым числом слайдов (FN) и пятница/суббота днём/вечером с умеренными слайдами (FP); воскресные 1–2 слайда фильтруются правилом.

## Скрипты
- `export_postgres.py`: выгрузка `song_shows.csv`/`slides.csv` из Postgres (по умолчанию из `SongList.Web/appsettings.json`).
- `service_model.py`: обучение/оценка модели, фичи, пост-правила.
  - Запуск: `.venv/bin/python service_model.py --no-cv` (по умолчанию без future-фич).
  - Артефакт: `--model-path model.joblib`.
- `export_features.py`: выгрузка `features_ml.csv` (label + фичи) для ML.NET/других фреймворков.
- `export_onnx.py`: тренирует на всех данных и экспортирует `model.onnx` + `onnx_meta.json` в `SongList.ServicePredict` (требует `skl2onnx`, `onnx`). По умолчанию target opset = 15 и IR version = 8.
- `ServiceOnnxPredictor.cs`: C# класс для инференса ONNX с полной пред-/пост-обработкой (класс `Slide`, метод `Predict(slides, gapPrevSec)`).

## Инференс (Python/C#)
- Python: можно загрузить `model.joblib` или дернуть `model.onnx` через onnxruntime, предварительно посчитав фичи как в `service_model.py`.
- C#: `ServiceOnnxPredictor` (NuGet `Microsoft.ML.OnnxRuntime`) строит фичи, вызывает ONNX, применяет правило `unique_slides<=2 -> False`. Нужен `gapPrevSec` снаружи, остальное вычисляется из массива слайдов.

## Как обновлять с новыми данными
1. Выгрузить данные: `.venv/bin/python export_postgres.py` (или с `--connection "Host=...;Port=...;Database=...;User Id=...;Password=..."`).
2. Прогнать `.venv/bin/python service_model.py --no-cv` для быстрой оценки.
3. При необходимости экспортировать `model.onnx`: `pip install skl2onnx onnx` (если не установлено) и `./.venv/bin/python export_onnx.py`.
4. Для ML.NET — `./.venv/bin/python export_features.py` → `features_ml.csv`, обучить LightGBM/LogReg внутри .NET или использовать ONNX.

## Замечания по домену
- Расписание гибкое: службы 11:00/19:00 (Sun), 19:00 (Wed/Fri), но длина 1–2.5 ч; бывают праздничные вне окон → используются мягкие дистанции, а не жёсткие правила.
- Спевки: суббота 20:00, воскресенье до 11:00 и после 19:30 — чаще не-служение.
- “Случайные включения” во время службы: мало слайдов/малое `total_over_length` → отсеиваются правилом 1–2 слайда и низкой долей.
