You are a .net expert with knowledge about authorization

# .Net 10 Project

You should create a .net 10 api project following DDD and clean architecture principle with the following endpoints:

POST   /auth/register        create credentials, publish UserRegistered
POST   /auth/login           verify password, issue JWT + refresh token
POST   /auth/refresh         exchange refresh token for a new JWT
POST   /auth/logout          revoke refresh token
POST   /auth/password/forgot start reset (email a token)
POST   /auth/password/reset  set new password with token
GET    /auth/me              decode current token (debug/whoami)

# Database
Use Sql Server

## Database name
The database name is "Authorization" and should contain the following tables. Use latest version of efcore to use an AuthorizationContext

Credentials
  Id             (GUID)  -- THIS is the userId, the shared contract
  Email          (unique)
  PasswordHash
  EmailConfirmed
  CreatedUtc
 
RefreshTokens
  Id, UserId (FK->Credentials.Id), TokenHash, ExpiresUtc, RevokedUtc
 
UserRoles            -- local copy, kept in sync via events, used to build JWT claims
  UserId, Role

# Login Flow

1. POST /auth/login { email, password }
            |
            v
2. AUTH SERVICE
   - loads Credentials by email
   - verifies password hash
   - reads roles from local UserRoles copy (auth_db)
   - builds JWT:
       sub   = UserId
       email = email
       roles = [Dev, Lead, ...]
   - returns { accessToken, refreshToken }

# Registration Flow

1. POST /auth/register  { email, password, firstName, lastName }
            |
            v
2. AUTH SERVICE
   - validates email not taken
   - generates UserId (GUID)          <-- the contract is born here
   - hashes password
   - saves Credentials to auth_db
   - publishes event: UserRegistered { UserId, Email, FirstName, LastName }

# Events

Events are to be published to an Azure Queue named "authorization".

The following events are defined:

user.registered
user.roles.changed

// Published
public record UserRegistered(Guid UserId, string Email, string FirstName, string LastName);
 
// Consumed
public record UserRolesChanged(Guid UserId, string[] Roles);  // update local role copy

# AppSettings
Use appsettings.json to store queue details and efcore connection string
