# ASM02 — Coach Razor Pages + SignalR

Đối chiếu main Quang commit `abf7d2e` ngày 22/10/2026. Giữ Entities → Repositories → Services → RazorWebApp, login cookie và giao diện Coach của ThaiNH; thêm `Hubs/CoachHub.cs`, DTO, `wwwroot/js/coaches-signalr.js` và các nút SignalR cạnh CRUD Razor Pages.

```powershell
dotnet build FA26_PRN222_SE1905_ASM02_ThaiNH.slnx
dotnet run --project zCoach.RazorWebApp.ThaiNH --no-launch-profile --urls http://localhost:5294
```

Visual Studio Community: chọn RazorWebApp làm Startup Project. Mở `/CoachThaiNhs`, đăng nhập bằng tài khoản đang có trong database của bạn.

## Test hai tab

1. Hai tab cùng `/CoachThaiNhs`, cùng tài khoản được, đợi `Live updates connected.`.
2. Tab A: SignalR Create → nhập Coach demo, chọn chuyên môn → Create via SignalR.
3. Cả hai tab nhận CoachChanged rồi tự tải lại khoảng 0,7 giây; dữ liệu giữ ở SQL Server.
4. Tab B: SignalR Edit → đổi lương → Update via SignalR → kiểm tra tab kia.
5. SignalR Delete trên đúng Coach demo → xác nhận → cả hai tab mất dòng đó.
6. CRUD thường, tìm kiếm AND ba tiêu chí, phân trang, validation, logout vẫn dùng như trước.

Hub `/coachHub` có `[Authorize]`, gọi service qua DI. Client gọi GetAll/GetById/GetSpecializations/Create/Update/Delete, server phát CoachChanged. Đây là CRUD realtime theo main Quang, không phải chat. Các bộ lọc GET được giữ khi tự tải lại; bản ghi không khớp lọc sẽ không xuất hiện.

Đã build 0 warning/error và kiểm tra hai client WebSocket có cookie đăng nhập, CRUD SQL, broadcast tới client thứ hai, repeated update, DateOnly, search, validation và chặn anonymous. Form Razor thường/login/logout/antiforgery cũng qua kiểm tra. Chưa kiểm tra click/modal/phân trang bằng browser tự động; cần bấm thử trước demo. Các bản ghi thử đã được dọn, không reset database.
