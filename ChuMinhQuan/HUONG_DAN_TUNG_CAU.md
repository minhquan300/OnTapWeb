# Hướng dẫn làm bài BookHub theo từng câu

Tài liệu này giải thích đề trong ảnh theo cấu trúc project hiện tại. Thay `NGUYỄN VĂN AN` bằng tên của bạn. Mở file trong **Solution Explorer**; có thể nhấn `Ctrl+;` để tìm file.

## Cấu trúc project

```text
Controllers/HomeController.cs
Models/Sach.cs, TheLoai.cs, QlnhaSachContext.cs
Views/_ViewStart.cshtml
Views/Cmq_layout.cshtml, Cmq_Header.cshtml, Cmq_Footer.cshtml
Views/Home/Index.cshtml, Create.cshtml
wwwroot/css/, wwwroot/js/, wwwroot/images/
```

---

## Câu 1 — Tạo project, layout, và đặt tài nguyên

### Tạo project

Trong Visual Studio: **Create a new project → ASP.NET Core Web App (Model-View-Controller)**. Đặt tên theo mẫu `HoTen_MaSV`. Project hiện tại tên `ChuMinhQuan`; nếu giảng viên chấm theo tên project, đổi tên trước khi nộp.

### 1.1 Tạo layout và tách phần

Trong `Views/Cmq_layout.cshtml`, tạo khung HTML chung, nạp CSS/JS và gọi header/footer. `@RenderBody()` là chỗ MVC chèn nội dung trang hiện tại:

```cshtml
<!DOCTYPE html>
<html lang="vi">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>BookHub</title>
    <link href="~/css/layout.css" rel="stylesheet" />
    <link href="~/css/style.css" rel="stylesheet" />
</head>
<body>
    <partial name="~/Views/Cmq_Header.cshtml" />
    <main class="main"><div class="container">@RenderBody()</div></main>
    <partial name="~/Views/Cmq_Footer.cshtml" />
    <script src="~/js/jquery.min.js"></script>
    <partial name="~/Views/Shared/_ValidationScriptsPartial.cshtml" />
    <script src="~/js/main.js"></script>
</body>
</html>
```

Viết header trong `Views/Cmq_Header.cshtml`, footer trong `Views/Cmq_Footer.cshtml`, nội dung trang chủ trong `Views/Home/Index.cshtml`. Chọn layout mặc định trong `Views/_ViewStart.cshtml`:

```cshtml
@{ Layout = "~/Views/Cmq_layout.cshtml"; }
```

`Views/Cmq_Maincontent.cshtml` là file mẫu cũ chưa được gọi; nội dung trang chủ hiện nằm ở `Index.cshtml` và được chèn qua `RenderBody()`.

### 1.2 Đặt CSS, JS, ảnh đúng vị trí

Trong Solution Explorer, đặt CSS vào `wwwroot/css`, JavaScript vào `wwwroot/js`, ảnh vào `wwwroot/images`. Trong Razor, tham chiếu với `~/`, ví dụ:

```cshtml
<img src="~/images/@item.Anh" alt="@item.TenSach" />
<link href="~/css/style.css" rel="stylesheet" />
<script src="~/js/main.js"></script>
```

Không ghi đường dẫn tuyệt đối như `C:\...` vào HTML.

---

## Câu 2 — Lấy thể loại từ database và hiện lên menu

### 2.1 Database First

Project đã có `QlnhaSachContext`, `TheLoai`, `Sach`. Nếu làm lại từ đầu, mở **Tools → NuGet Package Manager → Package Manager Console**, rồi scaffold database (cần cài các package EF Core SQL Server/Tools tương thích):

```powershell
Scaffold-DbContext "Server=DESKTOP-HRF5LSK\SQLEXPRESS;Database=QLNhaSach;Trusted_Connection=True;TrustServerCertificate=True;" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models -Context QlnhaSachContext
```

Không chạy scaffold lại lên model hiện tại nếu không muốn ghi đè các file. Chuỗi kết nối đang ở `appsettings.json` dưới `ConnectionStrings:DefaultConnection`; `Program.cs` đọc chuỗi này để đăng ký `QlnhaSachContext`.

### 2.2 Tạo menu từ bảng `TheLoai`

Trong `Controllers/HomeController.cs`, đọc bảng và gửi dữ liệu sang view:

```csharp
var theLoais = _context.TheLoais
    .AsNoTracking()
    .OrderBy(t => t.TenTl)
    .ToList();
ViewBag.TheLoai = theLoais;
```

Trong `Views/Cmq_Header.cshtml`, lặp danh sách. Mục Trang chủ đổi thành tên sinh viên:

