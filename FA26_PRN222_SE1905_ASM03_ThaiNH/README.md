# ASM03 — Coach Blazor Interactive Server

Bài mới dựa trên ASM03 Quang: Entities → Repositories → Services → BlazorWebApp. `Components/Pages/Coaches.razor` dùng InteractiveServer, EditForm, DataAnnotationsValidator, tìm kiếm ba tiêu chí và jQuery pagination sau render.

Tại thư mục này:

```powershell
dotnet build FA26_PRN222_SE1905_ASM03_ThaiNH.slnx
dotnet run --project zCoach.BlazorWebApp.ThaiNH --no-launch-profile --urls http://localhost:5104
```

Mở `http://localhost:5104/coaches`. Chọn BlazorWebApp làm Startup Project nếu dùng Visual Studio.

Demo: Create New → thử để trống FullName → nhập đúng và lưu → Edit hai lần liên tiếp → tìm FullName/Email/Phone → Clear → chọn 5 dòng/trang và chuyển trang → Delete Coach demo.

Giữ đầy đủ ngày sinh, liên hệ, địa chỉ, ảnh, kinh nghiệm, chứng chỉ, lương, ngày tuyển, trạng thái và chuyên môn của Coach. Bài này dùng SignalR nội bộ của Blazor, không dùng custom CRUD Hub như ASM01.

ConnectionStrings nằm trong `zCoach.BlazorWebApp.ThaiNH/appsettings.json`, dùng database hiện có của bạn.

Đã build thành công; tầng service/repository đã qua kiểm tra CRUD/validation trên SQL thật. Trang prerender và tài nguyên JS trả HTTP 200. **Chưa kiểm tra click trong circuit Blazor và phân trang bằng trình duyệt vì môi trường không có trình duyệt khả dụng.** Cần chạy kịch bản demo trên trước buổi chấm.
