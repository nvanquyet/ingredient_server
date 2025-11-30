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

## ⚠️ Quan trọng: Logic Tự Động của Backend

### 1. Tự Động Trừ Nguyên Liệu Khi Tạo Món Ăn

**Khi tạo món ăn (POST /api/food):**
- Backend **tự động trừ** số lượng nguyên liệu từ kho của user
- Nếu nguyên liệu không đủ: số lượng sẽ được set về 0 (không throw error)
- Response sẽ trả về `remainingQuantity` cho mỗi nguyên liệu (số lượng còn lại sau khi trừ)

**Ví dụ:**
- User có 500g thịt gà
- Tạo món ăn cần 200g thịt gà
- Sau khi tạo: thịt gà còn 300g
- Response: `{ "ingredientId": 1, "quantity": 200, "remainingQuantity": 300 }`

### 2. Tự Động Cập Nhật Meal Nutrition

**Khi tạo món ăn:**
- Backend **tự động cộng** calories, protein, carbs, fat, fiber vào `Meal.TotalCalories`, `Meal.TotalProtein`, etc.
- Meal được tìm theo `MealDate` và `MealType`, hoặc tạo mới nếu chưa có

**Khi sửa món ăn:**
- Backend **tự động trừ** nutrition cũ từ Meal cũ
- Backend **tự động cộng** nutrition mới vào Meal mới

**Khi xóa món ăn:**
- Backend **tự động trừ** nutrition từ Meal
- Nếu Meal không còn Food nào → Meal sẽ bị xóa tự động

### 3. Tự Động Hoàn Lại Nguyên Liệu

**Khi sửa món ăn:**
- Backend **tự động hoàn lại** nguyên liệu cũ (cộng lại vào kho)
- Sau đó **trừ** nguyên liệu mới

**Khi xóa món ăn:**
- Backend **tự động hoàn lại** tất cả nguyên liệu đã sử dụng (cộng lại vào kho)

### 4. Cache System cho Recipe Generation

**Khi generate recipe (POST /api/food/recipes):**
- Backend **tự động check cache** trước khi gọi Azure OpenAI API
- Nếu có trong cache: trả về ngay (nhanh hơn, không tốn API call)
- Nếu không có: gọi Azure OpenAI → lưu vào cache → trả về
- Cache là **public** (tất cả users dùng chung)

**Ingredient Mapping:**
- Cache lưu ingredients theo **tên** (không phải ID vì mỗi user có ID khác nhau)
- Khi trả về recipe từ cache, backend **tự động map** ingredients:
  - Tìm ingredient của user theo tên
  - Nếu user có: `ingredientId = userIngredientId` (> 0)
  - Nếu user không có: `ingredientId = 0` (frontend cần check và báo user thêm ingredient)

### 4. Detect API Chỉ Trả Về Gợi Ý

**Detect món ăn (POST /api/ai/detect_food) và Detect nguyên liệu (POST /api/ai/detect_ingredient):**
- Chỉ trả về **gợi ý** từ AI, **KHÔNG tự động tạo** Food/Ingredient
- Frontend cần sử dụng response để gọi API tạo Food/Ingredient riêng
- `ingredientId` trong response của detect_food sẽ là `0` (chưa link với nguyên liệu thực tế)

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

**Mô tả:**
- Phân tích hình ảnh món ăn và trả về thông tin gợi ý (tên, mô tả, calories, ingredients, etc.)
- **LƯU Ý:** API này chỉ trả về **gợi ý**, **KHÔNG tự động tạo** Food trong database
- Frontend cần sử dụng response để gọi API `POST /api/food` để tạo Food thực tế
- `ingredientId` trong response sẽ là `0` (chưa link với nguyên liệu thực tế)

