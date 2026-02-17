# Field Metadata API

A production-ready **ASP.NET Core Web API** built with **.NET 10** and **Dapper** for managing field metadata stored in SQL Server.

## 🏗️ Architecture

This API follows **Clean Architecture** principles with the following layers:

- **Controllers** - API endpoints and request handling
- **Services** - Business logic layer
- **Repositories** - Data access layer using Dapper
- **Models** - Entity models
- **DTOs** - Data Transfer Objects for requests/responses
- **Validators** - FluentValidation for input validation
- **Middleware** - Global exception handling
- **Data** - Database connection factory

---

## 🚀 Features

✅ **RESTful API** with versioning (`/api/v1/field-metadata`)  
✅ **CRUD Operations** (Create, Read, Update, Soft Delete)  
✅ **Pagination & Filtering** by FieldName, TableGroup, DataType  
✅ **Computed Columns** (ValidationType, IsMandatory, UIControlType)  
✅ **FluentValidation** for request validation  
✅ **Global Exception Handling** with standard API responses  
✅ **Serilog Logging** to console and file  
✅ **Swagger/OpenAPI** documentation  
✅ **Health Check** endpoint (`/health`)  
✅ **Dependency Injection** throughout the application  

---

## 📋 Prerequisites

- **.NET 10 SDK** or later
- **SQL Server** (Express or higher)
- **SQL Server Management Studio (SSMS)** or Azure Data Studio

---

## 🗄️ Database Setup

### Step 1: Create Database and Table

Run the following SQL scripts in order:

1. **Create Table**: `SQL/01_CreateTable.sql`
   - Creates the `DatabridgeDB` database
   - Creates the `Field_Metadata` table with computed columns
   - Adds performance indexes

2. **Seed Data**: `SQL/02_SeedData.sql`
   - Inserts 5 sample records

### Database Schema

**Table**: `Field_Metadata`

| Column         | Type          | Description                          | Computed? |
|----------------|---------------|--------------------------------------|-----------|
| FieldName      | NVARCHAR(100) | Primary Key                          | No        |
| DataElement    | NVARCHAR(100) | Data element                         | No        |
| Description    | NVARCHAR(500) | Field description                    | No        |
| KeyField       | NVARCHAR(1)   | Key field indicator (X = Mandatory)  | No        |
| CheckTable     | NVARCHAR(100) | Check table reference                | No        |
| DataType       | NVARCHAR(50)  | Data type (CHAR, NUMC, DATS, etc.)   | No        |
| FieldLength    | INT           | Field length                         | No        |
| Decimals       | INT           | Decimal places                       | No        |
| **ValidationType** | NVARCHAR   | Validation type                      | **Yes**   |
| HasDropdown    | NVARCHAR(1)   | Has dropdown indicator (X = Yes)     | No        |
| **IsMandatory**    | BIT        | Is mandatory flag                    | **Yes**   |
| TableGroup     | NVARCHAR(100) | Table group                          | No        |
| **UIControlType**  | NVARCHAR   | UI control type                      | **Yes**   |
| IsActive       | BIT           | Active status                        | No        |
| CreatedDate    | DATETIME2     | Creation timestamp                   | No        |

#### Computed Columns Logic

- **ValidationType**: 
  - `LOOKUP` if CheckTable exists
  - `DATE` if DataType = 'DATS'
  - `NUMERIC` if DataType = 'NUMC'
  - `TEXT` otherwise

- **IsMandatory**: 
  - `1` (true) if KeyField = 'X'
  - `0` (false) otherwise

- **UIControlType**: 
  - `DROPDOWN` if HasDropdown = 'X'
  - `DATEPICKER` if DataType = 'DATS'
  - `TEXTAREA` if FieldLength > 255
  - `TEXTBOX` otherwise

---

## ⚙️ Configuration

Update the connection string in `appsettings.json` if needed:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=localhost\\SQLEXPRESS;Initial Catalog=DatabridgeDB;Integrated Security=True;..."
  }
}
```

---

## 🏃 How to Run

### Step 1: Restore Dependencies

```bash
dotnet restore
```

### Step 2: Run the Application

```bash
dotnet run
```

The API will start on:
- **HTTPS**: `https://localhost:5001`
- **HTTP**: `http://localhost:5000`

### Step 3: Access Swagger UI

Open your browser and navigate to:

```
https://localhost:5001
```

Swagger UI will be displayed at the root URL.

---

## 📡 API Endpoints

### Base URL: `/api/v1/field-metadata`

| Method   | Endpoint                           | Description                          |
|----------|------------------------------------|--------------------------------------|
| `GET`    | `/api/v1/field-metadata`           | Get all active field metadata (paginated) |
| `GET`    | `/api/v1/field-metadata/{fieldName}` | Get field metadata by FieldName    |
| `POST`   | `/api/v1/field-metadata`           | Create new field metadata            |
| `PUT`    | `/api/v1/field-metadata/{fieldName}` | Update field metadata (editable fields only) |
| `DELETE` | `/api/v1/field-metadata/{fieldName}` | Soft delete (set IsActive = 0)     |

### Query Parameters (GET all)

- `fieldName` (string, optional) - Filter by field name (partial match)
- `tableGroup` (string, optional) - Filter by table group (exact match)
- `dataType` (string, optional) - Filter by data type (exact match)
- `pageNumber` (int, default: 1) - Page number
- `pageSize` (int, default: 10, max: 100) - Page size

### Example Requests

#### 1. Get All Field Metadata (Paginated)

