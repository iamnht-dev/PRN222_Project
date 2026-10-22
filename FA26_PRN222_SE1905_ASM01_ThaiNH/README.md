# ASM01 — Coach MVC

Đối chiếu main Quang commit `abf7d2e` ngày 22/10/2026: ASM01 là MVC CRUD thông thường, **không có SignalR**. Giữ giao diện Coach Roster/login của ThaiNH và các tầng Entities → Repositories → Services → MVCWebApp. ChatHub, chat.js và MapHub cũ đã bỏ.

Trong Visual Studio Community, mở `.slnx`, chọn `zCoach.MVCWebApp.ThaiNH` làm Startup Project, Ctrl+F5.

```powershell
dotnet build FA26_PRN222_SE1905_ASM01_ThaiNH.slnx
dotnet run --project zCoach.MVCWebApp.ThaiNH --no-launch-profile --urls http://localhost:5171
```

Đăng nhập `/Account/Login`, vào `/Coaches`. Demo Create New Coach → Details → Edit → Search theo FullName/Email/Phone → phân trang → Delete đúng Coach demo → Logout. Dữ liệu ở SQL Server theo appsettings.json hiện có.

Giữ login MVC theo bản của ThaiNH (mẫu Quang không có login ASM01). SignalR CRUD hiện nằm ở ASM02. Đã build 0 warning/error; kiểm tra Home/login và xác nhận hub chat cũ trả 404. Dừng app trước khi Rebuild để tránh khóa DLL.
