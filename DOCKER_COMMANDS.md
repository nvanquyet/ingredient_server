# Docker Commands cho Ingredient Server

## 🚀 Quick Start

### 1. Build và chạy lần đầu

```bash
# Build images và start containers
docker compose up -d --build

# Xem logs real-time
docker compose logs -f ingredientserver-app
```

### 2. Rebuild sau khi thay đổi code

```bash
# Stop containers
docker compose down

# Rebuild images (no cache để đảm bảo build mới)
docker compose build --no-cache

# Start lại
docker compose up -d

# Xem logs
docker compose logs -f ingredientserver-app
```

### 3. Rebuild chỉ API service

```bash
# Rebuild chỉ ingredientserver service
docker compose build --no-cache ingredientserver

# Restart service
docker compose up -d ingredientserver

# Xem logs
docker compose logs -f ingredientserver-app
```

## 📋 Các lệnh thường dùng

### Xem logs

```bash
# Tất cả services
docker compose logs -f

# Chỉ API service
docker compose logs -f ingredientserver-app

# Chỉ MySQL
docker compose logs -f mysql

# Chỉ Nginx
docker compose logs -f nginx

# Last 100 lines
docker compose logs --tail=100 ingredientserver-app
```

### Stop/Start/Restart

```bash
# Stop tất cả
docker compose stop

# Start tất cả
docker compose start

# Restart tất cả
docker compose restart

# Restart chỉ API
docker compose restart ingredientserver
```

### Xóa và tạo lại

```bash
# Stop và xóa containers (giữ volumes)
docker compose down

# Stop và xóa containers + volumes (xóa data)
docker compose down -v

# Xóa images
docker compose down --rmi all

# Xóa tất cả (containers + volumes + images)
docker compose down -v --rmi all

# Sau đó build lại
docker compose up -d --build
```

### Kiểm tra status

```bash
# Xem status các containers
docker compose ps

# Xem resource usage
docker stats

# Xem health check
docker compose ps
# Hoặc
curl http://localhost:8080/health
```

### Database Operations

```bash
# Connect vào MySQL container
docker exec -it ingredientserver-mariadb mysql -u ingredientuser -p

# Backup database
docker exec ingredientserver-mariadb mysqldump -u ingredientuser -p IngredientServerDb > backup.sql

# Restore database
docker exec -i ingredientserver-mariadb mysql -u ingredientuser -p IngredientServerDb < backup.sql
```

### Migration Operations

```bash
# Chạy migrations trong container
docker exec -it ingredientserver-app dotnet ef database update --project /app --startup-project /app/API

# Hoặc dùng script helper
./scripts/docker-migrate.sh update

# Xem danh sách migrations
./scripts/docker-migrate.sh list
```

## 🔧 Troubleshooting

### Container không start

```bash
# Xem logs để debug
docker compose logs ingredientserver-app

# Check health
docker compose ps

# Restart service
docker compose restart ingredientserver
```

### Database connection issues

```bash
# Check MySQL đang chạy
docker compose ps mysql

# Check MySQL logs
docker compose logs mysql

# Test connection từ API container
docker exec -it ingredientserver-app curl http://localhost:8080/health
```

### Rebuild sau khi thay đổi code

```bash
# Full rebuild (recommended)
docker compose down
docker compose build --no-cache
docker compose up -d

# Quick rebuild (nếu chỉ thay đổi code, không thay đổi dependencies)
docker compose build ingredientserver
docker compose up -d ingredientserver
```

### Clear cache và rebuild

```bash
# Xóa build cache
docker builder prune

# Rebuild
docker compose build --no-cache
docker compose up -d
```

## 📝 Environment Variables

Tất cả config trong file `.env`. Xem `.env.example` để biết các biến cần thiết.

Sau khi thay đổi `.env`:
```bash
docker compose down
docker compose up -d
```

## 🎯 Workflow thường dùng

### Sau khi thay đổi code:

```bash
# 1. Rebuild
docker compose build --no-cache ingredientserver

# 2. Restart
docker compose up -d ingredientserver

# 3. Xem logs
docker compose logs -f ingredientserver-app
```

### Sau khi thay đổi database schema:

```bash
# 1. Rebuild
docker compose build --no-cache ingredientserver

# 2. Start
docker compose up -d

# 3. Migrations sẽ tự động apply khi app khởi động
# Hoặc chạy thủ công:
docker exec -it ingredientserver-app dotnet ef database update --project /app --startup-project /app/API
```

### Clean rebuild (khi có vấn đề):

```bash
# 1. Stop và xóa tất cả
docker compose down -v

# 2. Xóa images
docker rmi ingredientserver:latest 2>/dev/null || true

# 3. Build lại từ đầu
docker compose build --no-cache

# 4. Start
docker compose up -d

# 5. Xem logs
docker compose logs -f ingredientserver-app
```

