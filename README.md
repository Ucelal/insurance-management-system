# Insurance Management System

A comprehensive insurance management platform built with modern technologies.

## Technology Stack

- **Frontend**: React with TypeScript
- **Backend**: .NET Core Web API
- **Database**: SQL Server (MSSQL)
- **Authentication**: JWT (JSON Web Tokens)
- **Password Hashing**: BCrypt

## Features

- ✅ User Management and Authorization (Completed)
- 🔄 Customer Management (In Progress)
- 🔄 Policy Management (Planned)
- 🔄 Claims Management (Planned)
- 🔄 Payment Processing (Planned)
- 🔄 Document Management (Planned)
- 🔄 Reporting System (Planned)

## Getting Started

### Prerequisites

- .NET Core 8.0 SDK
- SQL Server
- Node.js (for frontend)

### Backend Setup

1. Navigate to the backend directory:
   ```bash
   cd backend/InsuranceAPI
   ```

2. Update the connection string in `appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=DESKTOP-TD03B1K\\SQLEXPRESS;Database=InsuranceSystem;Trusted_Connection=true;TrustServerCertificate=true;MultipleActiveResultSets=true"
   }
   ```

3. Run the database creation script:
   ```sql
   -- Execute the script in backend/InsuranceAPI/Database/CreateDatabase.sql
   ```

4. Start the backend:
   ```bash
   dotnet run
   ```

5. The API will be available at:
   - HTTP: http://localhost:5000
   - Swagger UI: http://localhost:5000/swagger

### Frontend Setup

1. Navigate to the frontend directory:
   ```bash
   cd frontend
   ```

2. Install dependencies:
   ```bash
   npm install
   ```

3. Start the development server:
   ```bash
   npm start
   ```

4. The application will be available at http://localhost:3000

## API Endpoints

### Authentication
- `POST /api/auth/login` - User login
- `POST /api/auth/register` - User registration
- `POST /api/auth/validate` - Validate JWT token
- `GET /api/auth/me` - Get current user info
- `GET /api/auth/test` - Test API connectivity
- `GET /api/auth/debug-password` - Debug password verification

## Default Admin User

- **Email**: admin@insurance.com
- **Password**: Admin123!

## Database Schema

### Users Table
- `Id` (Primary Key)
- `Name` (NVARCHAR(255))
- `Role` (NVARCHAR(50)) - Admin, Agent, Customer
- `Email` (NVARCHAR(255), Unique)
- `Password_Hash` (NVARCHAR(MAX))
- `Created_At` (DATETIME)

### Customers Table
- `Id` (Primary Key)
- `User_Id` (Foreign Key to Users.Id)
- `Type` (NVARCHAR(50)) - Bireysel, Kurumsal
- `Id_No` (NVARCHAR(50), Unique)
- `Address` (NVARCHAR(1000))
- `Phone` (NVARCHAR(20))

## Project Structure

```
Insurance/
├── backend/
│   └── InsuranceAPI/
│       ├── Controllers/
│       │   └── AuthController.cs
│       ├── Models/
│       │   ├── User.cs
│       │   ├── Customer.cs
│       │   └── Enums.cs
│       ├── DTOs/
│       │   ├── AuthDTOs.cs
│       │   ├── UserDTOs.cs
│       │   └── CustomerDTOs.cs
│       ├── Services/
│       │   ├── IAuthService.cs
│       │   ├── AuthService.cs
│       │   └── JwtService.cs
│       ├── Data/
│       │   └── InsuranceDbContext.cs
│       ├── Database/
│       │   └── CreateDatabase.sql
│       └── Migrations/
├── frontend/
│   ├── src/
│   │   ├── components/
│   │   │   └── Login.tsx
│   │   ├── contexts/
│   │   │   └── AuthContext.tsx
│   │   ├── services/
│   │   │   └── api.ts
│   │   ├── types/
│   │   │   └── index.ts
│   │   ├── App.tsx
│   │   └── index.tsx
│   └── public/
├── .vscode/
│   ├── launch.json
│   ├── tasks.json
│   └── extensions.json
└── README.md
```

## Development Status

### ✅ Completed
- User Management and Authorization Module
- JWT Authentication
- Database Schema (Users, Customers)
- API Endpoints (Login, Register, Token Validation)
- Backend Structure
- Frontend Basic Structure

### 🔄 In Progress
- Frontend Implementation
- Customer Management Module

### 📋 Planned
- Policy Management
- Claims Management
- Payment Processing
- Document Management
- Reporting System

## Development

This project follows the GitHub Flow workflow:
- Main branch contains stable code
- Feature development happens in feature branches
- Pull requests are used for code review
- Continuous integration ensures code quality

## License

This project is part of a software internship program. 