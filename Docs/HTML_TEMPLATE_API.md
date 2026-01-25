# HTML Template Management API

## Overview
This API provides complete CRUD operations for managing HTML templates in MongoDB. Templates can be used for resume layouts, email templates, or any other HTML content storage needs.

## Database Collection
**Collection Name:** `html_templates`

**Schema:**
```javascript
{
  _id: ObjectId,
  html_template: String,
  template_name: String,
  created_at: DateTime,
  updated_at: DateTime,
  created_by: String (User ID)
}
```

---

## API Endpoints

### 1. Create HTML Template

**Endpoint:** `POST /api/template/create`

**Authentication:** Required (JWT Bearer Token)

**Request Body:**
```json
{
  "htmlTemplate": "<div><h1>Resume Template</h1><p>Content here...</p></div>",
  "templateName": "Modern Resume Template"
}
```

**Response (Success):**
```json
{
  "success": true,
  "message": "HTML template created successfully",
  "data": {
    "id": "65f1a2b3c4d5e6f7g8h9i0j1",
    "htmlTemplate": "<div><h1>Resume Template</h1><p>Content here...</p></div>",
    "templateName": "Modern Resume Template",
    "createdAt": "2026-01-23T18:00:00Z",
    "updatedAt": "2026-01-23T18:00:00Z",
    "createdBy": "user123"
  }
}
```

**Response (Error):**
```json
{
  "success": false,
  "message": "HTML template content is required"
}
```

**Validation Rules:**
- `htmlTemplate`: Required, cannot be empty
- `templateName`: Required, cannot be empty
- `createdBy`: Automatically extracted from JWT token

---

### 2. Get Paginated Templates List

**Endpoint:** `GET /api/template/list`

**Authentication:** Required (JWT Bearer Token)

**Query Parameters:**
- `page` (optional, default: 1): Page number (must be > 0)
- `pageSize` (optional, default: 10): Items per page (1-100)
- `search` (optional): Search term for template name or content

**Examples:**
```
GET /api/template/list
GET /api/template/list?page=2&pageSize=20
GET /api/template/list?search=resume
GET /api/template/list?page=1&pageSize=10&search=modern
```

**Response (Success):**
```json
{
  "success": true,
  "message": "Templates retrieved successfully",
  "data": {
    "templates": [
      {
        "id": "65f1a2b3c4d5e6f7g8h9i0j1",
        "templateName": "Modern Resume Template",
        "createdAt": "2026-01-23T18:00:00Z",
        "createdBy": "user123"
      },
      {
        "id": "65f1a2b3c4d5e6f7g8h9i0j2",
        "templateName": "Classic Resume Template",
        "createdAt": "2026-01-23T17:30:00Z",
        "createdBy": "user456"
      }
    ],
    "totalCount": 25,
    "page": 1,
    "pageSize": 10,
    "totalPages": 3
  }
}
```

**Response (Error):**
```json
{
  "success": false,
  "message": "Page size must be between 1 and 100"
}
```

**Features:**
- ✅ Pagination support
- ✅ Search by template name or content (case-insensitive)
- ✅ Sorted by creation date (newest first)
- ✅ Returns total count and total pages

---

### 3. Get Template by ID

**Endpoint:** `GET /api/template/{templateId}`

**Authentication:** Required (JWT Bearer Token)

**Path Parameters:**
- `templateId`: MongoDB ObjectId of the template

**Example:**
```
GET /api/template/65f1a2b3c4d5e6f7g8h9i0j1
```

**Response (Success):**
```json
{
  "success": true,
  "message": "Template retrieved successfully",
  "data": {
    "id": "65f1a2b3c4d5e6f7g8h9i0j1",
    "htmlTemplate": "<div><h1>Resume Template</h1><p>Content here...</p></div>",
    "templateName": "Modern Resume Template",
    "createdAt": "2026-01-23T18:00:00Z",
    "updatedAt": "2026-01-23T18:00:00Z",
    "createdBy": "user123"
  }
}
```

**Response (Not Found):**
```json
{
  "success": false,
  "message": "Template not found"
}
```

**Response (Error):**
```json
{
  "success": false,
  "message": "Template ID is required"
}
```

---

### 4. Delete Template by ID

**Endpoint:** `DELETE /api/template/{templateId}`

**Authentication:** Required (JWT Bearer Token)

**Path Parameters:**
- `templateId`: MongoDB ObjectId of the template

**Example:**
```
DELETE /api/template/65f1a2b3c4d5e6f7g8h9i0j1
```

**Response (Success):**
```json
{
  "success": true,
  "message": "Template deleted successfully"
}
```

**Response (Not Found):**
```json
{
  "success": false,
  "message": "Template not found"
}
```

**Response (Error):**
```json
{
  "success": false,
  "message": "Template ID is required"
}
```

---

## Usage Examples

### JavaScript/Fetch

```javascript
// Create Template
const createTemplate = async () => {
  const response = await fetch('http://localhost:5000/api/template/create', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${yourJwtToken}`
    },
    body: JSON.stringify({
      htmlTemplate: '<div><h1>My Template</h1></div>',
      templateName: 'My First Template'
    })
  });
  
  const data = await response.json();
  console.log(data);
};

