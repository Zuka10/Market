# Market Project

## 🛍️ Overview
A full-featured market system with product catalog, order management, user accounts, and transaction support using ASP.NET Core.

## 🚀 Technologies Used
- ASP.NET Core 8
- Dapper ORM with Repository Pattern
- Unit of Work Pattern
- MediatR + CQRS
- SQL Server
- JWT Authentication
- Clean Architecture
- Custom Migration System
- Health Checks

## 📦 Features
- Product and category management
- Order processing and procurement
- Vendor and location management
- User management with role-based authorization
- Payment processing and discounts
- Custom database migration system
- Comprehensive health monitoring

## 🗄️ Database Setup

### Prerequisites
1. SQL Server (LocalDB, Express, or Full)
2. Update connection string in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=MarketManagementSystem;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

### Database Setup
Run the main database schema creation script:
```sql
-- Execute the provided database schema script to create:
-- ✓ Auth schema (Users, Roles, RefreshTokens)
-- ✓ Market schema (Products, Orders, Vendors, Locations, etc.)
-- ✓ All required tables, indexes, and relationships
-- ✓ Sample roles (Admin, VendorManager, LocationManager)
```

### Migration System Usage

Our custom migration system handles incremental database changes and new features.

#### Available Migration Commands

**1. Check Migration Status**
```bash
dotnet run -- migrate status
```
*Shows all available migrations and their current status (Applied/Pending)*

**2. Run All Pending Migrations**
```bash
dotnet run -- migrate
```
*Applies all pending migrations in sequential order*

**3. Rollback to Specific Version**
```bash
dotnet run -- migrate rollback <version>
```
*Example: `dotnet run -- migrate rollback 001`*

**4. Show Help**
```bash
dotnet run -- migrate help
```

#### Migration System Architecture
```
Market.Migration/
├── Abstractions/
│   ├── IMigration.cs           # Migration interface
│   ├── IMigrationRunner.cs     # Runner interface
│   └── MigrationInfo.cs        # Migration metadata
├── Core/
│   └── MigrationRunner.cs      # Core migration logic
├── Migrations/
│   └── (Future migrations will go here)
├── CLI/
│   └── MigrationCliService.cs  # Command-line interface
└── Entities/
    └── MigrationHistory.cs     # History tracking entity
```

#### Migration Features
- **Automatic Discovery**: Finds migration classes using reflection
- **Transaction Safety**: Each migration runs in a database transaction
- **Rollback Support**: Can rollback to any previous version
- **History Tracking**: `__MigrationHistory` table tracks applied migrations
- **Checksum Validation**: Prevents accidental re-execution of modified migrations
- **Comprehensive Logging**: Detailed logs for debugging and monitoring

#### Creating New Migrations
1. Create a new class implementing `IMigration` in the `Migrations` folder:
```csharp
public class Migration001AddNewFeature : IMigration
{
    public string Version => "001";
    public string Description => "Add new feature description";
    
    public async Task UpAsync(IDbConnection connection, IDbTransaction? transaction)
    {
        const string sql = @"
            CREATE TABLE NewFeature (
                Id BIGINT IDENTITY(1,1) PRIMARY KEY,
                Name NVARCHAR(100) NOT NULL,
                CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
            )";
        
        await connection.ExecuteAsync(sql, transaction: transaction);
    }
    
    public async Task DownAsync(IDbConnection connection, IDbTransaction? transaction)
    {
        const string sql = "DROP TABLE IF EXISTS NewFeature";
        await connection.ExecuteAsync(sql, transaction: transaction);
    }
}
```

2. The migration will be automatically discovered and executed in the next run.

## 🔧 Getting Started

### 1. Clone and Build
```bash
git clone https://github.com/Zuka10/market.git
cd market
dotnet restore
dotnet build
```

### 2. Database Setup
```bash
# Run the main database schema script in SQL Server Management Studio first
# Then check if any migrations are available
dotnet run -- migrate status

# If migrations exist, apply them
dotnet run -- migrate
```

### 3. Run the Application
```bash
dotnet run
```

### 4. Verify Setup
- **API Documentation**: Navigate to `https://localhost:5001/swagger`
- **Health Check**: `https://localhost:5001/health`
- **Migration Status**: `dotnet run -- migrate status`

## 🏗️ Project Structure
```
Market/
├── Market.API/                 # Web API Layer
│   ├── Controllers/           # API Controllers
│   └── Program.cs             # Application entry point
├── Market.Application/         # Application Logic (CQRS)
│   ├── Features/              # CQRS Handlers organized by feature
│   ├── DTOs/                  # Data Transfer Objects
│   └── Common/                # Shared application logic
├── Market.Domain/             # Domain Entities & Interfaces
│   ├── Entities/              # Domain entities
│   └── Abstractions/          # Repository interfaces
├── Market.Infrastructure/     # Data Access & External Services
│   ├── Data/                  # Repository implementations
│   ├── Constants/             # Database constants
│   └── Extensions/            # Service registration
└── Market.Migration/          # Custom Migration System
    ├── Abstractions/          # Migration interfaces
    ├── Core/                  # Migration runner
    ├── CLI/                   # Command-line tools
    └── Migrations/            # Migration files
```

## 🔐 Authentication & Authorization

