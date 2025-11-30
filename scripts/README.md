# Migration Scripts

Helper scripts để quản lý database migrations cho Ingredient Server.

## Scripts

### 1. `migrate.sh` - Local Migration Helper

Script để chạy migrations trên local machine (cần .NET SDK).

**Usage:**
```bash
./scripts/migrate.sh [command] [options]
```

**Commands:**
- `add <name>` - Tạo migration mới
- `remove [name]` - Xóa migration (mặc định là migration cuối)
- `list` - Liệt kê tất cả migrations
- `status` - Kiểm tra trạng thái migrations
- `update` - Apply pending migrations vào database
- `script [output]` - Generate SQL script cho migrations
- `help` - Hiển thị help

**Examples:**
```bash
# Tạo migration mới
./scripts/migrate.sh add AddNewTable

# Apply migrations
./scripts/migrate.sh update

# Xem danh sách migrations
./scripts/migrate.sh list
```

### 2. `docker-migrate.sh` - Docker Migration Helper

Script để chạy migrations bên trong Docker container.

**Usage:**
```bash
./scripts/docker-migrate.sh [command] [options]
```

**Commands:**
- `add <name>` - Tạo migration mới trong container
- `remove [name]` - Xóa migration trong container
- `list` - Liệt kê migrations
- `status` - Kiểm tra trạng thái
- `update` - Apply migrations vào database
- `exec <command>` - Chạy custom dotnet ef command
- `help` - Hiển thị help

**Examples:**
```bash
# Tạo migration mới (trong container)
./scripts/docker-migrate.sh add AddNewTable

# Apply migrations (trong container)
./scripts/docker-migrate.sh update

# Xem danh sách migrations
./scripts/docker-migrate.sh list

# Chạy custom command
./scripts/docker-migrate.sh exec migrations list --verbose
```

## Lưu ý

1. **Local Script (`migrate.sh`):**
   - Cần cài đặt .NET SDK trên máy local
   - Database connection string phải đúng trong `appsettings.json`
   - Script sẽ tự động detect nếu đang chạy trong Docker

2. **Docker Script (`docker-migrate.sh`):**
   - Container `ingredientserver-app` phải đang chạy
   - Script sẽ tự động chạy commands bên trong container
   - Không cần .NET SDK trên máy local

3. **Auto Migration:**
   - App tự động apply migrations khi khởi động (xem `WebApplicationExtensions.cs`)
   - Không cần chạy script thủ công nếu chỉ cần apply migrations

## Workflow

### Tạo migration mới:

1. **Local:**
   ```bash
   ./scripts/migrate.sh add AddCachedFoodTable
   ```

2. **Docker:**
   ```bash
   # Start container first
   docker compose up -d
   
   # Create migration
   ./scripts/docker-migrate.sh add AddCachedFoodTable
   ```

### Apply migrations:

1. **Tự động:** App sẽ tự động apply khi khởi động
2. **Thủ công (Local):**
   ```bash
   ./scripts/migrate.sh update
   ```
3. **Thủ công (Docker):**
   ```bash
   ./scripts/docker-migrate.sh update
   ```

## Troubleshooting

### Migration không apply tự động:
- Kiểm tra logs: `docker compose logs ingredientserver-app`
- Chạy thủ công: `./scripts/docker-migrate.sh update`

### Lỗi connection string:
- Kiểm tra `.env` file
- Kiểm tra `appsettings.json`
- Đảm bảo MySQL container đang chạy: `docker compose ps`

### Migration conflicts:
- Xem danh sách: `./scripts/docker-migrate.sh list`
- Xóa migration lỗi: `./scripts/docker-migrate.sh remove`

