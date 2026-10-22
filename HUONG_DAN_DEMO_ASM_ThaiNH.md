# Hướng dẫn demo ASM01–ASM04 — ThaiNH

Cập nhật ngày 22/10/2026, đối chiếu `origin/main` repository Quang tại commit `abf7d2e` (`signalR hotfix`). Đã fetch và xác nhận main hiện tại. Giữ nghiệp vụ Coach/Specialization và giao diện riêng của ThaiNH.

| Bài | Công nghệ | Phần cần demo |
| --- | --- | --- |
| ASM01 | ASP.NET Core MVC | Login hiện có, CRUD Coach, tìm ba tiêu chí, phân trang; không có SignalR |
| ASM02 | Razor Pages + SignalR | Login, CRUD Razor Pages và CRUD SignalR cạnh nhau; dữ liệu đổi thì các tab nhận sự kiện và tự tải lại |
| ASM03 | Blazor Interactive Server | CRUD component, EditForm/validation, tìm kiếm, phân trang; giữ như trước |
| ASM04 | Worker Service | Scope mỗi chu kỳ, đồng bộ trạng thái Coach và log thống kê; giữ như trước |

SignalR mới của ASM02 là **CRUD realtime theo main Quang**, không phải trang chat cũ. ASM01 đã bỏ ChatHub, chat.js và đăng ký SignalR.

## Đường dẫn hiện tại

Cả bốn solution đều nằm dưới:

`D:\SE\Semester 7\PRN222\PRN222_Project`

Mở file `.slnx` trong thư mục `FA26_PRN222_SE1905_ASM01_ThaiNH` đến `FA26_PRN222_SE1905_ASM04_ThaiNH`. Mỗi solution gồm Entities, Repositories, Services và project ứng dụng tương ứng; chạy độc lập.

## Chạy bằng Visual Studio Community

1. Dừng chương trình đang debug bằng Shift+F5.
2. File → Open → Project/Solution → chọn `.slnx` của bài muốn chạy.
3. Chuột phải project ứng dụng → Set as Startup Project:
   - ASM01: `zCoach.MVCWebApp.ThaiNH`
   - ASM02: `zCoach.RazorWebApp.ThaiNH`
   - ASM03: `zCoach.BlazorWebApp.ThaiNH`
   - ASM04: `zCoach.WorkerService.ThaiNH`
4. Build → Rebuild Solution, sau đó Ctrl+F5.
5. Port do Visual Studio chọn có thể khác các port terminal bên dưới. Dùng địa chỉ thực tế trên trình duyệt.

SQL Server phải đang chạy. Cấu hình SQL riêng của máy nằm trong appsettings.Local.json của đúng project startup và được Git bỏ qua; appsettings.json chỉ chứa cấu hình mẫu. Máy mới cần copy appsettings.Local.example.json thành appsettings.Local.json rồi nhập kết nối SQL của mình. Database của bạn dùng `dbo.CoachThaiNH`, `dbo.SpecializationThaiNH`, và **`dbo.[System.UserAccount]`** (tên bảng có dấu chấm, không phải schema System). Dùng tài khoản đang có và IsActive=true trong database; không mặc định tài khoản Quang tồn tại ở database của bạn.

## Chạy bằng terminal

Tại `D:\SE\Semester 7\PRN222`:

```powershell
dotnet run --project ".\PRN222_Project\FA26_PRN222_SE1905_ASM01_ThaiNH\zCoach.MVCWebApp.ThaiNH" --no-launch-profile --urls http://localhost:5171
dotnet run --project ".\PRN222_Project\FA26_PRN222_SE1905_ASM02_ThaiNH\zCoach.RazorWebApp.ThaiNH" --no-launch-profile --urls http://localhost:5294
dotnet run --project ".\PRN222_Project\FA26_PRN222_SE1905_ASM03_ThaiNH\zCoach.BlazorWebApp.ThaiNH" --no-launch-profile --urls http://localhost:5104
dotnet run --project ".\PRN222_Project\FA26_PRN222_SE1905_ASM04_ThaiNH\zCoach.WorkerService.ThaiNH" --no-launch-profile
```

Mỗi ứng dụng dùng một terminal riêng. Dừng bằng Ctrl+C trước khi build lại để tránh khóa DLL. Các ứng dụng dùng chung database; dữ liệu sửa ở một bài sẽ được nhìn thấy ở các bài khác sau khi tải lại.

## Test ASM01 — MVC thông thường

1. Mở `/Account/Login`, đăng nhập rồi vào `/Coaches`. `/CoachThaiNh` vẫn là đường dẫn tương thích.
2. Create New Coach: tên `Demo Thai MVC`, email `demo@example.com`, điện thoại `0901234567`, lương `12000000`, Status=1, chọn chuyên môn. Các trường không bắt buộc có thể để trống.
3. Details → Edit lương thành `15000000` → Save → Details kiểm tra.
4. Search theo họ tên, email, điện thoại. Thêm một điều kiện không khớp để kiểm tra AND, rồi Clear.
5. Chuyển trang với phân trang gốc 5 dòng/trang.
6. Delete đúng bản ghi demo, Logout. Trang Home chỉ giới thiệu quản lý Coach, không có chat hay CRUD SignalR.

ASM01 giữ màn hình login của bạn; đây vẫn là luồng MVC bình thường. Mẫu Quang không có login ở ASM01, nên giao diện/login của bạn được giữ theo lựa chọn đã có, còn cấu trúc tầng và việc không dùng SignalR bám bản mới.

