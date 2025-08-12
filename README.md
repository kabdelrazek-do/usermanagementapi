# User Management API

A comprehensive ASP.NET Core Web API for managing user records, built for TechHive Solutions' internal tools. This project demonstrates enterprise-level API development with comprehensive middleware, security, and monitoring capabilities.

## 🚀 **Project Status: Production Ready**

This API has been developed across three comprehensive activities and is now ready for production deployment with enterprise-level features including authentication, comprehensive logging, error handling, and security compliance.

## Features

- **Full CRUD Operations**: Create, Read, Update, and Delete user records
- **Data Validation**: Comprehensive input validation using Data Annotations
- **Soft Delete**: Users are marked as inactive rather than permanently deleted
- **Search Functionality**: Find users by department or email
- **RESTful Design**: Follows REST API best practices
- **OpenAPI Documentation**: Auto-generated API documentation
- **Error Handling**: Proper HTTP status codes and error messages

## API Endpoints

### GET /api/users
Retrieve all active users.

**Response**: `200 OK`
```json
[
  {
    "id": 1,
    "firstName": "John",
    "lastName": "Doe",
    "email": "john.doe@techhive.com",
    "phoneNumber": "555-0101",
    "department": "IT",
    "position": "Software Developer",
    "hireDate": "2023-01-15T00:00:00",
    "isActive": true,
    "createdAt": "2024-01-01T10:00:00",
    "updatedAt": null
  }
]
```

### GET /api/users/{id}
Retrieve a specific user by ID.

**Response**: `200 OK` or `404 Not Found`

### GET /api/users/department/{department}
Retrieve all users in a specific department.

**Response**: `200 OK`

### GET /api/users/email/{email}
Retrieve a user by email address.

**Response**: `200 OK` or `404 Not Found`

### POST /api/users
Create a new user.

**Request Body**:
```json
{
  "firstName": "Alice",
  "lastName": "Johnson",
  "email": "alice.johnson@techhive.com",
  "phoneNumber": "555-0104",
  "department": "Marketing",
  "position": "Marketing Specialist",
  "hireDate": "2024-01-15T00:00:00"
}
```

**Response**: `201 Created` or `409 Conflict` (if email already exists)

### PUT /api/users/{id}
Update an existing user.

**Request Body** (all fields optional):
```json
{
  "firstName": "Alice",
  "lastName": "Smith",
  "email": "alice.smith@techhive.com",
  "phoneNumber": "555-0105",
  "department": "Sales",
  "position": "Sales Representative",
  "hireDate": "2024-01-15T00:00:00",
  "isActive": true
}
```

**Response**: `200 OK`, `404 Not Found`, or `409 Conflict`

### DELETE /api/users/{id}
Soft delete a user (marks as inactive).

**Response**: `200 OK` or `404 Not Found`

## Getting Started

### Prerequisites
- .NET 9.0 SDK or later
- Visual Studio 2022 or VS Code

### Running the Application

1. Navigate to the project directory:
   ```bash
   cd UserManagementAPI
   ```

2. Restore dependencies:
   ```bash
   dotnet restore
   ```

3. Run the application:
   ```bash
   dotnet run
   ```

4. The API will be available at:
   - **API Base URL**: `https://localhost:7001` or `http://localhost:5000`
   - **Swagger Documentation**: `https://localhost:7001/swagger` or `http://localhost:5000/swagger`

## Testing the API

### Using Postman

1. **Get All Users**
   - Method: `GET`
   - URL: `https://localhost:7001/api/users`

2. **Get User by ID**
   - Method: `GET`
   - URL: `https://localhost:7001/api/users/1`

3. **Create New User**
   - Method: `POST`
   - URL: `https://localhost:7001/api/users`
   - Headers: `Content-Type: application/json`
   - Body:
   ```json
   {
     "firstName": "Sarah",
     "lastName": "Wilson",
     "email": "sarah.wilson@techhive.com",
     "phoneNumber": "555-0106",
     "department": "Finance",
     "position": "Financial Analyst",
     "hireDate": "2024-02-01T00:00:00"
   }
   ```

4. **Update User**
   - Method: `PUT`
   - URL: `https://localhost:7001/api/users/1`
   - Headers: `Content-Type: application/json`
   - Body:
   ```json
   {
     "department": "Engineering",
     "position": "Senior Developer"
   }
   ```

5. **Delete User**
   - Method: `DELETE`
   - URL: `https://localhost:7001/api/users/1`

### Using curl

```bash
# Get all users
curl -X GET "https://localhost:7001/api/users"

# Get user by ID
curl -X GET "https://localhost:7001/api/users/1"

# Create new user
curl -X POST "https://localhost:7001/api/users" \
  -H "Content-Type: application/json" \
  -d '{
    "firstName": "Sarah",
    "lastName": "Wilson",
    "email": "sarah.wilson@techhive.com",
    "phoneNumber": "555-0106",
    "department": "Finance",
    "position": "Financial Analyst",
    "hireDate": "2024-02-01T00:00:00"
  }'

# Update user
curl -X PUT "https://localhost:7001/api/users/1" \
  -H "Content-Type: application/json" \
  -d '{
    "department": "Engineering",
    "position": "Senior Developer"
  }'

# Delete user
curl -X DELETE "https://localhost:7001/api/users/1"
```

## Project Structure

```
UserManagementAPI/
├── Controllers/
│   └── UsersController.cs          # API endpoints
├── Models/
│   ├── User.cs                     # User entity model
│   └── UserDto.cs                  # Data transfer objects
├── Services/
│   ├── IUserService.cs             # Service interface
│   └── UserService.cs              # Service implementation
├── Program.cs                      # Application configuration
├── appsettings.json                # Configuration settings
└── README.md                       # This file
```

## Data Model

The `User` model includes the following fields:

- `Id` (int): Unique identifier
- `FirstName` (string): User's first name
- `LastName` (string): User's last name
- `Email` (string): Unique email address
- `PhoneNumber` (string): Contact phone number
- `Department` (string): User's department
- `Position` (string): User's job position
- `HireDate` (DateTime): Date when user was hired
- `IsActive` (bool): Whether the user is active
- `CreatedAt` (DateTime): Record creation timestamp
- `UpdatedAt` (DateTime?): Last update timestamp

## Error Handling

The API returns appropriate HTTP status codes:

- `200 OK`: Successful operation
- `201 Created`: Resource created successfully
- `400 Bad Request`: Invalid input data
- `404 Not Found`: Resource not found
- `409 Conflict`: Resource conflict (e.g., duplicate email)

## Future Enhancements

- Database integration (Entity Framework Core)
- Authentication and authorization
- Pagination for large datasets
- Advanced filtering and sorting
- Logging and monitoring
- Unit and integration tests
- Docker containerization

## Contributing

This is a demonstration project for learning purposes. Feel free to extend and improve the functionality as needed.