**Request Body (Form Data):**
- `Image` (file, required): File hình ảnh món ăn (jpg, jpeg, png, gif, webp)

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
    "id": 0,
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
        "ingredientId": 0,
        "quantity": 200,
        "unit": "Gram",
        "ingredientName": "Chicken Breast"
      }
    ]
  },
  "message": "Food analysis successful"
}
```

**Luồng sử dụng:**
1. User upload ảnh món ăn → Gọi `POST /api/ai/detect_food`
2. Nhận response với thông tin gợi ý
3. User xem và chỉnh sửa thông tin (nếu cần)
4. User chọn nguyên liệu từ kho (map `ingredientId` từ 0 → ID thực tế)
5. Gọi `POST /api/food` với thông tin đã chỉnh sửa để tạo Food thực tế

---

#### 2.2. Phân tích nguyên liệu từ hình ảnh

**Endpoint:** `POST /api/ai/detect_ingredient`

**Authentication:** Cần (Bearer Token)

**Content-Type:** `multipart/form-data`

**Mô tả:**
- Phân tích hình ảnh nguyên liệu và trả về thông tin gợi ý (tên, số lượng, đơn vị, category, expiryDate)
- **LƯU Ý:** API này chỉ trả về **gợi ý**, **KHÔNG tự động tạo** Ingredient trong database
- Frontend cần sử dụng response để gọi API `POST /api/ingredient` để tạo Ingredient thực tế

**Request Body (Form Data):**
- `Image` (file, required): File hình ảnh nguyên liệu (jpg, jpeg, png, gif, webp)

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
    "id": 0,
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

**Luồng sử dụng:**
1. User upload ảnh nguyên liệu → Gọi `POST /api/ai/detect_ingredient`
2. Nhận response với thông tin gợi ý
3. User xem và chỉnh sửa thông tin (nếu cần)
4. Gọi `POST /api/ingredient` với thông tin đã chỉnh sửa để tạo Ingredient thực tế

---

### 3. Food APIs

#### 3.1. Tạo món ăn

**Endpoint:** `POST /api/food`

**Authentication:** Cần (Bearer Token)

**Content-Type:** `multipart/form-data`

**Mô tả:**
- Tạo món ăn mới và **tự động trừ nguyên liệu** từ kho của user
- **Tự động cập nhật** `Meal.TotalCalories`, `Meal.TotalProtein`, etc. cho Meal tương ứng
- Nếu nguyên liệu không đủ: số lượng sẽ được set về 0 (không throw error)
- Response trả về `remainingQuantity` cho mỗi nguyên liệu (số lượng còn lại sau khi trừ)

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
- `Instructions` (JSON string, required): Các bước hướng dẫn (array of strings)
- `Tips` (JSON string, required): Mẹo nấu ăn (array of strings)
- `DifficultyLevel` (int, optional, 1-5): Độ khó (mặc định: 1)
- `MealDate` (datetime string, optional): Ngày ăn (ISO 8601 format, mặc định: hiện tại)
- `MealType` (enum string, optional): Loại bữa ăn - `Breakfast`, `Lunch`, `Dinner`, `Snack` (mặc định: `Breakfast`)
- `ConsumedAt` (datetime string, optional): Thời gian đã ăn (ISO 8601 format)
- `Ingredients` (JSON string, optional): Danh sách nguyên liệu (array of objects)

**Format của `Ingredients` (JSON string):**
```json
[
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
```

**⚠️ Lưu ý về `ingredientId`:**
- `ingredientId > 0`: ID của ingredient trong kho của user (sẽ được trừ khi tạo food)
- `ingredientId = 0`: User không có ingredient này (thường từ cache/recipe suggestion)
  - Backend sẽ **skip** ingredients có `ingredientId = 0` khi tạo food
  - Frontend nên check và báo user cần thêm ingredient vào kho trước

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
  -F "MealDate=2025-12-01T00:00:00Z" \
  -F "Ingredients=[{\"ingredientId\":1,\"quantity\":200,\"unit\":\"Gram\"},{\"ingredientId\":2,\"quantity\":100,\"unit\":\"Gram\"}]"
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
    "mealDate": "2025-12-01T00:00:00Z",
    "consumedAt": "2025-12-01T12:00:00Z",
    "ingredients": [
      {
        "ingredientId": 1,
        "quantity": 200,
        "unit": "Gram",
        "ingredientName": "Chicken Breast",
        "remainingQuantity": 300
      },
      {
        "ingredientId": 2,
        "quantity": 100,
        "unit": "Gram",
        "ingredientName": "Lettuce",
        "remainingQuantity": 400
      }
    ]
  },
  "message": "Food created successfully"
}
```

**Giải thích Response:**
- `remainingQuantity`: Số lượng nguyên liệu còn lại sau khi trừ (ví dụ: có 500g, dùng 200g → còn 300g)
- Nếu `remainingQuantity = 0`: Nguyên liệu đã hết hoặc không đủ (đã được set về 0)

**Luồng xử lý tự động:**
1. Lưu Food vào database
2. Tìm hoặc tạo Meal theo `MealDate` và `MealType`
3. Link Food với Meal
4. **Tự động cộng** `calories`, `protein`, `carbs`, `fat`, `fiber` vào `Meal.TotalCalories`, `Meal.TotalProtein`, etc.
5. **Tự động trừ** số lượng nguyên liệu từ kho của user
6. Trả về response với `remainingQuantity`

---

#### 3.2. Cập nhật món ăn

**Endpoint:** `PUT /api/food`

**Authentication:** Cần (Bearer Token)

**Content-Type:** `multipart/form-data`

**Mô tả:**
- Cập nhật món ăn và **tự động xử lý nguyên liệu**:
  - **Hoàn lại** nguyên liệu cũ (cộng lại vào kho)
  - **Trừ** nguyên liệu mới (trừ từ kho)
- **Tự động cập nhật** Meal nutrition:
  - **Trừ** nutrition cũ từ Meal cũ
  - **Cộng** nutrition mới vào Meal mới
- Response trả về `remainingQuantity` cho mỗi nguyên liệu

**Request Body (Form Data):**
- `Id` (int, required): ID của món ăn cần cập nhật
- Tất cả các field giống như tạo món ăn (có thể chỉ gửi các field cần cập nhật)

**Example Request (cURL):**
```bash
curl -X PUT "http://localhost:80/api/food" \
  -H "Authorization: Bearer <your-token>" \
  -F "Id=1" \
  -F "Name=Updated Grilled Chicken Salad" \
  -F "Calories=380.0" \
  -F "Ingredients=[{\"ingredientId\":1,\"quantity\":250,\"unit\":\"Gram\"}]"
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "id": 1,
    "name": "Updated Grilled Chicken Salad",
    "calories": 380.0,
    "protein": 35.0,
    "ingredients": [
      {
        "ingredientId": 1,
        "quantity": 250,
        "unit": "Gram",
        "ingredientName": "Chicken Breast",
        "remainingQuantity": 250
      }
    ]
  },
  "message": "Food updated successfully"
}
```

**Luồng xử lý tự động:**
1. Lấy Food cũ với ingredients
2. Lấy Meal cũ (nếu có)
3. **Hoàn lại** nguyên liệu cũ: `Ingredient.Quantity += oldQuantity`
4. Xóa `FoodIngredient` cũ
5. **Trừ** nguyên liệu mới: `Ingredient.Quantity -= newQuantity`
6. Tạo `FoodIngredient` mới
7. **Trừ** nutrition cũ từ Meal cũ (nếu có)
8. **Cộng** nutrition mới vào Meal mới
9. Trả về response với `remainingQuantity`

---

#### 3.3. Xóa món ăn

**Endpoint:** `DELETE /api/food`

**Authentication:** Cần (Bearer Token)

**Content-Type:** `application/json`

**Mô tả:**
- Xóa món ăn và **tự động hoàn lại** tất cả nguyên liệu đã sử dụng (cộng lại vào kho)
- **Tự động trừ** nutrition từ Meal
- Nếu Meal không còn Food nào → Meal sẽ bị xóa tự động

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

**Luồng xử lý tự động:**
1. Lấy Food với ingredients và meals
2. **Trừ** nutrition từ Meal: `Meal.TotalCalories -= Food.Calories`, etc.
3. **Hoàn lại** nguyên liệu: `Ingredient.Quantity += FoodIngredient.Quantity`
4. Xóa relationships (MealFood, FoodIngredient)
5. Xóa ảnh (nếu có)
6. Xóa Food
7. Nếu Meal không còn Food → Xóa Meal

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
    "mealDate": "2025-12-01T00:00:00Z",
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

**Mô tả:**
- Generate công thức nấu ăn từ Azure OpenAI hoặc lấy từ **cache** (nếu đã có)
- Backend **tự động check cache** trước khi gọi Azure OpenAI API
- Nếu có trong cache: trả về ngay (nhanh hơn, không tốn API call)
- Nếu không có: gọi Azure OpenAI → lưu vào cache → trả về
- Cache là **public** (tất cả users dùng chung)
- Ingredients trong response sẽ được **tự động map** sang ingredient ID của user:
  - Nếu user có ingredient: `ingredientId = userIngredientId` (> 0)
  - Nếu user không có: `ingredientId = 0` (frontend cần check và báo user thêm ingredient)

**Request Body:**
```json
{
  "foodName": "Trứng luộc",
  "ingredients": [
    {
      "ingredientId": 1,
      "quantity": 2,
      "unit": "Piece",
      "ingredientName": "Trứng gà"
    }
  ]
}
```

**Request Parameters:**
- `foodName` (string, required): Tên món ăn cần generate recipe
- `ingredients` (array, optional): Danh sách nguyên liệu có sẵn (dùng để tối ưu recipe)

**Example Request:**
```json
{
  "foodName": "Trứng luộc",
  "ingredients": [
    {
      "ingredientId": 1,
      "quantity": 2,
      "unit": "Piece",
      "ingredientName": "Trứng gà"
    }
  ]
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "id": 0,
    "name": "Trứng luộc",
    "description": "Món trứng luộc đơn giản, bổ dưỡng",
    "preparationTimeMinutes": 5,
    "cookingTimeMinutes": 10,
    "calories": 140,
    "protein": 12,
    "carbohydrates": 1,
    "fat": 10,
    "fiber": 0,
    "instructions": ["Đun sôi nước", "Cho trứng vào", "Luộc 10 phút"],
    "tips": ["Luộc vừa lửa", "Ngâm nước lạnh sau khi luộc"],
    "difficultyLevel": 1,
    "ingredients": [
      {
        "ingredientId": 1,
        "quantity": 2,
        "unit": "Piece",
        "ingredientName": "Trứng gà"
      },
      {
        "ingredientId": 0,
        "quantity": 500,
        "unit": "Milliliter",
        "ingredientName": "Nước"
      }
    ]
  },
  "message": "Recipe retrieved successfully"
}
```

**Lưu ý về Ingredients trong Response:**
- `ingredientId > 0`: User có ingredient này, có thể dùng ngay khi tạo food
- `ingredientId = 0`: User **không có** ingredient này, cần thêm vào kho trước khi tạo food
- Frontend nên check `ingredientId = 0` và hiển thị warning/thông báo cho user

---

### 4. Ingredient APIs

#### 4.1. Tạo nguyên liệu

**Endpoint:** `POST /api/ingredient`

**Authentication:** Cần (Bearer Token)

**Content-Type:** `multipart/form-data`

**Mô tả:**
- Tạo nguyên liệu mới trong kho của user
- Không có logic tự động đặc biệt, chỉ tạo mới

**Request Body (Form Data):**
- `Name` (string, required, max 200 characters): Tên nguyên liệu
- `Description` (string, optional, max 1000 characters): Mô tả
- `Quantity` (decimal, required, >= 0): Số lượng
- `Unit` (enum string, required): Đơn vị - Xem danh sách đầy đủ ở phần Enums bên dưới
- `Category` (enum string, required): Danh mục - Xem danh sách đầy đủ ở phần Enums bên dưới
- `ExpiryDate` (datetime string, required, ISO 8601 format): Ngày hết hạn
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

**Mô tả:**
- Cập nhật thông tin nguyên liệu
- Có thể thay đổi `Quantity` trực tiếp (không có logic tự động)

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
    "unit": "Gram",
    "category": "Vegetables",
    "expiryDate": "2025-12-10T00:00:00Z",
    "imageUrl": "https://localhost/uploads/ingredient-123.jpg"
  },
  "message": "Ingredient updated successfully"
}
```

---

#### 4.3. Xóa nguyên liệu

**Endpoint:** `DELETE /api/ingredient/{id}`

**Authentication:** Cần (Bearer Token)

**Content-Type:** `application/json`

**Mô tả:**
- Xóa nguyên liệu khỏi kho
- **LƯU Ý:** Không kiểm tra xem nguyên liệu có đang được sử dụng trong Food không

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
- `category` (enum string, optional): Lọc theo danh mục
- `search` (string, optional): Tìm kiếm theo tên
- `expiryDateFrom` (datetime string, optional, ISO 8601 format): Lọc từ ngày hết hạn
- `expiryDateTo` (datetime string, optional, ISO 8601 format): Lọc đến ngày hết hạn

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

**Mô tả:**
- Lấy tóm tắt dinh dưỡng cho một ngày cụ thể
- Tính toán từ tất cả các Meal trong ngày
- `Meal.TotalCalories`, `Meal.TotalProtein`, etc. đã được cập nhật tự động khi tạo/sửa/xóa Food

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
- `Breakfast` - Bữa sáng
- `Lunch` - Bữa trưa
- `Dinner` - Bữa tối
- `Snack` - Đồ ăn vặt
- `Other` - Khác

### IngredientUnit

#### Weight Units (Đơn vị khối lượng)
- `Kilogram` - Kilôgam (kg)
- `Gram` - Gam (g)
- `Pound` - Pao (lb)
- `Ounce` - Ao-xơ (oz)

#### Volume Units (Đơn vị thể tích)
- `Liter` - Lít (L)
- `Milliliter` - Mililít (mL)
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
- `Vegetables` - Rau củ (tổng quát)
- `LeafyGreens` - Rau lá xanh
- `RootVegetables` - Rau củ (củ)
- `Herbs` - Rau thơm

#### Fruits (Trái cây)
- `Fruits` - Trái cây (tổng quát)
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
- `Male` - Nam
- `Female` - Nữ
- `Other` - Khác

### NutritionGoal
- `WeightLoss` - Giảm cân
- `WeightGain` - Tăng cân
- `Maintenance` - Duy trì
- `MuscleGain` - Tăng cơ

### ActivityLevel
- `Sedentary` - Ít vận động
- `Light` - Vận động nhẹ
- `Moderate` - Vận động vừa
- `Active` - Vận động nhiều
- `VeryActive` - Vận động rất nhiều

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
   - Chỉ chấp nhận file ảnh: jpg, jpeg, png, gif, webp
   - Kích thước tối đa: 10MB (có thể cấu hình trong `.env`)
   - File sẽ được lưu tại `/uploads/` và có thể truy cập qua URL: `https://localhost/uploads/{filename}`

