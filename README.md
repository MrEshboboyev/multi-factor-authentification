# 🔐 Multi-Factor Authentication (MFA) in .NET 🚀  

This project implements **Multi-Factor Authentication (MFA)** in **.NET** using **Domain-Driven Design (DDD) and Clean Architecture**. It enhances **security, user authentication, and login protection** by requiring a **recovery code** in addition to traditional email-password authentication.  

---

## 🌟 Key Features  

✅ **User Registration** – Secure account creation with password hashing.  
✅ **Token-Based Authentication** – Standard **JWT token issuance** upon login.  
✅ **MFA Activation** – Users can enable MFA and receive a recovery code.  
✅ **Login With MFA Enforcement** – When MFA is enabled, a recovery code is required.  
✅ **Disable MFA** – Users can turn off MFA when necessary.  
✅ **Domain-Driven Design (DDD) & Clean Architecture** – Modular, maintainable, and scalable.  

---

## 🔄 MFA Workflow  

### **1️⃣ User Registration**  
- A new user registers via the `/register` endpoint with **email and password**.  

### **2️⃣ Standard Login (No MFA)**  
- User logs in via `/login` endpoint.  
- **If MFA is disabled**, a **JWT token** is issued.  
- **If MFA is enabled**, login is **blocked**, and the user must enter a recovery code.  

### **3️⃣ Enabling MFA**  
- User calls the `/enableMfa` endpoint.  
- A **recovery code** is generated and returned.  

### **4️⃣ Login With MFA**  
- User must use `/loginWithAuth`, providing:  
  - **Email**  
  - **Password**  
  - **Recovery Code**  
- If the credentials are valid, a **JWT token is issued**.  

### **5️⃣ Disabling MFA**  
- User calls `/disableMfa` endpoint.  
- MFA is turned off, and the user can log in normally.  

---

## 🚀 Technologies Used  

🔹 **.NET Core** – Secure authentication framework.  
🔹 **JWT Authentication** – Token-based security mechanism.  
🔹 **Domain-Driven Design (DDD)** – Structured, scalable architecture.  
🔹 **Clean Architecture** – Separation of concerns for maintainability.  
🔹 **Role-Based Access Control (RBAC)** – Secure user authorization.  
🔹 **Fluent Validation** – Ensuring strong input validation.  
🔹 **Serilog** – Centralized logging for security auditing.  

---

## 🛠 Getting Started  

### **Prerequisites**  
Before using this system, ensure you have:  
✅ **.NET SDK installed**  
✅ **A database for user authentication**  
✅ **Postman or Swagger** for API testing  

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

## 🌐 API Endpoints  

| Method | Endpoint           | Description |
|--------|-------------------|-------------|
| POST   | `/register`       | Registers a new user |
| POST   | `/login`          | Logs in a user (JWT issued if MFA is disabled) |
| POST   | `/enableMfa`      | Enables MFA and generates a recovery code |
| POST   | `/loginWithAuth`  | Logs in a user with MFA (email, password, recovery code required) |
| POST   | `/disableMfa`     | Disables MFA for the user |

---

## 🔐 Security Measures  

✅ **MFA Enforcement** – Users must enter a recovery code for enhanced security.  
✅ **JWT-Based Authentication** – Secure token issuance and validation.  
✅ **Password Hashing** – User passwords are encrypted before storage.  
✅ **Rate Limiting & Logging** – Protects against brute-force attacks.  

---

## 🧪 Testing & Quality Assurance  

✅ **Unit Testing** – Covers authentication and security workflows.  
✅ **API Testing** – Supports Postman, Swagger, and automated tests.  
✅ **Logging & Monitoring** – Tracks login attempts and security events.  

---

## 🔥 Why Use This Project?  

✅ **Enhances Security** – Protects against password-based attacks.  
✅ **Scalable & Maintainable** – Built using **DDD & Clean Architecture**.  
✅ **Ready for Production** – Implements industry security best practices.  
✅ **Enterprise-Grade Authentication** – Works in cloud and microservices environments.  

---

## 🏗 About the Author  
Developed by [MrEshboboyev](https://github.com/MrEshboboyev), a **.NET expert** specializing in **secure authentication, clean architectures, and enterprise software development**.  

## 📄 License  
This project is licensed under the **MIT License**. Feel free to use and contribute!  

---

🚀 **Ready to implement MFA in your .NET applications?** Clone the repo and start securing your users today!  
