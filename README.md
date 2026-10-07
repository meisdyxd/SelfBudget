# SelfBudget

SelfBudget — веб-приложение для учёта личных финансов. В репозитории находятся клиент на React и API на ASP.NET Core с базой данных PostgreSQL.

Проект находится в активной разработке. API поддерживает регистрацию, вход с выдачей JWT, получение счетов и переводы между ними. API истории операций и защита маршрутов по токену ещё разрабатываются. Публичного демо пока нет.

## Возможности

- Обзор счетов и просмотр сведений о счёте
- Переводы между счетами с квитанцией
- Регистрация и вход с короткоживущим access token
- Экраны истории операций и фильтры в клиенте; API истории ещё в разработке
- API на ASP.NET Core с хранением данных в PostgreSQL
- Загрузка демонстрационных данных в режиме разработки

## Технологии

- .NET 10 и ASP.NET Core Web API
- Entity Framework Core и PostgreSQL
- React 18 и Vite 6
- Wolverine для обмена сообщениями между компонентами приложения

## Локальный запуск

### Требования

- .NET 10 SDK
- Docker Compose
- Node.js и npm
- Утилита командной строки `dotnet-ef` для применения миграций базы данных

### Запуск PostgreSQL

Создайте файл `.env` в корневой папке репозитория. Эти значения предназначены только для локальной разработки. Те же параметры нужно указать в строке подключения API ниже.

```dotenv
SELF_BUDGET_BD_USER=postgres
SELF_BUDGET_BD_PASSWORD=local_dev_password
SELF_BUDGET_BD_NAME=self-budget
```

Запустите базу данных:

```bash
docker compose up -d
```

### Настройка и запуск API

Укажите строку подключения в терминале, из которого будете запускать миграции и API. Если вы изменили пароль в `.env`, укажите здесь такое же значение.

PowerShell:

```powershell
$env:ConnectionStrings__Database = "Host=localhost;Port=5432;Database=self-budget;Username=postgres;Password=local_dev_password"
```

macOS или Linux:

```bash
export ConnectionStrings__Database='Host=localhost;Port=5432;Database=self-budget;Username=postgres;Password=local_dev_password'
```

Для входа нужен секрет подписи JWT. В проекте уже задан `UserSecretsId`, поэтому сохраните свой случайный секрет длиной не менее 32 байт через .NET User Secrets, подставив его вместо значения в угловых скобках:

```bash
dotnet user-secrets set "AuthOptions:SecretKey" "<ваш случайный секрет>" --project src/SelfBudget/SelfBudget.API/SelfBudget.API.csproj
```

В другой среде тот же параметр можно задать переменной окружения `AuthOptions__SecretKey`. Не добавляйте ключ в `appsettings*.json` или Git. Остальные настройки JWT (`Issuer`, `Audience` и срок жизни access token) находятся в `appsettings.Development.json`.

Примените миграции и запустите API:

```bash
dotnet ef database update --project src/SelfBudget/SelfBudget.API/SelfBudget.API.csproj
dotnet run --project src/SelfBudget/SelfBudget.API/SelfBudget.API.csproj --launch-profile https
```

Если команда `dotnet ef` недоступна, установите версию инструмента для EF Core 10:

```bash
dotnet tool install --global dotnet-ef --version 10.0.9
```

API будет доступно по адресу `https://localhost:7023`. Если сертификат локальной разработки не доверенный, выполните `dotnet dev-certs https --trust`.

### Запуск веб-клиента

В другом терминале выполните:

```bash
cd frontend
npm ci
npm run dev
```

Vite запустит клиент по адресу `http://localhost:5173` и будет перенаправлять запросы `/api` на `https://localhost:7023`. Чтобы указать другой адрес API, скопируйте `frontend/.env.example` в `frontend/.env` и измените `SELFBUDGET_API_PROXY_TARGET`.

Swagger UI доступен по адресу `https://localhost:7023/swagger`, пока API запущено в режиме Development.

## API

| Метод | Адрес | Назначение |
| --- | --- | --- |
| `GET` | `/api/users/health` | Проверка доступности API |
| `GET` | `/api/users/{id}` | Получение сведений о пользователе |
| `POST` | `/api/auth/register` | Регистрация пользователя; `201 Created`, занятая почта — `409 Conflict` |
| `POST` | `/api/auth/login` | Вход; `200 OK` с access token, неверные данные — `400 Bad Request` |
| `GET` | `/api/accounts` | Получение списка счетов |
| `GET` | `/api/accounts/{id}` | Получение сведений о счёте |
| `POST` | `/api/transfers` | Создание перевода между счетами |
| `GET` | `/api/transfers/{id}` | Получение сведений о переводе |

### Регистрация

`POST /api/auth/register` принимает имя, email, дату рождения в формате `YYYY-MM-DD` и пароль:

```json
{
  "name": "Анна",
  "email": "anna@example.com",
  "birthdate": "1990-05-12",
  "password": "replace-with-a-long-password"
}
```

При успехе API возвращает `201 Created` и публичные данные пользователя:

```json
{
  "id": "00000000-0000-0000-0000-000000000000",
  "name": "Анна",
  "email": "anna@example.com"
}
```

Если адрес уже зарегистрирован, API отвечает `409 Conflict` с кодом `error.register.conflict`.

### Вход

`POST /api/auth/login` принимает email и пароль. Email нормализуется перед поиском пользователя:

```json
{
  "email": "anna@example.com",
  "password": "replace-with-a-long-password"
}
```

При успехе API возвращает `200 OK` с access token, сроком его действия в UTC и публичными данными пользователя:

```json
{
  "accessToken": "<jwt>",
  "expiresAt": "2026-10-07T12:15:00Z",
  "user": {
    "id": "00000000-0000-0000-0000-000000000000",
    "name": "Анна",
    "email": "anna@example.com"
  }
}
```

Для неизвестного email и неверного пароля API возвращает одинаковую ошибку. Access token действует 15 минут при настройках разработки. Проверка токена и ограничение доступа к личным данным находятся в разработке.

## Тесты

```bash
dotnet test tests/SelfBudget.Tests/SelfBudget.Tests.csproj
```

## Структура репозитория

```text
frontend/                         Веб-клиент на React
src/SelfBudget/SelfBudget.API/    API на ASP.NET Core
tests/SelfBudget.Tests/           Тесты .NET
docker-compose.yaml               Локальная база PostgreSQL
```

API истории операций ещё разрабатывается. Проект предназначен для демонстрации; для локальной работы используйте тестовые данные.
