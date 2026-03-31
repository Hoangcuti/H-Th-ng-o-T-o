/**********************************************************
 * CẬP NHẬT DỮ LIỆU THẬT - SILICON DREAM 
 * Gán video YouTube và nội dung bài giảng mẫu chuyên nghiệp
 **********************************************************/

-- 1. Xóa nội dung cũ để làm mới (Chỉ chạy nếu bạn muốn xóa dữ liệu cũ)
-- DELETE FROM LessonVideos;
-- DELETE FROM LessonContents;

-- 2. Cập nhật Video YouTube thật cho một số bài học (Ví dụ ID: 1, 2, 3...)
-- Bạn có thể thay đổi LessonID cho khớp với dữ liệu của bạn
INSERT INTO LessonVideos (LessonId, Url)
VALUES 
(1, 'https://www.youtube.com/watch?v=R0_SreY_n2M'), -- .NET Core Web API
(2, 'https://www.youtube.com/watch?v=BfEjDD8mWYg'), -- Entity Framework
(3, 'https://www.youtube.com/watch?v=L_Qpzd0pXnU'); -- ASP.NET Core MVC

-- 3. Cập nhật Nội dung bài giảng chuyên nghiệp (HTML)
INSERT INTO LessonContents (LessonId, Content)
VALUES 
(1, '<h4 class="text-primary">Kiến thức trọng tâm</h4><p>Trong bài học này, chúng ta sẽ khám phá nền tảng của <strong>ASP.NET Core</strong>. Đây là một khung công tác mã nguồn mở, đa nền tảng và hiệu năng cao.</p><ul><li>Cấu trúc Middleware</li><li>Hệ thống Dependency Injection</li><li>Cấu hình tệp appsettings.json</li></ul>'),
(2, '<h4 class="text-primary">Thực hành Entity Framework</h4><p>Entity Framework Core là một trình ánh xạ quan hệ đối tượng (O/RM) hiện đại cho .NET. Nó giúp đơn giản hóa việc làm việc với dữ liệu.</p><div class="bg-dark p-3 rounded" style="color: #0ea5e9;"><code>public DbSet&lt;Course&gt; Courses { get; set; }</code></div>'),
(3, '<h4 class="text-primary">Xây dựng ứng dụng MVC</h4><p>Mô hình Model-View-Controller (MVC) là một mẫu thiết kế phần mềm giúp phân tách các mối quan tâm của ứng dụng.</p><blockquote>Giao diện người dùng sẽ được cập nhật rực rỡ thông qua Silicon Neon UI.</blockquote>');

-- 4. Thêm Bài tập mẫu (Quiz) để nút "LÀM BÀI TẬP" hiện ra
-- Giả sử LessonId = 1 có bài tập
INSERT INTO Exams (CourseId, LessonId, Title, DurationMinutes, PassingScore, Description)
VALUES (1, 1, 'Kỹ năng nền tảng .NET Core', 15, 80, 'Kiểm tra kiến thức cơ bản sau bài học đầu tiên.');

PRINT 'DỮ LIỆU THẬT ĐÃ ĐƯỢC CẬP NHẬT THÀNH CÔNG!';
