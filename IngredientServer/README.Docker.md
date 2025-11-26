# Docker Setup Guide

## Quick Start

### Option 1: Using Docker Compose (Recommended)

```bash
# Build and start all services (MySQL + API)
docker-compose up -d

# View logs
docker-compose logs -f api

# Stop services
docker-compose down

# Stop and remove volumes (clean slate)
docker-compose down -v
```

### Option 2: Manual Docker Build

```bash
# Build the image
docker build -t ingredient-server:latest .

# Run the container (requires MySQL to be running separately)
docker run -d \
  --name ingredient-api \
  -p 8080:8080 \
  -e ConnectionStrings__DefaultConnection="Server=mysql;Port=3306;Database=IngredientServerDb;User=ingredientuser;Password=YourPassword123!;" \
  ingredient-server:latest
```

## Configuration

### Environment Variables

You can override configuration via environment variables:

```bash
# Azure OpenAI
-e AzureOpenAI__Endpoint="your-endpoint"
-e AzureOpenAI__ApiKey="your-api-key"
-e AzureOpenAI__Model="gpt-4.1"

# JWT Secret
-e Jwt__Secret="your-jwt-secret"

# Database Connection
-e ConnectionStrings__DefaultConnection="Server=mysql;Port=3306;Database=IngredientServerDb;User=ingredientuser;Password=YourPassword123!;"
```

### Using .env file with docker-compose

Create a `.env` file:

```env
AZURE_OPENAI_ENDPOINT=https://your-endpoint
AZURE_OPENAI_API_KEY=your-api-key
AZURE_OPENAI_MODEL=gpt-4.1
JWT_SECRET=your-jwt-secret
```

Then update `docker-compose.yml` to use these variables.

## Health Check

The API includes a health check endpoint:

```bash
curl http://localhost:8080/health
```

## Access Points

- **API**: http://localhost:8080
- **Swagger UI**: http://localhost:8080 (in Development mode)
- **MySQL**: localhost:3306

## Troubleshooting

### Check container logs
```bash
docker-compose logs api
docker-compose logs mysql
```

### Check container status
```bash
docker-compose ps
```

### Restart services
```bash
docker-compose restart
```

### Database connection issues
- Ensure MySQL container is healthy: `docker-compose ps mysql`
- Check MySQL logs: `docker-compose logs mysql`
- Verify connection string in environment variables

