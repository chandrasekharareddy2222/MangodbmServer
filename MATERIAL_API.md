# Material API - Auto-Generated MATNR

## Overview
The Material API handles both **CREATE** and **UPDATE** operations in a single endpoint based on whether `MATNR` is provided.

## Recommended Workflow (Show Material Number in Form)

### Step 1: Pre-Generate MATNR for New Material Form
When user opens the **Create Material** form, call this endpoint to get the Material Number immediately:

**Request:**
```http
GET /api/v1/materials/generate-matnr
```

**Response:**
```json
{
  "success": true,
  "message": "Material Number generated successfully",
  "data": "000000000100000000"
}
```

**Angular Example:**
```typescript
ngOnInit() {
  if (this.isCreateMode) {
    this.materialService.generateMatnr().subscribe(response => {
      this.form.patchValue({ matnr: response.data });
      this.form.get('matnr').disable(); // Make it read-only
    });
  }
}
```

### Step 2: Display MATNR in Form
```html
<!-- Material Number field - visible and marked as required -->
<mat-form-field>
  <mat-label>Material Number *</mat-label>
  <input matInput formControlName="matnr" readonly>
</mat-form-field>
```

### Step 3: Submit Material with Pre-Generated MATNR

**Request:**
```json
POST /api/v1/materials/submit
{
  "matnr": "000000000100000000",  // ← Pre-generated from Step 1
  "mtart": "FERT",
  "meins": "EA",
  "mbrsh": "M",
  "matkl": "MAT001",
  "attributes": {
    "/BEV1/LULDEGRP": "GRP001",
    "/BEV1/LULEINH": "345",
    "SIZE1": "Large"
  },
  "submittedBy": "User123"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Material 000000000100000000 created successfully",
  "data": {
    "matnr": "000000000100000000",  // ← Auto-generated!
    "mtart": "FERT",
    "meins": "EA",
    "mbrsh": "M",
    "matkl": "MAT001",
    "attributes": {
      "/BEV1/LULDEGRP": "GRP001",
      "/BEV1/LULEINH": "345",
      "SIZE1": "Large"
    },
    "status": "ACTIVE",
    "createdDate": "2026-02-16T10:30:00"
  }
}
```

### UPDATE Existing Material
When `MATNR` is **provided**, the system updates the existing material.

**Request:**
```json
POST /api/v1/materials/submit
{
  "matnr": "000000000100000000",  // ← Material Number provided
  "mtart": "FERT",
  "meins": "EA",
  "attributes": {
    "/BEV1/LULDEGRP": "GRP002",  // Updated
    "SIZE1": "Extra Large"       // Updated
  },
  "submittedBy": "User123"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Material 000000000100000000 updated successfully",
  "data": {
    "matnr": "000000000100000000",
    "mtart": "FERT",
    "meins": "EA",
    "attributes": {
      "/BEV1/LULDEGRP": "GRP002",
      "SIZE1": "Extra Large"
    },
    "modifiedDate": "2026-02-16T11:45:00"
  }
}
```

## Key Features

### 1. **Pre-Generate MATNR for Form Display**
- Call `GET /generate-matnr` when opening create form
- Display Material Number immediately (marked as required *)
- MATNR shown as read-only field
- Uses SQL Server SEQUENCE starting from `100000000`
- Padded to 18 characters: `000000000100000000`

### 2. **Smart CREATE vs UPDATE in Submit**
- **MATNR provided** → CREATE new material with that number
- **MATNR provided (existing)** → UPDATE existing material

### 3. **Mandatory Fields**
- `MATNR` (Material Number) - **Always required** (pre-generated for new materials)
- `MTART` (Material Type) - Required for CREATE
- `MEINS` (Base Unit) - Required for CREATE
- `MBRSH` (Industry Sector) - Optional
- `MATKL` (Material Group) - Optional

### 4. **Dynamic Attributes**
- Store any additional field from `Field_Metadata`
- Automatically linked with field metadata for validation
- Stores DataType, FieldLength, Decimals from metadata

### 5. **Form Display Strategy (Recommended)**
In your Angular form:
- **New Material Form**: 
  1. Call `GET /generate-matnr` on form load
  2. Display MATNR in read-only field (marked as required *)
  3. User sees Material Number before submitting
- **Edit Material Form**: 
  1. Load existing material data
  2. Show MATNR as read-only field

## Database Tables

### Material_Master
```sql
MATNR CHAR(18) PRIMARY KEY  -- Pre-generated via GET /generate-matnr
MTART CHAR(4) NOT NULL      -- Material Type
MEINS CHAR(3) NOT NULL      -- Base Unit
MBRSH CHAR(1)               -- Industry Sector  
MATKL CHAR(9)               -- Material Group
Status VARCHAR(20)          -- ACTIVE, BLOCKED, DELETED, PENDING
```

### Material_Attributes
```sql
AttributeID BIGINT IDENTITY(1,1)
MATNR CHAR(18)              -- Foreign Key to Material_Master
FieldName VARCHAR(50)       -- Dynamic field name
FieldValue NVARCHAR(MAX)    -- Dynamic field value
DataType VARCHAR(20)        -- From Field_Metadata
```

## API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/materials/generate-matnr` | **Pre-generate Material Number for form display** |
| POST | `/api/v1/materials/submit` | Create new or update existing material |
| GET | `/api/v1/materials/{matnr}` | Get material by MATNR |
| GET | `/api/v1/materials` | Get all materials (paginated) |

## Validation Rules

