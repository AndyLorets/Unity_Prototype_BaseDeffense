# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**BaseDeffense** — мобильная игра на Unity 6 (6000.4.2f1) в жанре tower defense. Игрок защищает базу от волн цветных врагов. Управление — кроссхейр (свайп/мышь), урон зависит от совпадения цвета оружия и врага.

## Unity Version & Build

- Unity **6000.4.2f1** (Unity 6.0.4)
- Открывать через Unity Hub — стандартный билд через **File → Build Settings**
- Сцены: `MenuScene` (index 0), `GameScene` (index 1)
- Таргет-платформа: мобильные устройства (Android/iOS)

## Architecture

### Ключевые паттерны

**ServiceLocator** (`Infrastructure/ServiceLocator.cs`) — регистрация и получение сервисов по типу. `AudioManager` и `TargetController` регистрируются через него при старте.

**StaticSingleton** — `ScoreManager` хранит счёт статически и уведомляет подписчиков через `Action onScoreChange`.

**Object Pool** — `EnemySpawnManager` заранее создаёт по 10 копий каждого врага на каждой линии. Мёртвые враги деактивируются и возвращаются в пул через `OnEnemyDeath`.

**Observer (Action events)** — весь игровой поток строится на событиях:
- `CrosshairController.OnColorChanged` → UI кроссхейра меняет цвет (белый/красный)
- `TargetController.onTargetEnter/Exit` → оружия начинают/прекращают стрельбу
- `ScoreManager.onScoreChange` → `ScoreView` обновляет UI

### Система урона (цветная механика)

Каждый враг и оружие имеют `index` (0–3), которому соответствует цвет из `IndexInfo` (ScriptableObject). Полный урон наносится при совпадении индексов, 20% урона — при несовпадении. Это создаёт стратегическую глубину.

### Поток игры

```
CrosshairController (свайп/мышь) → двигает 3D-кроссхейр по плоскости worldY
CrosshairController.UpdateTarget → raycast вниз (3D) или через экран (2D) → ITakeDamage
CrosshairController.OnColorChanged → UI меняет цвет (белый = нет цели, красный = цель)
EnemySpawnManager → активирует врага из пула каждые 2 сек
Враг движется Vector3.right → входит в триггер TargetController
TargetController.onTargetEnter → WeaponBase.Shoot() (через OnMouseDown или автоматически)
Враг.TakeDamage() → смерть: +5 очков, эффект, пересоздание через 3 сек
Враг достигает Tags.Basa → -10 очков, сброс в пул
```

### Структура скриптов

```
Assets/Scripts/
├── Infrastructure/ServiceLocator.cs   # Реестр сервисов
├── Enemy/
│   ├── EnemyShipBase.cs               # Абстрактный враг: движение, HP, смерть, пул
│   └── EnemyShip.cs                   # Конкретный тип (заготовка для расширения)
├── CrosshairController.cs             # Кроссхейр: движение, raycast, цвет, CurrentTarget
├── TargetController.cs                # База игрока: движение, AttackState/IdleState
├── WeaponBase.cs                      # Абстрактное оружие: прицеливание, выстрел, индекс
├── WeaponTurel.cs                     # Конкретное оружие (наследует WeaponBase)
├── EnemySpawnManager.cs               # Пул врагов, спавн по линиям
├── AudioManager.cs                    # Сервис звука (gun, target, explosion)
├── ScoreManager.cs                    # Статический счёт + события
├── ScoreView.cs                       # UI счёта с DOTween анимациями
├── SwipeController.cs                 # Обработка свайпов (IDragHandler)
├── IndexInfo.cs                       # ScriptableObject: 4 цвета по индексу
├── ITakeDamage.cs                     # Интерфейс: index + TakeDamage(value, index)
├── GameManager.cs                     # Framerate (60 FPS), перезапуск → сцена 0
└── Tags.cs                            # Константа тега "Basa"
```

## Внешние зависимости

- **DOTween** — все анимации (движение, масштаб, цвет). Пространство имён `DG.Tweening`.
- **TextMeshPro** — UI текст.
- **Joystick Pack** — в проекте присутствует, но в текущих скриптах не используется активно.

## Расширение игры

- **Новый тип врага** → наследуй `EnemyShipBase`, переопредели нужное
- **Новое оружие** → наследуй `WeaponBase`, реализуй `Reload()`
- **Новый тип урона** → добавь цвет в `IndexInfo` ScriptableObject и обнови логику в `ITakeDamage`
- **Аудио** → добавляй через `AudioManager`, получай через `ServiceLocator.GetService<AudioManager>()`
- **Режим кроссхейра** → `GameSettings.CrosshairMode`: `World3D` (raycast вниз с офсетом) или `Screen2D` (raycast через экранные координаты)
