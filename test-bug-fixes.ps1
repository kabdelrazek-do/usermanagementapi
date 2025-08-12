# User Management API Bug Fixes Test Script
# This script tests all the bug fixes and edge cases for the User Management API

Write-Host "User Management API Bug Fixes Test Script" -ForegroundColor Green
Write-Host "=============================================" -ForegroundColor Green

# Base URL - Update this if your API runs on a different port
$baseUrl = "http://localhost:5000"

# Test Results tracking
$testResults = @()

function Test-Endpoint {
    param(
        [string]$TestName,
        [string]$Method,
        [string]$Url,
        [string]$Body = $null,
        [string]$ExpectedStatus = "200",
        [string]$ContentType = "application/json"
    )
    
    Write-Host "`nTesting: $TestName" -ForegroundColor Yellow
    Write-Host "Method: $Method, URL: $Url" -ForegroundColor Gray
    
    try {
        $headers = @{}
        if ($ContentType) {
            $headers["Content-Type"] = $ContentType
        }
        
        if ($Body) {
            $response = Invoke-WebRequest -Uri $Url -Method $Method -Body $Body -Headers $headers
        } else {
            $response = Invoke-WebRequest -Uri $Url -Method $Method -Headers $headers
        }
        
        $statusMatch = $response.StatusCode -eq [int]$ExpectedStatus
        $result = if ($statusMatch) { "PASS" } else { "FAIL" }
        $color = if ($statusMatch) { "Green" } else { "Red" }
        
        Write-Host "Status: $($response.StatusCode) (Expected: $ExpectedStatus)" -ForegroundColor $color
        Write-Host "Response: $($response.Content)" -ForegroundColor White
        
        $testResults += [PSCustomObject]@{
            TestName = $TestName
            Status = $result
            ExpectedStatus = $ExpectedStatus
            ActualStatus = $response.StatusCode
            Response = $response.Content
        }
        
    } catch {
        $statusCode = $_.Exception.Response.StatusCode.value__
        $statusMatch = $statusCode -eq [int]$ExpectedStatus
        $result = if ($statusMatch) { "PASS" } else { "FAIL" }
        $color = if ($statusMatch) { "Green" } else { "Red" }
        
        Write-Host "Status: $statusCode (Expected: $ExpectedStatus)" -ForegroundColor $color
        Write-Host "Error: $($_.Exception.Message)" -ForegroundColor White
        
        $testResults += [PSCustomObject]@{
            TestName = $TestName
            Status = $result
            ExpectedStatus = $ExpectedStatus
            ActualStatus = $statusCode
            Response = $_.Exception.Message
        }
    }
}

# 1. Test Valid Operations
Write-Host "`n=== VALID OPERATIONS TESTS ===" -ForegroundColor Cyan

Test-Endpoint -TestName "Get All Users" -Method "GET" -Url "$baseUrl/api/users" -ExpectedStatus "200"
Test-Endpoint -TestName "Get User by Valid ID" -Method "GET" -Url "$baseUrl/api/users/1" -ExpectedStatus "200"
Test-Endpoint -TestName "Get Users by Department" -Method "GET" -Url "$baseUrl/api/users/department/IT" -ExpectedStatus "200"
Test-Endpoint -TestName "Get User by Valid Email" -Method "GET" -Url "$baseUrl/api/users/email/john.doe@techhive.com" -ExpectedStatus "200"

# 2. Test Input Validation
Write-Host "`n=== INPUT VALIDATION TESTS ===" -ForegroundColor Cyan

# Invalid ID tests
Test-Endpoint -TestName "Get User by Invalid ID (0)" -Method "GET" -Url "$baseUrl/api/users/0" -ExpectedStatus "400"
Test-Endpoint -TestName "Get User by Invalid ID (-1)" -Method "GET" -Url "$baseUrl/api/users/-1" -ExpectedStatus "400"
Test-Endpoint -TestName "Get User by Invalid ID (999)" -Method "GET" -Url "$baseUrl/api/users/999" -ExpectedStatus "404"