3. **Pagination**: 
   - Mặc định: `page=1`, `pageSize=10`
   - Tối đa: `pageSize=100`

4. **Rate Limiting**: Hiện tại chưa có rate limiting, nhưng nên implement trong production

5. **CORS**: API hỗ trợ CORS, cho phép tất cả origins (có thể cấu hình)

6. **Tự Động Trừ Nguyên Liệu**: 
   - Khi tạo món ăn, backend tự động trừ nguyên liệu từ kho
   - Nếu không đủ: số lượng sẽ được set về 0 (không throw error)
   - Response trả về `remainingQuantity` để frontend hiển thị số lượng còn lại

7. **Tự Động Cập Nhật Meal Nutrition**: 
   - Khi tạo/sửa/xóa món ăn, `Meal.TotalCalories`, `Meal.TotalProtein`, etc. được cập nhật tự động
   - Frontend không cần tính toán lại, chỉ cần gọi API nutrition để lấy kết quả

8. **Cache System cho Recipe Generation**: 
   - Backend tự động cache các recipe đã generate từ Azure OpenAI
   - Cache là public (tất cả users dùng chung)
   - Khi request recipe: check cache trước → nếu có thì trả về ngay (nhanh hơn)
   - Ingredients trong cache được map tự động sang ingredient ID của user

