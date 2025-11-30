# Ingredient Server API Documentation

## Base URL

```
http://localhost:80/api
```

**Lưu ý**: Trong môi trường production, thay `localhost` bằng domain thực tế.

---

## Authentication

Hầu hết các API endpoints yêu cầu authentication thông qua JWT Bearer Token.

### Cách sử dụng Token

1. Đăng nhập qua endpoint `/api/auth/login` để nhận token
2. Thêm token vào header của mọi request:
   ```
   Authorization: Bearer <your-token>
   ```

### Endpoints không cần authentication

- `POST /api/auth/login`
- `POST /api/auth/register`
- `GET /api/auth/validateToken`
- `GET /health` (health check)

---

## Response Format

Tất cả API responses đều có format chuẩn:

```json
{
  "success": true,
  "data": { ... },
  "message": "Success message",
  "metadata": {
    "key": ["value1", "value2"]
  }
}
```

### Error Response

```json
{
  "success": false,
  "data": null,
  "message": "Error message",
  "metadata": {
    "errors": ["Error detail 1", "Error detail 2"]
  }
}
```

---

## API Endpoints

### 1. Authentication APIs

#### 1.1. Đăng nhập

**Endpoint:** `POST /api/auth/login`

**Authentication:** Không cần

**Request Body:**
```json
{
  "username": "string (required, 3-50 characters)",
  "password": "string (required, min 6 characters)"
}
```

**Example Request:**
```json
{
  "username": "john_doe",
  "password": "password123"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "expiresAt": "2025-12-01T10:00:00Z",
    "user": {
      "id": 1,
      "username": "john_doe",
      "email": "john@example.com",
      "firstName": "John",
      "lastName": "Doe"
    }
  },
  "message": "Login successful"
}
```

---

#### 1.2. Đăng ký

**Endpoint:** `POST /api/auth/register`

**Authentication:** Không cần

**Request Body:**
```json
{
  "username": "string (required, 3-50 characters)",
  "email": "string (optional, valid email format)",
  "password": "string (required, min 6 characters)",
  "firstName": "string (optional, max 50 characters)",
  "lastName": "string (optional, max 50 characters)"
}
```

**Example Request:**
```json
{
  "username": "jane_doe",
  "email": "jane@example.com",
  "password": "password123",
  "firstName": "Jane",
  "lastName": "Doe"
}
```

**Response (201 Created):**
```json
{
  "success": true,
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "expiresAt": "2025-12-01T10:00:00Z",
    "user": {
      "id": 2,
      "username": "jane_doe",
      "email": "jane@example.com",
      "firstName": "Jane",
      "lastName": "Doe"
    }
  },
  "message": "Registration successful"
}
```

---

#### 1.3. Xác thực Token

**Endpoint:** `GET /api/auth/validateToken`

**Authentication:** Không cần

**Query Parameters:**
- `token` (string, required): JWT token cần xác thực

**Example Request:**
```
GET /api/auth/validateToken?token=eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "valid": true,
    "userId": 1
  },
  "message": "Token is valid"
}
```

---

#### 1.4. Đăng xuất

**Endpoint:** `POST /api/auth/logout`

**Authentication:** Cần (Bearer Token)

**Request Body:** Không có

**Response (200 OK):**
```json
{
  "success": true,
  "data": true,
  "message": "Logout successful"
}
```

---

#### 1.5. Lấy thông tin người dùng

**Endpoint:** `GET /api/auth/me`

