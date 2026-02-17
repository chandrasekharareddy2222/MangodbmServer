# Complete API Reference

Base URL: `https://localhost:5001/api/v1`

---

## 📋 Material APIs

### 1. Generate Material Number (Pre-Create)

**Purpose:** Get a new Material Number before creating a material form.

```http
GET /api/v1/materials/generate-matnr
```

**Request Headers:**
```
Accept: application/json
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Material Number generated successfully",
  "data": "000000000100000000"
}
```

**When to Use:**
- Call this when user opens the "Create Material" form
- Display the returned MATNR in a read-only field marked as required (*)
- Store this MATNR to include in the submit request

**Postman Example:**
```
Method: GET
URL: https://localhost:5001/api/v1/materials/generate-matnr
Headers: 
  Accept: application/json
```

---

### 2. Submit Material (Create/Update)

**Purpose:** Create a new material or update an existing one.

```http
POST /api/v1/materials/submit
```

**Request Headers:**
```
Content-Type: application/json
Accept: application/json
```

**Request Body (CREATE - with pre-generated MATNR):**
```json
{
  "matnr": "000000000100000000",
  "mtart": "FERT",
  "meins": "EA",
  "mbrsh": "M",
  "matkl": "MAT001",
  "attributes": {
    "/BEV1/LULDEGRP": "GRP001",
    "/BEV1/LULEINH": "345",
    "SIZE1": "Large",
    "COLOR": "Blue"
  },
  "submittedBy": "User123"
}
```

**Request Body (UPDATE - existing material):**
```json
{
  "matnr": "000000000100000000",
  "mtart": "FERT",
  "meins": "PC",
  "attributes": {
    "/BEV1/LULDEGRP": "GRP002",
    "SIZE1": "Extra Large"
  },
  "submittedBy": "User456"
}
```

**Response (201 Created - for NEW material):**
```json
{
  "success": true,
  "message": "Material 000000000100000000 created successfully",
  "data": {
    "matnr": "000000000100000000",
    "mtart": "FERT",
    "meins": "EA",
    "mbrsh": "M",
    "matkl": "MAT001",
    "attributes": {
      "/BEV1/LULDEGRP": "GRP001",
      "/BEV1/LULEINH": "345",
      "SIZE1": "Large",
      "COLOR": "Blue"
    },
    "status": "ACTIVE",
    "createdDate": "2026-02-16T10:30:00Z",
    "createdBy": "User123"
  }
}
```

**Response (200 OK - for UPDATE):**
```json
{
  "success": true,
  "message": "Material 000000000100000000 updated successfully",
  "data": {
    "matnr": "000000000100000000",
    "mtart": "FERT",
    "meins": "PC",
    "mbrsh": "M",
    "matkl": "MAT001",
    "attributes": {
      "/BEV1/LULDEGRP": "GRP002",
      "SIZE1": "Extra Large"
    },
    "status": "ACTIVE",
    "createdDate": "2026-02-16T10:30:00Z",
    "createdBy": "User123",
    "modifiedDate": "2026-02-16T11:45:00Z",
    "modifiedBy": "User456"
  }
}
```

**Validation Rules:**

For **CREATE**:
- ✅ `matnr` - Required (use pre-generated from `/generate-matnr`)
- ✅ `mtart` - Required (Material Type)
- ✅ `meins` - Required (Base Unit)
- ❌ `mbrsh` - Optional (Industry Sector)
- ❌ `matkl` - Optional (Material Group)
- ❌ `attributes` - Optional (dynamic fields)

For **UPDATE**:
- ✅ `matnr` - Must exist in database
- ❌ All other fields optional (only updated fields need to be sent)

**Error Responses:**

**400 Bad Request - Validation Failed:**
```json
{
  "success": false,
  "message": "Validation failed",
  "data": null,
  "errors": {
    "MTART": ["Material Type is required for new materials"],
    "MEINS": ["Base Unit of Measure is required for new materials"]
  }
}
```

**404 Not Found - Material doesn't exist (UPDATE):**
```json
{
  "success": false,
  "message": "Material 000000000100000000 not found",
  "data": null
}
```

**Postman Example (CREATE):**
```
Method: POST
URL: https://localhost:5001/api/v1/materials/submit
Headers:
  Content-Type: application/json
  Accept: application/json
Body (raw JSON):
{
  "matnr": "000000000100000000",
  "mtart": "FERT",
  "meins": "EA",
  "mbrsh": "M",
  "matkl": "MAT001",
  "attributes": {
    "/BEV1/LULDEGRP": "GRP001",
    "SIZE1": "Large"
  },
  "submittedBy": "TestUser"
}
```

