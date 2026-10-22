# PRN222 — ThaiNH assignments

Bốn bài dùng domain **CoachThaiNH / SpecializationThaiNH** và cấu trúc Entities → Repositories → Services → ứng dụng. Các solution chạy độc lập trên .NET 8.

| Bài | Startup project | Chức năng |
| --- | --- | --- |
| ASM01 | zCoach.MVCWebApp.ThaiNH | MVC CRUD, login, tìm kiếm, phân trang |
| ASM02 | zCoach.RazorWebApp.ThaiNH | Razor Pages + cookie login, CRUD thường và SignalR realtime |
| ASM03 | zCoach.BlazorWebApp.ThaiNH | Blazor Interactive Server CRUD, validation, tìm kiếm, phân trang |
| ASM04 | zCoach.WorkerService.ThaiNH | Background worker đồng bộ trạng thái Coach và log thống kê |

## Chạy trên máy cá nhân

1. Cài .NET 8 runtime và SDK hỗ trợ solution `.slnx` (SDK 9.0.200 trở lên hoặc SDK 10), SQL Server và database PRN222 theo schema bài học.
2. Mở `.slnx` trong thư mục bài bằng Visual Studio Community; chọn project ứng dụng trong bảng làm Startup Project.
3. Trong project ứng dụng, copy `appsettings.Local.example.json` thành **`appsettings.Local.json`**, sửa ConnectionStrings cho SQL Server của máy bạn. File local được Git bỏ qua; không đưa mật khẩu vào appsettings.json.
4. Chạy Ctrl+F5. Máy đang phát triển đã giữ cấu hình kết nối cũ trong file local nên không cần nhập lại.

Thứ tự cấu hình: appsettings mặc định → appsettings.Local.json → biến môi trường → tham số dòng lệnh. Để cấu hình qua môi trường, dùng `ConnectionStrings__DefaultConnection`.

Bảng tài khoản của schema ThaiNH hiện dùng là **`dbo.[System.UserAccount]`** (tên bảng chứa dấu chấm). Dùng tài khoản hiện có trong database; repository không cung cấp hoặc công bố mật khẩu tài khoản.

Hướng dẫn chạy, các bước test và giới hạn kiểm tra: [HUONG_DAN_DEMO_ASM_ThaiNH.md](HUONG_DAN_DEMO_ASM_ThaiNH.md). Nếu hướng dẫn hiển thị đường dẫn máy phát triển, thay phần thư mục gốc bằng nơi bạn clone repository.

## Kiểm tra

Cả bốn solution đã build 0 warning/error. Đã kiểm tra SQL/service, form Razor và SignalR có đăng nhập bằng hai client WebSocket. Kiểm tra click giao diện Blazor, modal và phân trang bằng browser vẫn cần thực hiện thủ công.

Mã kiểm tra tích hợp nằm trong `verification/AssignmentChecks`. Các kiểm tra dùng SQL Server thật, transaction rollback hoặc xóa bản ghi thử có tên riêng. Chỉ chạy với database phát triển và dừng app trước khi build lại để tránh khóa DLL.