### CREATE (MATNR pre-generated)
- ✅ MATNR is **required** (pre-generated from GET /generate-matnr)
- ✅ MTART is **required**
- ✅ MEINS is **required**
- ❌ MBRSH is **optional**
- ❌ MATKL is **optional**

### UPDATE (MATNR from existing material)
- ✅ MATNR must exist in database
- ❌ MTART is **optional** (keeps existing if not provided)
- ❌ MEINS is **optional** (keeps existing if not provided)

## Complete Angular Example

### Step 1: Pre-Generate MATNR on Form Load
```typescript
export class MaterialFormComponent implements OnInit {
  form: FormGroup;
  isEditMode = false;
  
  ngOnInit() {
    this.form = this.fb.group({
      matnr: [{ value: '', disabled: true }, Validators.required],  // ← Always visible
      mtart: ['', Validators.required],
      meins: ['', Validators.required],
      mbrsh: [''],
      matkl: [''],
      attributes: this.fb.group({
        '/BEV1/LULDEGRP': [''],
        'SIZE1': ['']
      })
    });
    
    if (!this.isEditMode) {
      // Pre-generate MATNR for new material
      this.materialService.generateMatnr().subscribe(response => {
        this.form.patchValue({ matnr: response.data });
      });
    } else {
      // Load existing material
      this.loadMaterial();
    }
  }
  
  submitMaterial() {
    const formValue = this.form.getRawValue(); // Get disabled fields too
    const payload = {
      matnr: formValue.matnr,  // ← Always included
      mtart: formValue.mtart,
      meins: formValue.meins,
      mbrsh: formValue.mbrsh,
      matkl: formValue.matkl,
      attributes: formValue.attributes,
      submittedBy: this.currentUser
    };

    this.http.post('/api/v1/materials/submit', payload).subscribe(response => {
      console.log('Material saved:', response.data.matnr);
      this.router.navigate(['/materials']);
    });
  }
}
```

### Step 2: Service for Generate MATNR
```typescript
export class MaterialService {
  generateMatnr(): Observable<ApiResponse<string>> {
    return this.http.get<ApiResponse<string>>('/api/v1/materials/generate-matnr');
  }
}
```

### Step 3: HTML Template
```html
<form [formGroup]="form" (ngSubmit)="submitMaterial()">
  <!-- Material Number - Always visible, always required, always read-only -->
  <mat-form-field>
    <mat-label>Material Number *</mat-label>
    <input matInput formControlName="matnr" readonly>
    <mat-hint>Auto-generated unique identifier</mat-hint>
  </mat-form-field>

  <mat-form-field>
    <mat-label>Material Type *</mat-label>
    <mat-select formControlName="mtart">
      <mat-option value="FERT">Finished Goods</mat-option>
      <mat-option value="HALB">Semi-Finished</mat-option>
      <mat-option value="ROH">Raw Material</mat-option>
    </mat-select>
  </mat-form-field>

  <mat-form-field>
    <mat-label>Base Unit *</mat-label>
    <input matInput formControlName="meins">
  </mat-form-field>

  <!-- Optional fields -->
  <mat-form-field>
    <mat-label>Industry Sector</mat-label>
    <input matInput formControlName="mbrsh">
  </mat-form-field>

  <mat-form-field>
    <mat-label>Material Group</mat-label>
    <input matInput formControlName="matkl">
  </mat-form-field>

  <!-- Dynamic attributes -->
  <div formGroupName="attributes">
    <mat-form-field>
      <mat-label>LULDEGRP</mat-label>
      <input matInput formControlName="/BEV1/LULDEGRP">
    </mat-form-field>
    
    <mat-form-field>
      <mat-label>Size</mat-label>
      <input matInput formControlName="SIZE1">
    </mat-form-field>
  </div>

  <button mat-raised-button color="primary" type="submit" [disabled]="form.invalid">
    {{ isEditMode ? 'Update' : 'Create' }} Material
  </button>
</form>
```

### Alternative: Update Material (Legacy - MATNR from route)
```typescript
submitMaterial() {
  const payload = {
    mtart: this.form.get('mtart').value,
    meins: this.form.get('meins').value,
    attributes: {
      '/BEV1/LULDEGRP': this.form.get('luldegrp').value,
      'SIZE1': this.form.get('size').value
    },
    submittedBy: this.currentUser
  };

  this.http.post('/api/v1/materials/submit', payload).subscribe(response => {
    console.log('Created MATNR:', response.data.matnr);
    // Now show MATNR to user or navigate to edit form
  });
}
```

## SQL Script Execution Order

1. `01_CreateTable.sql` - Field_Metadata table
2. `02_SeedData.sql` - Field_Metadata seed data
3. `03_CreateLookupTables.sql` - Check_Table_Values, Passable_Values
4. `04_SeedLookupData.sql` - Lookup seed data
5. **`05_CreateMaterialTables.sql`** - Material_Master, Material_Attributes, MATNR sequence ← **Execute this next!**

## Next Steps

1. **Execute SQL Script**: Run `05_CreateMaterialTables.sql` in SSMS against DatabridgeDB
2. **Test Generate Endpoint**: `GET /api/v1/materials/generate-matnr` → Should return `000000000100000000`
3. **Test Submit Endpoint**: POST with pre-generated MATNR
4. **Update Angular Form**: 
   - Call `generateMatnr()` in `ngOnInit()` for create mode
   - Show MATNR field as read-only with required asterisk (*)
   - Include MATNR in submit payload using `getRawValue()`

