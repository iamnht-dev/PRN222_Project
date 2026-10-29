# ASM01 — Coach MVC + SignalR

Kiến trúc Entities → Repositories → Services → MVCWebApp. Giữ UI Coach Roster/login của ThaiNH; tích hợp CoachHub để CRUD realtime và đồng bộ giữa các cửa sổ.

Visual Studio Community: mở solution `.slnx`, chọn `zCoach.MVCWebApp.ThaiNH` làm Startup Project, Ctrl+F5. Đăng nhập với tài khoản SQL đang hoạt động rồi vào Coaches (`/Coaches/Index`).

```powershell
dotnet build FA26_PRN222_SE1905_ASM01_ThaiNH.slnx
dotnet run --project zCoach.MVCWebApp.ThaiNH --no-launch-profile --urls http://localhost:5171
```

## Demo SignalR

1. Mở hai cửa sổ cùng bài ASM01, đăng nhập và vào Coaches. Có thể dùng cùng một tài khoản. Đợi `Live updates connected.`.
2. Xoá bộ lọc ở cả hai. Cửa sổ A: SignalR Create → nhập tên Coach demo, chọn chuyên môn → Create via SignalR.
3. Cửa sổ B nhận thông báo CoachChanged và tự tải lại sau khoảng 0,7 giây, không cần F5. Kiểm tra trang phân trang nếu chưa thấy bản ghi.
4. SignalR Edit → đổi tên/lương → Update via SignalR; cửa sổ B cập nhật theo.
5. SignalR Delete trên Coach demo → xác nhận; cả hai cửa sổ cập nhật.
6. Create/Edit/Delete bằng form MVC thường cũng gửi CoachChanged cho các cửa sổ đang mở roster. Details, search ba tiêu chí, phân trang và logout giữ nguyên.

Hub `/coachHub` yêu cầu đăng nhập, dùng service/repository để đọc ghi SQL. Client gọi GetAll/GetById/GetSpecializations/Create/Update/Delete. Đây là SignalR CRUD; không có màn hình chat.

Cấu hình máy cá nhân nằm trong `appsettings.Local.json` (không đưa lên Git). Cookie ASM01 riêng với ASM02.

Kiểm tra: build 0 warning/error; hai client WebSocket đăng nhập, CRUD SQL, broadcast, validation, DateOnly, search và chặn anonymous đã qua. HTTP CRUD MVC cũng gửi đủ ba thông báo tới client thứ hai. Bản ghi thử đã xoá, không reset database. Chưa kiểm tra click/modal bằng trình duyệt tự động. Dừng app trước khi Rebuild để tránh khóa DLL.