---

### 3. Get Material by MATNR

**Purpose:** Retrieve a specific material by Material Number.

```http
GET /api/v1/materials/{matnr}
```

**Path Parameters:**
- `matnr` (string, required) - Material Number (18 characters)

**Request Headers:**
```
Accept: application/json
```

**Example Request:**
```http
GET /api/v1/materials/000000000100000000
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Material retrieved successfully",
  "data": {
    "matnr": "000000000100000000",
    "mtart": "FERT",
    "meins": "EA",
    "mbrsh": "M",
    "matkl": "MAT001",
    "attributes": {
      "/BEV1/LULDEGRP": "GRP001",
      "/BEV1/LULEINH": "345",
      "SIZE1": "Large",
      "COLOR": "Blue"
    },
    "status": "ACTIVE",
    "createdDate": "2026-02-16T10:30:00Z",
    "createdBy": "User123",
    "modifiedDate": "2026-02-16T11:45:00Z",
    "modifiedBy": "User456"
  }
}
```

**Response (404 Not Found):**
```json
{
  "success": false,
  "message": "Material 000000000999999999 not found",
  "data": null
}
```

**Postman Example:**
```
Method: GET
URL: https://localhost:5001/api/v1/materials/000000000100000000
Headers:
  Accept: application/json
```

---

### 4. Get All Materials (with Filtering & Pagination)

**Purpose:** Retrieve a list of materials with optional filters and pagination.

```http
GET /api/v1/materials
```

**Query Parameters:**
- `matnr` (string, optional) - Filter by Material Number (partial match)
- `mtart` (string, optional) - Filter by Material Type
- `matkl` (string, optional) - Filter by Material Group
- `status` (string, optional) - Filter by Status (ACTIVE, BLOCKED, DELETED, PENDING)
- `pageNumber` (integer, default: 1) - Page number
- `pageSize` (integer, default: 10) - Items per page

**Request Headers:**
```
Accept: application/json
```

**Example Requests:**

**Get all materials (first page):**
```http
GET /api/v1/materials?pageNumber=1&pageSize=10
```

**Filter by Material Type:**
```http
GET /api/v1/materials?mtart=FERT&pageNumber=1&pageSize=10
```

**Filter by Material Group and Status:**
```http
GET /api/v1/materials?matkl=MAT001&status=ACTIVE&pageNumber=1&pageSize=10
```

**Search by partial MATNR:**
```http
GET /api/v1/materials?matnr=100000&pageNumber=1&pageSize=10
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Materials retrieved successfully",
  "data": [
    {
      "matnr": "000000000100000000",
      "mtart": "FERT",
      "meins": "EA",
      "mbrsh": "M",
      "matkl": "MAT001",
      "attributes": {
        "/BEV1/LULDEGRP": "GRP001",
        "SIZE1": "Large"
      },
      "status": "ACTIVE",
      "createdDate": "2026-02-16T10:30:00Z",
      "createdBy": "User123"
    },
    {
      "matnr": "000000000100000001",
      "mtart": "FERT",
      "meins": "PC",
      "mbrsh": "M",
      "matkl": "MAT002",
      "attributes": {
        "/BEV1/LULDEGRP": "GRP002",
        "COLOR": "Red"
      },
      "status": "ACTIVE",
      "createdDate": "2026-02-16T12:00:00Z",
      "createdBy": "User456"
    }
  ],
  "pageNumber": 1,
  "pageSize": 10,
  "totalCount": 2,
  "totalPages": 1
}
```

**Response (200 OK - No Results):**
```json
{
  "success": true,
  "message": "Materials retrieved successfully",
  "data": [],
  "pageNumber": 1,
  "pageSize": 10,
  "totalCount": 0,
  "totalPages": 0
}
```

**Postman Examples:**

**Get all materials:**
```
Method: GET
URL: https://localhost:5001/api/v1/materials?pageNumber=1&pageSize=10
Headers:
  Accept: application/json
```

**Filter by Material Type:**
```
Method: GET
URL: https://localhost:5001/api/v1/materials?mtart=FERT&pageNumber=1&pageSize=10
Headers:
  Accept: application/json
```

**Search by MATNR:**
```
Method: GET
URL: https://localhost:5001/api/v1/materials?matnr=100000&status=ACTIVE
Headers:
  Accept: application/json
```

---

## 📋 Field Metadata APIs

### 5. Get All Field Metadata

**Purpose:** Retrieve all field definitions with optional filtering and pagination.

```http
GET /api/v1/fieldmetadata
```

