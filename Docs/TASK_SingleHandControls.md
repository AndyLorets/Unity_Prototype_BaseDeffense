# Задача: Переход с двух джойстиков на один тач + 2 кнопки спец-атак

## Контекст проекта

**BaseDeffense** — Unity 6 (6000.4.4f1) tower defense на мобильных. Игрок защищает базу от волн цветных врагов. Архитектура подробно описана в [CLAUDE.md](../CLAUDE.md) — обязательно прочитай его до начала работы.

**Текущее состояние (v2 Dual Joystick):**
- Два джойстика на Canvas (левый и правый), каждый — статичный, в своём нижнем углу.
- Каждый джойстик через [DualJoystickController.cs](../Assets/Scripts/DualJoystickController.cs) управляет своим [CrosshairController.cs](../Assets/Scripts/CrosshairController.cs) (два прицела в 3D).
- Две пушки [WeaponTurel.cs](../Assets/Scripts/WeaponTurel.cs) симметрично у базы, каждая привязана к своему прицелу, стреляет автоматически (`ShootMode.Auto`).
- Урон от пушек зависит от совпадения индекса/цвета оружия и врага (см. [IndexInfo.cs](../Assets/Scripts/IndexInfo.cs)).
- Два режима ввода (`InputMode` в инспекторе): `Delta` (`VariableJoystick`) и `Velocity` (`FixedJoystick`).

**Цель:** убрать левый джойстик и оставить **один тач/джойстик под правую руку** для прицела, а под левую руку поставить **две UI-кнопки спец-атак**. Хочется, чтобы в инспекторе можно было переключить, какая рука управляет прицелом (правая по умолчанию) — потом из этого сделаем настройку игрока.

## Что нужно сделать

### 1. Управление прицелом — одна рука (правая по умолчанию)

- В [DualJoystickController.cs](../Assets/Scripts/DualJoystickController.cs):
  - Добавить enum `HandSide { Right, Left }` и сериализованное поле `_handSide` (по умолчанию `Right`).
  - Оставить **только один** джойстик и одну ссылку на `CrosshairController` (правую). Левый джойстик/прицел больше не нужны.
  - Зона касания — половина экрана со стороны `_handSide`. Касания в противоположной половине игнорируются (там будут кнопки).
  - Логика `Delta` и `Velocity` сохраняется без изменений, просто работает с одним прицелом.
- В сцене [GameScene.unity](../Assets/Scenes/GameScene.unity) **удалить** левый прицел, левую пушку и левый джойстик. Остаётся только правый стек (прицел + автопушка + джойстик справа).
- Правая пушка продолжает стрелять автоматически, как только `CrosshairController.CurrentTarget != null` — цветная механика урона не меняется.

### 2. Реестр активных врагов

Спец-атакам нужно искать ближайшего к базе врага и врагов в радиусе вокруг курсора. Добавить в [EnemySpawnManager.cs](../Assets/Scripts/EnemySpawnManager.cs):

- Синглтон `public static EnemySpawnManager Instance { get; private set; }` (init в `Awake`).
- Список активных врагов и методы:
  - `RegisterActive(EnemyShipBase enemy)` / `UnregisterActive(EnemyShipBase enemy)`.
  - `EnemyShipBase FindNearestToPosition(Vector3 worldPos)` — линейный перебор по списку.
  - `void FindEnemiesInRadius(Vector3 worldPos, float radius, List<EnemyShipBase> output)` — заполняет переданный буфер, без аллокаций.
- В [EnemyShipBase.cs](../Assets/Scripts/Enemy/EnemyShipBase.cs):
  - `OnEnable` → `EnemySpawnManager.Instance.RegisterActive(this)`.
  - `OnDisable` → `UnregisterActive(this)`.
  - Добавить `public float CurrentHp { get; }`, `public bool IsDead { get; }`, `public const float MaxHp = ...` (или сериализованное `_maxHp`).
  - Метод `TakeDamagePercent(float percent01)` — отнимает `percent01 * MaxHp` от текущего HP **без учёта цвета/индекса**.
  - В общем `ApplyDamage` ставить `_isDead = true` **сразу** при достижении 0 HP, до вызова `Death()` — защита от двойного срабатывания смерти при splash-уроне.

