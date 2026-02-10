# Author Verification Refactoring - Complete Implementation

## Overview
Refactored author verification system to separate **catalog data** (Author entity) from **user identity verification** (AuthorProfile entity), following the same pattern as SellerProfile.

---

## Architecture Changes

### Before:
```
Author (CatalogAggregate)
├── Catalog data (FullName, Bio, etc.)
└── User verification (UserId, IsVerified, VerifiedAt) ❌ Mixed concerns
```

### After:
```
Author (CatalogAggregate)          AuthorProfile (UserAggregate)
├── Pure catalog data              ├── AuthorId (link to catalog)
└── BookAuthors[]                  ├── IsVerified (blue tick)
                                   └── VerifiedAt
```

---

## New Entities

### 1. AuthorProfile (UserAggregate)
**File:** `src/BookStation.Domain/Entities/UserAggregate/AuthorProfile.cs`

```csharp
public class AuthorProfile : Entity<Guid>
{
    public long AuthorId { get; private set; }      // Links to Author in catalog
    public bool IsVerified { get; private set; }    // Blue tick status
    public DateTime? VerifiedAt { get; private set; }
    
    public void Verify() { ... }
    public void RevokeVerification() { ... }
    public void UpdateAuthorId(long authorId) { ... }
}
```

---

## Updated Entities

### 1. Author (CatalogAggregate) - CLEANED UP
**File:** `src/BookStation.Domain/Entities/CatalogAggregate/Author.cs`

**Removed:**
- `UserId` property
- `IsVerified` property
- `VerifiedAt` property
- `LinkToUser()` method
- `Verify()` method
- `RevokeVerification()` method

**Now contains:** Pure catalog metadata only

---

### 2. User (UserAggregate) - EXTENDED
**File:** `src/BookStation.Domain/Entities/UserAggregate/User.cs`

**Added:**
```csharp
public AuthorProfile? AuthorProfile { get; private set; }

public AuthorProfile BecomeAnAuthor(long authorId) { ... }
```

---

### 3. Book - SIMPLIFIED
**File:** `src/BookStation.Domain/Entities/CatalogAggregate/Book.cs`

**Added:**
```csharp
public string? Translator { get; private set; }  // Simple string
public string? CoAuthor { get; private set; }    // Simple string

public void SetTranslator(string? translator) { ... }
public void SetCoAuthor(string? coAuthor) { ... }
```

**Changed:**
```csharp
// Before: public void AddAuthor(long authorId, EAuthorRole role, int displayOrder)
// After:
public void AddAuthor(long authorId, int displayOrder = 1)
```

---

### 4. BookAuthor - SIMPLIFIED
**File:** `src/BookStation.Domain/Entities/CatalogAggregate/BookAuthor.cs`

**Removed:**
- `EAuthorRole Role` property
- `Update(role, displayOrder)` method

**Changed:**
```csharp
// Before: internal static BookAuthor Create(long bookId, long authorId, EAuthorRole role, int displayOrder)
// After:
internal static BookAuthor Create(long bookId, long authorId, int displayOrder = 1)
```

---

### 5. BookEnums - REMOVED ENUM
**File:** `src/BookStation.Domain/Enums/BookEnums.cs`

**Removed:** Entire `EAuthorRole` enum

---

## Commands & Handlers

### New Files Created:

#### 1. AuthorProfileCommands.cs
**Path:** `src/BookStation.Application/Users/Commands/AuthorProfileCommands.cs`

**Commands:**
- `BecomeAuthorCommand(UserId, AuthorId)` → `BecomeAuthorResponse`
- `VerifyAuthorProfileCommand(UserId, IsVerified)` → `VerifyAuthorProfileResponse`

---

#### 2. AuthorProfileCommandHandlers.cs
**Path:** `src/BookStation.Application/Users/Commands/AuthorProfileCommandHandlers.cs`

**Handlers:**
- `BecomeAuthorCommandHandler` - Creates AuthorProfile linking user to catalog author
- `VerifyAuthorProfileCommandHandler` - Admin verifies/revokes author profile

---

### Updated Files:

#### 1. AuthorCommands.cs
**Path:** `src/BookStation.Application/Authors/Commands/AuthorCommands.cs`

**Removed:**
- `LinkAuthorToUserCommand`
- `VerifyAuthorCommand`
- `UserId` parameter from `CreateAuthorCommand`

---

#### 2. AuthorCommandHandlers.cs
**Path:** `src/BookStation.Application/Authors/Commands/AuthorCommandHandlers.cs`

**Removed:**
- `LinkAuthorToUserCommandHandler`
- `VerifyAuthorCommandHandler`

---

## Query Updates

### 1. GetAuthorByIdQuery.cs
**Path:** `src/BookStation.Query/Authors/GetAuthorByIdQuery.cs`

**DTO Changes:**
```csharp
// Before:
public record AuthorDetailDto(..., long? UserId, bool IsVerified, DateTime? VerifiedAt, ...)

// After:
public record AuthorDetailDto(..., bool IsVerified, DateTime? VerifiedAt, Guid? LinkedUserId, ...)
```

**Handler Changes:**
- Now queries `AuthorProfile` to get verification status
- `LinkedUserId` is from `AuthorProfile.Id` (equals `User.Id`)

---

