# Quản lý Ký Túc Xá (QLKTX)

## Mô tả
Dự án Quản lý Ký Túc Xá (QLKTX) là một hệ thống giúp quản lý thông tin sinh viên, phòng ký túc xá, và các dịch vụ liên quan. Hệ thống cho phép quản lý đăng ký phòng, và các chức năng cơ bản khác để hỗ trợ công tác quản lý ký túc xá.

## Các tính năng chính
- Quản lý thông tin sinh viên
- Quản lý phòng ký túc xá
- Đăng ký phòng cho sinh viên
- Quản lý lịch sử thanh toán
- Gửi thông báo cho sinh viên
- Quản lý việc thanh toán tiền thuê phòng
- Báo cáo thống kê về các phòng ký túc xá

## Công nghệ sử dụng
- **Frontend**: HTML, CSS, JavaScript, Bootstrap
- **Backend**: ASP.NET MVC
- **Database**: SQL Server
- **Framework**: Bootstrap (cho giao diện), jQuery (cho AJAX)
- **Development tools**: Visual Studio, Git

## Cài đặt và triển khai
### 1. Cài đặt môi trường phát triển
1. Cài đặt [Visual Studio](https://visualstudio.microsoft.com/) (có thể sử dụng phiên bản Community miễn phí).
2. Cài đặt [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) và [SQL Server Management Studio (SSMS)](https://aka.ms/ssmsfullsetup).
3. Cài đặt Git để quản lý mã nguồn.

### 2. Cài đặt dự án
1. Tải mã nguồn từ GitHub:
   ```bash
   git clone https://github.com/CPahihi/QLKTX.git
2. Mở dự án trong Visual Studio.
3. Cấu hình kết nối đến cơ sở dữ liệu SQL Server trong file web.config (đảm bảo rằng bạn đã tạo cơ sở dữ liệu tương ứng trong SQL Server).
Tạo cơ sở dữ liệu qlktx trong SQL Server.
Chạy các câu lệnh SQL có trong file QLKTXdb.sql để tạo các bảng cần thiết.
3. Chạy ứng dụng
Mở ứng dụng trong Visual Studio.
Chạy ứng dụng bằng cách nhấn F5 hoặc chọn "Start" trong Visual Studio.
