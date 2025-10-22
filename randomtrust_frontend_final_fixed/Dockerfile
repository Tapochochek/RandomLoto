# --- Этап сборки ---
FROM node:20 AS build

WORKDIR /app

# Копируем package.json и package-lock.json
COPY package*.json ./

# Устанавливаем зависимости
RUN npm install

# Устанавливаем vite глобально (если вдруг не подтянулся)
RUN npm install -g vite

# Копируем остальной код
COPY . .

# Сборка проекта
RUN npm run build

# --- Этап запуска ---
FROM nginx:alpine

# Копируем сборку фронтенда
COPY --from=build /app/dist /usr/share/nginx/html

# Открываем порт
EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