**Authentication:** Cần (Bearer Token)

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "id": 1,
    "username": "john_doe",
    "email": "john@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "gender": "Male",
    "dateOfBirth": "1990-01-01T00:00:00Z",
    "height": 175.5,
    "weight": 70.0,
    "targetWeight": 65.0,
    "primaryNutritionGoal": "WeightLoss",
    "activityLevel": "Moderate"
  },
  "message": "User profile retrieved successfully"
}
```

---

#### 1.6. Cập nhật thông tin người dùng

**Endpoint:** `PUT /api/auth/me`

**Authentication:** Cần (Bearer Token)

**Request Body:**
```json
{
  "firstName": "string (optional, max 50 characters)",
  "lastName": "string (optional, max 50 characters)",
  "email": "string (optional, valid email format)",
  "gender": "Male | Female | Other (optional)",
  "dateOfBirth": "datetime (optional, ISO 8601 format)",
  "height": "decimal (optional, in cm)",
  "weight": "decimal (optional, in kg)",
  "targetWeight": "decimal (optional, in kg)",
  "primaryNutritionGoal": "WeightLoss | WeightGain | Maintenance | MuscleGain (optional)",
  "activityLevel": "Sedentary | Light | Moderate | Active | VeryActive (optional)"
}
```

**Example Request:**
```json
{
  "firstName": "John",
  "lastName": "Smith",
  "height": 180.0,
  "weight": 75.0,
  "targetWeight": 70.0,
  "primaryNutritionGoal": "WeightLoss",
  "activityLevel": "Active"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "id": 1,
    "username": "john_doe",
    "email": "john@example.com",
    "firstName": "John",
    "lastName": "Smith",
    "height": 180.0,
    "weight": 75.0,
    "targetWeight": 70.0,
    "primaryNutritionGoal": "WeightLoss",
    "activityLevel": "Active"
  },
  "message": "User profile updated successfully"
}
```

---

#### 1.7. Đổi mật khẩu

**Endpoint:** `PUT /api/auth/change_password`

**Authentication:** Cần (Bearer Token)

**Request Body:**
```json
{
  "currentPassword": "string (required)",
  "newPassword": "string (required, min 6 characters)",
  "confirmPassword": "string (required, must match newPassword)"
}
```

**Example Request:**
```json
{
  "currentPassword": "oldpassword123",
  "newPassword": "newpassword456",
  "confirmPassword": "newpassword456"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": true,
  "message": "Password changed successfully"
}
```

---

### 2. AI APIs

#### 2.1. Phân tích món ăn từ hình ảnh

**Endpoint:** `POST /api/ai/detect_food`

**Authentication:** Cần (Bearer Token)

**Content-Type:** `multipart/form-data`

**Request Body (Form Data):**
- `Image` (file, required): File hình ảnh món ăn (jpg, jpeg, png, gif)

**Example Request (cURL):**
```bash
curl -X POST "http://localhost:80/api/ai/detect_food" \
  -H "Authorization: Bearer <your-token>" \
  -F "Image=@/path/to/food-image.jpg"
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "id": 1,
    "name": "Grilled Chicken Salad",
    "description": "Healthy grilled chicken with fresh vegetables",
    "imageUrl": "https://localhost/uploads/food-123.jpg",
    "preparationTimeMinutes": 15,
    "cookingTimeMinutes": 20,
    "calories": 350.5,
    "protein": 30.0,
    "carbohydrates": 25.0,
    "fat": 12.5,
    "fiber": 8.0,
    "instructions": [
      "Prepare vegetables",
      "Grill chicken",
      "Mix ingredients"
    ],
    "tips": [
      "Use fresh vegetables",
      "Cook chicken thoroughly"
    ],
    "difficultyLevel": 2,
    "mealType": "Lunch",
    "consumedAt": "2025-12-01T12:00:00Z",
    "ingredients": [
      {
        "ingredientId": 1,
        "quantity": 200,
        "unit": "Gram",
        "ingredientName": "Chicken Breast"
      }
    ]
  },
  "message": "Food analysis successful"
}
```

---

#### 2.2. Phân tích nguyên liệu từ hình ảnh

**Endpoint:** `POST /api/ai/detect_ingredient`

**Authentication:** Cần (Bearer Token)

**Content-Type:** `multipart/form-data`

**Request Body (Form Data):**
- `Image` (file, required): File hình ảnh nguyên liệu (jpg, jpeg, png, gif)

**Example Request (cURL):**
```bash
curl -X POST "http://localhost:80/api/ai/detect_ingredient" \
  -H "Authorization: Bearer <your-token>" \
  -F "Image=@/path/to/ingredient-image.jpg"
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "id": 1,
    "name": "Tomato",
    "description": "Fresh red tomato",
    "quantity": 500,
    "unit": "Gram",
    "category": "Vegetables",
    "expiryDate": "2025-12-10T00:00:00Z",
    "imageUrl": "https://localhost/uploads/ingredient-123.jpg"
  },
  "message": "Ingredient analysis successful"
}
```

---

### 3. Food APIs

#### 3.1. Tạo món ăn

**Endpoint:** `POST /api/food`

**Authentication:** Cần (Bearer Token)

**Content-Type:** `multipart/form-data`

**Request Body (Form Data):**
- `Name` (string, required, max 200 characters): Tên món ăn
- `Description` (string, optional, max 1000 characters): Mô tả món ăn
- `Image` (file, optional): Hình ảnh món ăn
- `PreparationTimeMinutes` (int, required): Thời gian chuẩn bị (phút)
- `CookingTimeMinutes` (int, required): Thời gian nấu (phút)
- `Calories` (decimal, required): Lượng calo
- `Protein` (decimal, required): Lượng protein (gram)
- `Carbohydrates` (decimal, required): Lượng carbohydrate (gram)
- `Fat` (decimal, required): Lượng chất béo (gram)
- `Fiber` (decimal, required): Lượng chất xơ (gram)
- `Instructions` (array of strings, required): Các bước hướng dẫn
- `Tips` (array of strings, required): Mẹo nấu ăn
- `DifficultyLevel` (int, optional, 1-5): Độ khó (mặc định: 1)
- `MealDate` (datetime, optional): Ngày ăn (mặc định: hiện tại)
- `MealType` (enum, optional): Loại bữa ăn - Breakfast, Lunch, Dinner, Snack (mặc định: Breakfast)
- `ConsumedAt` (datetime, optional): Thời gian đã ăn
- `Ingredients` (JSON string, optional): Danh sách nguyên liệu

**Example Request (cURL):**
```bash
curl -X POST "http://localhost:80/api/food" \
  -H "Authorization: Bearer <your-token>" \
  -F "Name=Grilled Chicken Salad" \
  -F "Description=Healthy grilled chicken with fresh vegetables" \
  -F "Image=@/path/to/image.jpg" \
  -F "PreparationTimeMinutes=15" \
  -F "CookingTimeMinutes=20" \
  -F "Calories=350.5" \
  -F "Protein=30.0" \
  -F "Carbohydrates=25.0" \
  -F "Fat=12.5" \
  -F "Fiber=8.0" \
  -F "Instructions=[\"Prepare vegetables\",\"Grill chicken\",\"Mix ingredients\"]" \
  -F "Tips=[\"Use fresh vegetables\",\"Cook chicken thoroughly\"]" \
  -F "DifficultyLevel=2" \
  -F "MealType=Lunch" \
  -F "Ingredients=[{\"ingredientId\":1,\"quantity\":200,\"unit\":\"Gram\"}]"
