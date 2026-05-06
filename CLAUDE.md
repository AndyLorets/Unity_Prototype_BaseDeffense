# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**BaseDeffense** — мобильная игра на Unity 6 (6000.4.4f1) в жанре tower defense. Игрок защищает базу от волн цветных врагов. Управление — два джойстика (по одному на каждую пушку), каждый управляет своим прицелом. Урон зависит от совпадения цвета оружия и врага.

## Unity Version & Build

- Unity **6000.4.4f1** (Unity 6.0.4)
- Открывать через Unity Hub — стандартный билд через **File → Build Settings**
- Сцены: `MenuScene` (index 0), `GameScene` (index 1)
- Таргет-платформа: мобильные устройства (Android/iOS)

## Architecture

### Ключевые паттерны

**ServiceLocator** (`Infrastructure/ServiceLocator.cs`) — регистрация и получение сервисов по типу. `AudioManager` и `TargetController` регистрируются через него при старте.

**StaticSingleton** — `ScoreManager` хранит счёт статически и уведомляет подписчиков через `Action onScoreChange`.

**Object Pool** — два независимых пула:
- `EnemySpawnManager` заранее создаёт по 10 копий каждого врага на каждой линии. Мёртвые враги деактивируются и возвращаются в пул через `OnEnemyDeath`.
- `HealthBarPool` (UI) заранее создаёт N экземпляров `HealthBarUI` на Canvas. Враг при `OnEnable` берёт бар через `ServiceLocator.GetService<HealthBarPool>().Get(transform, offset)`, при `OnDisable`/`Death` возвращает через `Release`.

**Observer (Action events)** — весь игровой поток строится на событиях:
- `CrosshairController.OnColorChanged` → UI кроссхейра меняет цвет (белый/красный)
- `TargetController.onTargetEnter/Exit` → оружия начинают/прекращают стрельбу
- `ScoreManager.onScoreChange` → `ScoreView` обновляет UI

**Dual Joystick / Dual Crosshair** — два независимых джойстика привязаны к двум пушкам:
- Каждый джойстик статичен и расположен прямо над своей пушкой (нижний левый / нижний правый угол экрана)
- Джойстик активируется только при касании зоны своей пушки — не появляется в произвольном месте
- Каждый джойстик управляет своим `CrosshairController` (два прицела в 3D-сцене)
- Выстрел автоматический: как только прицел навёлся на цель, соответствующая пушка стреляет сама (без кнопок)
- Два режима управления переключаются через `InputMode` в инспекторе (см. ниже)

**Два режима управления (`InputMode` enum в `DualJoystickController`):**

`Delta` — дельта позиции пальца (как старый свайп):
- Визуал: `VariableJoystick` (рекомендуемый режим `DynamicVisible` — фон всегда виден на месте, тянется за пальцем после `MoveThreshold`, возвращается на исходную позицию при отпускании). Зона касания фиксирована (левая/правая половина экрана)
- Логика: дельта пикселей → `ScreenToWorld` → смещение прицела; ощущение 1-в-1 как текущий свайп
- Плюс: привычно, точно; минус: неудобно при больших thumb-зонах

`Velocity` — отклонение стика (`FixedJoystick.Direction`):
- Визуал: `FixedJoystick` — стик статичен, handle отклоняется от центра
- Логика: `Direction * speed * deltaTime` → смещение прицела; держишь стик — прицел едет
- Плюс: аркадно, удобно на мобильном; минус: требует настройки `speed`

### Система урона (цветная механика)

Каждый враг и оружие имеют `index` (0–3), которому соответствует цвет из `IndexInfo` (ScriptableObject). Полный урон наносится при совпадении индексов, 20% урона — при несовпадении. Это создаёт стратегическую глубину.

### Поток игры

```
Два джойстика (Left / Right) → каждый читает касания в своей зоне экрана
Джойстик передаёт дельту → CrosshairController.MoveByDelta (левый/правый прицел)
CrosshairController.UpdateTarget → raycast вниз (World3D) → ITakeDamage
CrosshairController.OnColorChanged → UI прицела меняет цвет (белый = нет цели, красный = цель)
WeaponTurel (Auto) → как только CrosshairController.CurrentTarget != null → Shoot()
EnemySpawnManager → активирует врага из пула каждые 2 сек
Враг движется Vector3.right → входит в триггер TargetController
Враг.TakeDamage() → смерть: +5 очков, эффект, пересоздание через 3 сек
Враг достигает Tags.Basa → -10 очков, сброс в пул
```

### Расположение пушек и джойстиков

```
Вид экрана (портрет / ландшафт):
  [  Игровая сцена (камера)                    ]
  [ Левая пушка       |||       Правая пушка   ]  ← пушки у базы, нижняя часть экрана
  [ [JoyL]                           [JoyR]   ]  ← джойстики над пушками, большие пальцы
```

