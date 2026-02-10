# Author Profile API Examples

## 1. User Claims Author Identity

**Endpoint:** `POST /api/users/{userId}/author-profile`

**Request:**
```http
POST /api/users/a1b2c3d4-e5f6-7890-abcd-ef1234567890/author-profile
Authorization: Bearer {admin_token}
Content-Type: application/json

{
  "authorId": 123
}
```

**Response (200 OK):**
```json
{
  "userId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "authorId": 123,
  "isVerified": false,
  "message": "Successfully linked to author 'Nguyễn Du'. Your profile requires admin verification to receive the blue tick."
}
```

**Error Cases:**

**404 Not Found - User doesn't exist:**
```json
{
  "error": "User with ID a1b2c3d4-e5f6-7890-abcd-ef1234567890 not found."
}
```

**404 Not Found - Author doesn't exist:**
```json
{
  "error": "Author with ID 123 not found."
}
```

**400 Bad Request - Already has author profile:**
```json
{
  "error": "User is already linked to Author ID 456. Please contact admin to update your author profile."
}
```

---

## 2. Admin Verifies Author Profile (Grant Blue Tick)

**Endpoint:** `PATCH /api/users/{userId}/author-profile/verification`

**Request:**
```http
PATCH /api/users/a1b2c3d4-e5f6-7890-abcd-ef1234567890/author-profile/verification
Authorization: Bearer {admin_token}
Content-Type: application/json

{
  "isVerified": true
}
```

**Response (200 OK):**
```json
{
  "userId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "authorId": 123,
  "isVerified": true,
  "verifiedAt": "2026-02-09T09:15:30Z",
  "message": "Author profile verified successfully. Blue tick granted."
}
```

**Error Cases:**

**404 Not Found - User doesn't exist:**
```json
{
  "error": "User with ID a1b2c3d4-e5f6-7890-abcd-ef1234567890 not found."
}
```

**400 Bad Request - No author profile:**
```json
{
  "error": "User does not have an author profile. They must claim an author identity first."
}
```

---

## 3. Admin Revokes Verification

**Request:**
```http
PATCH /api/users/a1b2c3d4-e5f6-7890-abcd-ef1234567890/author-profile/verification
Authorization: Bearer {admin_token}
Content-Type: application/json

{
  "isVerified": false
}
```

**Response (200 OK):**
```json
{
  "userId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "authorId": 123,
  "isVerified": false,
  "verifiedAt": null,
  "message": "Author profile verification revoked."
}
```

---

## 4. Get Author Details (Shows Verification Status)

**Endpoint:** `GET /api/authors/{authorId}`

**Request:**
```http
GET /api/authors/123
```

**Response (200 OK):**
```json
{
  "id": 123,
  "fullName": "Nguyễn Du",
  "bio": "Nhà thơ lớn của Việt Nam...",
  "dateOfBirth": "1765-01-03T00:00:00Z",
  "diedDate": "1820-09-16T00:00:00Z",
  "country": "Vietnam",
  "photoUrl": "https://example.com/nguyen-du.jpg",
  "isVerified": true,
  "verifiedAt": "2026-02-09T09:15:30Z",
  "linkedUserId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "createdAt": "2026-02-01T10:00:00Z",
  "updatedAt": "2026-02-09T09:15:30Z",
  "books": [
    {
      "bookId": 1,
      "title": "Truyện Kiều",
      "coverImageUrl": "https://example.com/truyen-kieu.jpg"
    }
  ]
}
```

**Note:** 
- `isVerified` = true → Show blue tick ✓
- `linkedUserId` = the User who claimed this author

---

## 5. Get Authors List (Shows Verification)

**Endpoint:** `GET /api/authors`

**Request:**
```http
GET /api/authors?pageNumber=1&pageSize=10&searchTerm=Nguyễn
```

**Response (200 OK):**
```json
{
  "items": [
    {
      "id": 123,
      "fullName": "Nguyễn Du",
      "bio": "Nhà thơ lớn của Việt Nam...",
      "country": "Vietnam",
      "photoUrl": "https://example.com/nguyen-du.jpg",
      "isVerified": true,
      "bookCount": 5
    },
    {
      "id": 124,
      "fullName": "Nguyễn Nhật Ánh",
      "bio": "Tác giả Việt Nam...",
      "country": "Vietnam",
      "photoUrl": "https://example.com/nna.jpg",
      "isVerified": false,
      "bookCount": 12
    }
  ],
  "pageNumber": 1,
  "pageSize": 10,
  "totalCount": 2,
  "totalPages": 1
}
```

---

## Complete User Journey Example

### Step 1: Admin creates author in catalog
```http
POST /api/authors
Authorization: Bearer {admin_token}

{
  "fullName": "Nguyễn Du",
  "bio": "Nhà thơ lớn của Việt Nam, tác giả Truyện Kiều",
  "dateOfBirth": "1765-01-03",
  "country": "Vietnam"
}

→ Response: { "authorId": 123, "fullName": "Nguyễn Du" }
```

### Step 2: User registers and claims this author
```http
POST /api/users/a1b2c3d4-e5f6-7890-abcd-ef1234567890/author-profile
Authorization: Bearer {admin_token}

{
  "authorId": 123
}

→ Response: { "isVerified": false, ... }
```

### Step 3: Admin reviews and verifies
```http
PATCH /api/users/a1b2c3d4-e5f6-7890-abcd-ef1234567890/author-profile/verification
Authorization: Bearer {admin_token}

{
  "isVerified": true
}

→ Response: { "isVerified": true, "verifiedAt": "2026-02-09...", ... }
```

### Step 4: Frontend displays blue tick
```http
GET /api/authors/123

→ Response shows: "isVerified": true
→ UI displays: Nguyễn Du ✓ (blue tick)
```

---

## Testing with cURL

### 1. Claim author identity
```bash
curl -X POST "http://localhost:5000/api/users/a1b2c3d4-e5f6-7890-abcd-ef1234567890/author-profile" \
  -H "Authorization: Bearer YOUR_ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"authorId": 123}'
```

### 2. Verify author profile
```bash
curl -X PATCH "http://localhost:5000/api/users/a1b2c3d4-e5f6-7890-abcd-ef1234567890/author-profile/verification" \
  -H "Authorization: Bearer YOUR_ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"isVerified": true}'
```

### 3. Get author with verification status
```bash
curl -X GET "http://localhost:5000/api/authors/123"
```

---

## Authorization

- **POST /api/users/{id}/author-profile** → **Admin only**
- **PATCH /api/users/{id}/author-profile/verification** → **Admin only**
- **GET /api/authors/{id}** → **Public**
- **GET /api/authors** → **Public**
