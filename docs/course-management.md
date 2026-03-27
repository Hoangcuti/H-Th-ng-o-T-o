# Tài liệu phương thức quản lý khóa học

Dựa trên mã nguồn hiện tại, các tính năng quản lý khóa học được triển khai chủ yếu trong `Controllers/CoursesController.cs` với các model liên quan trong thư mục `Models/`. Tài liệu này mô tả nhanh luồng và dữ liệu sử dụng cho từng hành động.

## Model liên quan
- `Course`: Id, CourseName, CategoryId/Category, LevelId/Level, tập hợp `Instructors`, `Classes`, `LearningProgresses`.
- `CourseInstructor`: ghép nhiều-nhiều giữa Course và User (giảng viên).
- `LearningProgress`: theo dõi tiến độ học của một `User` trên một `Course` với trường `Percent` (0–100).

## Hành động trong CoursesController
| Hành động | HTTP | Route | Mô tả | Phụ thuộc |
|-----------|------|-------|-------|-----------|
| `Index()` | GET  | `/Courses/Index` (mặc định `/Courses`) | Lấy toàn bộ khóa học, include `Category`, `Level`, `Instructors.User`; trả về view danh sách. | `TrainingContext.Courses` |
| `Details(int id)` | GET | `/Courses/Details/{id}` | Lấy khóa học theo `Id` kèm `Category`, `Level`, `Instructors.User`; 404 nếu không thấy; trả view chi tiết. | `TrainingContext.Courses` |
| `Enroll(int id)` | POST, `[Authorize]` | `/Courses/Enroll/{id}` | Người dùng đăng nhập đăng ký khóa học. Kiểm tra claim `NameIdentifier`; nếu chưa đăng nhập chuyển tới `/Account/Login`. Nếu chưa có bản ghi `LearningProgress` cho (UserId, CourseId) thì thêm với `Percent = 0` và lưu. Ghi `TempData["SuccessMessage"]` cho cả hai trường hợp (đăng ký mới hoặc đã đăng ký). Nếu user có role `Student` thì redirect tới `Student/CourseDetail?id={id}`, ngược lại quay về `Details`. | `TrainingContext.LearningProgress`, cookie auth, TempData |

## Luồng dữ liệu Enroll
1. Lấy `UserId` từ claim `NameIdentifier`; nếu rỗng → redirect login.
2. Kiểm tra tồn tại `LearningProgress` cho (UserId, CourseId).
3. Nếu chưa có: tạo mới với tiến độ 0%, lưu thay đổi, đặt thông báo thành công.
4. Nếu đã có: đặt thông báo đã đăng ký.
5. Điều hướng: `Student` → `Student/CourseDetail`; còn lại → `Courses/Details`.

## Ghi chú triển khai
- Tất cả hành động sử dụng `TrainingContext` với EF Core; các include đảm bảo sẵn dữ liệu liên quan cho view.
- Tạm thời không giới hạn vai trò cho Enroll ngoài yêu cầu đăng nhập; nếu cần chỉ cho `Student`, thêm `[Authorize(Roles = "Student")]`.
- Thông báo sử dụng `TempData["SuccessMessage"]`; cần render trong view tương ứng để hiển thị.

## Mở rộng/kiểm thử gợi ý
- Thêm kiểm tra giới hạn ghi danh (nếu có) và xử lý ngoại lệ từ `_context.SaveChanges()`.
- Viết unit test cho `Enroll` để xác nhận tạo `LearningProgress` và thông báo đúng khi đăng ký lần 2.