**Query Parameters:**
- `fieldName` (string, optional) - Filter by field name (partial match)
- `dataType` (string, optional) - Filter by data type (String, Number, Date, Boolean)
- `isMandatory` (boolean, optional) - Filter by mandatory flag (true/false)
- `category` (string, optional) - Filter by category
- `pageNumber` (integer, default: 1) - Page number
- `pageSize` (integer, default: 10) - Items per page

**Request Headers:**
```
Accept: application/json
```

**Example Requests:**

```http
GET /api/v1/fieldmetadata
GET /api/v1/fieldmetadata?isMandatory=true
GET /api/v1/fieldmetadata?dataType=String&pageNumber=1&pageSize=20
GET /api/v1/fieldmetadata?fieldName=SIZE&category=Material
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Field metadata retrieved successfully",
  "data": [
    {
      "fieldName": "/BEV1/LULDEGRP",
      "displayName": "Delivery Group",
      "dataType": "String",
      "fieldLength": 3,
      "decimals": null,
      "isMandatory": true,
      "category": "Material Master",
      "subcategory": "Beverage",
      "description": "Delivery group for beverage materials",
      "defaultValue": null,
      "checkTable": null,
      "referenceField": null,
      "helpText": "Select appropriate delivery group",
      "validFrom": "2024-01-01T00:00:00",
      "validTo": null,
      "iconClass": "mdi-truck-delivery",
      "colorCode": "#4CAF50"
    }
  ],
  "pageNumber": 1,
  "pageSize": 10,
  "totalCount": 1,
  "totalPages": 1
}
```

**Postman Example:**
```
Method: GET
URL: https://localhost:5001/api/v1/fieldmetadata?isMandatory=true&pageNumber=1&pageSize=10
Headers:
  Accept: application/json
```

---

### 6. Get Field Metadata by Name

**Purpose:** Retrieve a specific field definition by name.

```http
GET /api/v1/fieldmetadata/{fieldName}
```

**Path Parameters:**
- `fieldName` (string, required) - Exact field name

**Request Headers:**
```
Accept: application/json
```

**Example Request:**
```http
GET /api/v1/fieldmetadata//BEV1/LULDEGRP
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Field metadata retrieved successfully",
  "data": {
    "fieldName": "/BEV1/LULDEGRP",
    "displayName": "Delivery Group",
    "dataType": "String",
    "fieldLength": 3,
    "decimals": null,
    "isMandatory": true,
    "category": "Material Master",
    "subcategory": "Beverage",
    "description": "Delivery group for beverage materials",
    "defaultValue": null,
    "checkTable": null,
    "referenceField": null,
    "helpText": "Select appropriate delivery group",
    "validFrom": "2024-01-01T00:00:00",
    "validTo": null,
    "iconClass": "mdi-truck-delivery",
    "colorCode": "#4CAF50"
  }
}
```

**Response (404 Not Found):**
```json
{
  "success": false,
  "message": "Field metadata with name 'INVALID_FIELD' not found",
  "data": null
}
```

**Postman Example:**
```
Method: GET
URL: https://localhost:5001/api/v1/fieldmetadata/%2FBEV1%2FLULDEGRP
Note: URL encode special characters (/ becomes %2F)
Headers:
  Accept: application/json
```

---

### 7. Get Field Metadata with Lookup Values

**Purpose:** Retrieve field metadata along with associated check table values and passable values.

```http
GET /api/v1/fieldmetadata/{fieldName}/with-values
```

**Path Parameters:**
- `fieldName` (string, required) - Exact field name

**Request Headers:**
```
Accept: application/json
```

**Example Request:**
```http
GET /api/v1/fieldmetadata//BEV1/LULDEGRP/with-values
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Field metadata with values retrieved successfully",
  "data": {
    "fieldName": "/BEV1/LULDEGRP",
    "displayName": "Delivery Group",
    "dataType": "String",
    "fieldLength": 3,
    "decimals": null,
    "isMandatory": true,
    "category": "Material Master",
    "subcategory": "Beverage",
    "description": "Delivery group for beverage materials",
    "defaultValue": null,
    "checkTable": "T001W",
    "referenceField": "WERKS",
    "helpText": "Select appropriate delivery group",
    "validFrom": "2024-01-01T00:00:00",
    "validTo": null,
    "iconClass": "mdi-truck-delivery",
    "colorCode": "#4CAF50",
    "checkTableValues": [
      {
        "valueId": 1,
        "fieldName": "/BEV1/LULDEGRP",
        "value": "001",
        "displayText": "Direct Delivery",
        "description": "Direct delivery to customer",
        "isActive": true
      },
      {
        "valueId": 2,
        "fieldName": "/BEV1/LULDEGRP",
        "value": "002",
        "displayText": "Warehouse Delivery",
        "description": "Delivery via warehouse",
        "isActive": true
      }
    ],
    "passableValues": [
      {
        "valueId": 101,
        "fieldName": "/BEV1/LULDEGRP",
        "value": "X",
        "displayText": "Active",
        "isActive": true
      }
    ]
  }
}
```