# Invalid email tests
Test-Endpoint -TestName "Get User by Empty Email" -Method "GET" -Url "$baseUrl/api/users/email/" -ExpectedStatus "400"
Test-Endpoint -TestName "Get User by Invalid Email" -Method "GET" -Url "$baseUrl/api/users/email/nonexistent@test.com" -ExpectedStatus "404"

# Invalid department tests
Test-Endpoint -TestName "Get Users by Empty Department" -Method "GET" -Url "$baseUrl/api/users/department/" -ExpectedStatus "400"

# 3. Test Data Validation
Write-Host "`n=== DATA VALIDATION TESTS ===" -ForegroundColor Cyan

# Test invalid user creation data
$invalidUser1 = @{
    firstName = ""  # Empty first name
    lastName = "Test"
    email = "test@test.com"
    phoneNumber = "555-0100"
    department = "Test"
    position = "Tester"
    hireDate = "2024-01-15T00:00:00"
} | ConvertTo-Json

Test-Endpoint -TestName "Create User with Empty First Name" -Method "POST" -Url "$baseUrl/api/users" -Body $invalidUser1 -ExpectedStatus "400"

# Test invalid email format
$invalidUser2 = @{
    firstName = "Test"
    lastName = "User"
    email = "invalid-email"  # Invalid email format
    phoneNumber = "555-0100"
    department = "Test"
    position = "Tester"
    hireDate = "2024-01-15T00:00:00"
} | ConvertTo-Json

Test-Endpoint -TestName "Create User with Invalid Email" -Method "POST" -Url "$baseUrl/api/users" -Body $invalidUser2 -ExpectedStatus "400"

# Test future hire date
$invalidUser3 = @{
    firstName = "Test"
    lastName = "User"
    email = "test@test.com"
    phoneNumber = "555-0100"
    department = "Test"
    position = "Tester"
    hireDate = "2025-01-15T00:00:00"  # Future date
} | ConvertTo-Json

Test-Endpoint -TestName "Create User with Future Hire Date" -Method "POST" -Url "$baseUrl/api/users" -Body $invalidUser3 -ExpectedStatus "400"

# 4. Test Duplicate Email Handling
Write-Host "`n=== DUPLICATE EMAIL TESTS ===" -ForegroundColor Cyan

# Create a valid user first
$validUser = @{
    firstName = "Alice"
    lastName = "Johnson"
    email = "alice.johnson@techhive.com"
    phoneNumber = "555-0104"
    department = "Marketing"
    position = "Marketing Specialist"
    hireDate = "2024-01-15T00:00:00"
} | ConvertTo-Json

Test-Endpoint -TestName "Create Valid User" -Method "POST" -Url "$baseUrl/api/users" -Body $validUser -ExpectedStatus "201"

# Try to create another user with the same email
$duplicateUser = @{
    firstName = "Bob"
    lastName = "Smith"
    email = "alice.johnson@techhive.com"  # Same email
    phoneNumber = "555-0105"
    department = "Sales"
    position = "Sales Rep"
    hireDate = "2024-01-16T00:00:00"
} | ConvertTo-Json

Test-Endpoint -TestName "Create User with Duplicate Email" -Method "POST" -Url "$baseUrl/api/users" -Body $duplicateUser -ExpectedStatus "409"

# 5. Test Update Operations
Write-Host "`n=== UPDATE OPERATION TESTS ===" -ForegroundColor Cyan

# Test valid update
$updateData = @{
    department = "Engineering"
    position = "Senior Developer"
} | ConvertTo-Json

Test-Endpoint -TestName "Update User with Valid Data" -Method "PUT" -Url "$baseUrl/api/users/1" -Body $updateData -ExpectedStatus "200"

# Test update with invalid ID
Test-Endpoint -TestName "Update User with Invalid ID" -Method "PUT" -Url "$baseUrl/api/users/999" -Body $updateData -ExpectedStatus "404"

# Test update with duplicate email
$updateWithDuplicateEmail = @{
    email = "alice.johnson@techhive.com"  # Email that already exists
} | ConvertTo-Json

Test-Endpoint -TestName "Update User with Duplicate Email" -Method "PUT" -Url "$baseUrl/api/users/2" -Body $updateWithDuplicateEmail -ExpectedStatus "409"

