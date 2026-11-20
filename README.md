# Test3 — Todo API

Учебный REST API для работы с задачами и пользователями.

Стек: **ASP.NET Core Minimal API, .NET 10, PostgreSQL, Npgsql, xUnit**.

> Авторизация и аутентификация пока не реализованы. API не проверяет пользователя и доступ к задачам. `ownerId` используется только как связь задачи с записью пользователя в БД.

## Требования

Необходимо:

- .NET 10 SDK
- PostgreSQL
- `dotnet-ef` версии, совместимой с используемой версией EF Core

Для получения и настройки проекта также понадобятся:

- Git
- терминал
- pgAdmin или `psql` для создания БД и при необходимости выполнения SQL-запросов

## Настройка

### 1. Создание БД

Создай **пустую** PostgreSQL БД `todo_api_migrations_lab`. Таблицы в ней будут созданы через EF Core Migrations (инструкция ниже).

Старые SQL-файлы остаются в проекте для отдельных SQL-упражнений:

```
Test3/sql 2/
├── 01_schema.sql   # ручное создание таблиц users и tasks
├── 02_seed.sql     # тестовые данные
└── 03_queries.sql  # примеры SQL-запросов
```

**Не запускай `01_schema.sql` в базе, для которой собираешься применять `InitialCreate`: таблицы будут созданы миграцией.** Не выполняй учебные миграции на существующих `todo_api` и `todo_api_test`.

### 2. Connection String

Строка подключения хранится через **User Secrets** и не должна добавляться в репозиторий.

Из корня репозитория:

```bash
dotnet user-secrets --project Test3/Test3.csproj set "ConnectionStrings:ToDoDb" "Host=localhost;Port=5432;Database=todo_api_migrations_lab;Username=postgres;Password=YOUR_PASSWORD"
```

### 3. Запуск

```bash
dotnet run --project Test3/Test3.csproj
```

При текущем `launchSettings.json` HTTP-адрес:

```
http://localhost:5177
```

## Миграции EF Core

Схема новой базы данных создаётся и изменяется через EF Core Migrations. Файлы миграций находятся в `Test3/Migrations/`.

В проекте созданы две миграции:

| Миграция | Назначение |
| --- | --- |
| `InitialCreate` | Создаёт таблицы `users`, `tasks`, ограничения и индекс внешнего ключа |
| `AddDescription` | Добавляет nullable-столбец `description` типа `text` в таблицу `tasks` |

### Создание базы с помощью миграций

Создай пустую базу PostgreSQL `todo_api_migrations_lab`. Для выполнения команд потребуется `dotnet-ef`, совместимый с используемой версией EF Core.

В PowerShell задай подключение к учебной базе для текущей сессии:

```powershell
$env:ConnectionStrings__ToDoDb = "Host=localhost;Port=5432;Database=todo_api_migrations_lab;Username=postgres;Password=YOUR_PASSWORD"
```

Из корня репозитория проверь, что выбран нужный DbContext и база данных:

```powershell
dotnet ef dbcontext info --context ToDoEFContext --project Test3/Test3.csproj --startup-project Test3/Test3.csproj
```

Убедись, что в выводе указана `todo_api_migrations_lab`. Затем примени все ожидающие миграции:

```powershell
dotnet ef database update --context ToDoEFContext --project Test3/Test3.csproj --startup-project Test3/Test3.csproj
```

**Важно:** не выполняй `01_schema.sql` в базе, которую создаёшь через `InitialCreate`. Скрипт уже создаёт таблицы, которыми управляет миграция.

## API

| Метод    | Endpoint                 | Описание                     |
| -------- | ------------------------ | ---------------------------- |
| `GET`    | `/ToDos`                 | Получить все задачи          |
| `GET`    | `/ToDos?ownerId=1`       | Получить задачи пользователя |
| `GET`    | `/ToDo/{id}`             | Получить задачу по ID        |
| `POST`   | `/ToDo`                  | Создать задачу               |
| `PATCH`  | `/ToDo/{id}/Title`       | Изменить название            |
| `PATCH`  | `/ToDo/{id}/IsCompleted` | Изменить статус выполнения   |
| `PUT`    | `/ToDo/{id}`             | Полностью обновить задачу    |
| `DELETE` | `/ToDo/{id}`             | Удалить задачу               |

> Для создания или обновления задачи `ownerId` должен соответствовать существующей записи в таблице `users`.

### POST `/ToDo`

```json
{
  "title": "Learn ASP.NET Core",
  "description": "Изучить EF Core Migrations",
  "ownerId": 1
}
```

### PATCH `/ToDo/{id}/Title`

```json
{
  "title": "New title"
}
```

### PATCH `/ToDo/{id}/IsCompleted`

```json
{
  "isCompleted": true
}
```

### PUT `/ToDo/{id}`

```json
{
  "title": "Updated task",
  "isCompleted": true,
  "ownerId": 1,
  "description": "Обновлённое описание задачи"
}
```

## DI endpoints

Учебные endpoints для проверки Dependency Injection:

| Endpoint              | Назначение                                       |
| --------------------- | ------------------------------------------------ |
| `GET /di-probe`       | Проверка `Scoped` lifetime                       |
| `GET /di-constructor` | Проверка внедрения зависимости через конструктор |
| `GET /Test`           | Проверка общей `Scoped` зависимости              |

## Тесты

Интеграционные тесты `Storage → Npgsql → PostgreSQL` находятся в:

```
TestProject2/
```

Перед запуском тестов необходимо создать **отдельную PostgreSQL БД** с точным именем:

```
todo_api_test
```

В ней нужно выполнить:

```
Test3/sql 2/01_schema.sql
```

`02_seed.sql` для тестов не требуется — необходимые данные тесты создают самостоятельно.

Тесты специально проверяют, что подключение происходит к БД `todo_api_test`, чтобы случайно не изменить основную БД.

Затем укажи подключение к тестовой БД. Для текущей PowerShell-сессии:

```powershell
$env:TEST_DB_CONNECTION_STRING="Host=localhost;Port=5432;Database=todo_api_test;Username=postgres;Password=YOUR_PASSWORD"
```

Запуск:

```bash
dotnet test TestProject2/TestProject2.csproj
```
