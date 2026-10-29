# ASM03 — Coach Blazor Interactive Server

Bài mới dựa trên ASM03 Quang: Entities → Repositories → Services → BlazorWebApp. `Components/Pages/Coaches.razor` dùng InteractiveServer, EditForm, DataAnnotationsValidator, tìm kiếm ba tiêu chí và jQuery pagination sau render.

Tại thư mục này:

```powershell
dotnet build FA26_PRN222_SE1905_ASM03_ThaiNH.slnx
dotnet run --project zCoach.BlazorWebApp.ThaiNH --no-launch-profile --urls http://localhost:5104
```

Mở `http://localhost:5104`. Chọn BlazorWebApp làm Startup Project nếu dùng Visual Studio.

## Login / Logout

- Khi chưa đăng nhập, trang chủ và `/coaches` chuyển tới `/Account/Login`.
- Dùng cùng tài khoản ASM01 trong bảng `System.UserAccount`: username, password đúng và `IsActive = true`.
- Đăng nhập thành công mở Coaches; thanh menu hiển thị username và nút **Logout**.
- Bấm Logout để xoá phiên và trở về Login. Mở lại `/coaches` sẽ yêu cầu đăng nhập.
- Cookie riêng `zCoach.Blazor.Session`, thời hạn 30 phút, có NameIdentifier / Name / Role giống ASM01. Form login/logout dùng POST và antiforgery token.

Trang tài khoản dùng Razor Pages để tạo/xoá cookie qua HTTP; phần quản lý Coach vẫn dùng Blazor Interactive Server và giữ giao diện hiện tại.

Demo: Create New → thử để trống FullName → nhập đúng và lưu → Edit hai lần liên tiếp → tìm FullName/Email/Phone → Clear → chọn 5 dòng/trang và chuyển trang → Delete Coach demo.

Giữ đầy đủ ngày sinh, liên hệ, địa chỉ, ảnh, kinh nghiệm, chứng chỉ, lương, ngày tuyển, trạng thái và chuyên môn của Coach. Bài này dùng SignalR nội bộ của Blazor; ASM01 dùng custom CRUD Hub.

ConnectionStrings mẫu nằm trong `zCoach.BlazorWebApp.ThaiNH/appsettings.json`; cấu hình máy cá nhân dùng `appsettings.Local.json` (được bỏ qua bởi Git).

Đã build thành công (0 warning, 0 error). Kiểm tra HTTP login/logout với SQL thật đã qua: chặn truy cập ẩn danh, tài khoản sai/rỗng, đăng nhập đúng, cookie riêng, tải Coaches có dữ liệu, logout, yêu cầu đăng nhập lại, return URL nội bộ và chặn POST thiếu antiforgery token. Không thay đổi dữ liệu khi kiểm tra tài khoản.

Tầng service/repository đã qua kiểm tra CRUD/validation trên SQL thật từ lần kiểm tra trước. **Chưa kiểm tra click trong circuit Blazor và phân trang bằng trình duyệt vì công cụ không tìm thấy Chrome.** Cần chạy kịch bản demo trên trước buổi chấm.
# Giao diện

UI dùng mẫu ASM01: menu zCoach App, nền trắng, Coach Roster có avatar và badge chuyên môn, bảng tiêu đề tối, form Create/Edit đơn giản. Phần CRUD vẫn chạy bằng Blazor Interactive Server. Đã build và kiểm tra HTTP login, roster, trang chủ, Privacy, tài nguyên CSS/JS và logout với SQL thật; chưa kiểm tra trực quan bằng trình duyệt.
