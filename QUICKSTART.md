# Field Metadata API - Quick Reference

## 🚀 Quick Start Commands

### 1. Restore dependencies
```powershell
dotnet restore
```

### 2. Build the project
```powershell
dotnet build
```

### 3. Run the application
```powershell
dotnet run
```

### 4. Access Swagger UI
```
https://localhost:5001
```

---

## 📊 Sample API Calls (PowerShell)

### Get All Field Metadata
```powershell
Invoke-RestMethod -Uri "https://localhost:5001/api/v1/field-metadata?pageNumber=1&pageSize=10" -Method GET | ConvertTo-Json -Depth 10
```

### Get Specific Field
```powershell
Invoke-RestMethod -Uri "https://localhost:5001/api/v1/field-metadata/CUSTOMER_ID" -Method GET | ConvertTo-Json -Depth 10
```

### Create New Field Metadata
```powershell
$body = @{
    fieldName = "TEST_FIELD"
    dataElement = "TEST"
    description = "Test Field Description"
    dataType = "CHAR"
    fieldLength = 50
    tableGroup = "TEST"
    isActive = $true
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://localhost:5001/api/v1/field-metadata" -Method POST -Body $body -ContentType "application/json" | ConvertTo-Json -Depth 10
```

### Update Field Metadata
```powershell
$body = @{
    description = "Updated Description"
    tableGroup = "UPDATED_GROUP"
    isActive = $true
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://localhost:5001/api/v1/field-metadata/TEST_FIELD" -Method PUT -Body $body -ContentType "application/json" | ConvertTo-Json -Depth 10
```

### Delete Field Metadata
```powershell
Invoke-RestMethod -Uri "https://localhost:5001/api/v1/field-metadata/TEST_FIELD" -Method DELETE | ConvertTo-Json -Depth 10
```

### Health Check
```powershell
Invoke-RestMethod -Uri "https://localhost:5001/health" -Method GET
```

---

## 🧪 Sample cURL Commands

### Get All
```bash
curl -k https://localhost:5001/api/v1/field-metadata?pageNumber=1&pageSize=10
```

### Create New
```bash
curl -k -X POST https://localhost:5001/api/v1/field-metadata \
  -H "Content-Type: application/json" \
  -d '{
    "fieldName": "TEST_FIELD",
    "description": "Test Field",
    "dataType": "CHAR",
    "fieldLength": 50,
    "tableGroup": "TEST",
    "isActive": true
  }'
```

---

## 🗄️ SQL Quick Queries

### View All Active Records
```sql
USE DatabridgeDB;
SELECT * FROM Field_Metadata WHERE IsActive = 1;
```

### View Computed Columns
```sql
SELECT 
    FieldName,
    KeyField,
    IsMandatory,
    CheckTable,
    ValidationType,
    HasDropdown,
    UIControlType
FROM Field_Metadata;
```

### Check Table Structure
```sql
EXEC sp_help 'Field_Metadata';
```

---

## ⚙️ Configuration Notes

- **Database**: DatabridgeDB
- **Table**: Field_Metadata
- **Swagger**: https://localhost:5001
- **Health**: https://localhost:5001/health
- **Logs**: logs/ folder (created automatically)

---

## 🔧 Troubleshooting

### Issue: Certificate error when accessing HTTPS
**Solution**: Trust the development certificate
```powershell
dotnet dev-certs https --trust
```

### Issue: Database connection fails
**Solution**: Verify SQL Server is running and connection string is correct
```powershell
# Test connection string in appsettings.json
```

### Issue: Port already in use
**Solution**: Change ports in Properties/launchSettings.json or stop the conflicting process

---

## 📦 NuGet Packages Used

- Dapper 2.1.35
- FluentValidation 11.9.0
- FluentValidation.AspNetCore 11.3.0
- Microsoft.AspNetCore.Mvc.Versioning 5.1.0
- Microsoft.Data.SqlClient 5.1.5
- Serilog.AspNetCore 8.0.0
- Swashbuckle.AspNetCore 6.5.0