```

**Response (201 Created):**
```json
{
  "success": true,
  "data": {
    "id": 1,
    "name": "Grilled Chicken Salad",
    "description": "Healthy grilled chicken with fresh vegetables",
    "imageUrl": "https://localhost/uploads/food-123.jpg",
    "preparationTimeMinutes": 15,
    "cookingTimeMinutes": 20,
    "calories": 350.5,
    "protein": 30.0,
    "carbohydrates": 25.0,
    "fat": 12.5,
    "fiber": 8.0,
    "instructions": ["Prepare vegetables", "Grill chicken", "Mix ingredients"],
    "tips": ["Use fresh vegetables", "Cook chicken thoroughly"],
    "difficultyLevel": 2,
    "mealType": "Lunch",
    "consumedAt": "2025-12-01T12:00:00Z",
    "ingredients": [
      {
        "ingredientId": 1,
        "quantity": 200,
        "unit": "Gram",
        "ingredientName": "Chicken Breast"
      }
    ]
  },
  "message": "Food created successfully"
}
```

---

#### 3.2. Cập nhật món ăn

**Endpoint:** `PUT /api/food`

**Authentication:** Cần (Bearer Token)

**Content-Type:** `multipart/form-data`

**Request Body (Form Data):**
- Tất cả các field giống như tạo món ăn, thêm:
- `Id` (int, required): ID của món ăn cần cập nhật

**Example Request (cURL):**
```bash
curl -X PUT "http://localhost:80/api/food" \
  -H "Authorization: Bearer <your-token>" \
  -F "Id=1" \
  -F "Name=Updated Grilled Chicken Salad" \
  -F "Calories=380.0"
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "id": 1,
    "name": "Updated Grilled Chicken Salad",
    "calories": 380.0,
    ...
  },
  "message": "Food updated successfully"
}
```

---

#### 3.3. Xóa món ăn

**Endpoint:** `DELETE /api/food`

**Authentication:** Cần (Bearer Token)

**Content-Type:** `application/json`

**Request Body:**
```json
{
  "id": 1
}
```

**Example Request:**
```json
{
  "id": 1
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": true,
  "message": "Food deleted successfully"
}
```

---

#### 3.4. Lấy thông tin món ăn theo ID

**Endpoint:** `GET /api/food/{id}`

**Authentication:** Cần (Bearer Token)

**Path Parameters:**
- `id` (int, required): ID của món ăn

**Example Request:**
```
GET /api/food/1
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "id": 1,
    "name": "Grilled Chicken Salad",
    "description": "Healthy grilled chicken with fresh vegetables",
    "imageUrl": "https://localhost/uploads/food-123.jpg",
    "preparationTimeMinutes": 15,
    "cookingTimeMinutes": 20,
    "calories": 350.5,
    "protein": 30.0,
    "carbohydrates": 25.0,
    "fat": 12.5,
    "fiber": 8.0,
    "instructions": ["Prepare vegetables", "Grill chicken", "Mix ingredients"],
    "tips": ["Use fresh vegetables", "Cook chicken thoroughly"],
    "difficultyLevel": 2,
    "mealType": "Lunch",
    "consumedAt": "2025-12-01T12:00:00Z",
    "ingredients": [
      {
        "ingredientId": 1,
        "quantity": 200,
        "unit": "Gram",
        "ingredientName": "Chicken Breast"
      }
    ]
  },
  "message": "Food found"
}
```

---

#### 3.5. Gợi ý món ăn

**Endpoint:** `POST /api/food/suggestions`

**Authentication:** Cần (Bearer Token)

**Content-Type:** `application/json`

**Request Body:**
```json
{
  "userInformation": {
    "gender": "Male | Female | Other (optional)",
    "dateOfBirth": "datetime (optional, ISO 8601 format)",
    "height": "decimal (optional, in cm)",
    "weight": "decimal (optional, in kg)",
    "targetWeight": "decimal (optional, in kg)",
    "primaryNutritionGoal": "WeightLoss | WeightGain | Maintenance | MuscleGain (optional)",
    "activityLevel": "Sedentary | Light | Moderate | Active | VeryActive (optional)"
  }
}
```

**Example Request:**
```json
{
  "userInformation": {
    "gender": "Male",
    "height": 175.5,
    "weight": 70.0,
    "targetWeight": 65.0,
    "primaryNutritionGoal": "WeightLoss",
    "activityLevel": "Moderate"
  }
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "name": "Grilled Chicken Salad",
      "description": "Healthy grilled chicken with fresh vegetables",
      "calories": 350.5,
      "protein": 30.0,
      "ingredients": [
        {
          "ingredientId": 1,
          "quantity": 200,
          "unit": "Gram",
          "ingredientName": "Chicken Breast"
        }
      ]
    }
  ],
  "message": "Food suggestions retrieved successfully",
  "metadata": {
    "count": ["1"]
  }
}
```

---

#### 3.6. Gợi ý công thức nấu ăn

**Endpoint:** `POST /api/food/recipes`

**Authentication:** Cần (Bearer Token)

**Content-Type:** `application/json`

**Request Body:**
```json
{
  "ingredients": [
    {
      "ingredientId": 1,
      "quantity": 200,
      "unit": "Gram"
    }
  ]
}
```

**Example Request:**
```json
{
  "ingredients": [
    {
      "ingredientId": 1,
      "quantity": 200,
      "unit": "Gram"
    },
    {
      "ingredientId": 2,
      "quantity": 100,
      "unit": "Gram"
    }
  ]
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "id": 1,
    "name": "Grilled Chicken Salad",
    "description": "Healthy grilled chicken with fresh vegetables",
    "instructions": ["Prepare vegetables", "Grill chicken", "Mix ingredients"],
    "tips": ["Use fresh vegetables", "Cook chicken thoroughly"],
    "ingredients": [
      {
        "ingredientId": 1,
        "quantity": 200,
        "unit": "Gram",
        "ingredientName": "Chicken Breast"
      }
    ]
  },
  "message": "Recipe retrieved successfully"
}
```

---

### 4. Ingredient APIs

#### 4.1. Tạo nguyên liệu

**Endpoint:** `POST /api/ingredient`

**Authentication:** Cần (Bearer Token)

**Content-Type:** `multipart/form-data`

**Request Body (Form Data):**
- `Name` (string, required, max 200 characters): Tên nguyên liệu
- `Description` (string, optional, max 1000 characters): Mô tả
- `Quantity` (decimal, required, >= 0): Số lượng
- `Unit` (enum, required): Đơn vị - Xem danh sách đầy đủ ở phần Enums bên dưới (ví dụ: Gram, Kilogram, Piece, Cup, Tablespoon, Teaspoon, Clove, Bunch, etc.)
- `Category` (enum, required): Danh mục - Xem danh sách đầy đủ ở phần Enums bên dưới (ví dụ: Vegetables, Fruits, Meat, Seafood, Dairy, Grains, Spices, etc.)
- `ExpiryDate` (datetime, required, ISO 8601 format): Ngày hết hạn
- `Image` (file, optional): Hình ảnh nguyên liệu

**Example Request (cURL):**
```bash
curl -X POST "http://localhost:80/api/ingredient" \
  -H "Authorization: Bearer <your-token>" \
  -F "Name=Tomato" \
  -F "Description=Fresh red tomato" \
  -F "Quantity=500" \
  -F "Unit=Gram" \
  -F "Category=Vegetables" \
  -F "ExpiryDate=2025-12-10T00:00:00Z" \
  -F "Image=@/path/to/tomato.jpg"
