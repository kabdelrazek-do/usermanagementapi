# User Management API Test Script
# This script tests all the CRUD endpoints of the User Management API

Write-Host "User Management API Test Script" -ForegroundColor Green
Write-Host "=================================" -ForegroundColor Green

# Base URL - Update this if your API runs on a different port
$baseUrl = "http://localhost:5000"

# Test 1: Get all users
Write-Host "`n1. Testing GET /api/users (Get all users)" -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "$baseUrl/api/users" -Method GET
    Write-Host "Status: $($response.StatusCode)" -ForegroundColor Green
    Write-Host "Response: $($response.Content)" -ForegroundColor White
} catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 2: Get user by ID
Write-Host "`n2. Testing GET /api/users/1 (Get user by ID)" -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "$baseUrl/api/users/1" -Method GET
    Write-Host "Status: $($response.StatusCode)" -ForegroundColor Green
    Write-Host "Response: $($response.Content)" -ForegroundColor White
} catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 3: Create new user
Write-Host "`n3. Testing POST /api/users (Create new user)" -ForegroundColor Yellow
$newUser = @{
    firstName = "Alice"
    lastName = "Johnson"
    email = "alice.johnson@techhive.com"
    phoneNumber = "555-0104"
    department = "Marketing"
    position = "Marketing Specialist"
    hireDate = "2024-01-15T00:00:00"
} | ConvertTo-Json

try {
    $response = Invoke-WebRequest -Uri "$baseUrl/api/users" -Method POST -Body $newUser -ContentType "application/json"
    Write-Host "Status: $($response.StatusCode)" -ForegroundColor Green
    Write-Host "Response: $($response.Content)" -ForegroundColor White
} catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 4: Update user
Write-Host "`n4. Testing PUT /api/users/1 (Update user)" -ForegroundColor Yellow
$updateUser = @{
    department = "Engineering"
    position = "Senior Developer"
} | ConvertTo-Json

try {
    $response = Invoke-WebRequest -Uri "$baseUrl/api/users/1" -Method PUT -Body $updateUser -ContentType "application/json"
    Write-Host "Status: $($response.StatusCode)" -ForegroundColor Green
    Write-Host "Response: $($response.Content)" -ForegroundColor White
} catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 5: Get users by department
Write-Host "`n5. Testing GET /api/users/department/IT (Get users by department)" -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "$baseUrl/api/users/department/IT" -Method GET
    Write-Host "Status: $($response.StatusCode)" -ForegroundColor Green
    Write-Host "Response: $($response.Content)" -ForegroundColor White
} catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 6: Get user by email
Write-Host "`n6. Testing GET /api/users/email/john.doe@techhive.com (Get user by email)" -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "$baseUrl/api/users/email/john.doe@techhive.com" -Method GET
    Write-Host "Status: $($response.StatusCode)" -ForegroundColor Green
    Write-Host "Response: $($response.Content)" -ForegroundColor White
} catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 7: Delete user (soft delete)
Write-Host "`n7. Testing DELETE /api/users/2 (Delete user)" -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "$baseUrl/api/users/2" -Method DELETE
    Write-Host "Status: $($response.StatusCode)" -ForegroundColor Green
    Write-Host "Response: $($response.Content)" -ForegroundColor White
} catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`nTest completed!" -ForegroundColor Green
