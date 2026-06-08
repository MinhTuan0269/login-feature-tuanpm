🌟 Dự án Quản lý Tài khoản (Auth & User Management)

Dự án này là một hệ thống Full-stack hoàn chỉnh bao gồm Backend (ASP.NET Core) và Frontend (ReactJS) với tính năng cốt lõi là Quản lý xác thực (Authentication), Phân quyền (Authorization), và Quản lý người dùng. 

Hệ thống cung cấp cơ chế bảo mật JWT, Refresh Token và tích hợp SignalR để xử lý các sự kiện thời gian thực (như ép người dùng đăng xuất khi bị khóa tài khoản).

---

Công nghệ sử dụng

### 💻 Frontend (Giao diện người dùng)
- Framework:** React 19 + Vite
- **Ngôn ngữ:** TypeScript
- **Styling:** Tailwind CSS v4, PostCSS
- **Routing:** React Router v7
- **HTTP Client:** Axios (cấu hình sẵn interceptors để xử lý Refresh Token)
- **Real-time:** `@microsoft/signalr`
- **Icons:** Lucide React

### ⚙️ Backend (Máy chủ API)
- **Framework:** ASP.NET Core Web API (.NET)
- **Cơ sở dữ liệu:** SQLite (có thể dễ dàng chuyển sang SQL Server/MySQL qua Entity Framework Core)
- **ORM:** Entity Framework Core
- **Xác thực:** JWT (JSON Web Token) & Refresh Token
- **Real-time:** SignalR Hub (`/hubs/auth`)
- **Tài liệu API:** Swagger / OpenAPI

---

## ✨ Các tính năng chính

1. **Xác thực & Phân quyền (Authentication & Authorization):**
   - Đăng nhập, Đăng ký tài khoản.
   - Bảo mật API bằng JWT Bearer Token.
   - Cơ chế **Refresh Token**: Tự động cấp lại Access Token mới khi token cũ hết hạn (giúp người dùng không bị văng ra ngoài).
   - Phân quyền người dùng (Admin, User).

2. **Quản lý Người dùng (User Management - Dành cho Admin):**
   - Danh sách người dùng với phân trang (Pagination) xử lý từ server (Server-side).
   - Khóa / Mở khóa tài khoản (Ban/Unban).
   - Xóa mềm (Soft-delete) tài khoản.

3. **Tính năng Real-time (Thời gian thực) qua SignalR:**
   - **Force Logout:** Khi Admin khóa tài khoản, Backend sẽ bắn một event qua SignalR tới Client. Client ngay lập tức xóa token và ép người dùng đó quay về màn hình đăng nhập.

---

## 🛠️ Hướng dẫn Cài đặt & Chạy dự án (Môi trường Local)

### 1. Khởi chạy Backend (.NET)
*Yêu cầu: Đã cài đặt .NET SDK.*
1. Mở terminal và di chuyển vào thư mục Backend:
2. Restore các packages:
3. Cập nhật Database (chạy Migration):
4. Chạy server:
   *Server Backend thường sẽ chạy ở cổng `http://localhost:5xxx` hoặc `https://localhost:7xxx`. Bạn có thể truy cập `/swagger` để xem tài liệu API.*

### 2. Khởi chạy Frontend (React + Vite)
*Yêu cầu: Đã cài đặt Node.js (phiên bản 18+ khuyến nghị).*
1. Mở terminal và di chuyển vào thư mục Frontend:
2. Cài đặt các thư viện cần thiết: npm install
3. Cấu hình biến môi trường:
   - Tạo (hoặc sửa) file `.env` ở thư mục gốc Frontend.
   - Thêm đường dẫn tới API Backend:
4. Chạy dự án Frontend: npm run dev
   *Ứng dụng sẽ chạy tại `http://localhost:5173`.*

---

## 📂 Cấu trúc thư mục Frontend cơ bản

```text
src/
├── api/          # Cấu hình Axios (axiosClient.ts) và định nghĩa các API (authApi, userApi)
├── components/   # Các UI components dùng chung (Button, Modal, Table...)
├── hooks/        # Các custom React Hooks (ví dụ: useUsers cho phân trang)
├── pages/        # Các trang chính (Login, Register, Admin Dashboard...)
├── types/        # Định nghĩa các TypeScript Interfaces (User, Response, DTOs)
├── App.tsx       # Component gốc, cấu hình React Router
└── main.tsx      # Điểm bắt đầu của ứng dụng React
```

---
- Mọi logic gọi API được tập trung trong thư mục `/src/api/`. 
- **Quy tắc Refresh Token:** Frontend đã được cấu hình Axios Interceptors. Nếu Access Token hết hạn (nhận lỗi 401), hệ thống sẽ tự động gọi API Refresh Token để lấy token mới và thực hiện lại request bị lỗi một cách trong suốt với người dùng.
- **SignalR:** Việc kết nối hub được thực hiện sau khi đăng nhập thành công. Lắng nghe event `ForceLogout` để xử lý đăng xuất an toàn.

