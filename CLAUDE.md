# GameJam Halloween 2026 — хоррор от первого лица

## Стек
- Unity 6000.3.8f1, URP 17.3, C#; платформа — ПК (клавиатура + мышь).
- Геймплей — LeoEcs Lite (`com.leopotam.ecslite`) + Zenject (Extenject).
- R3 (реактивные свойства ввода), UniTask (асинхронщина, загрузка), Cinemachine 3.1 (камера),
  Input System 1.18 (`Assets/InputActions.inputactions`, сгенерированный `Assets/InputActions.cs`).
- Текст — Typecraft (`Packages/com.velsi.typecraft`, локальный пакет) поверх TMP + TMPEffects +
  Unity Localization. Строки — `Assets/Game/Resources/Localization/Localization.csv`
  и `Resources/Configs/Typecraft/`. Сам пакет не править.
- NuGet-пакеты — `Assets/Packages/` (R3, UniTask), не править.

## Архитектура

Профиль: `LEOECS-LITE` (`~/.claude/Database/unity-architectures/LEOECS-LITE.md`) — геймплей.
Снаружи ECS — голый Zenject без фундамента `ZENJECT-MVP` (нет `MetaInstaller`, сейвов, страниц UI).
Корень кода: `Assets/Game/Scripts/`. Неймспейс: `Game.Scripts.*` по папкам.
Точка входа: `Init` → `InitSceneInstaller` → `GameBootstrapper` → `PlayRoom` →
`PlaySceneInstaller` → `EcsStartup` (Zenject `IInitializable`/`ITickable`).

Отличия от профиля:
- config: SO-конфиги приходят в системы через конструктор (`new PlayerMovementSystem(_gameConfigs.PlayerConfig)`),
  authoring тоже читает `GameConfigs` через `[Inject]`.
- lifetime: `EcsWorld` живёт в ProjectContext (`EcsWorldProvider`), системы — со сценой; см. «Ловушки».
- layer-boundary: Simulation-системы трогают Unity напрямую (`CharacterController.Move`,
  `Physics.SphereCast`, `Transform`) — так принято здесь, новым системам можно так же.
- input: `InputService` (R3 `ReactiveProperty`, проектный синглтон) → `PlayerInputSystem` пишет
  в компоненты игрока; фронт нажатия считается в системе.
- save-participant: сейвов нет.
- ui-entry: UI-системы нет — `Canvas` в сцене `PlayRoom`; мост ECS → UI делать
  Presentation-системой или view-MonoBehaviour, читающим мир/сервис.
- event-bus: MessagePipe не подключён; события — однокадровые компоненты + Cleanup-система.

## Где что лежит
- Код — `Assets/Game/Scripts/`: `ECS/{Authoring,Components/<Фича>,Systems/<Фича>}`, `Services/`,
  `Configs/` (SO), `Dependencies/` (инсталлеры), `Views/`.
- Сцены — `Assets/Game/Scenes/Init.unity` (бут), `PlayRoom.unity` (игровая).
- Конфиги — `Assets/Game/Resources/Configs/`, корневой `GameConfigs.asset` через `ConfigsInstaller`.
- Префабы — `Assets/Game/Resources/Prefabs/` (`Player`, `Loading/Curtain`).
- `_TypecraftDocs*`, `_TypecraftTmp` — служебное пакета, не трогать.
- Ретро-эффекты PSX и VHS: код `Scripts/Rendering/{PSX,VHS}/`, шейдеры `Resources/Shaders/{PSX,VHS}/`, профили
  `Resources/Configs/Rendering/` (`PSX_Default`, `VHS_Subtle`, `VHS_Heavy`); в сценах Volume `RetroFX`,
  `RetroFX_VHS`, `RetroFX_VHS_Heavy` (вес ведёт `VhsBoost`), правятся в инспекторе Volume, в т.ч. в Play Mode;
  подробности — `Docs/Retro-FX.md`, `Docs/PSX-Guide.md`.

## Конвенции
- Новая механика = компоненты-структуры в `ECS/Components/<Фича>/`, системы в
  `ECS/Systems/<Фича>/`, регистрация в `EcsStartup` в нужной стадии
  (Input → Simulation → Presentation → Debug → Cleanup).
- Одноразовые события (`InteractionRequest`) чистятся в Cleanup-стадии.
- MonoBehaviour-объекты сцены заводят сущность в `*Authoring.Start()` через `IEcsWorldProvider`
  (`[Inject]`). Интерактивный объект — наследник `InteractionObjectAuthoring`
  (`AddSpecificTag`), слой 6 выставляется автоматически.
- Настраиваемые числа — в SO-конфиг (`Configs/`), не хардкодом в системе.
- Текст для игрока — только ключом Typecraft, не строкой в коде.
- Механики с интеракцией (зоны-триггеры, интерактивные объекты) ставятся прямо в сцену `PlayRoom`
  тестовыми экземплярами, с подписью сверху (3D `TextMeshPro` над объектом): какая механика
  воспроизводится в этой зоне или у этого объекта. Подпись отладочная, не игровой текст — ключ
  Typecraft ей не нужен.