```

**Response (201 Created):**
```json
{
  "success": true,
  "data": {
    "id": 1,
    "name": "Tomato",
    "description": "Fresh red tomato",
    "quantity": 500,
    "unit": "Gram",
    "category": "Vegetables",
    "expiryDate": "2025-12-10T00:00:00Z",
    "imageUrl": "https://localhost/uploads/ingredient-123.jpg"
  },
  "message": "Ingredient created successfully"
}
```

---

#### 4.2. Cập nhật nguyên liệu

**Endpoint:** `PUT /api/ingredient/{id}`

**Authentication:** Cần (Bearer Token)

**Content-Type:** `multipart/form-data`

**Path Parameters:**
- `id` (int, required): ID của nguyên liệu

**Request Body (Form Data):**
- Tất cả các field giống như tạo nguyên liệu

**Example Request (cURL):**
```bash
curl -X PUT "http://localhost:80/api/ingredient/1" \
  -H "Authorization: Bearer <your-token>" \
  -F "Name=Updated Tomato" \
  -F "Quantity=600"
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "id": 1,
    "name": "Updated Tomato",
    "quantity": 600,
    ...
  },
  "message": "Ingredient updated successfully"
}
```

---

#### 4.3. Xóa nguyên liệu

**Endpoint:** `DELETE /api/ingredient/{id}`

**Authentication:** Cần (Bearer Token)

**Content-Type:** `application/json`

**Path Parameters:**
- `id` (int, required): ID của nguyên liệu

**Request Body:**
```json
{
  "id": 1
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": true,
  "message": "Ingredient deleted successfully"
}
```

---

#### 4.4. Lấy danh sách nguyên liệu

**Endpoint:** `GET /api/ingredient`

**Authentication:** Cần (Bearer Token)

**Query Parameters:**
- `page` (int, optional, default: 1): Số trang
- `pageSize` (int, optional, default: 10): Số lượng mỗi trang
- `category` (enum, optional): Lọc theo danh mục - Xem danh sách đầy đủ ở phần Enums bên dưới (ví dụ: Vegetables, Fruits, Meat, Seafood, Dairy, Grains, Spices, etc.)
- `search` (string, optional): Tìm kiếm theo tên
- `expiryDateFrom` (datetime, optional): Lọc từ ngày hết hạn
- `expiryDateTo` (datetime, optional): Lọc đến ngày hết hạn

**Example Request:**
```
GET /api/ingredient?page=1&pageSize=10&category=Vegetables&search=tomato
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "ingredients": [
      {
        "id": 1,
        "name": "Tomato",
        "description": "Fresh red tomato",
        "quantity": 500,
        "unit": "Gram",
        "category": "Vegetables",
        "expiryDate": "2025-12-10T00:00:00Z",
        "imageUrl": "https://localhost/uploads/ingredient-123.jpg"
      }
    ],
    "totalCount": 1,
    "page": 1,
    "pageSize": 10,
    "totalPages": 1
  },
  "message": "Ingredients retrieved successfully"
}
```

---

#### 4.5. Lấy thông tin nguyên liệu theo ID

**Endpoint:** `GET /api/ingredient/{id}`

**Authentication:** Cần (Bearer Token)

**Path Parameters:**
- `id` (int, required): ID của nguyên liệu

**Example Request:**
```
GET /api/ingredient/1
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "id": 1,
    "name": "Tomato",
    "description": "Fresh red tomato",
    "quantity": 500,
    "unit": "Gram",
    "category": "Vegetables",
    "expiryDate": "2025-12-10T00:00:00Z",
    "imageUrl": "https://localhost/uploads/ingredient-123.jpg"
  },
  "message": "Ingredient found"
}
```

---

### 5. Nutrition APIs

#### 5.1. Tóm tắt dinh dưỡng hàng ngày

**Endpoint:** `POST /api/nutrition/daily`

**Authentication:** Cần (Bearer Token)

**Content-Type:** `application/json`

**Request Body:**
```json
{
  "currentDate": "datetime (optional, ISO 8601 format, default: today)",
  "userInformationDto": {
    "gender": "Male | Female | Other (optional)",
    "dateOfBirth": "datetime (optional, ISO 8601 format)",
    "height": "decimal (optional, in cm)",
    "weight": "decimal (optional, in kg)",
    "targetWeight": "decimal (optional, in kg)",
    "primaryNutritionGoal": "WeightLoss | WeightGain | Maintenance | MuscleGain (optional)",
    "activityLevel": "Sedentary | Light | Moderate | Active | VeryActive (optional)"
  }
}
```

**Example Request:**
```json
{
  "currentDate": "2025-12-01T00:00:00Z",
  "userInformationDto": {
    "gender": "Male",
    "height": 175.5,
    "weight": 70.0,
    "primaryNutritionGoal": "WeightLoss",
    "activityLevel": "Moderate"
  }
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "totalCalories": 2000.0,
    "totalProtein": 150.0,
    "totalCarbohydrates": 200.0,
    "totalFat": 66.7,
    "totalFiber": 30.0,
    "mealBreakdown": [
      {
        "mealType": "Breakfast",
        "calories": 500.0,
        "protein": 30.0,
        "carbohydrates": 50.0,
        "fat": 20.0,
        "fiber": 10.0,
        "foods": [
          {
            "foodId": 1,
            "foodName": "Grilled Chicken Salad",
            "calories": 500.0,
            "protein": 30.0,
            "carbohydrates": 50.0,
            "fat": 20.0,
            "fiber": 10.0
          }
        ]
      }
    ]
  },
  "message": "Daily nutrition summary retrieved successfully",
  "metadata": {
    "date": ["2025-12-01"]
  }
}
```

---

#### 5.2. Tóm tắt dinh dưỡng hàng tuần

**Endpoint:** `POST /api/nutrition/weekly`

**Authentication:** Cần (Bearer Token)

**Content-Type:** `application/json`

**Request Body:**
```json
{
  "startDate": "datetime (required, ISO 8601 format)",
  "endDate": "datetime (required, ISO 8601 format)",
  "userInformationDto": {
    "gender": "Male | Female | Other (optional)",
    "dateOfBirth": "datetime (optional, ISO 8601 format)",
    "height": "decimal (optional, in cm)",
    "weight": "decimal (optional, in kg)",
    "targetWeight": "decimal (optional, in kg)",
    "primaryNutritionGoal": "WeightLoss | WeightGain | Maintenance | MuscleGain (optional)",
    "activityLevel": "Sedentary | Light | Moderate | Active | VeryActive (optional)"
  }
}
```

**Example Request:**
```json
{
  "startDate": "2025-11-25T00:00:00Z",
  "endDate": "2025-12-01T00:00:00Z",
  "userInformationDto": {
    "gender": "Male",
    "height": 175.5,
    "weight": 70.0
  }
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "totalCalories": 14000.0,
    "totalProtein": 1050.0,
    "totalCarbohydrates": 1400.0,
    "totalFat": 466.7,
    "totalFiber": 210.0,
    "averageDailyCalories": 2000.0,
    "dailyBreakdown": [
      {
        "date": "2025-11-25T00:00:00Z",
        "totalCalories": 2000.0,
        "totalProtein": 150.0,
        "totalCarbohydrates": 200.0,
        "totalFat": 66.7,
        "totalFiber": 30.0,
        "mealBreakdown": [...]
      }
    ]
  },
  "message": "Weekly nutrition summary retrieved successfully",
  "metadata": {
    "userId": ["1"],
    "startDate": ["2025-11-25"],
    "endDate": ["2025-12-01"]
  }
}
```

---

#### 5.3. Tổng quan dinh dưỡng

**Endpoint:** `POST /api/nutrition/overview`

**Authentication:** Cần (Bearer Token)

**Content-Type:** `application/json`

**Request Body:**
```json
{
  "gender": "Male | Female | Other (optional)",
  "dateOfBirth": "datetime (optional, ISO 8601 format)",
  "height": "decimal (optional, in cm)",
  "weight": "decimal (optional, in kg)",
  "targetWeight": "decimal (optional, in kg)",
  "primaryNutritionGoal": "WeightLoss | WeightGain | Maintenance | MuscleGain (optional)",
  "activityLevel": "Sedentary | Light | Moderate | Active | VeryActive (optional)"
}
```

**Example Request:**
```json
{
  "gender": "Male",
  "height": 175.5,
  "weight": 70.0,
  "targetWeight": 65.0,
  "primaryNutritionGoal": "WeightLoss",
  "activityLevel": "Moderate"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "totalCalories": 50000.0,
    "totalProtein": 3750.0,
    "totalCarbohydrates": 5000.0,
    "totalFat": 1666.7,
    "totalFiber": 750.0,
    "averageDailyCalories": 2000.0,
    "recommendedDailyCalories": 2200.0,
    "calorieDeficit": 200.0
  },
  "message": "Total nutrition summary retrieved successfully",
  "metadata": {
    "userId": ["1"]
  }
}
```

---

## Enums

### MealType
- `Breakfast`
- `Lunch`
- `Dinner`
- `Snack`

### IngredientUnit

#### Weight Units (Đơn vị khối lượng)
- `Kilogram` - Kilôgam
- `Gram` - Gam
- `Pound` - Pao (lb)
- `Ounce` - Ao-xơ (oz)

#### Volume Units (Đơn vị thể tích)
- `Liter` - Lít
- `Milliliter` - Mililít
- `Cup` - Cốc
- `Tablespoon` - Thìa canh
- `Teaspoon` - Thìa cà phê
- `FluidOunce` - Fluid ounce (fl oz)
- `Pint` - Pint
- `Quart` - Quart
- `Gallon` - Gallon

#### Countable Units (Đơn vị đếm)
- `Piece` - Cái, quả, miếng
- `Slice` - Lát
- `Clove` - Tép (tỏi, hành)
- `Head` - Củ, đầu (bắp cải, tỏi)
- `Bunch` - Bó (rau, hành)
- `Stalk` - Cọng (cần tây, rau muống)
- `Wedge` - Miếng (cam, chanh)
- `Sheet` - Lá (lá bánh tráng, lá nho)
- `Pod` - Quả (đậu bắp, đậu đũa)

#### Container Units (Đơn vị bao bì)
- `Box` - Hộp
- `Can` - Lon
- `Bottle` - Chai
- `Package` - Gói
- `Bag` - Túi
- `Jar` - Lọ
- `Tube` - Tuýp
- `Carton` - Thùng carton

#### Small Quantity Units (Đơn vị nhỏ)
- `Pinch` - Nhúm (muối, đường)
- `Dash` - Chút (nước mắm, giấm)
- `Drop` - Giọt

#### Other Units (Đơn vị khác)
- `Serving` - Phần
- `Portion` - Suất
- `Other` - Khác

### IngredientCategory

#### Protein Sources (Nguồn đạm)
- `Meat` - Thịt
- `Poultry` - Thịt gia cầm
- `Seafood` - Hải sản
- `Eggs` - Trứng
- `Dairy` - Sữa và sản phẩm từ sữa
- `Legumes` - Đậu, đỗ
- `Nuts` - Hạt, quả hạch
- `Tofu` - Đậu phụ

#### Vegetables (Rau củ)
- `Vegetables` - Rau củ
- `LeafyGreens` - Rau lá xanh
- `RootVegetables` - Rau củ
- `Herbs` - Rau thơm

#### Fruits (Trái cây)
- `Fruits` - Trái cây
- `Berries` - Quả mọng
- `Citrus` - Cam quýt

#### Grains & Starches (Ngũ cốc & tinh bột)
- `Grains` - Ngũ cốc
- `Rice` - Gạo
- `Pasta` - Mì, pasta
- `Bread` - Bánh mì
- `Noodles` - Mì, phở, bún

#### Cooking Essentials (Nguyên liệu nấu ăn)
- `Oils` - Dầu ăn
- `Vinegar` - Giấm
- `Sauces` - Nước sốt
- `Condiments` - Gia vị chấm
- `Spices` - Gia vị khô
- `Seasonings` - Gia vị nêm

#### Baking (Đồ làm bánh)
- `Baking` - Đồ làm bánh
- `Flour` - Bột
- `Sugar` - Đường
- `Sweeteners` - Chất tạo ngọt

#### Beverages (Đồ uống)
- `Beverages` - Đồ uống
- `Alcoholic` - Đồ uống có cồn

#### Processed Foods (Thực phẩm chế biến)
- `Snacks` - Đồ ăn vặt
- `Frozen` - Đồ đông lạnh
- `Canned` - Đồ hộp
- `Processed` - Thực phẩm chế biến sẵn

#### Other (Khác)
- `Other` - Khác

### Gender
- `Male`
- `Female`
- `Other`

### NutritionGoal
- `WeightLoss`
- `WeightGain`
- `Maintenance`
- `MuscleGain`

### ActivityLevel
- `Sedentary`
- `Light`
- `Moderate`
- `Active`
- `VeryActive`

---

## Status Codes

- `200 OK`: Request thành công
- `201 Created`: Tạo resource thành công
- `400 Bad Request`: Request không hợp lệ
- `401 Unauthorized`: Chưa đăng nhập hoặc token không hợp lệ
- `403 Forbidden`: Không có quyền truy cập
- `404 Not Found`: Resource không tồn tại
- `500 Internal Server Error`: Lỗi server
- `503 Service Unavailable`: Service tạm thời không khả dụng

---

## Error Handling

Khi có lỗi xảy ra, API sẽ trả về response với format:

```json
{
  "success": false,
  "data": null,
  "message": "Error message",
  "metadata": {
    "errors": [
      "Error detail 1",
      "Error detail 2"
    ]
  }
}
```

### Validation Errors

Khi có lỗi validation, `metadata.errors` sẽ chứa danh sách các lỗi:

```json
{
  "success": false,
  "data": null,
  "message": "Invalid model state",
  "metadata": {
    "errors": [
      "Name is required",
      "Calories must be greater than 0"
    ]
  }
}
```

---

## Notes

1. **DateTime Format**: Tất cả datetime đều sử dụng ISO 8601 format (UTC): `2025-12-01T12:00:00Z`

2. **File Upload**: 
   - Chỉ chấp nhận file ảnh: jpg, jpeg, png, gif
   - Kích thước tối đa: 10MB (có thể cấu hình)
   - File sẽ được lưu tại `/uploads/` và có thể truy cập qua URL: `https://localhost/uploads/{filename}`

3. **Pagination**: 
   - Mặc định: `page=1`, `pageSize=10`
   - Tối đa: `pageSize=100`

4. **Rate Limiting**: Hiện tại chưa có rate limiting, nhưng nên implement trong production

5. **CORS**: API hỗ trợ CORS, cho phép tất cả origins (có thể cấu hình)

---

## Health Check

**Endpoint:** `GET /health`

**Authentication:** Không cần

**Response (200 OK):**
```json
{
  "status": "healthy",
  "timestamp": "2025-12-01T12:00:00Z",
  "version": "1.0.0"
}
```

---

## Swagger Documentation

Khi chạy ứng dụng, có thể truy cập Swagger UI tại:
- Development: `http://localhost:5000/swagger`
- Production: `http://localhost:80/swagger`

Swagger UI cung cấp interactive documentation và có thể test API trực tiếp từ browser.