- Пушки стоят симметрично относительно базы на уровне земли
- Джойстики — статичные UI-элементы (Canvas), каждый в своём углу нижней части экрана
- Касание вне зоны джойстика игнорируется (нет floating joystick)

### Структура скриптов

```
Assets/Scripts/
├── Infrastructure/ServiceLocator.cs   # Реестр сервисов
├── Enemy/
│   ├── EnemyShipBase.cs               # Абстрактный враг: движение, HP, смерть, пул; запрос HealthBar из пула
│   └── EnemyShip.cs                   # Конкретный тип (заготовка для расширения)
├── UI/
│   ├── HealthBarUI.cs                 # UI-полоска HP: WorldToScreenPoint follow, DOFillAmount анимация
│   └── HealthBarPool.cs               # Pool HealthBarUI на Canvas; регистрируется в ServiceLocator
├── CrosshairController.cs             # Кроссхейр: движение (через джойстик), raycast, цвет, CurrentTarget
├── TargetController.cs                # База игрока: движение, AttackState/IdleState
├── WeaponBase.cs                      # Абстрактное оружие: прицеливание, выстрел, индекс
├── WeaponTurel.cs                     # Конкретное оружие (Auto-режим: стреляет при CurrentTarget != null)
├── DualJoystickController.cs          # Статичные левый/правый джойстик; передаёт дельту в CrosshairController
├── EnemySpawnManager.cs               # Пул врагов, спавн по линиям
├── AudioManager.cs                    # Сервис звука (gun, target, explosion)
├── ScoreManager.cs                    # Статический счёт + события
├── ScoreView.cs                       # UI счёта с DOTween анимациями
├── SwipeController.cs                 # (устарел) Старый свайп-контроллер — заменён DualJoystickController
├── IndexInfo.cs                       # ScriptableObject: 4 цвета по индексу
├── ITakeDamage.cs                     # Интерфейс: index + TakeDamage(value, index)
├── GameManager.cs                     # Framerate (60 FPS), перезапуск → сцена 0
└── Tags.cs                            # Константа тега "Basa"
```

## Внешние зависимости

- **DOTween** — все анимации (движение, масштаб, цвет). Пространство имён `DG.Tweening`.
- **TextMeshPro** — UI текст.
- **Joystick Pack** — `FixedJoystick` (статичный стик, Velocity-режим) и `VariableJoystick` (используется в Delta-режиме). `VariableJoystick` поддерживает четыре подрежима через `JoystickType`:
  - `Fixed` — фон закреплён, handle отклоняется от центра
  - `Floating` — фон скрыт, появляется в точке касания
  - `Dynamic` — как `Floating`, но фон тянется за пальцем после `MoveThreshold`
  - `DynamicVisible` (наш кастомный) — фон всегда виден на стартовой позиции, тянется за пальцем после `MoveThreshold`, возвращается на место при отпускании

## Расширение игры

- **Новый тип врага** → наследуй `EnemyShipBase`, переопредели нужное
- **Новое оружие** → наследуй `WeaponBase`, реализуй `Reload()`
- **Новый тип урона** → добавь цвет в `IndexInfo` ScriptableObject и обнови логику в `ITakeDamage`
- **Аудио** → добавляй через `AudioManager`, получай через `ServiceLocator.GetService<AudioManager>()`
- **Режим кроссхейра** → `GameSettings.CrosshairMode`: `World3D` (raycast вниз с офсетом) или `Screen2D` (raycast через экранные координаты)

## Изменения (v2 — Dual Joystick)

### Убрано
- Кнопки ручного выстрела (`ShootMode.Button`, `ManualShoot()` в `WeaponTurel`) — больше не используются
- Одиночный свайп-контроллер (`SwipeController.cs`) — заменён двойным джойстиком

### Добавлено / изменено
- **`DualJoystickController`** — новый скрипт; два джойстика на Canvas (левый / правый), каждый привязан к своему `CrosshairController`; переключаемый `InputMode` в инспекторе:
  - `Delta`: использует `VariableJoystick` (рекомендуется `DynamicVisible`) с фиксированной зоной касания (левая/правая половина экрана); дельта пикселей → `ScreenToWorld`
  - `Velocity`: использует `FixedJoystick`; `Direction * speed * deltaTime` → смещение прицела
- **`CrosshairController`** — убрана внутренняя обработка свайпов/тачей; принимает ввод извне через `ApplyDelta(Vector2 screenDelta)` (Delta-режим) и `ApplyVelocity(Vector2 direction)` (Velocity-режим)
- **`WeaponTurel`** — оставлен только `ShootMode.Auto`; стреляет в `AutoShootLoop` при `CurrentTarget != null`
- **Две пушки** на сцене, симметрично у базы; каждая ссылается на свой `CrosshairController`
- **Health bar (опционально)** — World Space Canvas над врагом, Image с `fillAmount` = `_hp / maxHp`; обновляется в `TakeDamage`; реализован в `EnemyShipBase` через `[SerializeField] private Image _healthBar`