```cshtml
<ul class="menu">
    <li><a asp-controller="Home" asp-action="Index">NGUYỄN VĂN AN</a></li>
    @foreach (var tl in ViewBag.TheLoai)
    {
        <li>
            <a href="@Url.Action("Index", "Home", new { maTl = tl.MaTl })"
               class="btn-theloai" data-id="@tl.MaTl">@tl.TenTl</a>
        </li>
    }
</ul>
```

Thay tên trong ví dụ bằng tên sinh viên của bạn. Project đã đọc và lặp thể loại; cần bảo đảm menu có mục tên sinh viên như trên.

---

## Câu 3 — Hiện sách có giá từ 100 trở lên

Trong `HomeController.Index`, lọc `Gia >= 100` (nghìn đồng), loại giá NULL, rồi tải danh sách trước khi trả view:

```csharp
var sachQuery = _context.Saches
    .AsNoTracking()
    .Where(s => s.Gia.HasValue && s.Gia.Value >= 100);

if (maTl.HasValue)
    sachQuery = sachQuery.Where(s => s.MaTl == maTl.Value);

var dsSach = sachQuery
    .OrderByDescending(s => s.MaSach)
    .Take(16)
    .ToList();

return View(dsSach);
```

`Take(16)` giữ trang chủ gọn và tránh tải bảng quá lớn. Đặt tiêu đề khu vực theo thể loại đang chọn; nếu chưa chọn thì dùng tên sinh viên:

```csharp
ViewBag.TenKhuVuc = maTl.HasValue
    ? theLoais.FirstOrDefault(t => t.MaTl == maTl.Value)?.TenTl ?? "SÁCH"
    : "NGUYỄN VĂN AN";
```

Trong `Views/Home/Index.cshtml`, tiêu đề dùng `<h3>@ViewBag.TenKhuVuc</h3>` và lặp `Model` để tạo thẻ sách. Hiện code đang bỏ sách NULL, nhưng cần đổi điều kiện thành `Gia >= 100` để đúng đề.

---

## Câu 4 — Lọc theo thể loại bằng AJAX, không tải lại trang

`LocSach` phải trả tên thể loại và danh sách sách phù hợp dưới dạng JSON. Trong `HomeController.cs`:

```csharp
[HttpGet]
public IActionResult LocSach(int maTl)
{
    var tenTheLoai = _context.TheLoais
        .Where(t => t.MaTl == maTl)
        .Select(t => t.TenTl)
        .FirstOrDefault();
    if (tenTheLoai == null) return NotFound();

    var sach = _context.Saches.AsNoTracking()
        .Where(s => s.MaTl == maTl && s.Gia.HasValue && s.Gia.Value >= 100)
        .OrderByDescending(s => s.MaSach)
        .Select(s => new { tenSach = s.TenSach, gia = s.Gia, anh = s.Anh })
        .ToList();

    return Json(new { tenTheLoai, sach });
}
```

Trong header, từng thể loại cần `class="btn-theloai"` và `data-id="@tl.MaTl"`. Sau đó nối đoạn dưới vào cuối `wwwroot/js/main.js` (giữ phần code hiện có):

```javascript
$(document).on('click', '.btn-theloai', function (event) {
    event.preventDefault();
    const maTl = $(this).data('id');

    $.getJSON('/Home/LocSach', { maTl: maTl })
        .done(function (data) {
            $('#tieu-de-khu-vuc').text(data.tenTheLoai);
            const $list = $('#danh-sach-sach').empty();
            if (!data.sach.length) {
                $list.append($('<p>').text('Chưa có sách trong thể loại này.'));
                return;
            }

            $.each(data.sach, function (_, item) {
                const $image = $('<img>').attr('src', '/images/' + item.anh)
                    .attr('alt', item.tenSach);
                const $footer = $('<div>').addClass('book-foot')
                    .append($('<span>').addClass('price').text(item.gia))
                    .append($('<button>').attr('type', 'button')
                        .addClass('btn-buy').text('Thêm vào giỏ'));
                const $card = $('<div>').addClass('book-card')
                    .append($('<a>').attr('href', '#').append($image))
                    .append($('<h2>').text(item.tenSach))
                    .append($footer);
                $list.append($card);
            });
        })
        .fail(function () { alert('Không tải được danh sách sách.'); });
});
```

AJAX chặn hành động điều hướng mặc định, gọi `LocSach`, rồi chỉ thay tiêu đề và danh sách sách. Hiện menu trong project vẫn điều hướng trang; nếu cần đạt đúng rubric AJAX, phải nối JavaScript như trên. (Bạn có thể giữ nguyên file JS cho tới khi quyết định triển khai phần này.)

---

## Câu 5 — Thêm sách và kiểm tra dữ liệu

### 5.1 Form, dropdown và lưu vào database

Trong `HomeController.cs`, GET nạp danh sách thể loại để đổ dropdown. POST kiểm tra `ModelState`, lưu sách và quay lại đúng thể loại:

