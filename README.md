# 🔐 Advanced Multi-Factor Authentication (MFA) in .NET 🚀  

This project implements **Advanced Multi-Factor Authentication (MFA)** in **.NET** using **Domain-Driven Design (DDD) and Clean Architecture**. It provides enterprise-grade security with multiple authentication factors, device management, rate limiting, and more.

---

## 🌟 Key Features  

✅ **User Registration** – Secure account creation with password hashing.  
✅ **Token-Based Authentication** – Standard **JWT token issuance** upon login.  
✅ **Multiple MFA Methods** – TOTP, Backup Codes, Recovery Codes  
✅ **QR Code Setup** – Easy TOTP app integration with QR codes  
✅ **Device Management** – Track and trust devices  
✅ **Rate Limiting** – Protection against brute-force attacks  
✅ **Session Management** – Secure session handling  
✅ **MFA Setup Wizard** – Guided MFA configuration  
✅ **API Documentation** – NSwag integration for Swagger UI  
✅ **Domain-Driven Design (DDD) & Clean Architecture** – Modular, maintainable, and scalable.  

---

## 🔐 Advanced MFA Methods

### **1️⃣ Recovery Codes** (Original)
- Traditional method from the base implementation

### **2️⃣ Time-based One-Time Passwords (TOTP)**
- Industry standard TOTP implementation (Google Authenticator, Authy, etc.)
- QR code setup for easy configuration
- 6-digit codes refreshed every 30 seconds

### **3️⃣ Backup Codes**
- 10 single-use backup codes for account recovery
- Format: XXXX-XXXX
- Regeneratable at any time

---

## 🔄 Enhanced MFA Workflow  

### **1️⃣ User Registration**  
- A new user registers via the `/register` endpoint with **email and password**.  

### **2️⃣ Standard Login (No MFA)**  
- User logs in via `/login` endpoint.  
- **If MFA is disabled**, a **JWT token** is issued.  
- **If MFA is enabled**, login is **blocked**, and the user must use MFA.  

### **3️⃣ MFA Setup Wizard**  
- User calls the `/mfa-setup-wizard` endpoint.  
- System automatically:
  - Enables MFA if not already enabled
  - Generates recovery code
  - Sets up TOTP with QR code
  - Generates 10 backup codes
- Returns all information in a single response

### **4️⃣ MFA Authentication Options**  
User can authenticate using any of these methods:
- **TOTP Code** via `/validate-totp`
- **Backup Code** via `/validate-backup-code`
- **Recovery Code** via `/login-with-mfa`

### **5️⃣ Device Management**  
- Track devices used for authentication
- Trust devices to bypass MFA for a period
- Manage trusted devices via API

### **6️⃣ Disabling MFA**  
- User calls `/disable-mfa` endpoint.  
- All MFA methods are disabled, and the user can log in normally.  

---

## 🚀 Technologies Used  

🔹 **.NET Core** – Secure authentication framework.  
🔹 **JWT Authentication** – Token-based security mechanism.  
🔹 **Domain-Driven Design (DDD)** – Structured, scalable architecture.  
🔹 **Clean Architecture** – Separation of concerns for maintainability.  
🔹 **Otp.NET** – TOTP implementation for time-based codes  
🔹 **NSwag** – API documentation and Swagger UI  
🔹 **Fluent Validation** – Ensuring strong input validation.  
🔹 **Serilog** – Centralized logging for security auditing.  
🔹 **MediatR** – Clean command/query handling  

---

## 🛠 Getting Started  

### **Prerequisites**  
Before using this system, ensure you have:  
✅ **.NET SDK installed**  
✅ **A database for user authentication**  
✅ **Postman or Swagger UI** for API testing  

### **Step 1: Clone the Repository**  
```bash  
git clone https://github.com/MrEshboboyev/multi-factor-authentification.git  
cd multi-factor-authentification  
```  

### **Step 2: Install Dependencies**  
```bash  
dotnet restore  
```  

### **Step 3: Run the Application**  
```bash  
dotnet run  
```  

---

## 🌐 API Documentation  

### **Swagger UI**  
When running the application in **Development** mode, you can access the Swagger UI at:  
```
https://localhost:5001/swagger
```

### **API Documentation**  
The OpenAPI specification is available at:  
```
https://localhost:5001/swagger/v1/swagger.json
```

---

## 🌐 API Endpoints  

| Method | Endpoint                    | Description |
|--------|----------------------------|-------------|
| POST   | `/register`                | Registers a new user |
| POST   | `/login`                   | Logs in a user (JWT issued if MFA is disabled) |
| POST   | `/login-with-mfa`          | Logs in a user with recovery code |
| POST   | `/enable-mfa`              | Enables MFA and generates a recovery code |
| POST   | `/disable-mfa`             | Disables MFA for the user |
| POST   | `/setup-totp`              | Sets up TOTP and returns QR code URL |
| POST   | `/validate-totp`           | Validates TOTP code and issues JWT |
| POST   | `/generate-backup-codes`   | Generates new backup codes |
| POST   | `/validate-backup-code`    | Validates backup code and issues JWT |
| POST   | `/mfa-setup-wizard`        | Complete MFA setup in one call |
| POST   | `/manage-trusted-device`   | Trust/Untrust devices |

---

## 🔐 Security Measures  

✅ **Multiple Authentication Factors** – TOTP, Backup Codes, Recovery Codes  
✅ **Rate Limiting** – Account lockout after 5 failed attempts  
✅ **Device Tracking** – Monitor authentication devices  
✅ **Session Management** – Secure session handling  
✅ **JWT-Based Authentication** – Secure token issuance and validation.  
✅ **Password Hashing** – User passwords are encrypted before storage.  
✅ **Input Validation** – Strong validation on all endpoints  

---

## 🧪 Testing & Quality Assurance  

✅ **Unit Testing** – Covers authentication and security workflows.  
✅ **API Testing** – Supports Postman, Swagger UI, and automated tests.  
✅ **Security Testing** – Rate limiting, brute force protection.  
✅ **Logging & Monitoring** – Tracks login attempts and security events.  

---

## 🔥 Why Use This Project?  

✅ **Enterprise-Grade Security** – Multiple MFA methods and protections  
✅ **Scalable & Maintainable** – Built using **DDD & Clean Architecture**.  
✅ **Ready for Production** – Implements industry security best practices.  
✅ **Developer Friendly** – Clean APIs and comprehensive documentation  
✅ **Extensible** – Easy to add new authentication methods  

---

## 🏗 About the Author  
Developed by [MrEshboboyev](https://github.com/MrEshboboyev), a **.NET expert** specializing in **secure authentication, clean architectures, and enterprise software development**.  

## 📄 License  
This project is licensed under the **MIT License**. Feel free to use and contribute!  

---

🚀 **Ready to implement advanced MFA in your .NET applications?** Clone the repo and start securing your users today!