- Структура такой зоны — префаб в `Resources/Prefabs/Zones/`: корень = только логика (триггер-
  `BoxCollider` + authoring), дочерний `Visual` (куб без коллайдера) и `Label` (3D-текст). Размер
  зоны — `BoxCollider.size` корня и масштаб `Visual` вместе. Образцы: `FearZone` (серый, «Триггер
  зона страха») и `ScareZone` (красноватый, «Триггер зона скримера», `ScareZoneAuthoring` создаёт
  `ScareRequest` при каждом входе игрока), `ThoughtZone` (синеватый, «Триггер зона мысли»).
- Подпись зоны (`Label`): всегда с `LabelBillboard` (`Scripts/Views/`, разворот лицом к камере игрока,
  вертикаль мира сохраняется) и общим материалом `Resources/Materials/LabelOutline.mat` (чёрный
  контур); стиль — жирный, белый, размер 8. Новые зоны делать от этого префаба-образца.
- Мысли игрока (субтитры внизу экрана): монолог = `DialogueAsset` Typecraft (`LineNode`,
  `Advance = AfterText`, `Seconds`) в `Resources/Dialogues/Thoughts/`. Запрос из кода —
  `world.RequestThought(asset | key, interrupt)` (`ECS/Extensions/`), создаёт однокадровую
  `ThoughtRequest`; уборщик один — `ClearThoughtRequestSystem`. `ThoughtPlaybackSystem` держит
  очередь поверх `DialoguePlayer` (без `interrupt` — в очередь, с ним — обрыв и очистка очереди;
  тот же диалог, что уже играет, перезапускается). Вывод — `ThoughtPresenter` (fade) на объекте
  `Subtitles` внизу Canvas, `DialoguePlayer` биндится в `PlaySceneInstaller`. Зона —
  `ThoughtTriggerAuthoring` (диалог, «один раз», «Interrupt»). `HideAfter` presenter-а должен быть
  меньше `Seconds` в нодах на длительность fade-out.
- Страх: `FearFactor` на игроке ведут `FearZoneContactSystem` + `FearFactorSystem` (зоны
  `FearZoneAuthoring`, побеждает максимальный `Target`) и `ScareRequestSystem`. Пугать из кода —
  создать сущность с `ScareRequest { Strength }`; уборщик один — `ClearScareRequestSystem`.
  Числа — `FearConfig` (`Resources/Configs/Fear/`). В редакторе F2 = скример (дублирует `ScareZone`), F1 = блокировка ввода.
- Предметы и замки: `ItemDefinition` (SO, `Resources/Configs/Items/`) — только данные; подбор —
  `PickupAuthoring` (`Pickup { Item }`) → `PickupSystem` кладёт предмет в `Inventory` игрока (список
  создаёт `IEcsAutoReset`), просит мысль и вешает `PickedUp`; `PickupViewSystem` выключает объект и
  удаляет сущность. Проверка из любой механики — `world.HasItem(item)` (`EcsWorldInventoryExtensions`).
  Замок двери — `RequiredItem` в `DoorAuthoring` → `Locked`; без ключа мысль + `DoorRattle`
  (дёргание, числа в authoring), с ключом `Locked` снимается и дверь открывается. Образцы в `PlayRoom`:
  `KeyTable`/`Pickup_Key`, `Door_Locked`.
- Переходы между локациями: `TransitionDoorAuthoring` (`_spawnPoint` — позиция и направление взгляда,
  опционально `_requiredItem` → `Locked`, `_enterThoughtKey`) → `TransitionDoorInteractionSystem` вешает на игрока
  `Transitioning` + `InputBlocked`; `TransitionSystem` ведёт FadeOut → телепорт (CharacterController
  выключается на время переноса) → Hold → FadeIn через `ScreenFadeView` (чёрная `CanvasGroup` `ScreenFade`,
  последняя на Canvas). Тайминги — `ScreenFadeConfig` (`Resources/Configs/Transition/`). Во время перехода
  курсор остаётся захваченным (`CursorLockSystem`). Образцы в `PlayRoom`: `TransitionDoor_ToCorridor` (заперта
  ключом), `TransitionDoor_ToRoom`, коридор `Corridor` в точке (0, 0, -60).
- Блок-аут игрового уровня — сцена `Apartment.unity` (основная, дневной цикл; `PlayRoom` — тестовая).
  Иерархия: `Level/{Apartment/{Hall,Kitchen,Bathroom,Hallway},BlockCorridor,Landing}`, рядом `Lights`
  (тёплые в квартире, холодные с `LightFlicker` в коридоре и на площадке) и `Points` (пустые Transform
  для задачи 13: `Spawn_Bed`, `BedLook`, `Sink`, `ToothbrushSpot`, `KeysHook`, `Kettle`, `Fridge`,
  `Diary`, `Mailbox`, `StairsDown`, `StairsUp`, `Spawn_Landing`). Материалы — `Resources/Materials/Blockout/`
  (PSX Lit, серые оттенки по зонам). Размеры: коридор блока 34 × 4 м, квартира ≈ 8,7 × 7,5 м, потолок 2,7 м;
  проёмы под проходимые двери — 1,3 м (капсула игрока диаметром 1 м).
