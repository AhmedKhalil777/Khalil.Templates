param([string]$OutputPath = "samples")

Write-Host "Khalil Templates - Package and Sample Creation" -ForegroundColor Cyan
Write-Host "===============================================" -ForegroundColor Cyan

# Clean up
Write-Host "Cleaning up..." -ForegroundColor Yellow
if (Test-Path $OutputPath) {
    Remove-Item $OutputPath -Recurse -Force
    Write-Host "  Removed existing samples" -ForegroundColor Gray
}
if (Test-Path "*.nupkg") {
    Remove-Item "*.nupkg" -Force
    Write-Host "  Removed existing packages" -ForegroundColor Gray
}

# Clean all build artifacts
Write-Host "  Cleaning build artifacts..." -ForegroundColor Gray
Get-ChildItem -Path "src" -Name "bin" -Recurse -Directory | ForEach-Object { Remove-Item -Path "src\$_" -Recurse -Force -ErrorAction SilentlyContinue }
Get-ChildItem -Path "src" -Name "obj" -Recurse -Directory | ForEach-Object { Remove-Item -Path "src\$_" -Recurse -Force -ErrorAction SilentlyContinue }

New-Item -ItemType Directory -Path $OutputPath -Force | Out-Null

# Build templates
Write-Host "Building templates..." -ForegroundColor Yellow
Push-Location "src\APISqlKhalil"
dotnet build --configuration Release | Out-Null
if ($LASTEXITCODE -eq 0) {
    Write-Host "  APISqlKhalil built successfully" -ForegroundColor Green
} else {
    Write-Host "  APISqlKhalil build failed" -ForegroundColor Red
    Pop-Location
    exit 1
}
Pop-Location

Push-Location "src\CleanKhalil"
dotnet build --configuration Release | Out-Null
if ($LASTEXITCODE -eq 0) {
    Write-Host "  CleanKhalil built successfully" -ForegroundColor Green
} else {
    Write-Host "  CleanKhalil build failed" -ForegroundColor Red
    Pop-Location
    exit 1
}
Pop-Location

# Clean build artifacts again before packaging
Write-Host "  Re-cleaning build artifacts..." -ForegroundColor Gray
Get-ChildItem -Path "src" -Name "bin" -Recurse -Directory | ForEach-Object { Remove-Item -Path "src\$_" -Recurse -Force -ErrorAction SilentlyContinue }
Get-ChildItem -Path "src" -Name "obj" -Recurse -Directory | ForEach-Object { Remove-Item -Path "src\$_" -Recurse -Force -ErrorAction SilentlyContinue }

# Create package
Write-Host "Creating NuGet package..." -ForegroundColor Yellow
dotnet pack Khalil.Templates.csproj --configuration Release --output .
if ($LASTEXITCODE -eq 0) {
    $pkg = Get-ChildItem "*.nupkg" | Select-Object -First 1
    Write-Host "  Package created: $($pkg.Name)" -ForegroundColor Green
} else {
    Write-Host "  Package creation failed" -ForegroundColor Red
    exit 1
}

# Install templates
Write-Host "Installing templates..." -ForegroundColor Yellow
Write-Host "  Uninstalling existing templates..." -ForegroundColor Gray
dotnet new uninstall Khalil.Templates 2>$null

Write-Host "  Installing from package..." -ForegroundColor Gray
dotnet new install $pkg.FullName
if ($LASTEXITCODE -eq 0) {
    Write-Host "  Templates installed successfully" -ForegroundColor Green
} else {
    Write-Host "  Template installation failed" -ForegroundColor Red
    exit 1
}

# Verify templates
Write-Host "Verifying templates..." -ForegroundColor Yellow
$templateList = dotnet new list | Out-String
if ($templateList -match "apisqlkhalil" -and $templateList -match "cleankhalil") {
    Write-Host "  Templates verified successfully" -ForegroundColor Green
} else {
    Write-Host "  Template verification failed - templates may not be properly installed" -ForegroundColor Yellow
    Write-Host "  Available templates:" -ForegroundColor Gray
    dotnet new list | Where-Object { $_ -match "khalil" }
}

# Create samples
Write-Host "Creating sample projects..." -ForegroundColor Yellow
Push-Location $OutputPath

# Sample 1: APISqlKhalil with LocalDB
Write-Host "  Creating TodoAPI-LocalDB..." -ForegroundColor Gray
New-Item -ItemType Directory -Path "TodoAPI-LocalDB" -Force | Out-Null
Push-Location "TodoAPI-LocalDB"
dotnet new apisqlkhalil --name "TodoAPI.LocalDB" --UseLocalDB true --force
if ($LASTEXITCODE -eq 0) {
    Write-Host "    Success" -ForegroundColor Green
} else {
    Write-Host "    Failed" -ForegroundColor Red
}
Pop-Location

# Sample 2: APISqlKhalil with SQL Server
Write-Host "  Creating TodoAPI-SqlServer..." -ForegroundColor Gray
New-Item -ItemType Directory -Path "TodoAPI-SqlServer" -Force | Out-Null
Push-Location "TodoAPI-SqlServer"
dotnet new apisqlkhalil --name "TodoAPI.SqlServer" --UseLocalDB false --force
if ($LASTEXITCODE -eq 0) {
    Write-Host "    Success" -ForegroundColor Green
} else {
    Write-Host "    Failed" -ForegroundColor Red
}
Pop-Location

