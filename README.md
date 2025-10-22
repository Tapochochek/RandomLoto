# Backend для проекта Frendzona

## Описание
Backend система для генерации криптографически стойких случайных чисел с верификацией статистических свойств. Проект включает в себя генераторы, статистические тесты и сервисы для проведения лотерей.

## Технологии
- .NET Core
- ASP.NET Core Web API
- xUnit для тестирования

## Требования
- .NET 6.0 SDK или выше
- Git

## Установка и запуск

### 1. Клонирование репозитория
```bash
git clone https://github.com/hackathonsrus/Product_programming_frendzona_499.git
cd Product_programming_frendzona_499
git checkout backend_eg

Восстановление зависимостей
dotnet restore

Сборка проекта
dotnet build

Запуск тестов
dotnet test

Запуск веб-приложения
cd src/RandomTrust.Web
dotnet run

Приложение будет доступно по адресу: https://localhost:7000 или http://localhost:5000


src/
├── RandomTrust.Core/          # Основная логика
│   ├── Generators/           # Генераторы случайных чисел
│   ├── Interfaces/           # Интерфейсы
│   ├── Models/               # Модели данных
│   └── Utilities/            # Вспомогательные утилиты
├── RandomTrust.Services/     # Бизнес-логика
├── RandomTrust.Web/          # Веб-API
└── RandomTrust.Tests/        # Модульные тесты

Генерация случайных чисел
GET /api/random/generate?count=10
POST /api/random/generate-batch

Статистические тесты
POST /api/random/test

Лотерейные розыгрыши
POST /api/lottery/draw

Через curl
curl -X GET "https://localhost:7000/api/random/generate?count=10"

curl -X POST "https://localhost:7000/api/random/test" \
  -H "Content-Type: application/json" \
  -d '{"numbers":[1,2,3,4,5,6,7,8,9,10]}'
