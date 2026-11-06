# ASM01 — Coach MVC + SignalR

Entities → Repositories → Services → MVCWebApp. Chọn zCoach.MVCWebApp.ThaiNH làm Startup Project trong Visual Studio Community, Ctrl+F5, đăng nhập rồi vào Coaches (/Coaches/Index).

## Giao diện CRUD

Mỗi chức năng có một nút chung: Create New Coach, Edit, Delete. Mở form/hộp xác nhận rồi chọn:

- Create / Edit / Delete: gửi HTTP POST tới MVC, tải lại trang ở tab thao tác. Tab khác giữ nguyên; phải F5 mới thấy dữ liệu mới. Controller không phát thông báo SignalR.
- Create by SignalR / Edit by SignalR / Delete by SignalR: gọi CoachHub qua kết nối hiện có. Server đẩy CoachChanged kèm dữ liệu/ID; cả hai tab cập nhật trực tiếp bảng, không reload hay gửi HTTP request lấy lại danh sách.

Details, tìm kiếm ba tiêu chí, phân trang, validation và login/logout giữ nguyên. Form MVC có antiforgery token; Hub yêu cầu đăng nhập. Cấu hình máy cá nhân nằm trong appsettings.Local.json (không đưa lên Git).

## Demo hai tab và Network

1. Mở hai cửa sổ, đăng nhập và vào Coaches, chờ Live updates connected.
2. Thao tác Create/Edit/Delete thường ở tab A: tab B không đổi. F5 tab B mới thấy kết quả.
3. Sau khi tải xong, mở Network ở cả hai tab và Clear.
4. Dùng Create/Edit/Delete by SignalR: cả hai tab tự đổi bảng, không thêm request document/fetch/XHR lấy danh sách. Xem thông điệp trong kết nối WS → Messages.
5. Ảnh ImageUrl ngoài có thể tải tài nguyên ảnh riêng, không phải HTTP CRUD.

Đã build 0 warning/error và kiểm tra trên hai cửa sổ Edge đủ sáu lựa chọn: luồng thường phải F5 ở tab nhận; luồng SignalR cập nhật trực tiếp không HTTP request trong CRUD mẫu. Đã kiểm tra SQL, WebSocket, validation và chặn anonymous; xoá dữ liệu thử sau kiểm tra.

Trang Details cũng kết nối CoachHub: khi tab khác Edit by SignalR, các trường của Coach đang xem tự cập nhật; khi Delete by SignalR, ẩn thông tin cũ và hiện thông báo đã xoá. Đã kiểm tra Details mở sẵn cập nhật tên/địa chỉ/lương và trạng thái xoá mà không F5 hoặc HTTP request mới. CRUD thường vẫn không broadcast.
