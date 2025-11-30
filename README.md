# Ingredient Server API

> Ứng dụng quản lý nguyên liệu, món ăn và theo dõi dinh dưỡng với AI hỗ trợ

## 📋 Mục lục

- [Giới thiệu](#giới-thiệu)
- [Tính năng](#tính-năng)
- [Kiến trúc](#kiến-trúc)
- [Công nghệ sử dụng](#công-nghệ-sử-dụng)
- [Cài đặt](#cài-đặt)
- [Cấu hình](#cấu-hình)
- [API Endpoints](#api-endpoints)
- [Docker](#docker)
- [Development](#development)
- [Cấu trúc Project](#cấu-trúc-project)

## 🎯 Giới thiệu

Ingredient Server là một RESTful API được xây dựng bằng ASP.NET Core 9.0, giúp người dùng quản lý nguyên liệu thực phẩm, tạo món ăn, theo dõi dinh dưỡng và nhận gợi ý từ AI. Ứng dụng sử dụng Azure AI Inference để phân tích hình ảnh món ăn và nguyên liệu, tự động tính toán thông tin dinh dưỡng.

## ✨ Tính năng

### 🔐 Xác thực & Người dùng
- Đăng ký, đăng nhập với JWT authentication
- Quản lý profile người dùng
- Đổi mật khẩu
- Xác thực token

### 🥬 Quản lý Nguyên liệu
- Thêm, sửa, xóa nguyên liệu
- Theo dõi hạn sử dụng
- Phân loại nguyên liệu (Dairy, Meat, Vegetables, Fruits, v.v.)
- Tìm kiếm và lọc nguyên liệu
- Phân tích nguyên liệu từ hình ảnh bằng AI

### 🍽️ Quản lý Món ăn
- Tạo và quản lý món ăn
- Phân tích món ăn từ hình ảnh bằng AI
- Tự động tính toán dinh dưỡng (calories, protein, carbs, fat, fiber)
- Gợi ý món ăn dựa trên nguyên liệu có sẵn
- Tạo công thức nấu ăn chi tiết với AI

### 📊 Theo dõi Dinh dưỡng
- Tổng hợp dinh dưỡng theo ngày
- Tổng hợp dinh dưỡng theo tuần
- Tổng quan dinh dưỡng với AI assistant
- So sánh với mục tiêu dinh dưỡng cá nhân

### 🤖 AI Features
- Phân tích hình ảnh món ăn (nhận diện món, tính dinh dưỡng)
- Phân tích hình ảnh nguyên liệu (nhận diện, ước tính hạn sử dụng)
- Gợi ý món ăn dựa trên nguyên liệu và mục tiêu dinh dưỡng
- Tạo công thức nấu ăn chi tiết
- Tính toán mục tiêu dinh dưỡng hàng ngày

## 🏗️ Kiến trúc

Project được xây dựng theo **Clean Architecture** với các layer rõ ràng:

```
┌─────────────────────────────────────┐
│         API Layer (Presentation)    │
│  - Controllers                       │
│  - Middlewares                       │
│  - Extensions                        │
└─────────────────────────────────────┘
                  ↓
┌─────────────────────────────────────┐
│      Core Layer (Business Logic)    │
│  - Entities                          │
│  - Services                          │
│  - Interfaces                        │
│  - Helpers                           │
│  - Configuration                     │
└─────────────────────────────────────┘
                  ↓
┌─────────────────────────────────────┐
│  Infrastructure Layer (Data Access) │
│  - Repositories                      │
│  - DbContext                         │
│  - Configurations                    │
└─────────────────────────────────────┘
                  ↓
┌─────────────────────────────────────┐
│      Utils Layer (Shared)           │
│  - DTOs                              │
│  - Mappers                           │
│  - Extensions                        │
│  - Constants                         │
└─────────────────────────────────────┘
```

### Nguyên tắc thiết kế

- **Separation of Concerns**: Mỗi layer có trách nhiệm riêng biệt
- **Dependency Inversion**: Core layer không phụ thuộc vào Infrastructure
- **Repository Pattern**: Tách biệt data access logic
- **Service Pattern**: Business logic được đóng gói trong services
- **DTO Pattern**: Tách biệt domain models và data transfer objects

## 🛠️ Công nghệ sử dụng

### Backend Framework
- **.NET 9.0** - Framework chính
- **ASP.NET Core 9.0** - Web API framework
- **Entity Framework Core 9.0** - ORM

### Database
- **MySQL/MariaDB** - Database chính
- **Pomelo.EntityFrameworkCore.MySql** - MySQL provider

### Authentication & Security
- **JWT Bearer Authentication** - Xác thực API
- **BCrypt.Net-Next** - Mã hóa mật khẩu

### AI Services
- **Azure AI Inference** - AI service cho phân tích hình ảnh và text
- **Azure.AI.Inference** - Azure SDK

### Other Libraries
- **Swashbuckle.AspNetCore** - Swagger/OpenAPI documentation
- **System.IdentityModel.Tokens.Jwt** - JWT token handling

## 📦 Cài đặt

### Yêu cầu hệ thống

- .NET 9.0 SDK
- MySQL/MariaDB 10.4+
- Docker & Docker Compose (optional)

### Cài đặt thủ công

1. **Clone repository**
```bash
git clone <repository-url>
cd ingredient_server
```

2. **Restore dependencies**
```bash
cd IngredientServer
dotnet restore
```

3. **Cấu hình database**
   - Tạo MySQL database
   - Cập nhật connection string trong `appsettings.json`

4. **Chạy migrations** (nếu có)
```bash
dotnet ef database update
```

5. **Chạy ứng dụng**
```bash
dotnet run --project IngredientServer
```

Ứng dụng sẽ chạy tại: `http://localhost:5000` hoặc `https://localhost:5001`

## ⚙️ Cấu hình

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=IngredientServerDb;User=root;Password=yourpassword;"
  },
  "AzureOpenAI": {
    "Endpoint": "https://your-endpoint.services.ai.azure.com",
    "ApiKey": "your-api-key",
    "Model": "gpt-4.1",
    "MaxTokens": 2000,
    "TimeoutMinutes": 2
  },
  "Jwt": {
    "Secret": "your-jwt-secret-key-change-in-production"
  }
}
```

### Environment Variables

Có thể override cấu hình bằng environment variables:

```bash
# Database
export ConnectionStrings__DefaultConnection="Server=mysql;Port=3306;Database=IngredientServerDb;User=user;Password=pass;"

# Azure OpenAI
export AzureOpenAI__Endpoint="https://your-endpoint.services.ai.azure.com"
export AzureOpenAI__ApiKey="your-api-key"
export AzureOpenAI__Model="gpt-4.1"

# JWT
export JWT__Secret="your-jwt-secret"
```

## 📡 API Endpoints

### Authentication (`/api/auth`)

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|--------------|
| POST | `/api/auth/login` | Đăng nhập | ❌ |
| POST | `/api/auth/register` | Đăng ký | ❌ |
| GET | `/api/auth/validateToken` | Xác thực token | ❌ |
| POST | `/api/auth/logout` | Đăng xuất | ✅ |
| GET | `/api/auth/me` | Lấy thông tin profile | ✅ |
| PUT | `/api/auth/me` | Cập nhật profile | ✅ |
| PUT | `/api/auth/change_password` | Đổi mật khẩu | ✅ |

### Ingredients (`/api/ingredient`)

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|--------------|
| POST | `/api/ingredient` | Tạo nguyên liệu mới | ✅ |
| PUT | `/api/ingredient/{id}` | Cập nhật nguyên liệu | ✅ |
| DELETE | `/api/ingredient/{id}` | Xóa nguyên liệu | ✅ |
| GET | `/api/ingredient` | Lấy danh sách nguyên liệu (có filter) | ✅ |
| GET | `/api/ingredient/{id}` | Lấy chi tiết nguyên liệu | ✅ |

### Foods (`/api/food`)

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|--------------|
| POST | `/api/food` | Tạo món ăn mới | ✅ |
| PUT | `/api/food` | Cập nhật món ăn | ✅ |
| DELETE | `/api/food` | Xóa món ăn | ✅ |
| POST | `/api/food/suggestions` | Gợi ý món ăn từ AI | ✅ |
| POST | `/api/food/recipes` | Tạo công thức nấu ăn từ AI | ✅ |
| GET | `/api/food/{id}` | Lấy chi tiết món ăn | ✅ |

### AI (`/api/ai`)

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|--------------|
| POST | `/api/ai/detect_food` | Phân tích món ăn từ hình ảnh | ✅ |
| POST | `/api/ai/detect_ingredient` | Phân tích nguyên liệu từ hình ảnh | ✅ |

### Nutrition (`/api/nutrition`)

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|--------------|
| POST | `/api/nutrition/daily` | Tổng hợp dinh dưỡng theo ngày | ✅ |
| POST | `/api/nutrition/weekly` | Tổng hợp dinh dưỡng theo tuần | ✅ |
| POST | `/api/nutrition/overview` | Tổng quan dinh dưỡng với AI | ✅ |

### Health & Info

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|--------------|
| GET | `/health` | Health check | ❌ |
| GET | `/api/info` | API information | ❌ |

## 🐳 Docker

### Quick Start với Docker Compose

1. **Tạo file `.env`** (optional)
```env
DB_ROOT_PASSWORD=YourRootPassword123!
DB_PASSWORD=YourPassword123!
JWT_SECRET=your-jwt-secret-key-change-in-production
```

2. **Build và chạy**
```bash
docker compose up -d --build
```

3. **Xem logs**
```bash
docker compose logs -f ingredientserver
```

4. **Dừng services**
```bash
docker compose down
```

### Services trong Docker Compose

- **mysql**: MariaDB 10.4 database
- **ingredientserver**: API application
- **nginx**: Reverse proxy (optional)
- **init-permissions**: Service để set permissions cho uploads folder

### Ports

- **API**: `http://localhost:8080` (internal)
- **MySQL**: `localhost:3307` (external)
- **Nginx**: `http://localhost:80` (nếu có)

## 💻 Development

### Cấu trúc Project

```
IngredientServer/
├── API/                          # Presentation Layer
│   ├── Controllers/              # API Controllers
│   │   ├── AuthController.cs
│   │   ├── IngredientController.cs
│   │   ├── FoodController.cs
│   │   ├── AIController.cs
│   │   ├── NutritionController.cs
│   │   └── BaseController.cs
│   ├── Middlewares/              # HTTP Middlewares
│   │   ├── GlobalErrorHandlingMiddleware.cs
│   │   └── JwtMiddleware.cs
│   ├── Extensions/               # Extension Methods
│   │   ├── ServiceCollectionExtensions.cs
│   │   └── WebApplicationExtensions.cs
│   ├── Program.cs                # Entry Point
│   └── appsettings.json          # Configuration
│
├── Core/                         # Business Logic Layer
│   ├── Entities/                 # Domain Entities
│   │   ├── User.cs
│   │   ├── Ingredient.cs
│   │   ├── Food.cs
│   │   ├── Meal.cs
│   │   └── BaseEntity.cs
│   ├── Services/                 # Business Services
│   │   ├── AuthService.cs
│   │   ├── IngredientService.cs
│   │   ├── FoodService.cs
│   │   ├── AIService.cs
│   │   ├── NutritionService.cs
│   │   └── ...
│   ├── Interfaces/               # Contracts
│   │   ├── Services/
│   │   └── Repositories/
│   ├── Helpers/                  # Utilities
│   │   └── DateTimeHelper.cs
│   ├── Configuration/            # Config Classes
│   │   └── AzureOpenAIOptions.cs
│   └── Exceptions/               # Custom Exceptions
│
├── Infrastructure/               # Data Access Layer
│   ├── Data/                     # DbContext & Configurations
│   │   ├── ApplicationDbContext.cs
│   │   └── Configurations/
│   └── Repositories/             # Data Repositories
│       ├── BaseRepository.cs
│       ├── UserRepository.cs
│       ├── IngredientRepository.cs
│       └── ...
│
└── Utils/                        # Shared Utilities
    ├── DTOs/                     # Data Transfer Objects
    │   ├── Common/
    │   ├── Entity/
    │   └── Auth/
    ├── Mappers/                  # Entity Mappers
    ├── Extension/                 # Extension Methods
    └── Constants/                # Constants
```

### Coding Standards

- **DateTime**: Tất cả DateTime operations sử dụng UTC thông qua `DateTimeHelper`
- **Naming**: 
  - Controllers: `{Entity}Controller`
  - Services: `{Entity}Service`
  - Repositories: `{Entity}Repository`
  - DTOs: `{Action}{Entity}Dto` (ví dụ: `CreateIngredientRequestDto`)
- **Response Format**: 
  - API Controllers: `ApiResponse<T>`
  - Service Layer: `ResponseDto<T>`

### Best Practices

1. **Dependency Injection**: Tất cả dependencies được inject qua constructor
2. **Async/Await**: Tất cả I/O operations đều async
3. **Error Handling**: Global error handling middleware
4. **Validation**: Model validation với Data Annotations
5. **Logging**: Structured logging với ILogger
6. **Security**: JWT authentication, password hashing với BCrypt

## 📝 API Documentation

Khi chạy ứng dụng ở Development mode, Swagger UI có sẵn tại:
- **Swagger UI**: `http://localhost:5000` hoặc `http://localhost:8080`
- **Swagger JSON**: `http://localhost:5000/swagger/v1/swagger.json`

## 🔒 Security

- **JWT Authentication**: Tất cả endpoints (trừ auth và health) yêu cầu JWT token
- **Password Hashing**: BCrypt với salt rounds
- **CORS**: Cấu hình trong `ServiceCollectionExtensions`
- **Input Validation**: Model validation và sanitization

## 📊 Database Schema

### Main Entities

- **User**: Thông tin người dùng
- **Ingredient**: Nguyên liệu thực phẩm
- **Food**: Món ăn
- **Meal**: Bữa ăn
- **MealFood**: Quan hệ giữa Meal và Food
- **FoodIngredient**: Quan hệ giữa Food và Ingredient
- **UserNutritionTargets**: Mục tiêu dinh dưỡng của người dùng

## 🚀 Deployment

### Production Checklist

- [ ] Đổi JWT Secret trong production
- [ ] Cấu hình Azure OpenAI endpoint và API key
- [ ] Cấu hình database connection string
- [ ] Set `ASPNETCORE_ENVIRONMENT=Production`
- [ ] Cấu hình CORS cho domain cụ thể
- [ ] Enable HTTPS
- [ ] Cấu hình logging
- [ ] Setup backup database

## 📄 License

[Specify your license here]

## 👥 Contributors

[Add contributors here]

## 📞 Support

[Add support information here]

---

**Version**: 1.0.0  
**Last Updated**: 2025

