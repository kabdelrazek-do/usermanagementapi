# User Management API Middleware Test Script
# This script tests all the middleware components: authentication, error handling, and logging

Write-Host "User Management API Middleware Test Script" -ForegroundColor Green
Write-Host "=============================================" -ForegroundColor Green

# Base URL - Update this if your API runs on a different port
$baseUrl = "http://localhost:5272"

# Test Results tracking
$testResults = @()

function Test-Endpoint {
    param(
        [string]$TestName,
        [string]$Method,
        [string]$Url,
        [string]$Body = $null,
        [string]$ExpectedStatus = "200",
        [string]$ContentType = "application/json",
        [hashtable]$Headers = @{}
    )
    
    Write-Host "`nTesting: $TestName" -ForegroundColor Yellow
    Write-Host "Method: $Method, URL: $Url" -ForegroundColor Gray
    
    try {
        $allHeaders = @{}
        if ($ContentType) {
            $allHeaders["Content-Type"] = $ContentType
        }
        
        # Add custom headers
        foreach ($key in $Headers.Keys) {
            $allHeaders[$key] = $Headers[$key]
        }
        
        if ($Body) {
            $response = Invoke-WebRequest -Uri $Url -Method $Method -Body $Body -Headers $allHeaders
        } else {
            $response = Invoke-WebRequest -Uri $Url -Method $Method -Headers $allHeaders
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

# 1. Test Health Check Endpoints (No Authentication Required)
Write-Host "`n=== HEALTH CHECK TESTS ===" -ForegroundColor Cyan

Test-Endpoint -TestName "Health Check Basic" -Method "GET" -Url "$baseUrl/health" -ExpectedStatus "200"
Test-Endpoint -TestName "Health Check Detailed" -Method "GET" -Url "$baseUrl/health/detailed" -ExpectedStatus "200"

# 2. Test Authentication Middleware
Write-Host "`n=== AUTHENTICATION TESTS ===" -ForegroundColor Cyan

# Test without authentication token
Test-Endpoint -TestName "Get Users Without Token" -Method "GET" -Url "$baseUrl/api/users" -ExpectedStatus "401"

# Test with invalid token
Test-Endpoint -TestName "Get Users With Invalid Token" -Method "GET" -Url "$baseUrl/api/users" -ExpectedStatus "401" -Headers @{"Authorization" = "Bearer invalid_token"}

# Test with valid token via Authorization header
Test-Endpoint -TestName "Get Users With Valid Bearer Token" -Method "GET" -Url "$baseUrl/api/users" -ExpectedStatus "200" -Headers @{"Authorization" = "Bearer techhive_admin_12345"}

# Test with valid token via X-API-Key header
Test-Endpoint -TestName "Get Users With Valid API Key" -Method "GET" -Url "$baseUrl/api/users" -ExpectedStatus "200" -Headers @{"X-API-Key" = "api_user_67890"}

# Test with valid token via query parameter
Test-Endpoint -TestName "Get Users With Valid Query Token" -Method "GET" -Url "$baseUrl/api/users?token=user_regular_11111" -ExpectedStatus "200"

# Test with different token types
Test-Endpoint -TestName "Get Users With Admin Token" -Method "GET" -Url "$baseUrl/api/users" -ExpectedStatus "200" -Headers @{"Authorization" = "Bearer techhive_admin_99999"}
Test-Endpoint -TestName "Get Users With API Token" -Method "GET" -Url "$baseUrl/api/users" -ExpectedStatus "200" -Headers @{"Authorization" = "Bearer api_service_22222"}
Test-Endpoint -TestName "Get Users With User Token" -Method "GET" -Url "$baseUrl/api/users" -ExpectedStatus "200" -Headers @{"Authorization" = "Bearer user_standard_33333"}

# 3. Test Error Handling Middleware
Write-Host "`n=== ERROR HANDLING TESTS ===" -ForegroundColor Cyan

# Test with valid token for error scenarios
$validHeaders = @{"Authorization" = "Bearer techhive_admin_12345"}

# Test 404 error (non-existent endpoint)
Test-Endpoint -TestName "Non-existent Endpoint" -Method "GET" -Url "$baseUrl/api/nonexistent" -ExpectedStatus "404" -Headers $validHeaders

# Test 404 error (non-existent user)
Test-Endpoint -TestName "Get Non-existent User" -Method "GET" -Url "$baseUrl/api/users/999" -ExpectedStatus "404" -Headers $validHeaders

# Test 400 error (invalid user ID)
Test-Endpoint -TestName "Get User With Invalid ID" -Method "GET" -Url "$baseUrl/api/users/0" -ExpectedStatus "400" -Headers $validHeaders

# Test 400 error (invalid request body)
$invalidUser = @{
    firstName = ""  # Empty first name should trigger validation error
    lastName = "Test"
    email = "invalid-email"
    phoneNumber = "555-0100"
    department = "Test"
    position = "Tester"
    hireDate = "2024-01-15T00:00:00"
} | ConvertTo-Json

Test-Endpoint -TestName "Create User With Invalid Data" -Method "POST" -Url "$baseUrl/api/users" -Body $invalidUser -ExpectedStatus "400" -Headers $validHeaders

# Test 409 error (duplicate email)
$duplicateUser = @{
    firstName = "John"
    lastName = "Doe"
    email = "john.doe@techhive.com"  # This email already exists
    phoneNumber = "555-0100"
    department = "Test"
    position = "Tester"
    hireDate = "2024-01-15T00:00:00"
} | ConvertTo-Json

Test-Endpoint -TestName "Create User With Duplicate Email" -Method "POST" -Url "$baseUrl/api/users" -Body $duplicateUser -ExpectedStatus "409" -Headers $validHeaders

# 4. Test Logging Middleware
Write-Host "`n=== LOGGING TESTS ===" -ForegroundColor Cyan

# Test various requests to ensure logging is working
Test-Endpoint -TestName "Logging Test - Get All Users" -Method "GET" -Url "$baseUrl/api/users" -ExpectedStatus "200" -Headers $validHeaders

Test-Endpoint -TestName "Logging Test - Get User by ID" -Method "GET" -Url "$baseUrl/api/users/1" -ExpectedStatus "200" -Headers $validHeaders

Test-Endpoint -TestName "Logging Test - Get Users by Department" -Method "GET" -Url "$baseUrl/api/users/department/IT" -ExpectedStatus "200" -Headers $validHeaders

# Test POST request to log request body
$newUser = @{
    firstName = "Alice"
    lastName = "Johnson"
    email = "alice.johnson@techhive.com"
    phoneNumber = "555-0104"
    department = "Marketing"
    position = "Marketing Specialist"
    hireDate = "2024-01-15T00:00:00"
} | ConvertTo-Json

Test-Endpoint -TestName "Logging Test - Create User" -Method "POST" -Url "$baseUrl/api/users" -Body $newUser -ExpectedStatus "201" -Headers $validHeaders

# 5. Test Middleware Pipeline Order
Write-Host "`n=== MIDDLEWARE PIPELINE TESTS ===" -ForegroundColor Cyan

# Test that error handling works correctly with authentication
Test-Endpoint -TestName "Pipeline Test - Unauthorized with Error" -Method "GET" -Url "$baseUrl/api/users" -ExpectedStatus "401"

# Test that logging works with authentication errors
Test-Endpoint -TestName "Pipeline Test - Logging with Auth Error" -Method "POST" -Url "$baseUrl/api/users" -Body $invalidUser -ExpectedStatus "401"

# 6. Test Swagger Access (Should be accessible without authentication)
Write-Host "`n=== SWAGGER ACCESS TESTS ===" -ForegroundColor Cyan

Test-Endpoint -TestName "Swagger UI Access" -Method "GET" -Url "$baseUrl/swagger" -ExpectedStatus "200"

# 7. Test Performance and Concurrent Requests
Write-Host "`n=== PERFORMANCE TESTS ===" -ForegroundColor Cyan

Write-Host "Testing concurrent requests with authentication..." -ForegroundColor Yellow
$jobs = @()
for ($i = 1; $i -le 5; $i++) {
    $jobs += Start-Job -ScriptBlock {
        param($baseUrl, $i, $token)
        try {
            $headers = @{"Authorization" = "Bearer $token"}
            $response = Invoke-WebRequest -Uri "$baseUrl/api/users" -Method "GET" -Headers $headers
            return "Request $i completed with status: $($response.StatusCode)"
        } catch {
            return "Request $i failed: $($_.Exception.Message)"
        }
    } -ArgumentList $baseUrl, $i, "techhive_admin_12345"
}

$jobs | Wait-Job | Receive-Job
$jobs | Remove-Job

# 8. Test Token Validation Edge Cases
Write-Host "`n=== TOKEN VALIDATION EDGE CASES ===" -ForegroundColor Cyan

# Test very short token
Test-Endpoint -TestName "Very Short Token" -Method "GET" -Url "$baseUrl/api/users" -ExpectedStatus "401" -Headers @{"Authorization" = "Bearer abc"}

# Test token without prefix
Test-Endpoint -TestName "Token Without Prefix" -Method "GET" -Url "$baseUrl/api/users" -ExpectedStatus "401" -Headers @{"Authorization" = "Bearer randomtoken123"}

# Test empty token
Test-Endpoint -TestName "Empty Token" -Method "GET" -Url "$baseUrl/api/users" -ExpectedStatus "401" -Headers @{"Authorization" = "Bearer "}

# Test malformed authorization header
Test-Endpoint -TestName "Malformed Auth Header" -Method "GET" -Url "$baseUrl/api/users" -ExpectedStatus "401" -Headers @{"Authorization" = "InvalidFormat token123"}

# 9. Summary
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

Write-Host "`nMiddleware testing completed!" -ForegroundColor Green
Write-Host "`nCheck the application logs to verify that:" -ForegroundColor Yellow
Write-Host "1. All requests and responses are being logged" -ForegroundColor White
Write-Host "2. Authentication attempts are being logged" -ForegroundColor White
Write-Host "3. Error handling is working correctly" -ForegroundColor White
Write-Host "4. Middleware pipeline order is correct" -ForegroundColor White
