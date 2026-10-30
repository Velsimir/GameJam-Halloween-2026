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
- Без комментариев в коде.

## Ловушки
- `EcsStartup.Dispose()` уничтожает `EcsWorld`, а `EcsWorldProvider` — синглтон ProjectContext
  и держит ссылку на мёртвый мир. Перезагрузка/смена игровой сцены сломает ECS. Переходы между
  локациями — телепортом внутри сцены, не загрузкой сцены (или сначала чинить провайдер).
- `LoadingService` не показывает `Curtain` (закомментировано) — для затемнения звать
  `ICurtain` напрямую или вернуть вызовы.
- Play Mode с бута — меню `PlayFromBootstrap/Enebled` (`EntryPointSceneAutoLoader`).

## Поиск по коду
Через `codegraph explore` (индекс `.codegraph/`, конфиг `codegraph.json`), не Grep.
Сцены, префабы и значения SO граф не видит — Unity MCP / Read.

## Состояние (2026-10-30)
Готово: WASD + гравитация, взгляд мышью, покачивание камеры, смешивание шумов
Idle/Tense/Panic от `FearFactor`, луч взаимодействия с обводкой, дверь открыть/закрыть.
Нет: захвата курсора, блокировки ввода, источников страха, HUD-текста, инвентаря, переходов,
интерактивных пространств. `CameraCutsceneSystem` — пустая заглушка, `FovDelta` не применяется.
