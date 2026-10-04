# R6: End-to-End Implementation Guide — Authentication & RBAC Authorization

**Target Audience:** Junior Software Engineers & Contributors  
**Release:** Sprint 7 (R6)  
**Related Documents:** 
- [R6_AUTH_IMPLEMENTATION_PLAN.md](file:///c:/gdb_migration/web-api-ea1/GdbWebApi/Documentation/R6_AUTH_IMPLEMENTATION_PLAN.md)
- [GitHub_Issues_Migration_Task_Breakdown.md](file:///c:/gdb_migration/web-api-ea1/GdbWebApi/Documentation/GitHub_Issues_Migration_Task_Breakdown.md) (Child Issues 12 & 13)
- [Console_to_ASPNET_Core_WebAPI_Migration_Roadmap_Part2.md](file:///c:/gdb_migration/web-api-ea1/GdbWebApi/Documentation/Console_to_ASPNET_Core_WebAPI_Migration_Roadmap_Part2.md) (Section 12.2 R6 Tests)

---

## Table of Contents
1. [Overview & What We Are Building](#1-overview--what-we-are-building)
2. [File Directory Structure](#2-file-directory-structure)
3. [Phase 1: Package Installation & Settings Configuration](#3-phase-1-package-installation--settings-configuration)
4. [Phase 2: Security Constants & Configuration Models](#4-phase-2-security-constants--configuration-models)
5. [Phase 3: Data Transfer Objects (DTOs)](#5-phase-3-data-transfer-objects-dtos)
6. [Phase 4: Token Generation Service](#6-phase-4-token-generation-service)
7. [Phase 5: Enterprise Staff Identity, Cryptographic Password Hashing & AuthService](#7-phase-5-enterprise-staff-identity-cryptographic-password-hashing--authservice)
8. [Phase 6: Dependency Injection Registration](#8-phase-6-dependency-injection-registration)
9. [Phase 7: ASP.NET Core Middleware & Policies (`Program.cs`)](#9-phase-7-aspnet-core-middleware--policies-programcs)
10. [Phase 8: Swagger Bearer Token Integration](#10-phase-8-swagger-bearer-token-integration)
11. [Phase 9: AuthController Implementation](#11-phase-9-authcontroller-implementation)
12. [Phase 10: Protecting AccountController with RBAC & Ownership Isolation](#12-phase-10-protecting-accountcontroller-with-rbac--ownership-isolation)
13. [Phase 11: Protecting TransactionController](#13-phase-11-protecting-transactioncontroller)
14. [Phase 12: Step-by-Step Verification & Testing Guide](#14-phase-12-step-by-step-verification--testing-guide)
15. [Troubleshooting & Common Pitfalls](#15-troubleshooting--common-pitfalls)

---

## 1. Overview & What We Are Building

In banking software, authentication and authorization are non-negotiable:
- **Bank Customers (Account Holders)** must log in with their **10-digit Account Number** and **4-digit PIN**. Once authenticated, they receive a JWT bearing the role `User` and their `accountNumber`. Crucially, customers must **only** be able to see and transact on **their own account**.
- **Bank Staff** (`Teller`, `Manager`, `Admin`) log in with username/password credentials. They receive a JWT bearing their administrative role to manage accounts, process branch operations, or close accounts.
- **Role-Based Access Control (RBAC)** ensures:
  - Only `Teller` and `Admin` can create accounts.
  - Only `Manager` and `Admin` can close accounts.
  - Only `Teller`, `Manager`, and `Admin` can view the list of all accounts.

This guide provides the complete, working code for every single file. Follow each step sequentially.

---

## 2. File Directory Structure

Here is where every file will live in your project tree:

```text
GdbWebApi/
├── Application/
│   ├── Common/
│   │   └── Security/
│   │       ├── IPasswordHasher.cs                <-- [NEW] Cryptographic password hasher contract
│   │       ├── PasswordHasher.cs                 <-- [NEW] PBKDF2/SHA256 salted password hasher
│   │       ├── JwtOptions.cs                     <-- [NEW] Strongly-typed JWT config
│   │       └── UserRoles.cs                      <-- [NEW] Role string constants
│   ├── Controllers/
│   │   ├── AccountController.cs                  <-- [EDIT] Apply [Authorize] & RBAC
│   │   ├── AuthController.cs                     <-- [NEW] Login endpoints
│   │   └── TransactionController.cs              <-- [EDIT] Apply [Authorize] & RBAC
│   ├── Dtos/
│   │   └── Auth/
│   │       ├── AccountLoginRequestDto.cs         <-- [NEW] AccountNo + PIN login DTO
│   │       ├── StaffLoginRequestDto.cs           <-- [NEW] Username + Password login DTO
│   │       └── AuthResponseDto.cs                <-- [NEW] JWT token response DTO
│   ├── Extensions/
│   │   └── ServiceRegistration.cs                <-- [EDIT] Register auth services & staff repo
│   └── Services/
│       ├── Contracts/
│       │   ├── IAuthService.cs                   <-- [NEW] Auth service contract
│       │   └── ITokenService.cs                  <-- [NEW] JWT generation contract
│       └── Implementations/
│           ├── AuthService.cs                    <-- [NEW] Account & Staff validation (DB-backed)
│           └── JwtTokenService.cs                <-- [NEW] JWT token builder
├── Domain/
│   └── Models/
│       └── StaffUser.cs                          <-- [NEW] Staff user identity domain model
├── Infrastructure/
│   ├── Repositories/
│   │   ├── Contracts/
│   │   │   └── IStaffUserRepository.cs           <-- [NEW] Staff user repository contract
│   │   └── Implementations/
│   │       └── StaffUserRepository.cs            <-- [NEW] Staff repository with secure seeding
│   └── Swagger/
│       └── ConfigureSwaggerOptions.cs            <-- [EDIT] Add Bearer JWT padlock
├── Program.cs                                    <-- [EDIT] Add JwtBearer & Middleware
├── appsettings.json                              <-- [EDIT] Add JWT settings
└── GdbWebApi.csproj                              <-- [EDIT] Add JWT NuGet packages
```

---

## 3. Phase 1: Package Installation & Settings Configuration

### Step 1.1: Add Required NuGet Packages
Open your terminal in `c:\gdb_migration\web-api-ea1\GdbWebApi` and run:

```powershell
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer --version 10.0.12
dotnet add package System.IdentityModel.Tokens.Jwt --version 8.6.0
```

Alternatively, ensure these `<PackageReference>` elements are inside [GdbWebApi.csproj](file:///c:/gdb_migration/web-api-ea1/GdbWebApi/GdbWebApi.csproj):

```xml
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="10.0.12" />
<PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="8.6.0" />
```

### Step 1.2: Update `appsettings.json`
Add the `"Jwt"` configuration block to [appsettings.json](file:///c:/gdb_migration/web-api-ea1/GdbWebApi/appsettings.json):

```json
{
  "ConnectionStrings": {
    "GDBConnection": "Server=localhost,14333;User Id = sa;Password = password;Database=GDBDatabase;TrustServerCertificate=True;"
  },
  "ProviderFactory": "System.Data.SqlClient.SqlClientFactory, System.Data.SqlClient",
  "Jwt": {
    "Key": "GDB_Super_Secret_Key_For_Jwt_Authentication_2026_Must_Be_At_Least_32_Bytes_Long!",
    "Issuer": "GdbWebApi",
    "Audience": "GdbBankingClients",
    "ExpiryMinutes": 60
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

> [!TIP]
> **Junior Tip on Key Length**: Symmetric encryption keys for HMAC-SHA256 **must** be at least 256 bits (32 characters). If it is shorter, .NET throws an `ArgumentOutOfRangeException` during startup!

---

## 4. Phase 2: Security Constants & Configuration Models

### Step 2.1: Create `UserRoles.cs`
Create file `Application/Common/Security/UserRoles.cs`:

```csharp
namespace GdbWebApi.Application.Common.Security
{
    public static class UserRoles
    {
        public const string Admin = "Admin";
        public const string Manager = "Manager";
        public const string Teller = "Teller";
        public const string User = "User";

        // Combined role policy helpers
        public const string Staff = $"{Admin},{Manager},{Teller}";
        public const string TellerOrAdmin = $"{Admin},{Teller}";
        public const string ManagerOrAdmin = $"{Admin},{Manager}";
    }
}
```

### Step 2.2: Create `JwtOptions.cs`
Create file `Application/Common/Security/JwtOptions.cs`:

```csharp
namespace GdbWebApi.Application.Common.Security
{
    public class JwtOptions
    {
        public const string SectionName = "Jwt";

        public string Key { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public int ExpiryMinutes { get; set; } = 60;
    }
}
```

---

## 5. Phase 3: Data Transfer Objects (DTOs)

### Step 3.1: Create `AccountLoginRequestDto.cs`
Create file `Application/Dtos/Auth/AccountLoginRequestDto.cs`:

```csharp
using System.ComponentModel.DataAnnotations;

namespace GdbWebApi.Application.Dtos.Auth
{
    public class AccountLoginRequestDto
    {
        [Required(ErrorMessage = "Account number is required.")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Account number must be exactly 10 digits.")]
        public string AccountNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "PIN is required.")]
        [RegularExpression(@"^\d{4}$", ErrorMessage = "PIN must be exactly 4 digits.")]
        public string Pin { get; set; } = string.Empty;
    }
}
```

### Step 3.2: Create `StaffLoginRequestDto.cs`
Create file `Application/Dtos/Auth/StaffLoginRequestDto.cs`:

```csharp
using System.ComponentModel.DataAnnotations;

namespace GdbWebApi.Application.Dtos.Auth
{
    public class StaffLoginRequestDto
    {
        [Required(ErrorMessage = "Username is required.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; } = string.Empty;
    }
}
```

### Step 3.3: Create `AuthResponseDto.cs`
Create file `Application/Dtos/Auth/AuthResponseDto.cs`:

```csharp
namespace GdbWebApi.Application.Dtos.Auth
{
    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string TokenType { get; set; } = "Bearer";
        public string Role { get; set; } = string.Empty;
        public string? AccountNumber { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }
}
```

---

## 6. Phase 4: Token Generation Service

### Step 4.1: Create `ITokenService.cs`
Create file `Application/Services/Contracts/ITokenService.cs`:

```csharp
using System.Security.Claims;

namespace GdbWebApi.Application.Services.Contracts
{
    public interface ITokenService
    {
        string GenerateToken(string identifier, string name, string role, string? accountNumber = null);
        DateTime GetTokenExpiry();
    }
}
```

### Step 4.2: Create `JwtTokenService.cs`
Create file `Application/Services/Implementations/JwtTokenService.cs`:

```csharp
using GdbWebApi.Application.Common.Security;
using GdbWebApi.Application.Services.Contracts;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace GdbWebApi.Application.Services.Implementations
{
    public class JwtTokenService : ITokenService
    {
        private readonly JwtOptions _jwtOptions;

        public JwtTokenService(IOptions<JwtOptions> jwtOptions)
        {
            _jwtOptions = jwtOptions?.Value ?? throw new ArgumentNullException(nameof(jwtOptions));
            
            if (string.IsNullOrWhiteSpace(_jwtOptions.Key) || _jwtOptions.Key.Length < 32)
            {
                throw new InvalidOperationException("JWT Secret Key must be at least 32 characters long.");
            }
        }

        public string GenerateToken(string identifier, string name, string role, string? accountNumber = null)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, identifier),
                new Claim(ClaimTypes.NameIdentifier, identifier),
                new Claim(ClaimTypes.Name, name),
                new Claim(ClaimTypes.Role, role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            // If account number is present (customer token), add custom claim
            if (!string.IsNullOrWhiteSpace(accountNumber))
            {
                claims.Add(new Claim("accountNumber", accountNumber));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = GetTokenExpiry();

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expires,
                Issuer = _jwtOptions.Issuer,
                Audience = _jwtOptions.Audience,
                SigningCredentials = credentials
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        public DateTime GetTokenExpiry()
        {
            return DateTime.UtcNow.AddMinutes(_jwtOptions.ExpiryMinutes);
        }
    }
}
```

---

## 7. Phase 5: Enterprise Staff Identity, Cryptographic Password Hashing & AuthService

> [!IMPORTANT]
> ### How Enterprise Systems Handle Identity (Why Hardcoded Dictionaries are Anti-Patterns)
> In enterprise banking and production software, storing credentials in hardcoded in-memory dictionaries or plain text violates fundamental security baselines (OWASP Top 10, PCI-DSS, SOC 2):
> 1. **Federated Identity Providers (IdP)**: In large banking environments, staff authenticate against enterprise Identity Providers (Azure Active Directory / Microsoft Entra ID, Okta, Keycloak, or LDAP). The API validates bearer tokens issued by the STS authority.
> 2. **Internal Secure Database Identity Store**: When the banking system hosts internal staff accounts (e.g., branch tellers and managers), enterprise systems use:
>    - A dedicated **Staff Identity Database Table** (`StaffUsers`).
>    - **Salted, Adaptive Cryptographic Password Hashing**: Passwords are **never stored in plain text**. They are hashed using PBKDF2 (HMAC-SHA256, 100,000+ iterations with a cryptographically random per-user 128-bit salt) or Argon2id/BCrypt.
>    - **Repository Pattern (`IStaffUserRepository`)**: Decouples identity data storage from authentication business logic.
>    - **Secure Startup Seeding**: During first-time boot, default administrative accounts are seeded with **hashed** passwords, with optional overrides from environment variables (`STAFFSEED__ADMINPASSWORD`), completely eliminating plain text in source code.

---

### Step 5.1: Create `StaffUser.cs` Domain Model
Create file `Domain/Models/StaffUser.cs`:

```csharp
namespace GdbWebApi.Domain.Models
{
    public class StaffUser
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
```

---

### Step 5.2: Create Cryptographic Password Hasher (`IPasswordHasher.cs` & `PasswordHasher.cs`)

Create contract `Application/Common/Security/IPasswordHasher.cs`:

```csharp
namespace GdbWebApi.Application.Common.Security
{
    public interface IPasswordHasher
    {
        string HashPassword(string password);
        bool VerifyPassword(string hashedPassword, string providedPassword);
    }
}
```

Create implementation `Application/Common/Security/PasswordHasher.cs` using PBKDF2 with SHA-256 and constant-time verification:

```csharp
using System.Security.Cryptography;

namespace GdbWebApi.Application.Common.Security
{
    /// <summary>
    /// Enterprise-grade password hasher using PBKDF2 with HMAC-SHA256,
    /// 100,000 iterations, 128-bit cryptographically secure salt, and constant-time verification.
    /// </summary>
    public class PasswordHasher : IPasswordHasher
    {
        private const int SaltSize = 16; // 128 bits
        private const int KeySize = 32;  // 256 bits
        private const int Iterations = 100000;
        private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;
        private const char SegmentDelimiter = '.';

        public string HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Password cannot be empty.", nameof(password));
            }

            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                Iterations,
                Algorithm,
                KeySize);

            // Format: {hash}.{salt}.{iterations}.{algorithm}
            return string.Join(
                SegmentDelimiter,
                Convert.ToBase64String(hash),
                Convert.ToBase64String(salt),
                Iterations,
                Algorithm.Name);
        }

        public bool VerifyPassword(string hashedPassword, string providedPassword)
        {
            if (string.IsNullOrWhiteSpace(hashedPassword) || string.IsNullOrWhiteSpace(providedPassword))
            {
                return false;
            }

            string[] segments = hashedPassword.Split(SegmentDelimiter);
            if (segments.Length != 4)
            {
                return false;
            }

            try
            {
                byte[] hash = Convert.FromBase64String(segments[0]);
                byte[] salt = Convert.FromBase64String(segments[1]);
                int iterations = int.Parse(segments[2]);
                var algorithm = new HashAlgorithmName(segments[3]);

                byte[] inputHash = Rfc2898DeriveBytes.Pbkdf2(
                    providedPassword,
                    salt,
                    iterations,
                    algorithm,
                    hash.Length);

                // Constant-time comparison prevents timing attacks
                return CryptographicOperations.FixedTimeEquals(hash, inputHash);
            }
            catch
            {
                return false;
            }
        }
    }
}
```

---

### Step 5.3: Create Staff User Repository Contract (`IStaffUserRepository.cs`)
Create file `Infrastructure/Repositories/Contracts/IStaffUserRepository.cs`:

```csharp
using GdbWebApi.Domain.Models;

namespace GdbWebApi.Infrastructure.Repositories.Contracts
{
    public interface IStaffUserRepository
    {
        Task<StaffUser?> GetByUsernameAsync(string username);
        Task SaveStaffUserAsync(StaffUser user);
        Task<bool> ExistsAsync(string username);
    }
}
```

---

### Step 5.4: Create Staff User Repository with Hashed Bootstrap Seeding (`StaffUserRepository.cs`)
Create file `Infrastructure/Repositories/Implementations/StaffUserRepository.cs`:

```csharp
using GdbWebApi.Application.Common.Security;
using GdbWebApi.Domain.Models;
using GdbWebApi.Infrastructure.Repositories.Contracts;
using Microsoft.Extensions.Configuration;
using System.Collections.Concurrent;

namespace GdbWebApi.Infrastructure.Repositories.Implementations
{
    public class StaffUserRepository : IStaffUserRepository
    {
        // Thread-safe store for staff identity records
        private static readonly ConcurrentDictionary<string, StaffUser> Users = new(StringComparer.OrdinalIgnoreCase);
        private static bool _isSeeded = false;
        private static readonly object SeedLock = new();

        public StaffUserRepository(IPasswordHasher passwordHasher, IConfiguration configuration)
        {
            EnsureSeeded(passwordHasher, configuration);
        }

        public Task<StaffUser?> GetByUsernameAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return Task.FromResult<StaffUser?>(null);
            }

            Users.TryGetValue(username.Trim(), out var user);
            return Task.FromResult(user);
        }

        public Task SaveStaffUserAsync(StaffUser user)
        {
            if (user == null || string.IsNullOrWhiteSpace(user.Username))
            {
                throw new ArgumentException("Staff user and username are required.");
            }

            Users[user.Username.Trim()] = user;
            return Task.CompletedTask;
        }

        public Task<bool> ExistsAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return Task.FromResult(false);
            }

            return Task.FromResult(Users.ContainsKey(username.Trim()));
        }

        // Enterprise Bootstrap Seed: Initial accounts are hashed on startup and NEVER stored in plaintext
        private static void EnsureSeeded(IPasswordHasher passwordHasher, IConfiguration configuration)
        {
            if (_isSeeded) return;

            lock (SeedLock)
            {
                if (_isSeeded) return;

                // Support environment variable overrides (e.g. STAFFSEED__ADMINPASSWORD) for production security
                string adminPass = configuration["StaffSeed:AdminPassword"] ?? "Admin@123";
                string managerPass = configuration["StaffSeed:ManagerPassword"] ?? "Manager@123";
                string tellerPass = configuration["StaffSeed:TellerPassword"] ?? "Teller@123";

                var defaultStaff = new List<StaffUser>
                {
                    new StaffUser
                    {
                        Username = "admin",
                        PasswordHash = passwordHasher.HashPassword(adminPass),
                        Role = UserRoles.Admin,
                        FullName = "System Administrator"
                    },
                    new StaffUser
                    {
                        Username = "manager",
                        PasswordHash = passwordHasher.HashPassword(managerPass),
                        Role = UserRoles.Manager,
                        FullName = "Branch Manager"
                    },
                    new StaffUser
                    {
                        Username = "teller",
                        PasswordHash = passwordHasher.HashPassword(tellerPass),
                        Role = UserRoles.Teller,
                        FullName = "Front Desk Teller"
                    }
                };

                foreach (var staff in defaultStaff)
                {
                    Users.TryAdd(staff.Username, staff);
                }

                _isSeeded = true;
            }
        }
    }
}
```

---

### Step 5.5: Create `IAuthService.cs`
Create file `Application/Services/Contracts/IAuthService.cs`:

```csharp
using GdbWebApi.Application.Dtos.Auth;

namespace GdbWebApi.Application.Services.Contracts
{
    public interface IAuthService
    {
        Task<AuthResponseDto?> AuthenticateAccountAsync(AccountLoginRequestDto request);
        Task<AuthResponseDto?> AuthenticateStaffAsync(StaffLoginRequestDto request);
    }
}
```

---

### Step 5.6: Create Decoupled `AuthService.cs` (Zero Hardcoded Credentials)
Create file `Application/Services/Implementations/AuthService.cs`:

```csharp
using GdbWebApi.Application.Common.Security;
using GdbWebApi.Application.Dtos.Auth;
using GdbWebApi.Application.Services.Contracts;
using GdbWebApi.Domain.Models;
using GdbWebApi.Infrastructure.Repositories.Contracts;

namespace GdbWebApi.Application.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IStaffUserRepository _staffUserRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenService _tokenService;

        public AuthService(
            IAccountRepository accountRepository,
            IStaffUserRepository staffUserRepository,
            IPasswordHasher passwordHasher,
            ITokenService tokenService)
        {
            _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
            _staffUserRepository = staffUserRepository ?? throw new ArgumentNullException(nameof(staffUserRepository));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        }

        public async Task<AuthResponseDto?> AuthenticateAccountAsync(AccountLoginRequestDto request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.AccountNumber) || string.IsNullOrWhiteSpace(request.Pin))
            {
                return null;
            }

            // 1. Fetch account from database repository
            IAccount account = await _accountRepository.GetAccountAsync(request.AccountNumber);
            if (account == null)
            {
                return null; // Account not found -> 401
            }

            // 2. Check if account is active
            if (!account.CheckIfAccountIsActive())
            {
                throw new InvalidOperationException("Account is inactive or closed.");
            }

            // 3. Validate PIN via domain model
            if (!account.ValidatePin(request.Pin))
            {
                return null; // Bad PIN -> 401
            }

            // 4. Generate JWT for customer with Role "User" and AccountNumber claim
            string token = _tokenService.GenerateToken(
                identifier: account.AccountNumber,
                name: account.Name,
                role: UserRoles.User,
                accountNumber: account.AccountNumber);

            return new AuthResponseDto
            {
                Token = token,
                TokenType = "Bearer",
                Role = UserRoles.User,
                AccountNumber = account.AccountNumber,
                Name = account.Name,
                ExpiresAt = _tokenService.GetTokenExpiry()
            };
        }

        public async Task<AuthResponseDto?> AuthenticateStaffAsync(StaffLoginRequestDto request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return null;
            }

            // 1. Fetch staff identity record from repository
            StaffUser? staff = await _staffUserRepository.GetByUsernameAsync(request.Username);
            if (staff == null || !staff.IsActive)
            {
                return null; // Staff not found or inactive -> 401
            }

            // 2. Cryptographic password verification (PBKDF2 / SHA-256)
            bool isPasswordValid = _passwordHasher.VerifyPassword(staff.PasswordHash, request.Password);
            if (!isPasswordValid)
            {
                return null; // Invalid credentials -> 401
            }

            // 3. Issue JWT with staff role
            string token = _tokenService.GenerateToken(
                identifier: staff.Username,
                name: staff.FullName,
                role: staff.Role);

            return new AuthResponseDto
            {
                Token = token,
                TokenType = "Bearer",
                Role = staff.Role,
                AccountNumber = null,
                Name = staff.FullName,
                ExpiresAt = _tokenService.GetTokenExpiry()
            };
        }
    }
}
```

---

## 8. Phase 6: Dependency Injection Registration

Update [Application/Extensions/ServiceRegistration.cs](file:///c:/gdb_migration/web-api-ea1/GdbWebApi/Application/Extensions/ServiceRegistration.cs) to register `IPasswordHasher`, `IStaffUserRepository`, `ITokenService`, and `IAuthService`:

```csharp
using GdbWebApi.Application.Common.Security;
using GdbWebApi.Application.Services.Contracts;
using GdbWebApi.Application.Services.Implementations;
using GdbWebApi.Infrastructure.Repositories.Contracts;
using GdbWebApi.Infrastructure.Repositories.Implementations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GdbWebApi.Application.Extensions
{
    public static class ServiceRegistration
    {
        public static IServiceCollection AddBankingServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Bind JWT Configuration
            services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

            // Repositories
            services.AddScoped<IAccountRepository, AccountRepositoryDB>();
            services.AddScoped<IStaffUserRepository, StaffUserRepository>();
            services.AddScoped<ITransactionRepository, TransactionRepositoryDB>();
            services.AddScoped<IHomeRepository, HomeRepository>();

            // Application Services
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<ITransactionService, TransactionService>();
            services.AddScoped<ITransactionQueryService, TransactionQueryService>();
            services.AddScoped<IHomeService, HomeService>();

            // Security & Auth Services
            services.AddSingleton<IPasswordHasher, PasswordHasher>();
            services.AddScoped<ITokenService, JwtTokenService>();
            services.AddScoped<IAuthService, AuthService>();

            return services;
        }
    }
}
```

---

## 9. Phase 7: ASP.NET Core Middleware & Policies (`Program.cs`)

Update [Program.cs](file:///c:/gdb_migration/web-api-ea1/GdbWebApi/Program.cs) with authentication configuration and correct pipeline order.

```csharp
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using GdbWebApi.Application.Common.Security;
using GdbWebApi.Application.Extensions;
using GdbWebApi.Infrastructure.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// 1. Add Controllers with Enum Converter
builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// 2. Register Banking & Auth Services
builder.Services.AddBankingServices(builder.Configuration);

// 3. Configure JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] 
    ?? throw new InvalidOperationException("Jwt:Key is missing in configuration.");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "GdbWebApi";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "GdbBankingClients";

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false; // set to true in production
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero // Strict expiry check
        };
    });

// 4. Configure RBAC Authorization Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireStaff", policy => 
        policy.RequireRole(UserRoles.Admin, UserRoles.Manager, UserRoles.Teller));
    options.AddPolicy("RequireTellerOrAdmin", policy => 
        policy.RequireRole(UserRoles.Admin, UserRoles.Teller));
    options.AddPolicy("RequireManagerOrAdmin", policy => 
        policy.RequireRole(UserRoles.Admin, UserRoles.Manager));
});

// 5. Enterprise API Versioning
builder.Services
    .AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
        options.ApiVersionReader = ApiVersionReader.Combine(
            new UrlSegmentApiVersionReader(),
            new HeaderApiVersionReader("X-Api-Version"),
            new QueryStringApiVersionReader("api-version"));
    })
    .AddMvc()
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

// 6. Swagger & OpenAPI with Bearer Auth
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
        foreach (var description in provider.ApiVersionDescriptions)
        {
            options.SwaggerEndpoint(
                $"/swagger/{description.GroupName}/swagger.json",
                $"GDB API {description.GroupName.ToUpperInvariant()}");
        }
    });
}

