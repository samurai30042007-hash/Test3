# Test3 — Todo API

Учебный REST API для работы с задачами и пользователями.

Стек: **ASP.NET Core Minimal API, .NET 10, PostgreSQL, Npgsql, xUnit**.

> Авторизация и аутентификация пока не реализованы. API не проверяет пользователя и доступ к задачам. `ownerId` используется только как связь задачи с записью пользователя в БД.

## Требования

Необходимо:

- .NET 10 SDK
- PostgreSQL

Для получения и настройки проекта также понадобятся:

- Git
- терминал
- pgAdmin или `psql` для создания БД и выполнения SQL-скриптов

## Настройка

### 1. Создание БД

Создай пустую PostgreSQL БД, например:

```
todo_api
```

SQL-файлы находятся в основном проекте:

```
Test3/sql 2/
├── 01_schema.sql   # создание таблиц users и tasks
├── 02_seed.sql     # тестовые данные
└── 03_queries.sql  # примеры SQL-запросов
```

В созданной БД выполни:

```
01_schema.sql
02_seed.sql
```

`02_seed.sql` предназначен для новой учебной БД и необязателен, если начальные данные не нужны.

### 2. Connection String

Строка подключения хранится через **User Secrets** и не должна добавляться в репозиторий.

Из корня репозитория:

```bash
dotnet user-secrets --project Test3/Test3.csproj set "ConnectionStrings:ToDoDb" "Host=localhost;Port=5432;Database=todo_api;Username=postgres;Password=YOUR_PASSWORD"
```

### 3. Запуск

```bash
dotnet run --project Test3/Test3.csproj
```

При текущем `launchSettings.json` HTTP-адрес:

```
http://localhost:5177
```

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
  "ownerId": 1
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
