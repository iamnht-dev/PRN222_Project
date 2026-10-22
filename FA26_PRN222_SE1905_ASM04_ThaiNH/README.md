# ASM04 — Coach Worker Service

Cấu trúc theo ASM04 Quang: Entities → Repositories → Services → WorkerService. `CoachWorker : BackgroundService`, options từ cấu hình, IServiceScopeFactory tạo scope mới mỗi chu kỳ, gọi `ICoachMaintenanceService`, xử lý lỗi và dừng qua CancellationToken.

Tại thư mục này:

```powershell
dotnet build FA26_PRN222_SE1905_ASM04_ThaiNH.slnx
dotnet run --project zCoach.WorkerService.ThaiNH --no-launch-profile
```

Chọn WorkerService làm Startup Project nếu dùng Visual Studio. Không có trang web. Dừng bằng Ctrl+C.

Trong `zCoach.WorkerService.ThaiNH/appsettings.json`:

```json
"Worker": {
  "IntervalSeconds": 30,
  "DeactivateStoppedCoaches": true
}
```

Coach không có EndDate. Quy tắc thích ứng: Coach có **Status=0 và IsActive=true** được chuyển IsActive=false, cập nhật UpdateAt. Mỗi lượt log tổng Coach, active/inactive, số ngừng hoạt động, tổng lương/ lương Coach active và số chuyên môn. Chu kỳ tối thiểu 5 giây.

Demo: chạy worker → tạo một Coach demo riêng ở web app, Status=0, Active=true → chờ chu kỳ → kiểm tra log Deactivated và reload thấy Active=false → chờ lượt tiếp theo không xử lý lại → dừng worker → xóa Coach demo qua web. Không đổi Coach thật chỉ để thử.

Đã build và chạy thật ở chế độ thống kê, đọc đúng SQL Server. Logic đồng bộ đã kiểm tra trong transaction rồi rollback. Quy tắc Status=0 là quy ước được chọn cho bài Coach do chưa có rubric gốc; không khẳng định đây là yêu cầu chấm cụ thể của giảng viên.