# Sample 3: CleanKhalil with SQL Server + ADFS
Write-Host "  Creating EnterpriseApp-ADFS..." -ForegroundColor Gray
New-Item -ItemType Directory -Path "EnterpriseApp-ADFS" -Force | Out-Null
Push-Location "EnterpriseApp-ADFS"
dotnet new cleankhalil --name "EnterpriseApp" --UseSqlServer true --UseADFS true --CompanyName "Acme Corporation" --force
if ($LASTEXITCODE -eq 0) {
    Write-Host "    Success" -ForegroundColor Green
} else {
    Write-Host "    Failed" -ForegroundColor Red
}
Pop-Location

# Sample 4: CleanKhalil with LocalDB + No ADFS
Write-Host "  Creating StartupApp-Simple..." -ForegroundColor Gray
New-Item -ItemType Directory -Path "StartupApp-Simple" -Force | Out-Null
Push-Location "StartupApp-Simple"
dotnet new cleankhalil --name "StartupApp" --UseSqlServer false --UseADFS false --CompanyName "StartupCorp" --force
if ($LASTEXITCODE -eq 0) {
    Write-Host "    Success" -ForegroundColor Green
} else {
    Write-Host "    Failed" -ForegroundColor Red
}
Pop-Location

# Sample 5: CleanKhalil with SQL Server + No ADFS
Write-Host "  Creating WebApp-JWT..." -ForegroundColor Gray
New-Item -ItemType Directory -Path "WebApp-JWT" -Force | Out-Null
Push-Location "WebApp-JWT"
dotnet new cleankhalil --name "WebApp" --UseSqlServer true --UseADFS false --CompanyName "TechCorp" --force
if ($LASTEXITCODE -eq 0) {
    Write-Host "    Success" -ForegroundColor Green
} else {
    Write-Host "    Failed" -ForegroundColor Red
}
Pop-Location

Pop-Location

# Create documentation
Write-Host "Creating documentation..." -ForegroundColor Yellow
$currentDate = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
$readmeContent = @"
# Khalil Templates - Sample Projects

Generated: $currentDate

This directory contains sample projects demonstrating different configurations of the Khalil Templates.

## Sample Projects:

### 1. TodoAPI-LocalDB
- **Template**: APISqlKhalil
- **Configuration**: UseLocalDB = true
- **Description**: Simple Todo API using LocalDB
- **Run**: cd TodoAPI-LocalDB && dotnet run

### 2. TodoAPI-SqlServer
- **Template**: APISqlKhalil
- **Configuration**: UseLocalDB = false
- **Description**: Todo API configured for SQL Server
- **Run**: cd TodoAPI-SqlServer && dotnet run

### 3. EnterpriseApp-ADFS
- **Template**: CleanKhalil
- **Configuration**: UseSqlServer = true, UseADFS = true, CompanyName = "Acme Corporation"
- **Description**: Enterprise application with ADFS authentication
- **Run API**: cd EnterpriseApp-ADFS && dotnet run --project src/EnterpriseApp.API
- **Run Client**: cd EnterpriseApp-ADFS && dotnet run --project src/EnterpriseApp.Client

### 4. StartupApp-Simple
- **Template**: CleanKhalil
- **Configuration**: UseSqlServer = false, UseADFS = false, CompanyName = "StartupCorp"
- **Description**: Simplified clean architecture for startups
- **Run API**: cd StartupApp-Simple && dotnet run --project src/StartupApp.API
- **Run Client**: cd StartupApp-Simple && dotnet run --project src/StartupApp.Client

### 5. WebApp-JWT
- **Template**: CleanKhalil
- **Configuration**: UseSqlServer = true, UseADFS = false, CompanyName = "TechCorp"
- **Description**: Clean architecture with JWT authentication only
- **Run API**: cd WebApp-JWT && dotnet run --project src/WebApp.API
- **Run Client**: cd WebApp-JWT && dotnet run --project src/WebApp.Client

## Template Usage:

```bash
# Install templates
dotnet new install Khalil.Templates

# Create APISqlKhalil project
dotnet new apisqlkhalil --name MyTodoAPI --UseLocalDB true

# Create CleanKhalil project
dotnet new cleankhalil --name MyApp --UseSqlServer true --UseADFS false --CompanyName "My Company"

# List all templates
dotnet new list
```

## Template Parameters:

### APISqlKhalil:
- UseLocalDB (bool): Whether to use LocalDB (true) or SQL Server (false)

### CleanKhalil:
- UseSqlServer (bool): Whether to use SQL Server (true) or LocalDB (false)
- UseADFS (bool): Whether to include ADFS authentication
- CompanyName (string): Company name for branding
"@

$readmeContent | Out-File -FilePath "$OutputPath/README.md" -Encoding UTF8
Write-Host "  README.md created" -ForegroundColor Green

# Test one sample
Write-Host "Testing sample build..." -ForegroundColor Yellow
if (Test-Path "$OutputPath/TodoAPI-LocalDB") {
    Push-Location "$OutputPath/TodoAPI-LocalDB"
    dotnet build --verbosity quiet | Out-Null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  Sample builds successfully" -ForegroundColor Green
    } else {
        Write-Host "  Sample has build issues" -ForegroundColor Yellow
    }
    Pop-Location
}

# Summary
Write-Host "`nCompleted successfully!" -ForegroundColor Green
Write-Host "=======================================" -ForegroundColor Cyan
Write-Host "Package: $($pkg.Name)" -ForegroundColor White
Write-Host "Samples: $PWD\$OutputPath" -ForegroundColor White
Write-Host "Documentation: $PWD\$OutputPath\README.md" -ForegroundColor White
Write-Host "`nNext steps:" -ForegroundColor Yellow
Write-Host "  1. Explore the sample projects" -ForegroundColor White
Write-Host "  2. Run any sample with 'dotnet run'" -ForegroundColor White
Write-Host "  3. Create new projects with 'dotnet new apisqlkhalil' or 'dotnet new cleankhalil'" -ForegroundColor White 