### 3. Компонент `WeaponSpecial`

Новый скрипт `Assets/Scripts/WeaponSpecial.cs` на игроке/корне пушки. Поля в инспекторе:

- `Transform _basePoint` — базовая точка (Transform базы / Player root).
- `CrosshairController _cursorCrosshair` — правый прицел.
- `float _nearestDamagePercent = 0.2f`, `float _nearestCooldown = 1f`.
- `float _splashRadius` — радиус splash (≈ длина корабля), настраивается.
- `float _directHitRadius` — радиус прямого попадания (меньше splash).
- `float _radiusFullDamagePercent = 1f`, `float _radiusSplashDamagePercent = 0.5f`, `float _radiusCooldown = 5f`.
- Два независимых таймера cooldown (отдельные поля), чтобы кнопки не блокировали друг друга.

Публичный API:

- `bool TryFireNearestToBase()` — если cd готов: находит `FindNearestToPosition(_basePoint.position)`, вызывает `TakeDamagePercent(_nearestDamagePercent)`, сбрасывает cd. Возвращает `true`.
- `bool TryFireAtCursorRadius()` — если cd готов: берёт `_cursorCrosshair.transform.position`, через `FindEnemiesInRadius` собирает врагов в `_splashRadius`. Для каждого:
  - расстояние ≤ `_directHitRadius` → `TakeDamagePercent(_radiusFullDamagePercent)` (100%);
  - иначе → `TakeDamagePercent(_radiusSplashDamagePercent)` (50%).
- `float GetCooldownRemaining(AttackKind kind)` и/или `bool IsReady(AttackKind kind)` — для UI.

Где `enum AttackKind { NearestToBase, CursorRadius }`.

### 4. UI-компонент `SpecialAttackButtonUI`

Новый скрипт `Assets/Scripts/UI/SpecialAttackButtonUI.cs`, висит на UI-кнопке:

- `[RequireComponent(typeof(Button))]`. В `Awake` добавить `CanvasGroup`, если его нет.
- Поля: `WeaponSpecial _weapon`, `AttackKind _kind`.
- `OnClick` → вызывает соответствующий `TryFire...` у `_weapon`.
- В `Update` опрашивает `_weapon.IsReady(_kind)`:
  - `_button.interactable = isReady`;
  - `_canvasGroup.alpha = isReady ? 1f : 0.5f`.
- Без прогресс-бара / radial fill — только alpha + interactable.

### 5. Сцена

- Удалить левый джойстик / левый прицел / левую пушку.
- Повесить `WeaponSpecial` на игрока (или правую пушку): в `_cursorCrosshair` — правый `CrosshairController`, в `_basePoint` — Transform базы.
- Создать две UI-кнопки в нижнем левом углу Canvas, на каждую повесить `SpecialAttackButtonUI` со ссылкой на общий `WeaponSpecial` и нужным `_kind`:
  - Кнопка 1: `NearestToBase` — −20% HP, cd 1 c.
  - Кнопка 2: `CursorRadius` — 100% / 50% HP по радиусу, cd 5 c.

## Расположение элементов на экране

```
Вид экрана (ландшафт):
  [  Игровая сцена (камера)                                ]
  [                                                        ]
  [ [Btn1] [Btn2]                       Правая пушка       ]
  [                                       [Joystick]       ]
```

- Кнопки спец-атаки слева снизу.
- Джойстик/тач-зона справа.
- Касания в левой половине НЕ двигают прицел.

## Что проверить перед сдачей

