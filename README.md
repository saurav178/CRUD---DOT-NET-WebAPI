# 📋 TaskApi — ASP.NET Core 8 Web API & MongoDB

> **Welcome!** This document is written so that **anyone**—whether a beginner, student, or non-technical person—can understand exactly how this application works, what has been built, and how concepts like **Dependency Injection (DI)** actually work under the hood.

---

## 📑 Table of Contents
1. [What is this Project? (The Big Picture)](#1-what-is-this-project-the-big-picture)
2. [What Has Been Built So Far? (Step-by-Step)](#2-what-has-been-built-so-far-step-by-step)
3. [🧠 Dependency Injection (DI) — Complete Deep Dive](#3--dependency-injection-di--complete-deep-dive)
   - [Real-World Analogy (Restaurant Kitchen)](#real-world-analogy-the-restaurant-kitchen)
   - [The Problem: Life Without DI](#the-problem-life-without-di)
   - [The Solution: Life With DI](#the-solution-life-with-di)
   - [WHY do we use DI?](#why-do-we-use-di)
   - [WHERE is DI in our Code? (File-by-File)](#where-is-di-in-our-code-file-by-file)
   - [WHEN does DI happen? (The Request Timeline)](#when-does-di-happen-the-request-timeline)
   - [The 3 DI Lifetimes (Singleton vs Scoped vs Transient)](#the-3-di-lifetimes-singleton-vs-scoped-vs-transient)
4. [Project Structure & Every File Explained](#4-project-structure--every-file-explained)
5. [How MongoDB Connects (Local Compass)](#5-how-mongodb-connects-local-compass)
6. [How to Run and Test This Project](#6-how-to-run-and-test-this-project)
7. [Phase 1 Roadmap & Next Step](#7-phase-1-roadmap--next-step)

---

## 1. What is this Project? (The Big Picture)

Imagine you want a digital notebook to manage your daily tasks. You need to be able to:
1. **Create** a new task (e.g., *"Buy groceries"*).
2. **Read** (view) all your tasks or find a specific one.
3. **Update** a task (e.g., mark it as completed).
4. **Delete** a task when you don't need it anymore.

In software, these 4 actions are called **CRUD** (**C**reate, **R**ead, **U**pdate, **D**elete).

This project is a **Web API (Application Programming Interface)** built with **ASP.NET Core (.NET 8)**. It acts as the "brain" and "waiter": it receives requests from users (or mobile apps / websites) and talks to a database called **MongoDB** to store and retrieve tasks.

```
┌─────────────────────────────────┐
│ User / Browser / Swagger Client │
└────────────────┬────────────────┘
                 │ (HTTP Request: "Give me all tasks")
                 ▼
┌─────────────────────────────────┐
│     TasksController (Waiter)    │  Receives the request & validates it
└────────────────┬────────────────┘
                 │ (Calls Service via Interface)
                 ▼
┌─────────────────────────────────┐
│      TaskService (Chef)         │  Contains the business rules
└────────────────┬────────────────┘
                 │ (Executes database queries)
                 ▼
┌─────────────────────────────────┐
│    MongoDB Driver (Pantry)      │  Sends instructions to MongoDB
└────────────────┬────────────────┘
                 │
                 ▼
┌─────────────────────────────────┐
│  MongoDB Database ("TaskDb")    │  Where task data is permanently saved
└─────────────────────────────────┘
```

---

## 2. What Has Been Built So Far? (Step-by-Step)

| Step | What was done | Purpose |
| :---: | :--- | :--- |
| **Step 1** | Created ASP.NET Core Web API with Controllers | Set up the .NET 8 application foundation with Swagger documentation enabled. |
| **Step 2** | Installed `MongoDB.Driver` and configured connection | Linked our project to your local MongoDB Compass running on `mongodb://localhost:27017`. |
| **Step 3** | Built `TaskItem` model, `ITaskService`, and `TaskService` | Created the task data model and implemented all database logic (find, insert, update, delete). |
| **Step 3** | Registered services in Dependency Injection (DI) | Connected the database client and service into .NET's central DI engine in `Program.cs`. |

---

## 3. 🧠 Dependency Injection (DI) — Complete Deep Dive

### Real-World Analogy: The Restaurant Kitchen

Imagine you are a **Chef** preparing pasta:
* **WITHOUT DI (Bad Way):**
  Every time a customer orders pasta, you have to leave the kitchen, go outside, chop down wood to build a stove, dig up a field to harvest wheat, grind it into flour, and make the noodles. 
  * *Result:* You are exhausted, tightly coupled to farming, and if wheat is unavailable, your kitchen completely breaks.
* **WITH DI (Smart Way):**
  You simply state: *"To make pasta, I need noodles and a stove."* 
  The **Restaurant Manager** supplies pre-made noodles and a ready-to-use stove directly onto your kitchen counter. You only focus on cooking!

In software:
* The **Chef** = `TaskService` (or `TasksController`)
* The **Supplies** = `IMongoClient` or `ITaskService`
* The **Restaurant Manager** = ASP.NET Core's **Dependency Injection Container**

---

### The Problem: Life Without DI

Without DI, classes create their own dependencies using the `new` keyword:

```csharp
// ❌ BAD: TIGHTLY COUPLED CODE (No DI)
public class TaskService
{
    private readonly MongoClient _client;

    public TaskService()
    {
        // HARDCODED: If connection string changes, this class breaks.
        // IMPOSSIBLE TO TEST: You can't test this class without a real MongoDB server running.
        _client = new MongoClient("mongodb://localhost:27017"); 
    }
}
```

**Why this is bad:**
1. **Tight Coupling:** `TaskService` is permanently glued to a specific MongoDB server.
2. **Resource Waste:** Every time a request comes in, a brand new `MongoClient` is created, wasting server memory and opening hundreds of redundant database connections.
3. **Untestable:** You cannot write unit tests with fake data.

---

### The Solution: Life With DI

With DI, classes **never** use `new`. Instead, they declare what they need in their **constructor** (called *Constructor Injection*):

```csharp
// ✅ GOOD: CLEAN & MODULAR (With DI)
public class TaskService : ITaskService
{
    private readonly IMongoCollection<TaskItem> _tasksCollection;

    // "Hey .NET DI Container, please deliver IMongoClient and MongoDbSettings to me!"
    public TaskService(IMongoClient mongoClient, IOptions<MongoDbSettings> settings)
    {
        var database = mongoClient.GetDatabase(settings.Value.DatabaseName);
        _tasksCollection = database.GetCollection<TaskItem>(settings.Value.CollectionName);
    }
}
```

---

### WHY do we use DI?

1. **Loose Coupling:** The controller doesn't care *how* tasks are saved (in MongoDB, SQL, or in-memory). It only talks to `ITaskService`.
2. **Easy Testing:** You can test the controller by passing a "Mock" service without needing a real database.
3. **Centralized Configuration:** You change the connection string in one place (`appsettings.json`), and the entire app automatically gets the update.
4. **Efficient Memory Management:** .NET automatically handles creating, sharing, and destroying instances when done.

---

### WHERE is DI in our Code? (File-by-File)

#### 1. The "Registration" (In `Program.cs`)
This is where we tell .NET what services exist and how long they should live:

```csharp
// File: Program.cs

// 1. Tell DI how to read MongoDB settings from appsettings.json
builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDbSettings"));

// 2. Tell DI to keep ONE shared MongoClient alive for the whole app (Singleton)
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var settings = builder.Configuration.GetSection("MongoDbSettings").Get<MongoDbSettings>();
    return new MongoClient(settings?.ConnectionString ?? "mongodb://localhost:27017");
});

// 3. Tell DI: Whenever someone asks for "ITaskService", give them an instance of "TaskService" (Scoped)
builder.Services.AddScoped<ITaskService, TaskService>();
```

#### 2. The "Consumption" (In `TaskService.cs`)
```csharp
// File: Services/TaskService.cs

public class TaskService : ITaskService
{
    // The DI container automatically fills these two arguments!
    public TaskService(IMongoClient mongoClient, IOptions<MongoDbSettings> settings)
    {
        // Ready to use immediately!
    }
}
```

#### 3. The "Next Step" (In `TasksController.cs`)
```csharp
// File: Controllers/TasksController.cs (Coming in Step 4)

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;

    // The DI container automatically supplies ITaskService here!
    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }
}
```

---

### WHEN does DI happen? (The Request Timeline)

Here is the exact second-by-second journey when someone uses the API:

```text
[Time: 0.00s] Web API starts up
              └─► Program.cs runs.
              └─► DI Container is built with instructions for:
                  • MongoDbSettings
                  • IMongoClient (created ONCE as Singleton)
                  • ITaskService (mapped to TaskService as Scoped)

[Time: 1.15s] A user clicks "GET /api/tasks" in their browser
              └─► 1. HTTP request reaches ASP.NET Core.
              └─► 2. ASP.NET sees TasksController needs "ITaskService".
              └─► 3. DI Container creates a new "TaskService".
              └─► 4. TaskService needs "IMongoClient", so DI container supplies the shared Singleton MongoClient.
              └─► 5. TasksController executes GetAllAsync() and sends back JSON data to user.
              └─► 6. Request ends: TaskService is automatically disposed and cleaned from RAM.
```

---

### The 3 DI Lifetimes (Singleton vs Scoped vs Transient)

ASP.NET Core gives us 3 ways to manage the lifetime of an object:

| Lifetime | How It Works | Real-World Example | In Our Project | Why We Used It |
| :--- | :--- | :--- | :--- | :--- |
| **`Singleton`** | Created **once** when the app starts. The exact same instance is shared by every user forever. | The building's front door key or Wi-Fi router. | `IMongoClient` | Official MongoDB recommendation. Reusing one client manages connection pooling efficiently without exhausting server ports. |
| **`Scoped`** | Created **once per HTTP request**. If one user request touches 5 services, all 5 share the same copy. When request finishes, it's deleted. | A shopping cart during one checkout session. | `ITaskService` / `TaskService` | Best practice for database services. Isolates each user request so they don't interfere with each other. |
| **`Transient`** | Created **fresh every single time** it is requested. | A paper cup from a water dispenser (use once and discard). | Lightweight utilities (calculators, formatters). | Not needed yet for our database services. |

---

## 4. Project Structure & Every File Explained

```text
D:\CRUD\
│
├── Controllers\                     # The "Receptionists" (receives HTTP requests)
│   └── WeatherForecastController.cs # (Default template, to be replaced by TasksController)
│
├── Models\                          # The "Data Blueprints"
│   ├── MongoDbSettings.cs           # Holds ConnectionString, DatabaseName, CollectionName
│   └── TaskItem.cs                  # The Task document model (Id, Title, Description, etc.)
│
├── Services\                        # The "Workers / Business Logic"
│   ├── ITaskService.cs              # The Contract (interface) listing all available actions
│   └── TaskService.cs               # The Worker that actually runs MongoDB queries
│
├── Properties\
│   └── launchSettings.json          # Ports and URLs for local development (e.g. https://localhost:7075)
│
├── appsettings.json                 # Configuration values (MongoDB connection string)
├── Program.cs                       # App starting point: registers DI and builds the server pipeline
├── TaskApi.csproj                   # Project configuration file listing .NET 8 and NuGet packages
└── README.md                        # This documentation file
```

---

## 5. How MongoDB Connects (Local Compass)

In [appsettings.json](file:///D:/CRUD/appsettings.json):
```json
{
  "MongoDbSettings": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "TaskDb",
    "CollectionName": "Tasks"
  }
}
```

* **`mongodb://localhost:27017`**: Points to your local MongoDB service (which MongoDB Compass connects to).
* **`TaskDb`**: MongoDB automatically creates this database as soon as we insert the first task!
* **`Tasks`**: The collection (table) where task documents live.

---

## 6. How to Run and Test This Project

### Step 1: Open your terminal inside the project
```powershell
cd D:\CRUD
```

### Step 2: Build the project (checks for errors)
```powershell
dotnet build
```

### Step 3: Run the API
```powershell
dotnet run
```
You will see output showing the API is live:
```text
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:7075
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5087
```

### Step 4: Open Swagger UI in Browser
Open your browser and navigate to:
👉 **`https://localhost:7075/swagger`**

---

## 7. Phase 1 Roadmap & Next Step

- [x] **Step 1: Create ASP.NET Core Web API**
- [x] **Step 2: Connect MongoDB using official C# driver**
- [x] **Step 3: Create Task Model & Service Layer (with DI)**
- [ ] **Step 4: Build Task CRUD Endpoints (`TasksController.cs`)**
- [ ] **Step 5: Review & Understand Controllers, Services, and Dependency Injection**

---

### What happens in Step 4?
We will create **`TasksController.cs`**, inject `ITaskService`, and build:
* `GET /api/tasks` — List all tasks
* `GET /api/tasks/{id}` — Find a task by its ID
* `POST /api/tasks` — Add a new task
* `PUT /api/tasks/{id}` — Edit an existing task
* `DELETE /api/tasks/{id}` — Remove a task