**Postman Example:**
```
Method: GET
URL: https://localhost:5001/api/v1/fieldmetadata/%2FBEV1%2FLULDEGRP/with-values
Headers:
  Accept: application/json
```

---

### 8. Create Field Metadata

**Purpose:** Create a new field definition.

```http
POST /api/v1/fieldmetadata
```

**Request Headers:**
```
Content-Type: application/json
Accept: application/json
```

**Request Body:**
```json
{
  "fieldName": "NEW_FIELD",
  "displayName": "New Custom Field",
  "dataType": "String",
  "fieldLength": 50,
  "decimals": null,
  "isMandatory": false,
  "category": "Custom",
  "subcategory": "User Defined",
  "description": "A new custom field for testing",
  "defaultValue": "DEFAULT",
  "checkTable": null,
  "referenceField": null,
  "helpText": "Enter custom value",
  "validFrom": "2026-02-16",
  "validTo": null,
  "iconClass": "mdi-star",
  "colorCode": "#FF5722"
}
```

**Response (201 Created):**
```json
{
  "success": true,
  "message": "Field metadata created successfully",
  "data": {
    "fieldName": "NEW_FIELD",
    "displayName": "New Custom Field",
    "dataType": "String",
    "fieldLength": 50,
    "decimals": null,
    "isMandatory": false,
    "category": "Custom",
    "subcategory": "User Defined",
    "description": "A new custom field for testing",
    "defaultValue": "DEFAULT",
    "checkTable": null,
    "referenceField": null,
    "helpText": "Enter custom value",
    "validFrom": "2026-02-16T00:00:00",
    "validTo": null,
    "iconClass": "mdi-star",
    "colorCode": "#FF5722"
  }
}
```

**Error Response (400 Bad Request):**
```json
{
  "success": false,
  "message": "Validation failed",
  "data": null,
  "errors": {
    "FieldName": ["Field name is required", "Field name must be between 1 and 50 characters"],
    "DataType": ["Must be one of: String, Number, Date, Boolean"]
  }
}
```

**Postman Example:**
```
Method: POST
URL: https://localhost:5001/api/v1/fieldmetadata
Headers:
  Content-Type: application/json
  Accept: application/json
Body (raw JSON):
{
  "fieldName": "TEST_FIELD",
  "displayName": "Test Field",
  "dataType": "String",
  "fieldLength": 100,
  "isMandatory": false,
  "category": "Test"
}
```

---

### 9. Update Field Metadata

**Purpose:** Update an existing field definition.

```http
PUT /api/v1/fieldmetadata/{fieldName}
```

**Path Parameters:**
- `fieldName` (string, required) - Field name to update

**Request Headers:**
```
Content-Type: application/json
Accept: application/json
```

**Request Body:**
```json
{
  "displayName": "Updated Display Name",
  "description": "Updated description",
  "isMandatory": true,
  "helpText": "Updated help text",
  "colorCode": "#2196F3"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Field metadata updated successfully",
  "data": {
    "fieldName": "/BEV1/LULDEGRP",
    "displayName": "Updated Display Name",
    "dataType": "String",
    "fieldLength": 3,
    "decimals": null,
    "isMandatory": true,
    "category": "Material Master",
    "description": "Updated description",
    "helpText": "Updated help text",
    "colorCode": "#2196F3"
  }
}
```

**Postman Example:**
```
Method: PUT
URL: https://localhost:5001/api/v1/fieldmetadata/%2FBEV1%2FLULDEGRP
Headers:
  Content-Type: application/json
  Accept: application/json
Body (raw JSON):
{
  "displayName": "Updated Name",
  "isMandatory": true
}
```

---

### 10. Delete Field Metadata (Soft Delete)

