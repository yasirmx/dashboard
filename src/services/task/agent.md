Task Service

You are a .net expert with knowledge about user agile task creation.

You will need to implement the task microservice using .net 10 and create the dockerfile and update the docker compose file

You need to use clean architecture

# Database
Use Sql Server

## Database name
Database name is Task

Core Domain Design — Task Management Dashboard (Microservices, .NET)

1. Context Diagram — The Big Picture
                          +---------------------------------------------+
                          |                TASK SERVICE                 |
                          |            (core domain, richest)           |
                          +---------------------------------------------+
   Angular -- JWT -->     |  RESPONSIBILITIES                           |
   (via API Gateway)      |  - Create / edit / delete tasks             |
                          |  - Sub-tasks (parent <-> child hierarchy)   |
                          |  - Status workflow: New->Active->Pending->  |
                          |    Closed                                   |
                          |  - Assign task to a user (AssigneeId)       |
                          |  - Link task to a sprint (SprintId)         |
                          |  - Source of truth: TASKS & SUB-TASKS       |
                          +---------------------------------------------+
                                            |  task_db
        +-----------------------------------+-----------------------------------+
        | consumes events                   | owns data           publishes events
        v                                   v                                   v
  SprintCompleted                     +----------+                  TaskCreated
  UserRegistered (cache name?)        |  Tasks   |                  TaskStatusChanged
  SprintCreated                       | TaskTags |                  TaskAssigned
                                      +----------+                  TaskAddedToSprint
                                                                    TaskRemovedFromSprint

2. Responsibilities — Owns vs References
Owns (source of truth)	References (just a GUID, no FK/join)
Task entity (title, description, status, priority)	AssigneeId → a user in User Service
Sub-tasks (the parent/child tree)	SprintId → a sprint in Sprint Service
Status transitions & rules	CreatedByUserId → from JWT
Task ↔ Sprint membership (SprintId lives here)	Roles → read from JWT, not stored

The Task Service owns the Task.SprintId and Task.AssigneeId links. It never stores user profiles or sprint details — only the GUIDs. Display data is fetched from the other services at the edge.
3. Database Structure (task_db)
Tasks
  Id               (GUID)   PK        -- the contract: TaskId
  ParentTaskId     (GUID)   NULL FK-> Tasks.Id   -- self-reference = sub-task
  Title            (string, required)
  Description      (text)
  Status           (int/enum)         -- New | Active | Pending | Closed
  Priority         (int/enum)         -- Low | Medium | High | Critical
  SprintId         (GUID)   NULL       -- which sprint (null = backlog)
  AssigneeId       (GUID)   NULL       -- which user (from JWT/User Service)
  CreatedByUserId  (GUID)              -- from JWT
  EstimatePoints   (int)    NULL
  OrderIndex       (int)               -- position within a status column (drag-drop)
  CreatedUtc, UpdatedUtc, ClosedUtc
 
  INDEX (SprintId)            -- "all tasks in sprint X"
  INDEX (AssigneeId)          -- "my tasks"
  INDEX (ParentTaskId)        -- "sub-tasks of task Y"
  INDEX (Status)
 
TaskTags                       -- optional: labels
  TaskId (FK), Tag
 
TaskActivity                   -- optional: per-task audit ("status changed by X")
  Id, TaskId, UserId, Type, OldValue, NewValue, CreatedUtc
 
OutboxMessages                 -- transactional outbox for reliable events
  Id, Type, Payload, OccurredUtc, ProcessedUtc

The Sub-task Model (Self-referencing)
Tasks table (self-reference via ParentTaskId)
 
   Task A (ParentTaskId = NULL)        <- top-level task
     |- Task A1 (ParentTaskId = A)     <- sub-task
     |- Task A2 (ParentTaskId = A)     <- sub-task
     |    \- Task A2a (ParentTaskId=A2)<- sub-sub-task (if you allow depth)
     \- Task A3 (ParentTaskId = A)

Design decisions for sub-tasks:
•	One table, self-referencing ParentTaskId — simplest, flexible depth.
•	Decide max depth: usually limit to 1 level (task → sub-task) to keep the UI/board sane. Enforce in code: reject creating a sub-task whose parent already has a parent.
•	A sub-task inherits the SprintId of its parent (typically) — or you forbid assigning sub-tasks directly to sprints.
•	Closing rules: parent can't be Closed until all sub-tasks are Closed.
4. Status Workflow (State Machine)
            +-----+  start   +--------+  block    +---------+
  create -> | New | -------> | Active | --------> | Pending |
            +-----+          +--------+ <-------- +---------+
               |                  |      resume         |
               | close            | close               | close
               v                  v                     v
                          +------------+
                          |   Closed   |
                          +------------+
                                ^
                                | reopen (Closed -> Active)
                                +----------

