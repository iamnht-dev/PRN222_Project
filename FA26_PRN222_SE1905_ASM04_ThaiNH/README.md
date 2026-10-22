# ASM04 — Windows Service + file .txt (theo mẫu Lab4)

Bản này giữ các tầng Coach Entities → Repositories → Services nhưng thực hiện tác vụ giống `PRN222_Lab4_80463`: mỗi **5 giây** đọc danh sách Coach từ SQL và ghi nối tiếp JSON có timestamp vào file **ThaiNH_SE1905_WS.txt**. Chạy được dạng console/F5 và hỗ trợ Windows Service tên **ThaiNH_SE1905_WS**.

Mặc định chỉ đọc SQL và ghi file; không tự sửa trạng thái Coach. Chức năng maintenance cũ vẫn có thể bật bằng Worker:DeactivateStoppedCoaches=true.

## 1. Test nhanh bằng Visual Studio Community

1. Mở `FA26_PRN222_SE1905_ASM04_ThaiNH.slnx`.
2. Chuột phải `zCoach.WorkerService.ThaiNH` → Set as Startup Project.
3. Ctrl+F5, chờ khoảng 10 giây.
4. Console phải báo `Appended ... Coaches to ...ThaiNH_SE1905_WS.txt` mỗi 5 giây.
5. Mở file bằng Notepad. Khi chạy Debug, file mặc định ở:

`zCoach.WorkerService.ThaiNH\bin\Debug\net8.0\Logs\ThaiNH_SE1905_WS.txt`

Mỗi dòng gồm timestamp và JSON danh sách Coach. Đóng/mở lại hoặc tải lại Notepad để thấy dòng mới. Thêm/sửa một Coach bằng web app, chờ lượt kế tiếp để thấy dữ liệu cập nhật trong file. Dừng worker trước khi Rebuild.

## 2. Publish

Tại thư mục ASM04:

```powershell
dotnet publish .\zCoach.WorkerService.ThaiNH\zCoach.WorkerService.ThaiNH.csproj -c Release -o .\publish
```

Trong `publish` phải có `.exe`, DLL và các file appsettings. Giữ `appsettings.Local.json` chứa kết nối SQL riêng của máy; file này không lên GitHub. Sau khi clone ở máy khác, copy appsettings.Local.example.json thành appsettings.Local.json và điền SQL của máy đó.

Có thể chạy thử `publish\zCoach.WorkerService.ThaiNH.exe` trước khi cài service. File khi chạy bản publish là:

`publish\Logs\ThaiNH_SE1905_WS.txt`

App đọc cấu hình tương đối với thư mục executable, không phụ thuộc thư mục hiện tại của Windows Service.

## 3. Cài và demo trong Windows Services

Đăng ký service làm thay đổi hệ thống, cần PowerShell chạy **Run as administrator**. Chỉ thực hiện sau khi đã quyết định cài. Script đặt Startup Type=Manual, không tự khởi động khi bật máy, và không ghi đè service đã có cùng tên.

```powershell
cd "D:\SE\Semester 7\PRN222\PRN222_Project\FA26_PRN222_SE1905_ASM04_ThaiNH"
.\Install-Service.ps1
```

Script tạo nguồn Event Log trong Application, đăng ký `ThaiNH_SE1905_WS` trỏ đến executable trong publish, rồi Start service. Không di chuyển/xóa thư mục publish sau khi đã cài.

Mở Start → tìm **Services** → mở ứng dụng → tìm **ThaiNH_SE1905_WS**. Khi demo:

1. Start service, Status phải Running.
2. Mở `publish\Logs\ThaiNH_SE1905_WS.txt`, chờ khoảng 5–10 giây để thấy thêm dòng.
3. Stop service trong Services, đợi khoảng 10 giây: file phải ngừng tăng.
4. Start lại: file phải được ghi nối tiếp, không xóa dữ liệu cũ.
5. Có thể mở Event Viewer → Windows Logs → Application, lọc Source=ThaiNH_SE1905_WS.

Tên service trong appsettings.json và Install-Service.ps1 phải giống nhau. Service mặc định chạy bằng LocalSystem. Với cấu hình SQL Authentication hiện tại, service đọc appsettings.Local.json; nếu dùng Windows Authentication thì phải cấp quyền SQL cho tài khoản chạy service. Không đổi mật khẩu/cấp quyền SQL chỉ để thử nếu chưa hiểu tài khoản đó.

## 4. Gỡ đăng ký sau khi demo nếu cần

Trong PowerShell Administrator tại thư mục ASM04:

```powershell
.\Uninstall-Service.ps1
```

Script chỉ Stop và xóa đăng ký service tên ThaiNH_SE1905_WS; giữ nguyên publish, file .txt và Event Log. Không xóa database.

## Cấu hình

```json
"Worker": {
  "IntervalSeconds": 5,
  "FilePath": "Logs/ThaiNH_SE1905_WS.txt",
  "DeactivateStoppedCoaches": false
},
"WindowsService": {
  "ServiceName": "ThaiNH_SE1905_WS"
}
```

FilePath tương đối được tính từ thư mục executable. Logs và publish được Git bỏ qua. JSON dùng IgnoreCycles để tránh vòng lặp navigation, không ghi các field null, giống mục tiêu mẫu Lab4.

Build/publish đã qua kiểm tra; bản executable đã đọc SQL thật và ghi nhiều dòng JSON hợp lệ với đầy đủ 13 Coach hiện tại. Ngày 22/10/2026 đã cài thật service ThaiNH_SE1905_WS và kiểm tra qua Windows Service Control Manager: Running thì file tăng, Stopped thì file giữ nguyên, Start lại thì ghi nối tiếp. Event Log có thông tin chạy và không có error trong các event kiểm tra. Sau kiểm tra để service Stopped/Manual để chủ động Start khi demo.