// Get Paginated List
const getTemplates = async (page = 1, search = '') => {
  const url = new URL('http://localhost:5000/api/template/list');
  url.searchParams.append('page', page);
  url.searchParams.append('pageSize', 10);
  if (search) url.searchParams.append('search', search);
  
  const response = await fetch(url, {
    headers: {
      'Authorization': `Bearer ${yourJwtToken}`
    }
  });
  
  const data = await response.json();
  console.log(data);
};

// Get Template by ID
const getTemplate = async (templateId) => {
  const response = await fetch(`http://localhost:5000/api/template/${templateId}`, {
    headers: {
      'Authorization': `Bearer ${yourJwtToken}`
    }
  });
  
  const data = await response.json();
  console.log(data);
};

// Delete Template
const deleteTemplate = async (templateId) => {
  const response = await fetch(`http://localhost:5000/api/template/${templateId}`, {
    method: 'DELETE',
    headers: {
      'Authorization': `Bearer ${yourJwtToken}`
    }
  });
  
  const data = await response.json();
  console.log(data);
};
```

### cURL

```bash
# Create Template
curl -X POST http://localhost:5000/api/template/create \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -d '{
    "htmlTemplate": "<div><h1>My Template</h1></div>",
    "templateName": "My First Template"
  }'

# Get Paginated List
curl -X GET "http://localhost:5000/api/template/list?page=1&pageSize=10&search=resume" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"

# Get Template by ID
curl -X GET http://localhost:5000/api/template/65f1a2b3c4d5e6f7g8h9i0j1 \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"

# Delete Template
curl -X DELETE http://localhost:5000/api/template/65f1a2b3c4d5e6f7g8h9i0j1 \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

---

## Architecture

### Components Created

1. **Model:** `ResumeInOneMinute.Domain/Model/HtmlTemplate.cs`
   - MongoDB document model with BSON attributes

2. **DTOs:** `ResumeInOneMinute.Domain/DTO/HtmlTemplateDto.cs`
   - `CreateHtmlTemplateDto`: For creating templates
   - `HtmlTemplateResponseDto`: Full template details
   - `HtmlTemplateListDto`: Summary for list view
   - `PaginatedHtmlTemplatesDto`: Paginated results wrapper

3. **Repository Interface:** `ResumeInOneMinute.Domain/Interface/IHtmlTemplateRepository.cs`
   - Defines contract for template operations

4. **Repository Implementation:** `ResumeInOneMinute.Repository/Repositories/HtmlTemplateRepository.cs`
   - MongoDB operations with error handling and logging

5. **Controller:** `ResumeInOneMinute/Controllers/Template/HtmlTemplateController.cs`
   - REST API endpoints with validation

6. **Dependency Injection:** `Program.cs`
   - Repository registered as scoped service

---

## Features

✅ **Full CRUD Operations**
- Create, Read, List, Delete

✅ **Pagination**
- Configurable page size (1-100)
- Total count and total pages

✅ **Search Functionality**
- Search by template name or content
- Case-insensitive regex search

✅ **Authentication & Authorization**
- JWT token required for all endpoints
- User ID automatically captured from token

✅ **Validation**
- Required field validation
- Page/PageSize range validation
- Template ID validation

✅ **Error Handling**
- Comprehensive try-catch blocks
- Meaningful error messages
- Proper HTTP status codes

✅ **Logging**
- All operations logged with Serilog
- Success and error logging
- Template ID and user ID tracking

✅ **Standardized Responses**
- Consistent `Response<T>` format
- Success/failure indication
- Descriptive messages

---

## Testing Checklist

### 1. Create Template
- [ ] Create with valid data
- [ ] Create without htmlTemplate (should fail)
- [ ] Create without templateName (should fail)
- [ ] Create without authentication (should fail)
- [ ] Verify createdBy is set from JWT token

### 2. Get Paginated List
- [ ] Get first page with default pageSize
- [ ] Get specific page (e.g., page 2)
- [ ] Get with custom pageSize
- [ ] Search by template name
- [ ] Search by content
- [ ] Invalid page number (should fail)
- [ ] Invalid pageSize (should fail)
- [ ] Verify sorting (newest first)

### 3. Get by ID
- [ ] Get existing template
- [ ] Get non-existent template (should return 404)
- [ ] Get with invalid ID format
- [ ] Verify full template content returned

### 4. Delete
- [ ] Delete existing template
- [ ] Delete non-existent template (should return 404)
- [ ] Delete with invalid ID format
- [ ] Verify template is actually deleted

---

## MongoDB Indexes (Recommended)

For better performance, create these indexes:

```javascript
// In MongoDB shell or Compass
db.html_templates.createIndex({ "template_name": "text", "html_template": "text" });
db.html_templates.createIndex({ "created_at": -1 });
db.html_templates.createIndex({ "created_by": 1 });
```

---

## Future Enhancements

Potential improvements:
1. **Update Template** - Add PUT endpoint to update existing templates
2. **Template Categories** - Add category/tags for better organization
3. **Template Versioning** - Track template versions
4. **Template Sharing** - Share templates between users
5. **Template Preview** - Generate preview images
6. **Bulk Operations** - Delete/export multiple templates
7. **Template Cloning** - Duplicate existing templates
8. **Access Control** - Only allow users to delete their own templates

---

## Status

✅ **Implementation Complete**
- All endpoints implemented
- Repository registered
- Ready for testing

**Next Steps:**
1. Build and run the application
2. Test all endpoints via Swagger
3. Verify MongoDB collection creation
4. Test pagination and search