1. **Один прицел и одна пушка в сцене.** Левый стек удалён.
2. **`HandSide` переключаемо в инспекторе.** Поменяй на `Left` — тач-зона должна сместиться влево, прицел двигаться оттуда. Это понадобится для будущей настройки игрока.
3. **Кулдауны независимы.** Кнопка 1 на cd не блокирует кнопку 2.
4. **Кнопка 2 не убивает одного врага дважды.** Splash может летально задеть врага, который уже в зоне прямого попадания — флаг `_isDead` в `EnemyShipBase` ставится **до** `Death()`, последующий вызов `TakeDamagePercent` игнорируется.
5. **Регистрация врагов в пуле.** Враги корректно регистрируются в `OnEnable` и снимаются в `OnDisable`, иначе спец-атаки не найдут целей.
6. **`CanvasGroup` на кнопке.** Если в префабе кнопки его нет — добавляется автоматически в `Awake`.
7. **Цветная механика автопушки не сломалась.** Основная пушка по-прежнему наносит полный/20% урон в зависимости от совпадения индекса.
8. **Спец-атаки игнорируют цвет.** Урон считается через `TakeDamagePercent`, индекс не сравнивается.
9. **Сборка и Play без ошибок и NullReference.**

## Файлы для работы

Существующие — править:
- [Assets/Scripts/DualJoystickController.cs](../Assets/Scripts/DualJoystickController.cs)
- [Assets/Scripts/Enemy/EnemyShipBase.cs](../Assets/Scripts/Enemy/EnemyShipBase.cs)
- [Assets/Scripts/EnemySpawnManager.cs](../Assets/Scripts/EnemySpawnManager.cs)
- [Assets/Scenes/GameScene.unity](../Assets/Scenes/GameScene.unity)
- [CLAUDE.md](../CLAUDE.md) — после реализации добавить раздел "Изменения (v3 — Single-hand + Special Attacks)" с кратким описанием.

Существующие — не трогать без необходимости:
- [Assets/Scripts/CrosshairController.cs](../Assets/Scripts/CrosshairController.cs) — API `ApplyDelta` / `ApplyVelocity` остаётся.
- [Assets/Scripts/WeaponTurel.cs](../Assets/Scripts/WeaponTurel.cs) — только `ShootMode.Auto`, ничего возвращать не надо.
- [Assets/Scripts/WeaponBase.cs](../Assets/Scripts/WeaponBase.cs).

Новые:
- `Assets/Scripts/WeaponSpecial.cs`
- `Assets/Scripts/UI/SpecialAttackButtonUI.cs`

## Чего НЕ делать

- Не добавлять прогресс-бар / radial fill кулдауна на кнопках — только alpha + interactable.
- Не учитывать цвет/индекс в спец-атаках — они бьют в процентах от max HP.
- Не возвращать ручную кнопку выстрела для основной пушки — она остаётся `Auto`.
- Не делать feature flag / fallback на двурукое управление — старый код удаляем, не оставляем "на всякий случай".
- Не трогать health bar, ScoreManager, AudioManager, цветную механику основной пушки.

## Definition of Done

- В сцене один правый прицел + автопушка + две левые кнопки спец-атак, левый стек удалён.
- `HandSide` в `DualJoystickController` переключает сторону через инспектор.
- Кнопка 1: −20% HP ближайшему к базе, cd 1 c.
- Кнопка 2: 100% HP при прямом попадании / 50% HP в splash-радиусе, cd 5 c.
- Кулдаун кнопок визуализируется через `CanvasGroup.alpha = 0.5` + `Button.interactable = false`.
- `EnemyShipBase` имеет `TakeDamagePercent`, `IsDead` ставится до `Death()`.
- `EnemySpawnManager.Instance` экспортирует `FindNearestToPosition` / `FindEnemiesInRadius`.
- Unity компилирует проект без ошибок, Play-mode проходит без NullReference.
- `CLAUDE.md` обновлён разделом про v3.