9. **IngredientId = 0**: 
   - Nghĩa là user không có ingredient này trong kho
   - Backend sẽ skip khi tạo food (không trừ quantity)
   - Frontend nên check và báo user cần thêm ingredient vào kho

10. **Detect API**: 
   - Chỉ trả về gợi ý, không tự động tạo Food/Ingredient
   - Frontend cần sử dụng response để gọi API tạo riêng

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

---

## Ví Dụ Luồng Sử Dụng Cho Frontend

### Luồng 1: Tạo món ăn từ ảnh

1. User chụp/upload ảnh món ăn
2. Frontend gọi `POST /api/ai/detect_food` với ảnh
3. Nhận response với thông tin gợi ý (tên, calories, ingredients, etc.)
4. Frontend hiển thị form với thông tin đã điền sẵn
5. User chỉnh sửa thông tin (nếu cần)
6. User chọn nguyên liệu từ kho (map `ingredientId` từ 0 → ID thực tế)
7. Frontend gọi `POST /api/food` với thông tin đã chỉnh sửa
8. Nhận response với `remainingQuantity` cho mỗi nguyên liệu
9. Frontend hiển thị thông báo: "Món ăn đã được tạo. Nguyên liệu còn lại: ..."

### Luồng 2: Tạo nguyên liệu từ ảnh