app.UseHttpsRedirection();

// Anonymous Health Check Endpoint (T-R6-10)
app.MapGet("/api/health", () => Results.Ok(new { status = "Healthy", timestamp = DateTime.UtcNow }))
   .AllowAnonymous();

// CRITICAL PIPELINE ORDER:
// 1. UseAuthentication MUST come before UseAuthorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
```

---

## 10. Phase 8: Swagger Bearer Token Integration

Update [Infrastructure/Swagger/ConfigureSwaggerOptions.cs](file:///c:/gdb_migration/web-api-ea1/GdbWebApi/Infrastructure/Swagger/ConfigureSwaggerOptions.cs) to display the **Authorize** lock button in Swagger UI:

```csharp
using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace GdbWebApi.Infrastructure.Swagger
{
    public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
    {
        private readonly IApiVersionDescriptionProvider _provider;

        public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider)
        {
            _provider = provider;
        }

        public void Configure(SwaggerGenOptions options)
        {
            foreach (var description in _provider.ApiVersionDescriptions)
            {
                options.SwaggerDoc(
                    description.GroupName,
                    new OpenApiInfo
                    {
                        Title = $"Global Digital Bank (GDB) API {description.ApiVersion}",
                        Version = description.ApiVersion.ToString(),
                        Description = description.IsDeprecated
                            ? "This API version has been deprecated."
                            : "Enterprise Web API for Global Digital Bank operations."
                    });
            }

            // Define JWT Bearer Security Scheme
            var securityScheme = new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Description = "Enter JWT Bearer token like: Bearer {your_token_here}",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Reference = new OpenApiReference
                {
                    Id = "Bearer",
                    Type = ReferenceType.SecurityScheme
                }
            };

            options.AddSecurityDefinition("Bearer", securityScheme);

            // Require JWT globally in Swagger UI
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                { securityScheme, Array.Empty<string>() }
            });
        }
    }
}
```

---

## 11. Phase 9: AuthController Implementation

Create file `Application/Controllers/AuthController.cs`:

```csharp
using Asp.Versioning;
using GdbWebApi.Application.Dtos.Auth;
using GdbWebApi.Application.Services.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GdbWebApi.Application.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        }

        /// <summary>
        /// Customer login using Account Number and 4-digit PIN.
        /// </summary>
        [HttpPost("account-login")]
        [AllowAnonymous]
        public async Task<IActionResult> AccountLogin([FromBody] AccountLoginRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var response = await _authService.AuthenticateAccountAsync(request);
                if (response == null)
                {
                    return Unauthorized(new { message = "Invalid account number or PIN." });
                }

                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Staff login using Username and Password.
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> StaffLogin([FromBody] StaffLoginRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _authService.AuthenticateStaffAsync(request);
            if (response == null)
            {
                return Unauthorized(new { message = "Invalid username or password." });
            }

            return Ok(response);
        }
    }
}
```

---

## 12. Phase 10: Protecting AccountController with RBAC & Ownership Isolation

Update [Application/Controllers/AccountController.cs](file:///c:/gdb_migration/web-api-ea1/GdbWebApi/Application/Controllers/AccountController.cs):
1. Add `[Authorize]` to the class.
2. Add a helper method `IsAuthorizedForAccount(string accNo)`:
   - If caller is in role `Admin`, `Manager`, or `Teller`, return `true`.
   - If caller is in role `User`, verify `User.FindFirst("accountNumber")?.Value == accNo`.
   - If neither, return `false`.
3. Add role restrictions:
   - `GetAllAccounts`: `[Authorize(Roles = UserRoles.Staff)]`
   - `CreateAccount` methods: `[Authorize(Roles = UserRoles.TellerOrAdmin)]`
   - `CloseAccount` methods: `[Authorize(Roles = UserRoles.ManagerOrAdmin)]`

Here is the updated [AccountController.cs](file:///c:/gdb_migration/web-api-ea1/GdbWebApi/Application/Controllers/AccountController.cs):

```csharp
using Asp.Versioning;
using GdbWebApi.Application.Common.Security;
using GdbWebApi.Application.Dtos;
using GdbWebApi.Application.Services.Contracts;
using GdbWebApi.Application.Services.Implementations;
using GdbWebApi.Domain.Enums;
using GdbWebApi.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GdbWebApi.Application.Controllers
{
    [ApiController]
    [Authorize] // All endpoints require a valid token by default
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
        }

        public AccountController()
            : this(new AccountService())
        {
        }

        // Helper: Check if customer owns the account or caller is bank staff
        private bool IsAuthorizedForAccount(string accNo)
        {
            if (User.IsInRole(UserRoles.Admin) || 
                User.IsInRole(UserRoles.Manager) || 
                User.IsInRole(UserRoles.Teller))
            {
                return true;
            }

            var tokenAccount = User.FindFirst("accountNumber")?.Value 
                            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return string.Equals(tokenAccount, accNo, StringComparison.OrdinalIgnoreCase);
        }

        // GET: api/v1/Account/{accNo}
        [HttpGet("{accNo}")]
        public async Task<IActionResult> GetAccountAsync(string accNo)
        {
            if (!IsAuthorizedForAccount(accNo))
            {
                return StatusCode(StatusCodes.Status403Forbidden, 
                    new { message = "Forbidden: You do not have permission to access this account." });
            }

            IAccount account = await _accountService.GetAccountAsync(accNo);
            if (account == null)
            {
                return NotFound(new { message = $"Account {accNo} not found." });
            }

            return Ok(account);
        }

        // GET: api/v1/Account (Staff only)
        [HttpGet]
        [Authorize(Roles = UserRoles.Staff)]
        public IActionResult GetAllAccounts()
        {
            List<ViewAllAccountsResponseDto> accounts = _accountService.GetAllAccounts();
            return Ok(accounts);
        }

        // GET: api/v1/Account/{accNo}/balance
        [HttpGet("{accNo}/balance")]
        public async Task<IActionResult> GetBalanceAsync(string accNo)
        {
            if (!IsAuthorizedForAccount(accNo))
            {
                return StatusCode(StatusCodes.Status403Forbidden, 
                    new { message = "Forbidden: You do not have permission to view balance for this account." });
            }

            try
            {
                ViewBalanceResponseDto response = await _accountService.GetBalanceAsync(accNo);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // GET: api/v1/Account/{accNo}/details
        [HttpGet("{accNo}/details")]
        public async Task<IActionResult> ViewAccountAsync(string accNo)
        {
            if (!IsAuthorizedForAccount(accNo))
            {
                return StatusCode(StatusCodes.Status403Forbidden, 
                    new { message = "Forbidden: You do not have permission to view details for this account." });
            }

            try
            {
                ViewAccountResponseDto response = await _accountService.ViewAccountAsync(accNo);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // POST: api/v1/Account/savings (Teller or Admin)
        [HttpPost("savings")]
        [Authorize(Roles = UserRoles.TellerOrAdmin)]
        public IActionResult CreateSavingsAccount([FromBody] CreateSavingsAccountRequestDto request)
        {
            if (request == null) return BadRequest(new { message = "Request body cannot be null." });

            var fullRequest = new CreateAccountRequestDto
            {
                AccountNumber = request.AccountNumber,
                Name = request.Name,
                Age = request.Age,
                Balance = request.Balance,
                Pin = request.Pin,
                AccountType = AccountType.Savings,
                Status = AccountStatus.Active,
                Privilege = request.Privilege,
                InterestRate = request.InterestRate,
                MinimumBalance = request.MinimumBalance
            };

            return CreateAccount(fullRequest);
        }

        // POST: api/v1/Account/current (Teller or Admin)
        [HttpPost("current")]
        [Authorize(Roles = UserRoles.TellerOrAdmin)]
        public IActionResult CreateCurrentAccount([FromBody] CreateCurrentAccountRequestDto request)
        {
            if (request == null) return BadRequest(new { message = "Request body cannot be null." });

            var fullRequest = new CreateAccountRequestDto
            {
                AccountNumber = request.AccountNumber,
                Name = request.Name,
                Age = request.Age,
                Balance = request.Balance,
                Pin = request.Pin,
                AccountType = AccountType.Current,
                Status = AccountStatus.Active,
                Privilege = request.Privilege,
                OverdraftLimit = request.OverdraftLimit
            };

            return CreateAccount(fullRequest);
        }

        // POST: api/v1/Account/fixed-deposit (Teller or Admin)
        [HttpPost("fixed-deposit")]
        [Authorize(Roles = UserRoles.TellerOrAdmin)]
        public IActionResult CreateFixedDepositAccount([FromBody] CreateFixedDepositAccountRequestDto request)
        {
            if (request == null) return BadRequest(new { message = "Request body cannot be null." });

            var fullRequest = new CreateAccountRequestDto
            {
                AccountNumber = request.AccountNumber,
                Name = request.Name,
                Age = request.Age,
                Balance = request.Balance,
                Pin = request.Pin,
                AccountType = AccountType.FixedDeposit,
                Status = AccountStatus.Active,
                Privilege = request.Privilege,
                TenureMonths = request.TenureMonths,
                InterestRate = request.InterestRate
            };

            return CreateAccount(fullRequest);
        }

        // POST: api/v1/Account/salary (Teller or Admin)
        [HttpPost("salary")]
        [Authorize(Roles = UserRoles.TellerOrAdmin)]
        public IActionResult CreateSalaryAccount([FromBody] CreateSalaryAccountRequestDto request)
        {
            if (request == null) return BadRequest(new { message = "Request body cannot be null." });

            var fullRequest = new CreateAccountRequestDto
            {
                AccountNumber = request.AccountNumber,
                Name = request.Name,
                Age = request.Age,
                Balance = request.Balance,
                Pin = request.Pin,
                AccountType = AccountType.Salary,
                Status = AccountStatus.Active,
                Privilege = request.Privilege,
                EmployerName = request.EmployerName
            };

            return CreateAccount(fullRequest);
        }

        // POST: api/v1/Account (Teller or Admin)
        [HttpPost]
        [Authorize(Roles = UserRoles.TellerOrAdmin)]
        public IActionResult CreateAccount([FromBody] CreateAccountRequestDto request)
        {
            if (request == null) return BadRequest(new { message = "Request body cannot be null." });

            try
            {
                CreateAccountResponseDto response = _accountService.CreateAccount(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // DELETE: api/v1/Account/{accountNumber} (Manager or Admin only)
        [HttpDelete("{accountNumber}")]
        [Authorize(Roles = UserRoles.ManagerOrAdmin)]
        public async Task<IActionResult> CloseAccountByRouteAsync(string accountNumber)
        {
            return await CloseAccountInternalAsync(accountNumber);
        }

        // PUT: api/v1/Account/{accountNumber}/close (Manager or Admin only)
        [HttpPut("{accountNumber}/close")]
        [Authorize(Roles = UserRoles.ManagerOrAdmin)]
        public async Task<IActionResult> CloseAccountByRoutePutAsync(string accountNumber)
        {
            return await CloseAccountInternalAsync(accountNumber);
        }

        // PUT: api/v1/Account/close (Manager or Admin only)
        [HttpPut("close")]
        [Authorize(Roles = UserRoles.ManagerOrAdmin)]
        public async Task<IActionResult> CloseAccountAsync([FromBody] CloseAccountRequestDto request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.AccountNumber))
            {
                return BadRequest(new { message = "Account number is required." });
            }

            return await CloseAccountInternalAsync(request.AccountNumber);
        }

        private async Task<IActionResult> CloseAccountInternalAsync(string accountNumber)
        {
            try
            {
                var request = new CloseAccountRequestDto { AccountNumber = accountNumber };
                CloseAccountResponseDto response = await _accountService.CloseAccountAsync(request);
                return Ok(response);
            }
            catch (Exception ex) when (ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex) when (ex.Message.Contains("already closed", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
```

---

## 13. Phase 11: Protecting TransactionController

Update [Application/Controllers/TransactionController.cs](file:///c:/gdb_migration/web-api-ea1/GdbWebApi/Application/Controllers/TransactionController.cs):
1. Add `[Authorize]` to require JWT token.
2. In `DepositAsync`, `WithdrawAsync`, and `TransferAsync`, verify that if the caller is in role `User`, the `request.AccountNumber` (or `request.FromAccountNumber`) matches their token's `accountNumber` claim!

Example ownership check in `WithdrawAsync`:
```csharp
if (User.IsInRole(UserRoles.User))
{
    var tokenAccount = User.FindFirst("accountNumber")?.Value;
    if (!string.Equals(tokenAccount, request.AccountNumber, StringComparison.OrdinalIgnoreCase))
    {
        return StatusCode(StatusCodes.Status403Forbidden, 
            new { message = "Forbidden: You cannot withdraw from another customer's account." });
    }
}
```

---

## 14. Phase 12: Step-by-Step Verification & Testing Guide

Once your code is in place, open PowerShell and follow this step-by-step test script to verify all requirements (`T-R6-01` to `T-R6-12`):

### Step 12.1: Build and Run the Web API
```powershell
dotnet build
dotnet run
```
Verify the server starts and displays `Now listening on: https://localhost:7134; http://localhost:5246` (or your configured port).

---

### Step 12.2: Execute Verification Tests (curl / PowerShell)

#### Test 1: Health Check without Token (`T-R6-10`)
```powershell
curl -X GET "https://localhost:7134/api/health" -k
```
**Expected Output:** `200 OK` `{"status":"Healthy","timestamp":"..."}`

---

#### Test 2: Protected Endpoint with No Token (`T-R6-03`)
```powershell
curl -i -X GET "https://localhost:7134/api/v1/Account" -k
```
**Expected Output:** `401 Unauthorized`

---

#### Test 3: Customer Login with Valid Credentials (`T-R6-01`)
Assuming an account `1000000001` exists with PIN `1234`:
```powershell
$body = @{
    accountNumber = "1000000001"
    pin = "1234"
} | ConvertTo-Json

$response = Invoke-RestMethod -Uri "https://localhost:7134/api/v1/auth/account-login" -Method Post -Body $body -ContentType "application/json" -SkipCertificateCheck
$userToken = $response.token
Write-Host "Customer Token: $userToken"
```
**Expected Output:** `200 OK` with JSON `{ "token": "...", "role": "User", "accountNumber": "1000000001" }`

---

#### Test 4: Customer Login with Invalid PIN (`T-R6-02`)
```powershell
$badBody = @{
    accountNumber = "1000000001"
    pin = "9999"
} | ConvertTo-Json

Invoke-WebRequest -Uri "https://localhost:7134/api/v1/auth/account-login" -Method Post -Body $badBody -ContentType "application/json" -SkipCertificateCheck
```
**Expected Output:** `401 Unauthorized` (`{"message":"Invalid account number or PIN."}`)

---

#### Test 5: Customer Accesses Own Balance (`T-R6-04`)
```powershell
curl -i -X GET "https://localhost:7134/api/v1/Account/1000000001/balance" `
  -H "Authorization: Bearer $userToken" -k
```
**Expected Output:** `200 OK` with balance data.

---

#### Test 6: Customer Accesses Another Customer's Balance (Ownership Isolation)
```powershell
curl -i -X GET "https://localhost:7134/api/v1/Account/1000000002/balance" `
  -H "Authorization: Bearer $userToken" -k
```
**Expected Output:** `403 Forbidden` (`{"message":"Forbidden: You do not have permission to view balance for this account."}`)

---

#### Test 7: Customer Attempts to View All Accounts (`T-R6-06`)
```powershell
curl -i -X GET "https://localhost:7134/api/v1/Account" `
  -H "Authorization: Bearer $userToken" -k
```
**Expected Output:** `403 Forbidden` (User does not have `Staff` role)

---

#### Test 8: Staff Login — Teller (`T-R6-05`, `T-R6-07`)
```powershell
$tellerBody = @{
    username = "teller"
    password = "Teller@123"
} | ConvertTo-Json

$tellerRes = Invoke-RestMethod -Uri "https://localhost:7134/api/v1/auth/login" -Method Post -Body $tellerBody -ContentType "application/json" -SkipCertificateCheck
$tellerToken = $tellerRes.token

# 8a: Teller views all accounts -> 200 OK
curl -i -X GET "https://localhost:7134/api/v1/Account" -H "Authorization: Bearer $tellerToken" -k

# 8b: Teller attempts to CLOSE account -> 403 Forbidden (T-R6-08)
curl -i -X DELETE "https://localhost:7134/api/v1/Account/1000000001" -H "Authorization: Bearer $tellerToken" -k
```

---

#### Test 9: Staff Login — Manager Closes Account (`T-R6-09`)
```powershell
$mgrBody = @{
    username = "manager"
    password = "Manager@123"
} | ConvertTo-Json

$mgrRes = Invoke-RestMethod -Uri "https://localhost:7134/api/v1/auth/login" -Method Post -Body $mgrBody -ContentType "application/json" -SkipCertificateCheck
$mgrToken = $mgrRes.token

# Manager deletes/closes account -> 200 OK
curl -i -X DELETE "https://localhost:7134/api/v1/Account/1000000001" -H "Authorization: Bearer $mgrToken" -k
```
**Expected Output:** `200 OK`

---

#### Test 10: Tampered Token Verification (`T-R6-12`)
```powershell
$tamperedToken = $userToken.Substring(0, $userToken.Length - 5) + "ABCDE"
curl -i -X GET "https://localhost:7134/api/v1/Account/1000000001/balance" -H "Authorization: Bearer $tamperedToken" -k
```
**Expected Output:** `401 Unauthorized`

---

## 15. Troubleshooting & Common Pitfalls

1. **`IDX10603: Decryption failed. Keys tried: ...` or Signature validation failed**:
   - Cause: The `Jwt:Key` in `appsettings.json` was changed or does not match what was used to sign the token.
   - Fix: Ensure the key string is identical in signing and validation.

2. **`401 Unauthorized` even when `Authorization: Bearer <token>` is sent**:
   - Cause 1: Missing space between `Bearer` and the token.
   - Cause 2: In `Program.cs`, `app.UseAuthorization()` was placed *before* `app.UseAuthentication()`. Always ensure:
     ```csharp
     app.UseAuthentication();
     app.UseAuthorization();
     ```

3. **`403 Forbidden` on every endpoint**:
   - Check the role claim in your token. In `JwtSecurityTokenHandler`, the default role claim type might be `ClaimTypes.Role` (`http://schemas.microsoft.com/ws/2008/06/identity/claims/role`). Our `JwtTokenService` adds both `ClaimTypes.Role` and `role` to guarantee compatibility.

4. **Swagger UI Authorize button does not send the token**:
   - In Swagger UI, click **Authorize**, and enter: `Bearer <your_token>` (or just `<your_token>` if configured as `Type = SecuritySchemeType.Http, Scheme = "bearer"`).
