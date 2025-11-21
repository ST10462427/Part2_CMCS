# Part2_CMCS
1️⃣ Program.cs — Application Startup

Configures everything the application needs:

Adds MVC controllers with views

Adds Entity Framework database context

Registers SignalR for live updates

Configures cookie authentication

Sets up routing and middleware (static files, HTTPS, sessions, etc.)

This file is responsible for launching and wiring the entire application.

2️⃣ Data/ApplicationDbContext.cs — Database Layer

This is your Entity Framework Core database context.
It manages the following tables:

Users

Claims

ClaimDocuments

It inherits from DbContext and maps each model to the database, allowing CRUD operations across the system.

3️⃣ Models

Models define the database schema and domain objects.

✔ User.cs

Represents a system user.

Username

PasswordHash

Email

Role (“User” or “Admin”)

✔ Claim.cs

Represents an insurance or system claim.

Claim type

Description

Status

User reference

✔ ClaimDocument.cs

Stores uploaded files connected to a claim.

4️⃣ Controllers — Application Logic
⭐ AccountController

Handles all authentication:

User registration

Password hashing

Login (cookie authentication)

Logout

Redirects based on role (User/Admin)

Uses PasswordHasher<User> instead of Identity framework.

⭐ AdminController

Admin-only area:

View all claims

Update claim statuses (Pending → Approved → Rejected)

Manage users

Communicate with claim owners

⭐ ClaimController

Handles:

Submitting new claims

Uploading documents

Viewing claim history

Real-time notifications via SignalR

⭐ HomeController

Displays:

Landing page

Dashboard

Navigation

5️⃣ SignalR — Real-Time Updates

StatusHub.cs sends instant notifications when:

Claim status updates

Admin changes something important

Users receive status updates without refreshing the page.
