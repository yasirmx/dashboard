You are a .net expert with knowledge about user profile

# .Net 10 Project

You should create a .net 10 api project following DDD and clean architecture principle with the following endpoints:

GET    /users/{id}            get a single profile (name, avatar, roles)
GET    /users?ids=a,b,c       batch fetch (for composing task cards)
GET    /users?search=&role=   list / filter (user picker, admin)
PUT    /users/{id}            update profile (name, avatar, department)
PUT    /users/{id}/roles      set roles  -> publishes UserRolesChanged
POST   /users/{id}/deactivate disable user -> publishes UserDeactivated


# Database
Use Sql Server

## Database name
The database name is "Users" and should contain the following tables. Use latest version of efcore to use an AuthorizationContext

Profiles
  UserId         (GUID)  -- SAME id as Credentials.Id (the contract)
  Email          -- denormalized copy for display/search
  FirstName, LastName, AvatarUrl, Department
  CreatedUtc
 
UserRoles            -- SOURCE OF TRUTH for role assignment
  UserId, Role         -- Dev | QA | PO | Lead

# Roles
Dev						--Create/edit own tasks, change status, add sub-
QA						--Same as Dev + move tasks to Pending/Closed after testing
PO						--Manage sprints, prioritize backlog, assign work
Engineering Lead        --All of the above + delete, reassign across team, admin actions

Roles are read from the JWT by other services for authorization; User Service manages the assignment and emits UserRolesChanged when it changes.

# Seed Data
Seed 10 user profiles with different roles

# The Registration Sync (Consumer)

on UserRegistered { UserId, Email, FirstName, LastName }:
   - create Profile in user_db (same UserId)
   - assign default role (e.g. Dev)
   - publish UserRolesChanged { UserId, Roles:[Dev] }

This is how a profile comes into existence — reactively, after Auth creates the credential. Make the consumer idempotent (upsert by UserId) to tolerate duplicates.


# Events

// Consumed
public record UserRegistered(Guid UserId, string Email, string FirstName, string LastName);

//Event Names:

user.roles.changed   		-> Auth Service → update local role copy for JWT claims
user.profile.updated 		-> Task Service (optional cache) → refresh denormalized name/avatar
user.profile.deactivated	-> Task Service → optionally unassign that user's tasks

// Published
public record UserRolesChanged(Guid UserId, string[] Roles);
public record UserProfileUpdated(Guid UserId, string FirstName, string LastName, string AvatarUrl);
public record UserDeactivated(Guid UserId);

## Queues
Publisher queue name is users
Consumer queue name is authorization

# Cross-service Composition
Task card needs assignee name/avatar:
   Task Service stores only AssigneeId (GUID)
   -> GET /users/{assigneeId}   OR   GET /users?ids=...   (batch)

# Reliability Patterns
Idempotent UserRegistered consumer  -> Duplicate events must not create duplicate profiles
Transactional Outbox				-> Role change + UserRolesChanged event atomic
Eventual consistency				-> Profile appears shortly after registration — acceptable
Batch endpoint (?ids=)				-> Avoid N+1 calls when composing lists of task cards

# Key Principles Recap
•	User Service owns profiles and is the source of truth for role assignment.
•	UserId (GUID) matches the Auth Credentials.Id — the shared contract.
•	Creates profiles reactively by consuming UserRegistered.
•	Publishes UserRolesChanged / UserProfileUpdated for others to sync.
•	Provides batch lookups so task cards compose efficiently.

# Postman collection
Create a folder called Postman and generate the collection for the API

# AppSettings
Use appsettings.json to store queue details and efcore connection string
