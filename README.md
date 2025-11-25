# Ingredient Server API

API server quản lý nguyên liệu, món ăn, bữa ăn và theo dõi dinh dưỡng.

## Yêu cầu hệ thống

### 1. .NET SDK 9.0
- Tải và cài đặt từ: https://dotnet.microsoft.com/download/dotnet/9.0
- Kiểm tra phiên bản: `dotnet --version` (phải >= 9.0.0)

### 2. Docker và Docker Compose
- **Docker Desktop** (Windows): https://www.docker.com/products/docker-desktop
- Hoặc cài đặt Docker Engine và Docker Compose riêng biệt
- Kiểm tra cài đặt:
  ```bash
  docker --version
  docker compose version
  ```

## Cách chạy dự án

### Bước 1: Tạo file `.env`

Sao chép file `.env.example` thành `.env`:

```bash
cd ingredient_server
copy .env.example .env
```

Sau đó chỉnh sửa file `.env` và điền các giá trị:
- `DB_ROOT_PASSWORD`: Mật khẩu root cho MySQL
- `DB_PASSWORD`: Mật khẩu cho user database
- `JWT_SECRET`: Chuỗi bí mật cho JWT (tối thiểu 32 ký tự)
- `OPENAI_API_KEY`: API key từ OpenAI (nếu muốn dùng tính năng AI)

### Bước 2: Chạy với Docker Compose

```bash
cd ingredient_server
docker compose up -d
```

Lệnh này sẽ:
- Tạo và chạy container MySQL (MariaDB)
- Build và chạy ứng dụng .NET API
- Chạy Nginx reverse proxy
- Tự động tạo database nếu chưa có

### Bước 3: Kiểm tra ứng dụng

- **API**: http://localhost
- **Swagger UI**: http://localhost/swagger (chỉ trong môi trường Development)
- **Health Check**: http://localhost/health
- **API Info**: http://localhost/api/info

### Bước 4: Xem logs

```bash
# Xem logs của tất cả services
docker compose logs -f

# Xem logs của một service cụ thể
docker compose logs -f ingredientserver
docker compose logs -f mysql
docker compose logs -f nginx
```

## Chạy không dùng Docker (Development)

Nếu muốn chạy trực tiếp trên máy:

### 1. Cài đặt MySQL/MariaDB
- Cài đặt MySQL hoặc MariaDB trên máy
- Tạo database: `IngredientServerDb`
- Tạo user: `ingredientuser` với password

### 2. Cấu hình `appsettings.json`

Chỉnh sửa `IngredientServer/API/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=IngredientServerDb;User=ingredientuser;Password=YourPassword123!;"
  },
  "Jwt": {
    "Secret": "your-super-secret-jwt-key-here-change-this-in-production-minimum-32-characters-long"
  },
  "OpenAI": {
    "ApiKey": "sk-your-openai-api-key-here",
    "Model": "gpt-4-turbo"
  }
}
```

### 3. Chạy ứng dụng

```bash
cd ingredient_server/IngredientServer
dotnet restore
dotnet run --project API
```

Ứng dụng sẽ chạy tại: http://localhost:5000 hoặc https://localhost:5001

## Cấu trúc dự án

```
ingredient_server/
├── compose.yaml          # Docker Compose configuration
├── nginx.conf            # Nginx configuration
├── .env                  # Environment variables (tạo từ .env.example)
├── IngredientServer/
│   ├── API/              # ASP.NET Core API
│   ├── Core/             # Business logic, entities, interfaces
│   ├── Infrastructure/   # Data access, repositories
│   └── Utils/            # DTOs, extensions, constants
└── README.md
```

## Các tính năng chính

- ✅ Authentication & Authorization (JWT)
- ✅ Quản lý nguyên liệu (Ingredients)
- ✅ Quản lý món ăn (Foods)
- ✅ Quản lý bữa ăn (Meals)
- ✅ Theo dõi dinh dưỡng (Nutrition tracking)
- ✅ Tích hợp OpenAI (AI features)
- ✅ Upload và quản lý hình ảnh
- ✅ Swagger/OpenAPI documentation

## Dừng ứng dụng

```bash
docker compose down
```

Để xóa cả volumes (database data):
```bash
docker compose down -v
```

## Troubleshooting

### Lỗi kết nối database
- Kiểm tra MySQL container đã chạy: `docker compose ps`
- Kiểm tra logs: `docker compose logs mysql`
- Đảm bảo các biến môi trường trong `.env` đúng

### Lỗi JWT
- Đảm bảo `JWT_SECRET` trong `.env` có ít nhất 32 ký tự

### Port đã được sử dụng
- Thay đổi port trong `compose.yaml` nếu port 80, 443, 3307 đã được sử dụng

## Liên hệ

Nếu gặp vấn đề, vui lòng kiểm tra logs và cấu hình.





