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

## Как собрать новый день
Дневной сценарий — только данные: строго по шагам, активна лишь цель текущего шага.
1. Ассет `DayScenario` в `Resources/Configs/Days/` (меню `Configs/Scenario/DayScenario`): номер дня,
   ключ заголовка, список `DayStep`. Добавить его в `GameConfigs._dayScenarios`; какой день играть —
   `IDayProgressService.CurrentDay` (ProjectContext, по умолчанию 1). Нет сценария на день или день завершён —
   шагов нет, все цели отпираются и работают как обычные объекты. Ошибки шагов (пустой `TargetId` у
   `Interact`/`EnterZone`, пустой `Item` у `HasItem`, пустой `Id` у `External`) — `LogError` на старте сцены.
2. `DayStep`: `Id`, `ObjectiveKey` (пусто = скрытый шаг без карточки), мысль старта и напоминания
   (`DialogueAsset` или ключ — ключ превращается в мысль через `ThoughtDialogueFactory`), `IdleSeconds`
   (0 = без напоминаний, таймер стоит при `InputBlocked`), `TargetId`, условие: `Interact` (нажал E на цели;
   запертая `Locked` дверь без предмета и отказавший переход `TransitionDoor` не засчитываются), `EnterZone`, `HasItem` (поле `Item`),
   `External` (из кода `world.CompleteScenarioStep(stepId)` — вызывать до `DayScenarioSystem` в кадре:
   из Input/Simulation или Unity-колбэка). `StartActions`/`CompleteActions` — кнопка-выпадашка у элемента
   списка: `ThoughtAction`, `ScareAction`, `SetActiveAction`, `GiveItemAction`, `TakeItemAction`, `SoundAction` (2D).
3. На объект сцены — `ScenarioTargetAuthoring { Id }`. Рядом с `InteractionObjectAuthoring` берёт его сущность
   (дверь, подбор, осмотр); без него — своя сущность, а триггер-коллайдер даёт `EnterZone` (префаб
   `Resources/Prefabs/Zones/ScenarioZone`; срабатывает и когда игрок уже стоит в зоне на старте шага).
   Цель с `ScenarioLocked` не подсвечивается, без подсказки, на E — мысль «не сейчас» (`ScenarioConfig`,
   `Resources/Configs/Scenario/`; не чаще кулдауна и не поверх играющей мысли). Заперта цель, только пока её
   `Id` встречается в текущем или будущих шагах; после — обычный объект. Объекты без authoring
   (межкомнатные двери) не затрагиваются. `_gated = false` — цель никогда не запирается (authoring только ради
   `SetActiveAction`).
4. `SetActiveAction` ищет объект по `Id` в `ScenarioTargetRegistry` (регистрация в `Awake`). Изначально
   выключенный объект сам не зарегистрируется: на активный родитель — отдельный `ScenarioTargetAuthoring`
   со своим `Id`, `_gated = false` и `_toggleTarget` = выключенный объект; дубль `Id` — предупреждение в консоли (образец `Scenario/KeysHook_Scenario` → `Pickup_Keys`
   в `Apartment`).
- ECS: компоненты `ECS/Components/Scenario/` (`ScenarioTarget`, `ScenarioActive`, `ScenarioLocked`, `ScenarioProgress`;
  однокадровые `ScenarioZoneEntered`, `ScenarioExternalComplete`, `DayFinished` — уборщики в Cleanup).
  `ScenarioGateSystem` стоит сразу после `InteractionRequestSystem` (снимает `InteractionRequest` у неактивной цели),
  `DayScenarioSystem` — после `PickupSystem`, до `ScareRequestSystem` (иначе `ScareAction` теряется).
- Отладка в редакторе: F3 — завершить текущий шаг (с `CompleteActions`), F6 — шаг в консоль.
- Образец: `Day01_Test` в `Apartment` — зона `bathroom_sink` → ключи `keys` (`HasItem Keys_Apartment`,
  появляются `SetActiveAction keys_hook`) → входная дверь `entrance_door` (скример 0.35 + мысль).

## Объекты шагов дня
Префабы — `Resources/Prefabs/DayObjects/` (`Bed`, `ToothbrushPickup`, `Sink`, `KeyHook`, `Kettle`, `Fridge`,
`Mailbox`, `Diary`, `StairsDown`), сцена `Apartment`, группа `DayObjects`. У каждого корневой `ScenarioTargetAuthoring`
(id: `bed`, `toothbrush`, `sink`, `keys`, `kettle`, `fridge`, `mailbox`, `diary`, `stairs_down`).
- Действие с затемнением: `FadeActionAuthoring` → `FadeActionTrigger` → `FadeActionSystem` вешает на игрока `FadeAction`
  + `InputBlocked`: FadeOut → пик (звук, `_requiredItem` снимается при `_consumeItem`, `_showOnPeak`/`_hideOnPeak`) → Hold →
  FadeIn → `CompleteScenarioStep(_completeStepId)`. Тайминги `<0` = из `ScreenFadeConfig`. Условие шага — `External`
  (`Interact` засчитался бы сразу при нажатии).