**Purpose:** Soft delete a field definition (marks as deleted, doesn't physically remove).

```http
DELETE /api/v1/fieldmetadata/{fieldName}
```

**Path Parameters:**
- `fieldName` (string, required) - Field name to delete

**Request Headers:**
```
Accept: application/json
```

**Example Request:**
```http
DELETE /api/v1/fieldmetadata/TEST_FIELD
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Field metadata deleted successfully",
  "data": null
}
```

**Response (404 Not Found):**
```json
{
  "success": false,
  "message": "Field metadata with name 'TEST_FIELD' not found",
  "data": null
}
```

**Postman Example:**
```
Method: DELETE
URL: https://localhost:5001/api/v1/fieldmetadata/TEST_FIELD
Headers:
  Accept: application/json
```

---

### 11. Bulk Update Mandatory Flag

**Purpose:** Update the `isMandatory` flag for multiple fields at once.

```http
PATCH /api/v1/fieldmetadata/bulk-update-mandatory
```

**Request Headers:**
```
Content-Type: application/json
Accept: application/json
```

**Request Body:**
```json
{
  "updates": [
    {
      "fieldName": "/BEV1/LULDEGRP",
      "isMandatory": true
    },
    {
      "fieldName": "/BEV1/LULEINH",
      "isMandatory": true
    },
    {
      "fieldName": "SIZE1",
      "isMandatory": false
    },
    {
      "fieldName": "COLOR",
      "isMandatory": false
    }
  ]
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "4 field(s) updated successfully",
  "data": null
}
```

**Response (400 Bad Request - Validation):**
```json
{
  "success": false,
  "message": "Validation failed",
  "data": null,
  "errors": {
    "Updates": ["At least one field update is required"]
  }
}
```

**Postman Example:**
```
Method: PATCH
URL: https://localhost:5001/api/v1/fieldmetadata/bulk-update-mandatory
Headers:
  Content-Type: application/json
  Accept: application/json
Body (raw JSON):
{
  "updates": [
    { "fieldName": "/BEV1/LULDEGRP", "isMandatory": true },
    { "fieldName": "SIZE1", "isMandatory": false }
  ]
}
```

---

## 🔧 Common Response Codes

| Status Code | Meaning | When It Occurs |
|-------------|---------|----------------|
| 200 OK | Success | GET, PUT, DELETE, PATCH operations successful |
| 201 Created | Resource Created | POST operations create new resource |
| 400 Bad Request | Validation Error | Invalid input data |
| 404 Not Found | Resource Not Found | Requested resource doesn't exist |
| 500 Internal Server Error | Server Error | Unexpected server-side error |

---

## 📝 Testing Workflow

### Complete Material Creation Flow

1. **Generate MATNR:**
   ```
   GET /api/v1/materials/generate-matnr
   → Returns: "000000000100000000"
   ```

2. **Get Field Metadata (to build form):**
   ```
   GET /api/v1/fieldmetadata?category=Material&isMandatory=true
   → Returns: List of mandatory fields
   ```

3. **Get Lookup Values (for dropdowns):**
   ```
   GET /api/v1/fieldmetadata//BEV1/LULDEGRP/with-values
   → Returns: Field definition + dropdown options
   ```

4. **Submit Material:**
   ```
   POST /api/v1/materials/submit
   Body: { "matnr": "000000000100000000", "mtart": "FERT", ... }
   → Returns: Created material with 201 status
   ```

5. **Verify Material:**
   ```
   GET /api/v1/materials/000000000100000000
   → Returns: Full material details
   ```

6. **List All Materials:**
   ```
   GET /api/v1/materials?status=ACTIVE&pageNumber=1&pageSize=10
   → Returns: Paginated list of materials
   ```

---

## 🎯 Quick Test Scenarios

### Scenario 1: Create Material with Pre-Generated MATNR
1. GET `/materials/generate-matnr` → Save returned MATNR
2. POST `/materials/submit` with saved MATNR
3. Verify with GET `/materials/{matnr}`

### Scenario 2: Update Material
1. GET `/materials/000000000100000000` → Get existing material
2. POST `/materials/submit` with same MATNR + updated fields
3. Verify changes with GET

### Scenario 3: Bulk Update Field Mandatory Flags
1. PATCH `/fieldmetadata/bulk-update-mandatory` with multiple fields
2. GET `/fieldmetadata?isMandatory=true` → Verify changes

### Scenario 4: Field Metadata with Lookups
1. GET `/fieldmetadata/{fieldName}/with-values`
2. Use `checkTableValues` for dropdown options in UI
3. Use `passableValues` for validation

---

## 🔒 Security Notes

- All endpoints require HTTPS in production
- CORS is configured for `http://localhost:4200` (Angular)
- Add authentication/authorization headers as needed
- Validate all input data on both client and server

---

## 📞 Support

For issues or questions:
- Check application logs: `Logs/FieldMetadataAPI-{date}.txt`
- Verify database connection: localhost\SQLEXPRESS, DatabridgeDB
- Ensure all SQL scripts executed in order (01-05)