```http
GET /api/v1/field-metadata?pageNumber=1&pageSize=10
```

#### 2. Filter by Table Group

```http
GET /api/v1/field-metadata?tableGroup=CUSTOMER&pageNumber=1&pageSize=10
```

#### 3. Get by FieldName

```http
GET /api/v1/field-metadata/CUSTOMER_ID
```

#### 4. Create New Field Metadata

```http
POST /api/v1/field-metadata
Content-Type: application/json

{
  "fieldName": "EMAIL_ADDRESS",
  "dataElement": "EMAIL",
  "description": "Customer Email Address",
  "keyField": null,
  "checkTable": null,
  "dataType": "CHAR",
  "fieldLength": 100,
  "decimals": null,
  "hasDropdown": null,
  "tableGroup": "CUSTOMER",
  "isActive": true
}
```

#### 5. Update Field Metadata (Editable Fields Only)

```http
PUT /api/v1/field-metadata/EMAIL_ADDRESS
Content-Type: application/json

{
  "description": "Updated Email Address Description",
  "checkTable": null,
  "hasDropdown": null,
  "tableGroup": "CONTACT",
  "isActive": true
}
```

#### 6. Soft Delete

```http
DELETE /api/v1/field-metadata/EMAIL_ADDRESS
```

---

## 📦 Standard API Response Format

All responses follow this structure:

```json
{
  "success": true,
  "message": "Success message",
  "data": { /* response data */ },
  "errors": null
}
```

### Success Response Example

```json
{
  "success": true,
  "message": "Field metadata retrieved successfully",
  "data": {
    "items": [
      {
        "fieldName": "CUSTOMER_ID",
        "dataElement": "KUNNR",
        "description": "Customer Number",
        "keyField": "X",
        "checkTable": null,
        "dataType": "CHAR",
        "fieldLength": 10,
        "decimals": null,
        "validationType": "TEXT",
        "hasDropdown": null,
        "isMandatory": true,
        "tableGroup": "CUSTOMER",
        "uiControlType": "TEXTBOX",
        "isActive": true,
        "createdDate": "2026-02-16T10:30:00Z"
      }
    ],
    "totalCount": 1,
    "pageNumber": 1,
    "pageSize": 10,
    "totalPages": 1,
    "hasPreviousPage": false,
    "hasNextPage": false
  },
  "errors": null
}
```

### Error Response Example

```json
{
  "success": false,
  "message": "Validation failed",
  "data": null,
  "errors": [
    "FieldName is required.",
    "FieldLength must be greater than 0."
  ]
}
```

---

## 🛡️ Business Rules

1. **FieldName must be unique** - Duplicate FieldName will return an error
2. **KeyField = 'X'** → `IsMandatory` is computed automatically (cannot be set manually)
3. **Computed columns** (`ValidationType`, `IsMandatory`, `UIControlType`) are read-only
4. **Only active records** (`IsActive = 1`) are returned by default
5. **Update restrictions** - Only the following fields can be updated:
   - Description
   - CheckTable
   - HasDropdown
   - TableGroup
   - IsActive

---

## 📁 Project Structure

```
POC/
├── Controllers/
│   └── FieldMetadataController.cs
├── DTOs/
│   ├── ApiResponse.cs
│   └── FieldMetadataDto.cs
├── Data/
│   └── DbConnectionFactory.cs
├── Middleware/
│   └── GlobalExceptionMiddleware.cs
├── Models/
│   └── FieldMetadata.cs
├── Repositories/
│   └── FieldMetadataRepository.cs
├── Services/
│   └── FieldMetadataService.cs
├── SQL/
│   ├── 01_CreateTable.sql
│   └── 02_SeedData.sql
├── Validators/
│   └── FieldMetadataValidators.cs
├── appsettings.json
├── appsettings.Development.json
├── FieldMetadataAPI.csproj
├── Program.cs
└── README.md
```

---

## 🔍 Health Check

Monitor the API health:

```http
GET /health
```

Response: `Healthy`

---

## 📝 Logging

Logs are written to:
- **Console** (stdout)
- **File**: `logs/log-YYYYMMDD.txt` (rotating daily)

Log levels can be configured in `appsettings.json` under the `Serilog` section.

---

## 🧪 Testing with Swagger

1. Run the application
2. Navigate to `https://localhost:5001`
3. Swagger UI will display all endpoints
4. Click "Try it out" on any endpoint
5. Fill in parameters and execute requests

---

## 🔒 Security Notes

- Update connection string credentials for production
- Enable authentication/authorization as needed
- Configure CORS policy based on your requirements
- Use HTTPS in production (already enabled)

---

## 📚 Technologies Used

- **.NET 10** - Latest .NET framework
- **ASP.NET Core** - Web API framework
- **Dapper** - Lightweight ORM
- **FluentValidation** - Validation library
- **Serilog** - Logging framework
- **Swagger/OpenAPI** - API documentation
- **Microsoft.Data.SqlClient** - SQL Server data provider
- **API Versioning** - Endpoint versioning

---

## 📄 License

This project is for internal use in the Databridge Backend system.

---

## 👥 Contact

For questions or support, contact the Databridge Development Team.

---

## 🎉 Quick Start Summary

1. **Setup Database**: Run `SQL/01_CreateTable.sql` and `SQL/02_SeedData.sql`
2. **Restore Packages**: `dotnet restore`
3. **Run Application**: `dotnet run`
4. **Browse API**: Open `https://localhost:5001`
5. **Test Endpoints**: Use Swagger UI or your favorite API client

---

**Happy Coding! 🚀**