```csharp
[HttpGet]
public IActionResult Create(int? maTl)
{
    PrepareCreateForm(maTl);
    return View(new Sach { MaTl = maTl ?? 0 });
}

[HttpPost]
[ValidateAntiForgeryToken]
public IActionResult Create(Sach sach)
{
    if (!_context.TheLoais.Any(t => t.MaTl == sach.MaTl))
        ModelState.AddModelError(nameof(sach.MaTl), "Vui lòng chọn thể loại hợp lệ.");

    if (!ModelState.IsValid)
    {
        PrepareCreateForm(sach.MaTl);
        return View(sach);
    }

    _context.Saches.Add(sach);
    _context.SaveChanges();
    return RedirectToAction(nameof(Index), new { maTl = sach.MaTl });
}
```

`PrepareCreateForm` tạo `SelectList`:

```csharp
private void PrepareCreateForm(int? maTlChon = null)
{
    var theLoais = _context.TheLoais.OrderBy(t => t.TenTl).ToList();
    ViewBag.TheLoai = theLoais;
    ViewBag.MaTlChon = maTlChon;
    ViewBag.TenKhuVuc = theLoais.FirstOrDefault(t => t.MaTl == maTlChon)?.TenTl
        ?? "THÊM SÁCH MỚI";
    ViewBag.MaTl = new SelectList(theLoais, "MaTl", "TenTl", maTlChon);
}
```

Trong `Views/Home/Create.cshtml`, dùng `asp-for` và `asp-validation-for`:

```cshtml
@model ChuMinhQuan.Models.Sach
<form asp-controller="Home" asp-action="Create" method="post">
    <div asp-validation-summary="ModelOnly"></div>
    <label asp-for="MaTl">MaTL</label>
    <select asp-for="MaTl" asp-items="ViewBag.MaTl">
        <option value="">-- Chọn thể loại --</option>
    </select>
    <span asp-validation-for="MaTl"></span>

    <label asp-for="TenSach">TenSach</label>
    <input asp-for="TenSach" />
    <span asp-validation-for="TenSach"></span>

    <label asp-for="Gia">Gia</label>
    <input asp-for="Gia" type="number" min="50" max="1000" step="1" />
    <span asp-validation-for="Gia"></span>

    <label asp-for="Anh">Anh</label>
    <input asp-for="Anh" />
    <span asp-validation-for="Anh"></span>
    <button type="submit" class="btn-buy">Create</button>
</form>
```

Trường `Anh` hiện nhận tên ảnh, ví dụ `Img_1.jpg`; file ảnh cần nằm trong `wwwroot/images`.

### 5.2 Data Annotations trong `Models/Sach.cs`

```csharp
[Required(ErrorMessage = "Tên sách không được để trống.")]
public string TenSach { get; set; } = null!;

[Required(ErrorMessage = "Giá không được để trống.")]
[Range(50, 1000, ErrorMessage = "Giá phải nằm trong giới hạn từ 50 đến 1000.")]
public decimal? Gia { get; set; }

[Required(ErrorMessage = "Tên file ảnh không được để trống.")]
[RegularExpression(
    @"^.+\.([jJ][pP][gG]|[jJ][pP][eE][gG]|[pP][nN][gG]|[wW][eE][bB][pP])$",
    ErrorMessage = "Tên file ảnh phải có đuôi .jpg, .jpeg, .png hoặc .webp.")]
public string? Anh { get; set; }
```

Để lỗi hiện ngay trên form, `Cmq_layout.cshtml` cần nạp jQuery trước partial validation:

```cshtml
<script src="~/js/jquery.min.js"></script>
<partial name="~/Views/Shared/_ValidationScriptsPartial.cshtml" />
```

Trong EF mapping của `Gia` giữ `.IsRequired(false)`: database có thể có sách cũ chưa nhập giá, còn `[Required]` vẫn buộc người dùng nhập giá khi tạo sách mới.

---

## Chạy thử và checklist trước khi nộp

Chọn project làm **Startup Project**, nhấn `F5` hoặc `Ctrl+F5`, rồi kiểm tra:

1. Trang chủ hiện layout, CSS và ảnh.
2. Menu có tên sinh viên và thể loại từ database.
3. Trang chủ chỉ hiện sách `Gia >= 100`; giá NULL không gây exception.
4. Bấm thể loại không tải lại toàn trang; tiêu đề và danh sách đổi.
5. Form thêm sách báo lỗi khi bỏ trống tên/giá/ảnh, giá ngoài 50–1000 hoặc đuôi ảnh sai.
6. Dữ liệu hợp lệ lưu được và quay về danh sách thể loại vừa chọn.

Build hiện tại thành công. Các điểm cần làm tiếp để khớp đầy đủ rubric là: thêm mục tên sinh viên vào menu, lọc đúng `Gia >= 100`, và triển khai AJAX cho Câu 4.
