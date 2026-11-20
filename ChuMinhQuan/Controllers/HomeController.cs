using ChuMinhQuan.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ChuMinhQuan.Controllers
{
    public class HomeController : Controller
    {
        private readonly QlnhaSachContext _context;
        public HomeController(QlnhaSachContext context)
        {
            _context = context;
        }
        public IActionResult Index(int? maTl)
        {
            var theLoais = _context.TheLoais
                .AsNoTracking()
                .OrderBy(t => t.TenTl)
                .ToList();
            ViewBag.TheLoai = theLoais;
            ViewBag.MaTlChon = maTl;
            ViewBag.TenKhuVuc = maTl.HasValue
                ? theLoais.FirstOrDefault(t => t.MaTl == maTl.Value)?.TenTl ?? "SÁCH"
                : "CÔNG NGHỆ THÔNG TIN";

            var dsSachQuery = _context.Saches
                .AsNoTracking()
                .Where(s => s.Gia.HasValue);

            if (maTl.HasValue)
            {
                dsSachQuery = dsSachQuery.Where(s => s.MaTl == maTl.Value);
            }

            // Execute the query before rendering the Razor view, and keep the home page bounded.
            var dsSach = dsSachQuery
                .OrderByDescending(s => s.MaSach)
                .Take(16)
                .ToList();

            return View(dsSach);
        }
        [HttpGet]
        public IActionResult LocSach(int maTl)
        {
            var dsSach = _context.Saches.Where(s => s.MaTl == maTl).Select(s => new
            {
                tenSach = s.TenSach,
                gia = s.Gia,
                anh = s.Anh
            }).ToList();
            return Json(dsSach);
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
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
            var theLoaiTonTai = _context.TheLoais.Any(t => t.MaTl == sach.MaTl);
            if (!theLoaiTonTai)
            {
                ModelState.AddModelError(nameof(sach.MaTl), "Vui lòng chọn thể loại hợp lệ.");
            }

            if (!ModelState.IsValid)
            {
                PrepareCreateForm(sach.MaTl);
                return View(sach);
            }

            _context.Saches.Add(sach);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index), new { maTl = sach.MaTl });
        }

        private void PrepareCreateForm(int? maTlChon = null)
        {
            var theLoais = _context.TheLoais.OrderBy(t => t.TenTl).ToList();
            ViewBag.TheLoai = theLoais;
            ViewBag.MaTlChon = maTlChon;
            ViewBag.TenKhuVuc = theLoais.FirstOrDefault(t => t.MaTl == maTlChon)?.TenTl ?? "THÊM SÁCH MỚI";
            ViewBag.MaTl = new SelectList(theLoais, "MaTl", "TenTl", maTlChon);
        }
    }
}