## Test ASM02 — Razor Pages + SignalR CRUD

1. Mở `/CoachThaiNhs` khi chưa login: phải chuyển tới `/Account/Login`.
2. Đăng nhập. Test CRUD thường trước: Create → Details → Edit → Delete một Coach demo, tìm theo ba trường và phân trang.
3. Mở **hai tab cùng `/CoachThaiNhs`**, giữ ứng dụng chạy. Hai tab có thể cùng dùng một tài khoản.
4. Đợi `Live updates connected.`; các nút SignalR được bật.
5. Tab A bấm **SignalR Create**, tên `Demo Thai SignalR`, email `signalr@example.com`, điện thoại `0901234567`, lương `12000000`, chọn chuyên môn. Lưu bằng **Create via SignalR**.
6. Cả hai tab nhận sự kiện `CoachChanged` và **tự tải lại** sau khoảng 0,7 giây. Không cần bấm F5. Coach mới phải xuất hiện nếu khớp bộ lọc đang đặt.
7. Tab B bấm **SignalR Edit** đúng Coach đó, đổi lương thành `15000000`, bấm **Update via SignalR**. Cả hai tab tự cập nhật.
8. Bấm **SignalR Delete** trên đúng Coach demo, xác nhận. Cả hai tab mất bản ghi. Đây là xóa dữ liệu SQL thật nên không dùng bản ghi thật để thử.
9. Thử tên rỗng/email sai/lương âm trong modal: phải báo lỗi; server cũng kiểm tra lại dữ liệu và khóa ngoại chuyên môn.
10. Logout: truy cập trang/hub khi chưa đăng nhập phải bị từ chối.

Luồng: `coaches-signalr.js → CoachHub → ICoachThaiNhService → CoachThaiNhRepository → PRN222Context → SQL Server`. Hub có `[Authorize]`, ánh xạ DTO và phát `Clients.All.SendAsync("CoachChanged", ...)`. Chức năng này lưu dữ liệu SQL; khác với chat tạm thời trước đây. CRUD form thường vẫn dùng OnGetAsync/OnPostAsync, antiforgery và partial _FormFields.

## Test ASM03 — giữ như trước

Mở `/coaches`: Create New → thử FullName rỗng để thấy validation → nhập đúng và lưu → Edit hai lần liên tiếp → tìm ba tiêu chí → chọn 5 dòng/trang và chuyển trang → Delete bản ghi demo. Dùng InteractiveServer, EditForm và DataAnnotationsValidator. Blazor có kết nối SignalR nội bộ cho circuit, không có custom CoachHub như ASM02.

## Test ASM04 — giữ như trước

Chạy worker, quan sát log thống kê. Chu kỳ mặc định 30 giây, tối thiểu 5 giây. Tạo Coach demo riêng ở web với `Status=0, IsActive=true`; sau một lượt worker phải chuyển IsActive=false và cập nhật UpdateAt. Lượt sau không xử lý lại cùng bản ghi. Dừng worker và xóa Coach demo.

Quy tắc này thích ứng cho Coach vì Coach không có EndDate như MembershipFee của Quang. Bật/tắt bằng Worker:DeactivateStoppedCoaches. Thống kê gồm tổng Coach, active/inactive, số Status=0, tổng lương, lương Coach active và số chuyên môn. Chưa có rubric gốc để khẳng định yêu cầu chấm cụ thể.

## Kiến trúc cần giải thích

- Entities: model Coach, Specialization, tài khoản và summary; validation bằng DataAnnotations.
- Repositories: GenericRepository và truy vấn EF, Include chuyên môn, tìm kiếm AND theo ba điều kiện.
- Services: nghiệp vụ, kiểm tra chuyên môn tồn tại, timestamp, tránh cập nhật lan sang navigation.
- Web/Worker: constructor DI qua interface, không tự tạo repository/context. Các thao tác database dùng async/await.
- Worker: IServiceScopeFactory tạo scope mới mỗi lượt, không giữ DbContext scoped trong singleton.

## Trạng thái kiểm tra 22/10/2026

- Cả bốn solution build thành công: 0 warning/error.
- ASM01: trang Home/login tải được, không còn nội dung chat; endpoint `/chatHub/negotiate` trả 404.
- ASM02: 2 client WebSocket có cookie đăng nhập đã qua CRUD, đọc đầy đủ field/date, sửa lặp lại, tìm ba điều kiện, nhận thông báo create/update/delete ở client thứ hai.
- Hub đã kiểm tra từ chối chưa đăng nhập, tên rỗng, lương âm, ID lỗi và chuyên môn không tồn tại; bản ghi thử đã xóa, số Coach được giữ nguyên.
- SQL/service và form Razor thường: CRUD, DateOnly, validation, antiforgery, login/logout đều qua kiểm tra.
- JS kiểm tra cú pháp thành công. Chưa xác nhận click/modal/phân trang bằng trình duyệt tự động; cần bấm thử kịch bản hai tab trước buổi demo. ASM03 UI vẫn cần kiểm tra thủ công.
- Chưa commit/push. Đã sao lưu source ASM01/02 trước lần chuyển này trong `assignment-backups/before-signalr-move-2026-10-22T10-28-21-218Z`.

Nếu báo MSB3021/MSB3026 hoặc file đang được dùng: dừng chương trình bằng Shift+F5 hoặc Ctrl+C ở terminal chạy bài đó trước khi Rebuild. Không reset/drop database để xử lý lỗi demo.