- Все `*System` объектов стоят в Simulation до `DayScenarioSystem` (иначе `External` теряется в Cleanup);
  `DoorInteractionSystem` перенесён туда же ради `Fridge`/`Mailbox`.
- Кровать: `BedAuthoring` (`_lieOnStart`, `_spawn`, `_look`, `_wakeStepId`, `_completeStepId` сна). Лёжа: `LyingInBed` +
  `InputBlocked` + `InteractWhenBlocked` (Interact проходит при блокировке), камеру ведёт `CameraCutsceneSystem` в `_look`;
  «встать» — отдельная сущность `BedWake` с подсказкой `ui.interact.get_up`. Поза лёжа — по `BedLook` в `Points`.
- Крючок: `KeyHookAuthoring`, видимость ключей = `_keysVisual`; один Interact — взять, если ключи висят, иначе повесить
  (`_takeStepId`/`_hangStepId`). Дочерний `KeysHookToggle` (`keys_hook`, `_gated=false`) оставляет `SetActiveAction`.
  Входная дверь `Door_Entrance` заперта `Keys_Apartment` (`_requiredItem`).
- Холодильник и почтовый ящик — наследники `DoorAuthoring` (шарнир `DoorPivot`). Холодильник: с `Groceries` в инвентаре
  первое открытие кладёт продукты (`_stockStepId`), иначе мысль «пусто». Письмо — `SetMailboxLetterAction`
  (`TargetId` + ключ мысли) в `StartActions` шага.
- Чайник: `KettleAuthoring`, индикатор — смена материала, выключается сам через `_duration`. Лестница вниз:
  `StepInteractAuthoring` (мысль + `External`). Лестница вверх — прежний `Level/StairsUp` (заперта, мысль).
- Звук: `AudioClip` в authoring, пусто = тишина (`Services/Audio/OneShotAudio`).
- Тест всех объектов: `Configs/Days/Day99_ObjectsTest` (12 шагов, в `GameConfigs`); чтобы играть — поставить ему `_day = 1`,
  а `Day01_Test` сдвинуть на другой номер.

## Поток дня и перезагрузка сцены
- Мир: `EcsStartup.Dispose()` зовёт `IEcsWorldProvider.Reset()` (уничтожает мир и обнуляет ссылку); следующий
  `World` создаёт новый. Сцену игры можно перезагружать (`SceneLoaderService`). Authoring в `OnDestroy` проверяет
  `IsAlive`, чтобы не создать мир во время выгрузки. Состояние между сценами живёт только в ProjectContext
  (`IDayProgressService`, инвентарь пересоздаётся с игроком).
- Смена дня: `DayFinished` → `DayFlowSystem` (после `DayScenarioSystem`): `DayFadeOut` → `GameConfigs.FindNextDayScenario`
  (следующий в массиве `_dayScenarios`, порядок массива = порядок дней; сейчас `Day99_ObjectsTest`, `Day01_Test`, `Day02`)
  → `CurrentDay` → перезагрузка текущей игровой сцены. Следующего нет — `EndScreenView` («Конец», «Начать сначала»
  = день 1 и перезагрузка, «Выйти»). Числа — `DayFlowConfig` (`Resources/Configs/Day/`, в `GameConfigs`).
- Заставка: `DayIntroView` (чёрная, `ui.day.title` с `{0}` = номер дня) + `DayIntroSystem`; идёт только если на день есть
  `DayScenario`; на время заставки игрок `InputBlocked` + `DayIntro`. UI — префаб `Resources/Prefabs/UI/DayFlowUI` в Canvas
  сцены (`DayIntroView`, `EndScreenView`; в `ScreenFade` — `Caption` для подписи времени). Новая игровая сцена без этого
  префаба не стартует (`PlaySceneInstaller`).
- Эпизод на чёрном: `DayStep.Episode` (`DayEpisode`: `Clips`, `Seconds`, `TimeLabel`, `GiveItem`) + `StepInteractAuthoring`
  с `_episodeSpawn` (точка лестницы вниз → `Spawn_Landing`). `StepInteractSystem` вешает `Transitioning` с `Episode`;
  `TransitionSystem`: FadeOut → Episode (звук, подпись, N секунд) → телепорт → Hold → FadeIn →
  `PendingStepComplete` (→ `External` до `DayScenarioSystem`). Звук пока заглушка: `IDayAudio` пишет `[DayAudio]` в консоль.
- Редактор: F7 — завершить день.

## Ловушки
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

## Состояние (2026-10-31)
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
лестница наверх дают мысль), ядро дневного сценария (`DayScenario` в данных, шаги по порядку, гейт
«не сейчас», тестовый `Day01_Test` в `Apartment`; смена дней и «Конец» работают; сценарий выбирается
по номеру дня, не по сцене — `Day01_Test` запустится и в `PlayRoom`).
Нет: остального HUD-текста, интерактивных пространств, постоянных источников страха кроме тестовой
зоны, логики выхода по лестнице вниз (задача 14), финальных моделей и текстов. `CameraCutsceneSystem` —
пустая заглушка.
