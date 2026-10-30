# Ретро-эффекты: PSX и VHS

Два независимых эффекта после стандартной постобработки URP. Порядок: сначала PSX, потом VHS. Работают только в Game view. Включаются в `PC_Renderer` (фичи `PSX` и `VHS`).

## Где лежит

| Что | Путь |
|---|---|
| Код PSX | `Assets/Game/Scripts/Rendering/PSX/` |
| Код VHS | `Assets/Game/Scripts/Rendering/VHS/` |
| Шейдеры | `Assets/Game/Resources/Shaders/PSX/`, `Assets/Game/Resources/Shaders/VHS/` |
| Профили | `Assets/Game/Resources/Configs/Rendering/` (`PSX_Default`, `VHS_Subtle`, `VHS_Heavy`) |
| Volume в сценах | `RetroFX` (PSX), `RetroFX_VHS` (Subtle, priority 11), `RetroFX_VHS_Heavy` (Heavy, priority 12, weight 0) |

Сцены: `PlayRoom` и `ModelTest`.

## Как настраивать

1. Выберите Volume в Hierarchy (например `RetroFX_VHS`).
2. В Inspector нажмите на профиль (поле `Profile`), раскройте компонент `VHS` или `PSX`.
3. Двигайте параметры. В Play Mode изменения видны сразу и сохраняются в ассете профиля.
4. Выключатель: снимите галочку с override у нужного параметра (вернётся значение по умолчанию) или с самого Volume. `Intensity = 0` отключает проход полностью.

Чтобы выключить весь эффект: снимите галочку с Volume или поставьте `Intensity` 0.

## Параметры PSX (Add Override → Retro → PSX)

| Параметр | Что делает | Диапазон |
|---|---|---|
| Intensity | Мастер: 0 — проход не выполняется; масштабирует дизеринг и туман | 0–1 |
| Vertical Resolution | Внутреннее разрешение по вертикали, пропорции сохраняются, апскейл point | 32–1080 |
| Color Levels | Число уровней на канал | 2–256 |
| Dither Strength | Сила Bayer 4x4 | 0–1 |
| Dither Scale | Размер ячейки дизеринга в пикселях низкого разрешения | 1–8 |
| Fog Enabled | Туман по глубине | вкл/выкл |
| Fog Color | Цвет тумана (небо тоже уходит в него) | цвет |
| Fog Start / Fog End | Начало и конец тумана в метрах | ≥ 0 |

## Параметры VHS (Add Override → Retro → VHS)

Intensity масштабирует все группы.

| Группа | Параметр | Что делает | Диапазон |
|---|---|---|---|
| Общее | Intensity | Мастер, 0 — проход не выполняется | 0–1 |
| Цвет | Channel Shift | Расхождение R и B по горизонтали, px | 0–8 |
| Цвет | Chroma Bleed | Сила растёкшейся цветной полосы | 0–1 |
| Цвет | Chroma Bleed Width | Ширина растекания, px | 0–40 |
| Цвет | Saturation | Насыщенность | 0–2 |
| Цвет | Warm Shift | Тёплый (+) или холодный (−) сдвиг | −1…1 |
| Цвет | Contrast | Контраст | 0.5–1.5 |
| Строки | Scanline Strength | Сила scanlines | 0–1 |
| Строки | Scanline Density | Число линий по высоте | 60–1080 |
| Строки | Jitter Strength | Горизонтальная дрожь строк, px | 0–6 |
| Строки | Jitter Frequency | Частота смены дрожи, раз/с | 0–60 |
| Трекинг | Tracking Strength | Сила катящейся полосы помех | 0–1 |
| Трекинг | Tracking Speed | Скорость полосы, экранов/с | 0–1 |
| Трекинг | Tracking Height | Высота полосы (доля экрана) | 0.01–0.5 |
| Трекинг | Tracking Noise | Шум внутри полосы | 0–1 |
| Трекинг | Vertical Hold Chance | Вероятность срыва кадра за каждые 0.5 с | 0–1 |
| Трекинг | Vertical Hold Strength | Величина вертикального сдвига | 0–1 |
| Шум | Grain Strength | Зерно плёнки | 0–1 |
| Шум | Snow Strength | «Снег» (яркие точки) | 0–1 |
| Шум | Noise Size | Размер зерна, px | 1–8 |
| Шум | Noise Speed | Скорость смены шума | 0–4 |
| Шум | Dropout Chance | Белые выпадения-полоски | 0–1 |
| Экран | Curvature | Бочкообразная кривизна | 0–1 |
| Экран | Vignette | Тёмные края | 0–1 |
| Экран | Horizontal Blur | Мягкость по горизонтали, px | 0–4 |

Время идёт от `Time.time`, не зависит от FPS.

## Пресеты VHS

- `VHS_Subtle` — фон всей игры (Volume `RetroFX_VHS`, weight 1).
- `VHS_Heavy` — момент страха или скример (Volume `RetroFX_VHS_Heavy`, weight 0). Его вес поднимает `VhsBoost`, и помехи плавно переходят в сильные.

## API для геймплея: `VhsBoost`

Компонент на объекте `RetroFX_VHS_Heavy`. Интерфейс `IVhsBoost` (`Game.Scripts.Rendering.VHS`):

| Метод | Что делает |
|---|---|
| `Pulse(peak, attack, hold, release)` | Вес поднимается до `peak` за `attack` секунд, держится `hold`, спадает за `release`. Для скримера |
| `SetTarget(weight, smoothTime)` | Плавно ведёт вес к `weight` и держит. Для зон страха |
| `Clear()` | Мгновенно сбросить в 0 |
| `Weight` | Текущий вес |

Использует `Time.unscaledDeltaTime`. Если действуют и пульс, и цель, берётся большее значение.

### Как подключить к FearFactorSystem или ScareRequestSystem (пока НЕ подключено)

1. Получить `IVhsBoost`. Самый простой путь: в `PlaySceneInstaller` добавить
   `Container.Bind<IVhsBoost>().FromComponentInHierarchy().AsSingle();`
   и принять `IVhsBoost` в конструктор системы при создании в `EcsStartup`.
2. `ScareRequestSystem`: там, где обрабатывается `ScareRequest`, вызвать `_vhsBoost.Pulse(strengthNormalized, 0.05f, 0.3f, 1.2f)`. Для силы удара пригодится поле `Strength` из запроса.
3. `FearFactorSystem`: каждый кадр вызывать `_vhsBoost.SetTarget(fearNormalized, 0.3f)`, где `fearNormalized` — значение `FearFactor` в диапазоне 0–1.
4. Если боитесь, что Volume не найдётся, подгружайте `IVhsBoost` через `FindFirstObjectByType<VhsBoost>()` как запасной вариант.

Все значения времени в примерах — стартовые, подбираются на глаз.
