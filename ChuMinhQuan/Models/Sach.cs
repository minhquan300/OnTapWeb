using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ChuMinhQuan.Models;

public partial class Sach
{
    public int MaSach { get; set; }

    public int MaTl { get; set; }
    [Required(ErrorMessage = "tên sách không được để trống")]

    public string TenSach { get; set; } = null!;
    [Required(ErrorMessage = "giá không đưọc để trống")]
    [Range(50, 1000, ErrorMessage = "Giá phải nằm trong giới hạn từ 50 đến 1000.")]
    public decimal? Gia { get; set; }
    [Required(ErrorMessage = "Tên file ảnh không được để trống.")]
    [RegularExpression(@"^.+\.([jJ][pP][gG]|[jJ][pP][eE][gG]|[pP][nN][gG]|[wW][eE][bB][pP])$", ErrorMessage = "Tên file ảnh phải có đuôi .jpg, .jpeg, .png hoặc .webp.")]
    public string? Anh { get; set; }

    public virtual TheLoai MaTlNavigation { get; set; } = null!;
}
