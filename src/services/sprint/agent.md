Sprint Service
Sprint Lifecycle — Task Management Dashboard (Microservices, .NET)

1. Responsibilities — What It Owns
+----------------------------------------------+
|              SPRINT SERVICE                  |
+----------------------------------------------+
| - Create / edit / delete sprints             |
| - Sprint lifecycle (Planned->Active->Done)   |
| - Start date / end date / goal               |
| - Sprint capacity & metadata                 |
| - Source of truth: the SPRINT itself         |
| - NOT the tasks - only references to them    |
+----------------------------------------------+
                    |  sprint_db
                    v
        +------------------------+
        | Sprints                |
        | SprintMembers (opt.)   |
        | SprintStats (read mdl) |
        +------------------------+

Crucial boundary: the Task Service owns tasks; the Sprint Service owns sprints. A task stores which sprint it belongs to (Task.SprintId). The Sprint Service never stores task rows.
2. Database Schema (sprint_db)
Sprints
  Id              (GUID)   -- the contract: SprintId
  Name            -- "Sprint 24 - Q2 Hardening"
  Goal            -- short description / objective
  Status          -- Planned | Active | Completed | Cancelled
  StartDateUtc
  EndDateUtc
  Capacity        -- optional: story points / hours
  CreatedByUserId -- a userId GUID (from Auth/JWT)
  CreatedUtc, UpdatedUtc
 
SprintMembers          -- optional, if a sprint has an explicit team
  SprintId (FK), UserId
 
SprintStats            -- denormalized read model, eventually consistent
  SprintId
  TotalTasks
  NewCount, ActiveCount, PendingCount, ClosedCount
  UpdatedUtc

3. Sprint Lifecycle (State Machine)
   create
     |
     v
 +---------+   start    +---------+   complete   +-----------+
 | Planned | ---------> | Active  | -----------> | Completed |
 +---------+            +---------+              +-----------+
     |                       |
     | cancel                | cancel
     v                       v
 +------------+         +------------+
 | Cancelled  |         | Cancelled  |
 +------------+         +------------+

Business rules:
•	Only one Active sprint per team/project at a time (optional but common).
•	Can't start a sprint whose StartDate is in the future (or auto-start by schedule).
•	Can't complete a Planned sprint (must go Active first).
•	When a sprint completes, unfinished tasks should be handled (rollover).
•	EndDate must be after StartDate.
4. API Surface
POST   /sprints                 create a sprint (Planned)
GET    /sprints                 list (filter by status, date range)
GET    /sprints/{id}            get one
PUT    /sprints/{id}            edit name/goal/dates (only if Planned/Active)
POST   /sprints/{id}/start      Planned -> Active
POST   /sprints/{id}/complete   Active -> Completed
POST   /sprints/{id}/cancel     -> Cancelled
DELETE /sprints/{id}            delete (only if Planned & no tasks)
 
GET    /sprints/{id}/summary    aggregated stats (task counts by status)

Authorization (from JWT roles):
•	Create/start/complete/cancel → typically Lead or PO only.
•	Read → any authenticated user.
5. The Sprint ↔ Task Relationship
A. Query-time composition (for reads)
Angular Sprint Board
   |
   |-> GET /sprints/{id}          (Sprint Service)  -> sprint meta
   \-> GET /tasks?sprintId={id}   (Task Service)    -> tasks in sprint

The frontend / API gateway composes them. The Sprint Service does not call the Task Service to embed tasks — keep them decoupled.
B. Event-driven stats (for the summary)
Task Service publishes:
   TaskAddedToSprint     { TaskId, SprintId }
   TaskStatusChanged     { TaskId, SprintId, OldStatus, NewStatus }
   TaskRemovedFromSprint { TaskId, SprintId }
        |
        v  message bus
Sprint Service consumes -> updates SprintStats (fast local /summary)

6. Events
// Published
public record SprintCreated(Guid SprintId, string Name, DateTime StartUtc, DateTime EndUtc);
public record SprintStarted(Guid SprintId);
public record SprintCompleted(Guid SprintId, Guid[] UnfinishedTaskIds);
public record SprintCancelled(Guid SprintId);
 
// Consumed (from Task Service) -> maintain SprintStats
TaskAddedToSprint, TaskStatusChanged, TaskRemovedFromSprint

The complete-sprint flow:
Sprint Service:  POST /sprints/{id}/complete
   - set Status = Completed
   - publish SprintCompleted { SprintId }
        |
        v
Task Service (consumer):
   - find tasks where SprintId == X and Status != Closed
   - move them to backlog (SprintId = null) or next sprint
   - publish TaskRemovedFromSprint for each

7. Reliability Patterns
Pattern	Why
Transactional Outbox	Status=Completed + SprintCompleted event written atomically
Idempotent consumers	Task Service handles duplicate SprintCompleted safely
Eventual consistency	SprintStats may lag task changes — fine for a dashboard

8. Runtime Picture
            +------------+
 Angular --> | API Gateway|
            +-----+------+
        +---------+----------+
        v                    v
  +-----------+        +-----------+
  |  Sprint   |        |   Task    |
  |  Service  |        |  Service  |
  | sprint_db |        |  task_db  |
  +-----+-----+        +-----+-----+
        |   SprintCompleted  |
        | -----------------> |
        | <----------------- |
        |  TaskStatusChanged |
        |  (updates Stats)   |
        +------- bus --------+

9. How It Maps to Angular Learning
Sprint Service feature	Angular concept
Sprint list + filters	Routing, list components, RxJS switchMap on query params
Sprint board (sprint + tasks composed)	Component composition, parallel forkJoin, BFF pattern
Start/Complete buttons (role-gated)	Role guards, structural directives (*hasRole)
Live status counts via events → SignalR	Signals + effect() reacting to pushed SprintStats
Create/edit sprint form	Reactive Forms, date validation

10. Build Order
1.	Sprint contract records (SprintCreated, SprintCompleted...) in the shared library.
2.	Sprint Service core — CRUD + lifecycle endpoints + sprint_db.
3.	Authorization — role checks from JWT.
4.	Publish lifecycle events (with outbox).
5.	Consume task events → maintain SprintStats read model.
6.	/summary endpoint from the read model.
Key Principles Recap
•	Sprint Service owns sprints, not tasks. The link (SprintId) lives on the task.
•	SprintId (GUID) is the contract.
•	Reads compose at the edge (frontend/gateway calls both services).
•	Stats come from an event-driven read model (SprintStats), not live cross-service calls.
•	Workflow coordination via events (SprintCompleted → Task Service rolls over tasks).
•	Outbox + idempotency keep it reliable.
