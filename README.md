# 🎰 RandomTrust — Система генерации случайных чисел с проверкой честности

**RandomTrust** — это open-source проект, предназначенный для генерации случайных чисел с использованием гибридных источников энтропии и статистической проверки честности.  
Проект состоит из фронтенда (React + Vite) и бэкенда (.NET 8 + PostgreSQL).

## 🚀 Технологии

| Компонент | Технологии |
|------------|-------------|
| **Frontend** | React, Vite, JavaScript, Fetch API |
| **Backend** | ASP.NET Core (.NET 8), C#, REST API |
| **Database** | PostgreSQL 16 |
| **Инфраструктура** | Docker, Docker Compose |

## ⚙️ Установка и запуск

### Сайт имеет хост
http://80.93.62.78:3000 - основной сайт

### 1. Клонирование репозитория

```bash
git clone https://github.com/hackathonsrus/Product_programming_frendzona_499.git
cd Product_programming_frendzona_499
```

### 2. Запуск через Docker

Перед первым запуском убедись, что установлены Docker и Docker Compose.

```bash
docker-compose up --build -d
```

Это запустит:
- **Бэкенд** → http://80.93.62.78:8080  
- **Фронтенд** → http://80.93.62.78:3000  
- **PostgreSQL** — внутри Docker-сети

### 3. Локальный запуск (без Docker)

#### Backend:
```bash
cd randomtrust_backend
dotnet run
```

#### Frontend:
```bash
cd randomtrust_frontend_final_fixed
npm install
npm run dev
```

## 🔌 API эндпоинты

| Метод | URL | Описание |
|--------|------|-----------|
| POST | /api/random/entropy | Добавление энтропии |
| POST | /api/random/initialize | Инициализация генератора |
| GET | /api/random/lottery/{count} | Генерация лотерейных чисел |
| POST | /api/random/generate-test-data | Генерация тестовых данных |
| POST | /api/random/run-statistical-tests | Проверка статистических свойств |
| POST | /api/random/verify | Проверка честности и целостности данных |

Пример запроса:
```bash
curl http://80.93.62.78:8080/api/random/lottery/6
```

Пример ответа:
```json
{
  "numbers": [5, 9, 44, 45, 12, 35],
  "timestamp": "2025-10-23T04:25:18.846284Z",
  "drawId": "abc123"
}
```

## 🌍 CORS

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .WithOrigins(
                "http://80.93.62.78:3000",
                "http://localhost:3000"
            ));
});
```

## 🧠 Основная идея

RandomTrust обеспечивает **прозрачную и проверяемую генерацию случайных чисел**, применимую для:
- лотерей;
- жеребьёвок;
- тестирования алгоритмов;
- исследований случайности;
- образовательных демонстраций принципов энтропии.

## ☁️ Деплой на sweb.ru (или другой VPS)

```bash
ssh root@IP_АДРЕС
apt update && apt install -y docker docker-compose nodejs npm git
git clone https://github.com/hackathonsrus/Product_programming_frendzona_499.git
cd Product_programming_frendzona_499
docker-compose up --build -d
```

После этого:
- Бэкенд: `http://<IP>:8080`
- Фронтенд: `http://<IP>:3000`