Rules to enforce (in the domain, not the controller):
•	Valid transitions only (e.g., can't go New → Closed directly if your policy requires work first).
•	Pending requires a reason (blocked-by note).
•	Setting Closed stamps ClosedUtc.
•	Parent ≠ Closed while any sub-task is open.
•	Status change emits TaskStatusChanged (drives Sprint stats + SignalR live board).
5. API Surface
# Tasks
POST   /tasks                      create a task (Status=New)
GET    /tasks?sprintId=&assigneeId=&status=&parentId=   filtered list
GET    /tasks/{id}                 get one (optionally ?include=subtasks)
PUT    /tasks/{id}                 edit title/description/priority/estimate
DELETE /tasks/{id}                 delete (and its sub-tasks)
 
# Status & workflow
PATCH  /tasks/{id}/status          { status: Active }   -- validated transition
PATCH  /tasks/{id}/assign          { assigneeId }       -- assign/reassign
PATCH  /tasks/{id}/sprint          { sprintId | null }  -- move to sprint/backlog
PATCH  /tasks/{id}/order           { status, orderIndex } -- drag-drop reposition
 
# Sub-tasks
POST   /tasks/{id}/subtasks        create a sub-task under {id}
GET    /tasks/{id}/subtasks        list children
 
# Bulk / board
GET    /sprints/{sprintId}/board   tasks grouped by status (board view)

Authorization (roles from JWT):
•	Create/edit/assign → any authenticated member (Dev/QA can manage their work).
•	Delete / reassign others' tasks → maybe Lead/PO only — your policy.
•	Read → any authenticated user.
6. Events — Published and Consumed
Publishes (others react)
public record TaskCreated(Guid TaskId, Guid? SprintId, Guid? AssigneeId, string Title);
public record TaskStatusChanged(Guid TaskId, Guid? SprintId, string OldStatus, string NewStatus);
public record TaskAssigned(Guid TaskId, Guid AssigneeId, string Title);
public record TaskAddedToSprint(Guid TaskId, Guid SprintId);
public record TaskRemovedFromSprint(Guid TaskId, Guid SprintId);

Event	Who cares
TaskStatusChanged, TaskAddedToSprint, TaskRemovedFromSprint	Sprint Service → updates SprintStats read model
TaskAssigned	Notification Service → 'a task was assigned to you'
TaskCreated	Analytics / Notification

Consumes (reacts to others)
SprintCompleted   -> roll over unfinished tasks (Status != Closed) to backlog/next sprint
SprintCancelled   -> detach tasks (SprintId = null)
UserDeactivated   -> optional: unassign that user's tasks

The sprint-completion rollover (consumer logic):
on SprintCompleted { SprintId }:
   tasks = find where SprintId == X and Status != Closed
   foreach t: t.SprintId = null   (back to backlog)
   publish TaskRemovedFromSprint for each

7. Cross-service Composition (Reads)
The Task Service stores only GUIDs. To show a task card with assignee name + sprint name, the frontend/gateway composes:
Angular Task Card needs: title, status, assignee NAME, sprint NAME
   |
   |-> GET /tasks/{id}            (Task Service)   -> title, status, assigneeId, sprintId
   |-> GET /users/{assigneeId}    (User Service)   -> name, avatar
   \-> GET /sprints/{sprintId}    (Sprint Service) -> sprint name

Optimization options:
•	BFF / API Gateway aggregation — one endpoint composes all three.
•	Local cache — Task Service caches {userId → name} by consuming UserRegistered/UserProfileUpdated (denormalization). Trade-off: more storage, less chatty.

8. Reliability Patterns
Pattern	Why
Transactional Outbox	Save task change + event atomically (e.g., status change + TaskStatusChanged)
Idempotent consumers	Safely handle duplicate SprintCompleted
Optimistic concurrency (RowVersion)	Two users editing the same task → detect conflict
Eventual consistency	Sprint stats / notifications lag slightly — fine

9. Full Runtime Picture
                    +------------+
        Angular --> | API Gateway|  (JWT validated here + per service)
                    +-----+------+
          +---------------+---------------+
          v               v               v
    +----------+    +----------+    +----------+
    |   Task   |    |  Sprint  |    |   User   |
    | Service  |    | Service  |    | Service  |
    | task_db  |    | sprint_db|    | user_db  |
    +----+-----+    +----+-----+    +----------+
         |   TaskStatusChanged
         | --------------->  Sprint (updates SprintStats)
         | <---------------
         |   SprintCompleted
         |
         |   TaskAssigned
         \--------------->  Notification Service -> SignalR -> Angular effect()
                                 (bus: RabbitMQ / MassTransit)

10. How It Maps to Angular Learning (the richest service)
Task Service feature	Angular concept
Sub-tasks (parent/child tree)	Recursive components, FormArray for editing children
Status board with drag-drop	CDK DragDrop, optimistic UI, custom directive
Status/priority badges	Custom pipes, conditional styling
Assign task (user picker)	Reactive Forms, typeahead with RxJS debounceTime + switchMap
Live board updates via TaskStatusChanged → SignalR	Signals + effect(), real-time
Filtered list (?status=&assignee=)	Route query params + switchMap
'My tasks' view	Guard + filtered service call
Create task with sub-tasks form	Reactive Forms + nested FormArray

11. Build Order
1.	Task contracts (TaskCreated, TaskStatusChanged, TaskAssigned...) in shared library.
2.	Core entity + CRUD + task_db with self-referencing ParentTaskId.
3.	Status state machine (validated transitions) + PATCH /status.
4.	Sub-tasks endpoints + parent-close rule.
5.	Assignment & sprint linkage (/assign, /sprint).
6.	Publish events (outbox) → wire to Sprint stats.
7.	Consume SprintCompleted → rollover logic.
8.	Board endpoint + drag-drop OrderIndex.
Key Principles Recap
•	Task Service is the core, richest domain — owns tasks, sub-tasks, status, and the SprintId/AssigneeId links.
•	References users & sprints by GUID only — no cross-DB joins; compose at the edge.
•	Sub-tasks via self-referencing ParentTaskId — cap the depth, enforce parent-close rules.
•	Status is a guarded state machine, not a free field — each change emits TaskStatusChanged.
•	Publishes events that drive Sprint stats, notifications, and the real-time Angular board.
•	Consumes SprintCompleted to roll over unfinished tasks.
•	Outbox + idempotency + optimistic concurrency keep it correct under load.

# Postman Collection

Create the postman collection with the endpoints and sample data