- Префабы-заглушки интерактива — `Resources/Prefabs/Level/` (`Door`, `NeighborDoor`, `StairsDown`,
  `StairsUp`). Правило: корень = только логика (коллайдер + authoring, `_pivot` двери = корень),
  дочерний `Visual` = кубы без логики и коллайдеров. `OutlineRefs` (`_outlineRefs`) ссылается на
  `MeshRenderer` внутри `Visual`; `Default`/`WithOutline` — материалы этого рендерера (+ `Outline.mat`).
- Замена заглушки на модель: открыть префаб → удалить дочерние кубы `Visual` (или весь `Visual`) →
  положить модель под корень с тем же именем `Visual`, сохранив нулевой локальный Transform корня и
  положение петли (у `Door` корень стоит в петле, полотно смещено по +X) → у authoring на корне
  назначить `OutlineRefs.MeshRenderer` на `MeshRenderer` модели (главная деталь, которая подсвечивается),
  `Default` = материалы этого рендерера, `WithOutline` = те же материалы + `Resources/Shaders/Outline/Materials/Outline.mat`
  → подогнать `BoxCollider` корня под модель. Размер/позицию правят на префабе, не на экземплярах в сцене.
- Новые проходимые проёмы — не уже 1,3 м. Тексты мыслей в блок-ауте помечены `TODO:` в `Localization.csv`
  (ключи `ui.thought.neighbor_door`, `stairs_up_closed`, `stairs_down`, `block_door_locked`); после
  правки CSV строки в таблицу `Main` подтягиваются через окно Typecraft (синхронизация таблицы).
- Без комментариев в коде.

## Ловушки
- `EcsStartup.Dispose()` уничтожает `EcsWorld`, а `EcsWorldProvider` — синглтон ProjectContext
  и держит ссылку на мёртвый мир. Перезагрузка/смена игровой сцены сломает ECS. Переходы между
  локациями — телепортом внутри сцены, не загрузкой сцены (или сначала чинить провайдер).
- `LoadingService` не показывает `Curtain` (закомментировано) — для затемнения звать
  `ICurtain` напрямую или вернуть вызовы.
- `TMPWriter`/`TMPAnimator`, добавленные на объект скриптом в редакторе, сами выключаются (`Reset`) и
  остаются выключенными в сохранённой сцене. Выключенный `TMPWriter` — нет печати и эффектов Typecraft
  (реплика показывается сразу). После `AddComponent` включать `enabled = true` и сохранять сцену.
- Play Mode с бута — меню `PlayFromBootstrap/Enebled` (`EntryPointSceneAutoLoader`).
  Какую сцену грузить после `Init` — `PlayFromBootstrap/Scene/{PlayRoom,ModelTest,Apartment}` (EditorPrefs, только в
  редакторе; в билде всегда `Playroom`). `ModelTest.unity` — копия `PlayRoom` без тестовых механик,
  моделек кладут под корень `Models`.

## Поиск по коду
Через `codegraph explore` (индекс `.codegraph/`, конфиг `codegraph.json`), не Grep.
Сцены, префабы и значения SO граф не видит — Unity MCP / Read.

## Состояние (2026-10-30)
Готово: WASD + гравитация, взгляд мышью, покачивание камеры, смешивание шумов
Idle/Tense/Panic от `FearFactor`, луч взаимодействия с обводкой, дверь открыть/закрыть,
захват курсора и блокировка ввода, зоны страха, скример (тряска + FOV), постобработка от страха
(виньетка, film grain, хроматическая аберрация; runtime-копия профиля `Global Volume`),
подсказка взаимодействия (`InteractionPrompt { Key }` ← поле `_promptKey` в `InteractionObjectAuthoring`,
дверь переключает `ui.interact.open/close`; `InteractionPromptView` на Canvas + `InteractionPromptViewSystem`,
тайминги — `InteractionPromptConfig`) и точка-прицел (скрыты при `InputBlocked`), мысли игрока
(субтитры, монологи на диалогах Typecraft, три тестовые зоны в `PlayRoom`).
Готово также: предметы и инвентарь (подбор, `HasItem`, замки дверей), переходы между локациями
(`TransitionDoor`, фейд), осмотр объектов (`InspectableAuthoring`), блок-аут `Apartment` (квартира,
коридор блока, площадка с лестницами-заглушками; внутренние двери открываются, соседские двери и
лестница наверх дают мысль).
Нет: остального HUD-текста, интерактивных пространств, постоянных источников страха кроме тестовой
зоны, логики выхода по лестнице вниз (задача 14), финальных моделей и текстов. `CameraCutsceneSystem` —
пустая заглушка.