# 6. Test Delete Operations
Write-Host "`n=== DELETE OPERATION TESTS ===" -ForegroundColor Cyan

# Test delete with invalid ID
Test-Endpoint -TestName "Delete User with Invalid ID" -Method "DELETE" -Url "$baseUrl/api/users/999" -ExpectedStatus "404"

# Test delete with valid ID
Test-Endpoint -TestName "Delete User with Valid ID" -Method "DELETE" -Url "$baseUrl/api/users/2" -ExpectedStatus "200"

# 7. Test Edge Cases
Write-Host "`n=== EDGE CASES TESTS ===" -ForegroundColor Cyan

# Test with special characters in names
$specialCharUser = @{
    firstName = "Jean-Pierre"
    lastName = "O'Connor"
    email = "jean-pierre.oconnor@techhive.com"
    phoneNumber = "555-0106"
    department = "Research & Development"
    position = "Senior Researcher"
    hireDate = "2024-01-15T00:00:00"
} | ConvertTo-Json

Test-Endpoint -TestName "Create User with Special Characters" -Method "POST" -Url "$baseUrl/api/users" -Body $specialCharUser -ExpectedStatus "201"

# Test with very long names (should be rejected)
$longNameUser = @{
    firstName = "A" * 51  # 51 characters (exceeds 50 limit)
    lastName = "Test"
    email = "longname@test.com"
    phoneNumber = "555-0107"
    department = "Test"
    position = "Tester"
    hireDate = "2024-01-15T00:00:00"
} | ConvertTo-Json

Test-Endpoint -TestName "Create User with Too Long Name" -Method "POST" -Url "$baseUrl/api/users" -Body $longNameUser -ExpectedStatus "400"

# 8. Test Null/Empty Input Handling
Write-Host "`n=== NULL/EMPTY INPUT TESTS ===" -ForegroundColor Cyan

# Test with null body
Test-Endpoint -TestName "Create User with Null Body" -Method "POST" -Url "$baseUrl/api/users" -Body "null" -ExpectedStatus "400"

# Test with empty JSON
Test-Endpoint -TestName "Create User with Empty JSON" -Method "POST" -Url "$baseUrl/api/users" -Body "{}" -ExpectedStatus "400"

# 9. Test Performance and Thread Safety
Write-Host "`n=== PERFORMANCE TESTS ===" -ForegroundColor Cyan

# Test concurrent requests (simulate multiple requests)
Write-Host "Testing concurrent requests..." -ForegroundColor Yellow
$jobs = @()
for ($i = 1; $i -le 5; $i++) {
    $jobs += Start-Job -ScriptBlock {
        param($baseUrl, $i)
        try {
            $response = Invoke-WebRequest -Uri "$baseUrl/api/users" -Method "GET"
            return "Request $i completed with status: $($response.StatusCode)"
        } catch {
            return "Request $i failed: $($_.Exception.Message)"
        }
    } -ArgumentList $baseUrl, $i
}

$jobs | Wait-Job | Receive-Job
$jobs | Remove-Job

# 10. Summary
Write-Host "`n=== TEST SUMMARY ===" -ForegroundColor Cyan

$passedTests = ($testResults | Where-Object { $_.Status -eq "PASS" }).Count
$failedTests = ($testResults | Where-Object { $_.Status -eq "FAIL" }).Count
$totalTests = $testResults.Count

Write-Host "Total Tests: $totalTests" -ForegroundColor White
Write-Host "Passed: $passedTests" -ForegroundColor Green
Write-Host "Failed: $failedTests" -ForegroundColor Red
Write-Host "Success Rate: $([math]::Round(($passedTests / $totalTests) * 100, 2))%" -ForegroundColor Yellow

if ($failedTests -gt 0) {
    Write-Host "`nFailed Tests:" -ForegroundColor Red
    $testResults | Where-Object { $_.Status -eq "FAIL" } | ForEach-Object {
        Write-Host "- $($_.TestName): Expected $($_.ExpectedStatus), Got $($_.ActualStatus)" -ForegroundColor Red
    }
}

Write-Host "`nBug fixes testing completed!" -ForegroundColor Green