1. User chụp/upload ảnh nguyên liệu
2. Frontend gọi `POST /api/ai/detect_ingredient` với ảnh
3. Nhận response với thông tin gợi ý (tên, số lượng, đơn vị, category, expiryDate)
4. Frontend hiển thị form với thông tin đã điền sẵn
5. User chỉnh sửa thông tin (nếu cần)
6. Frontend gọi `POST /api/ingredient` với thông tin đã chỉnh sửa
7. Nhận response với thông tin nguyên liệu đã tạo
8. Frontend cập nhật danh sách nguyên liệu

### Luồng 3: Xem dinh dưỡng hàng ngày

1. User chọn ngày cần xem
2. Frontend gọi `POST /api/nutrition/daily` với `currentDate`
3. Nhận response với `totalCalories`, `totalProtein`, etc. và `mealBreakdown`
4. Frontend hiển thị biểu đồ và danh sách các bữa ăn
5. **Lưu ý:** Nutrition đã được tính toán tự động, không cần tính lại

---

## Tóm Tắt Logic Tự Động

| Hành động | Tự động trừ nguyên liệu | Tự động cập nhật Meal nutrition | Tự động hoàn lại nguyên liệu |
|-----------|------------------------|----------------------------------|------------------------------|
| Tạo món ăn | ✅ Có | ✅ Có (cộng) | ❌ Không |
| Sửa món ăn | ✅ Có (trừ mới) | ✅ Có (trừ cũ, cộng mới) | ✅ Có (hoàn lại cũ) |
| Xóa món ăn | ❌ Không | ✅ Có (trừ) | ✅ Có |
| Tạo nguyên liệu | ❌ Không | ❌ Không | ❌ Không |
| Sửa nguyên liệu | ❌ Không | ❌ Không | ❌ Không |
| Xóa nguyên liệu | ❌ Không | ❌ Không | ❌ Không |

---

**Chúc bạn phát triển frontend thành công! 🚀**
