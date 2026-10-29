# ASM02 — Coach Razor Pages

Kiến trúc Entities → Repositories → Services → RazorWebApp. Bài này chỉ dùng Razor Pages CRUD, không tích hợp SignalR. Giữ giao diện giống ASM01 và login/logout cookie hiện có.

Visual Studio Community: mở solution `.slnx`, chọn `zCoach.RazorWebApp.ThaiNH` làm Startup Project, Ctrl+F5. Đăng nhập bằng tài khoản SQL đang hoạt động rồi vào `/CoachThaiNhs`.

```powershell
dotnet build FA26_PRN222_SE1905_ASM02_ThaiNH.slnx
dotnet run --project zCoach.RazorWebApp.ThaiNH --no-launch-profile --urls http://localhost:5294
```

## Demo

1. Login → Coaches → Create New Coach.
2. Thử bỏ trống FullName để xem validation, rồi nhập đúng và lưu.
3. Details → Edit tên/lương → Save.
4. Search theo tên/email/phone, Clear và thử phân trang.
5. Delete đúng Coach demo → Logout.

Các form xử lý HTTP GET/POST qua PageModel; lưu xong chuyển về danh sách. Nếu mở hai cửa sổ thì cửa sổ thứ hai cần F5 để thấy dữ liệu mới. Phần realtime nằm ở ASM01.

Cấu hình máy cá nhân nằm trong `appsettings.Local.json` (không đưa lên Git). Đã gỡ Hub, endpoint `/coachHub`, JavaScript và toàn bộ nút/popup SignalR khỏi ASM02.

Kiểm tra: build 0 warning/error; login/logout, validation, Create/Details/Edit/Search/Delete với SQL thật đã qua. Đã xác nhận endpoint Hub và client script cũ trả 404. Dữ liệu thử đã xoá. Chưa kiểm tra click/phân trang trực quan bằng trình duyệt.