The system uses JWT tokens with role-based authorization:

### Default Roles
- **Admin**: Full system access
- **VendorManager**: Vendor and product management
- **LocationManager**: Location and inventory management

### Authorization Usage
```csharp
[Authorize(Roles = "Admin")]
public async Task<IActionResult> AdminOnlyAction() { ... }

[Authorize(Roles = "Admin,VendorManager")]
public async Task<IActionResult> VendorManagement() { ... }
```

### JWT Configuration
```json
{
  "JWT": {
    "Secret": "your-secret-key-here",
    "Issuer": "MarketAPI",
    "Audience": "MarketAPI-Users",
    "ExpiryHours": 24
  }
}
```

## 📊 Health Monitoring

The application includes comprehensive health checks:

### Health Check Endpoints
- **Overall Health**: `/health` - Complete health status with detailed JSON response
- **Simple Check**: Basic application and database connectivity

### Health Check Features
- **SQL Server Connectivity**: Tests database connection with simple query
- **Application Status**: Verifies application is running properly
- **Memory Usage**: Monitors memory consumption
- **Custom JSON Response**: Detailed status information for monitoring tools

### Example Health Response
```json
{
  "status": "Healthy",
  "timestamp": "2024-01-15T10:30:45.123Z",
  "duration": 45.67,
  "checks": [
    {
      "name": "self",
      "status": "Healthy",
      "description": "The application is running.",
      "duration": 0.12
    },
    {
      "name": "database",
      "status": "Healthy",
      "description": "Health check passed",
      "duration": 45.55
    }
  ]
}
```

## 🛠️ Development

### Adding New Features
1. **Domain Layer**: Create entities in `Market.Domain/Entities`
2. **Repository**: Add repository interface and implementation
3. **Application Layer**: Create CQRS handlers in `Market.Application/Features`
4. **API Layer**: Add controllers in `Market.API/Controllers`
5. **Database Changes**: Create migration if database changes are needed
6. **Apply Migration**: Run `dotnet run -- migrate`

### Database Migration Workflow
```bash
# Before making database changes
dotnet run -- migrate status        # Check current state

# After creating new migration
dotnet run -- migrate              # Apply new changes

# If something goes wrong
dotnet run -- migrate rollback 001 # Rollback to previous version
```

### Migration Best Practices
- **Always test migrations** on a copy of production data
- **Include both Up and Down** implementations for all migrations
- **Use transactions** for data integrity (automatically handled)
- **Descriptive names**: Use clear migration names and descriptions
- **Version control**: Commit migrations with your feature code
- **Sequential versioning**: Use 001, 002, 003... format for versions

## 🧪 Testing

### Manual Testing
1. **API Testing**: Use Swagger UI at `/swagger`
2. **Health Checks**: Visit `/health` endpoint
3. **Database**: Verify tables created correctly
4. **Migrations**: Test with `dotnet run -- migrate status`

### Database Testing
```sql
-- Verify main tables exist
SELECT TABLE_SCHEMA, TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_SCHEMA IN ('auth', 'market')
ORDER BY TABLE_SCHEMA, TABLE_NAME

-- Check sample data
SELECT COUNT(*) as UserCount FROM auth.[User]
SELECT COUNT(*) as RoleCount FROM auth.Role
```

## 🚀 Deployment

### Production Deployment
1. **Database Setup**: Run schema script on production database
2. **Apply Migrations**: `dotnet run -- migrate` (in production environment)
3. **Configuration**: Update `appsettings.Production.json`
4. **Health Checks**: Configure monitoring to check `/health` endpoint

### Environment Configuration
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Production-Connection-String"
  },
  "JWT": {
    "Secret": "Production-Secret-Key"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

## 🛟 Troubleshooting

### Common Issues

**Migration Problems**:
```bash
# Check what migrations are available
dotnet run -- migrate status

# If migration fails, check logs and rollback
dotnet run -- migrate rollback <previous-version>
```

**Database Connection**:
- Verify connection string in `appsettings.json`
- Ensure SQL Server is running
- Test connection with `/health` endpoint

**Authentication Issues**:
- Check JWT configuration
- Verify user roles are properly assigned
- Test with `/swagger` authentication

### Logging
The application uses built-in .NET logging. Check console output for:
- Migration execution details
- Health check results
- Authentication/authorization issues
- Database connection problems

## 📝 API Documentation
After running the application, visit `/swagger` for complete interactive API documentation including:
- Authentication endpoints
- User management
- Product and category operations
- Order processing
- Vendor and location management

## 🤝 Contributing
1. Fork the repository
2. Create a feature branch
3. **Create migrations** for any database changes
4. Test your changes thoroughly
5. **Test migrations** on sample data
6. Submit a pull request

### Pull Request Checklist
- [ ] Code builds successfully
- [ ] Database schema changes include migrations
- [ ] Migrations tested (up and down)
- [ ] Health checks pass
- [ ] API documentation updated if needed

## 📞 Support
For issues related to:
- **Migrations**: Use `dotnet run -- migrate status` and check logs
- **Database**: Use `/health` endpoint for diagnostics
- **Authentication**: Verify JWT configuration and user roles
- **API Issues**: Check `/swagger` for endpoint documentation