### 2. GetAuthorsQuery.cs
**Path:** `src/BookStation.Query/Authors/GetAuthorsQuery.cs`

**Handler Changes:**
- Queries `AuthorProfile` separately for verification status
- Joins in-memory to populate `IsVerified` in results

---

### 3. AuthorBookDto
**Removed:** `Role` field (no longer needed)

---

## API Endpoints

### AuthorsController - REMOVED ENDPOINTS
**Path:** `src/BookStation.PublicApi/Controllers/AuthorsController.cs`

**Removed:**
- `POST /api/authors/{id}/link-user`
- `PATCH /api/authors/{id}/verification`

---

### UsersController - NEW ENDPOINTS
**Path:** `src/BookStation.PublicApi/Controllers/UsersController.cs`

**Added:**

#### 1. Claim Author Identity (Admin Only)
```
POST /api/users/{id:guid}/author-profile
Body: { "authorId": 123 }
```

**Response:**
```json
{
  "userId": "guid",
  "authorId": 123,
  "isVerified": false,
  "message": "Successfully linked to author 'Nguyễn Du'. Your profile requires admin verification..."
}
```

---

#### 2. Verify/Revoke Author Profile (Admin Only)
```
PATCH /api/users/{id:guid}/author-profile/verification
Body: { "isVerified": true }
```

**Response:**
```json
{
  "userId": "guid",
  "authorId": 123,
  "isVerified": true,
  "verifiedAt": "2026-02-09T16:00:00Z",
  "message": "Author profile verified successfully. Blue tick granted."
}
```

---

## Repository Changes

### IAuthorRepository & AuthorRepository
**Removed:** `GetByUserIdAsync()` method

---

## User Flow

### 1. User Claims Author Identity
```
User → POST /api/users/{userId}/author-profile
     → BecomeAuthorCommand
     → User.BecomeAnAuthor(authorId)
     → AuthorProfile created (IsVerified = false)
```

### 2. Admin Verifies
```
Admin → PATCH /api/users/{userId}/author-profile/verification
      → VerifyAuthorProfileCommand  
      → AuthorProfile.Verify()
      → IsVerified = true, VerifiedAt = now
```

### 3. Frontend Displays Blue Tick
```
GET /api/authors/{authorId}
  → Returns: { ..., "isVerified": true, "verifiedAt": "...", "linkedUserId": "guid" }
  → UI shows ✓ blue tick next to author name
```

---

## Database Migration Required

```sql
-- 1. Create AuthorProfiles table
CREATE TABLE AuthorProfiles (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    AuthorId BIGINT NOT NULL,
    IsVerified BIT NOT NULL DEFAULT 0,
    VerifiedAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    FOREIGN KEY (Id) REFERENCES Users(Id),
    FOREIGN KEY (AuthorId) REFERENCES Authors(Id)
);

-- 2. Remove columns from Authors
ALTER TABLE Authors DROP COLUMN UserId;
ALTER TABLE Authors DROP COLUMN IsVerified;
ALTER TABLE Authors DROP COLUMN VerifiedAt;

-- 3. Remove column from BookAuthors
ALTER TABLE BookAuthors DROP COLUMN Role;

-- 4. Add columns to Books
ALTER TABLE Books ADD Translator NVARCHAR(MAX) NULL;
ALTER TABLE Books ADD CoAuthor NVARCHAR(MAX) NULL;
```

---

## Benefits of This Design

| Aspect | Benefit |
|--------|---------|
| **Separation of Concerns** | Catalog data ≠ User verification |
| **Consistency** | Same pattern as SellerProfile |
| **Flexibility** | Author can exist without user account |
| **Scalability** | Easy to add PublisherProfile, EditorProfile, etc. |
| **Clarity** | "User X is verified as Author Y" vs "Author Y is verified" |

---

## Files Summary

### Created (3 files):
1. `Domain/Entities/UserAggregate/AuthorProfile.cs`
2. `Application/Users/Commands/AuthorProfileCommands.cs`
3. `Application/Users/Commands/AuthorProfileCommandHandlers.cs`

### Modified (11 files):
1. `Domain/Entities/CatalogAggregate/Author.cs`
2. `Domain/Entities/CatalogAggregate/Book.cs`
3. `Domain/Entities/CatalogAggregate/BookAuthor.cs`
4. `Domain/Entities/CatalogAggregate/BookEnums.cs`
5. `Domain/Entities/UserAggregate/User.cs`
6. `Domain/Repositories/IAuthorRepository.cs`
7. `Infrastructure/Repositories/AuthorRepository.cs`
8. `Application/Authors/Commands/AuthorCommands.cs`
9. `Application/Authors/Commands/AuthorCommandHandlers.cs`
10. `Query/Authors/GetAuthorByIdQuery.cs`
11. `Query/Authors/GetAuthorsQuery.cs`
12. `PublicApi/Controllers/AuthorsController.cs`
13. `PublicApi/Controllers/UsersController.cs`

---

## Next Steps

1. ✅ Create EF Configuration for `AuthorProfile`
2. ✅ Run database migration
3. ✅ Test API endpoints
4. ✅ Update frontend to use new endpoints
5. ✅ Update documentation/API specs

---

**Status:** Implementation Complete ✅
**Date:** 2026-02-09
