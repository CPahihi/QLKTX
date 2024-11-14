using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using QLKTX.Models;
using QLKTX.Common;

namespace QLKTX.Areas.Admin.Controllers
{
    public class SinhViensController : Controller
    {
        private QLKTXEntities db = new QLKTXEntities();
        public ActionResult TaoTheLuuTru(int sinhVienID)
        {
            var sinhVien = db.SinhViens.FirstOrDefault(sv => sv.SinhVienID == sinhVienID);

            if (sinhVien == null)
            {
                return HttpNotFound("Không tìm thấy sinh viên.");
            }

            // Truy vấn thông tin đăng ký phòng của sinh viên từ bảng DKPhong
            var dkPhong = db.DKPhongs
                    .Where(dk => dk.SinhVienID == sinhVienID)
                    .OrderByDescending(dk => dk.NgayDK)
                    .ThenByDescending(dk => dk.DKPhongID)
                    .FirstOrDefault();

            if (dkPhong == null)
            {
                return HttpNotFound("Sinh viên chưa đăng ký phòng.");
            }
            var phong = (from p in db.Phongs
                         join tn in db.ToaNhas on p.ToaNhaID equals tn.ToaNhaID
                         where p.PhongID == dkPhong.PhongID
                         select new
                         {
                             TenToaNha = tn.TenToaNha,
                             TenPhong = p.TenPhong
                         }).FirstOrDefault();

            if (phong == null)
            {
                return HttpNotFound("Không tìm thấy thông tin phòng.");
            }
            // Lấy thông tin sinh viên từ csdl hoặc bất kỳ nguồn dữ liệu nào khác
            var theLuuTruViewModel = new TheLuuTruViewModel
            {
                HoTen = sinhVien.HoSV + " " + sinhVien.TenSV,
                MSSV = sinhVien.MSSV,
                NgaySinh = (DateTime)sinhVien.NgaySinh,
                GioiTinh = sinhVien.GioiTinh,
                Lop = sinhVien.Lop,
                Phong = phong.TenToaNha + "." + phong.TenPhong // K1.101
            };

            return View(theLuuTruViewModel);
        }
        public ActionResult FilterSinhViens(int? toaNhaId, int? phongId, string hoTen, string mssv)
        {
            // Bắt đầu với tất cả sinh viên
            var filteredSinhViens = db.SinhViens.AsQueryable();

            // Lọc theo tòa nhà
            if (toaNhaId != null)
            {
                filteredSinhViens = filteredSinhViens.Where(sv => sv.DKPhongs.Any(dp => dp.Phong.ToaNhaID == toaNhaId));
            }

            // Lọc theo phòng
            if (phongId != null)
            {
                filteredSinhViens = filteredSinhViens.Where(sv => sv.DKPhongs.Any(dp => dp.PhongID == phongId));
            }

            // Lọc theo họ và tên
            if (!string.IsNullOrEmpty(hoTen))
            {
                filteredSinhViens = filteredSinhViens.Where(sv => (sv.HoSV + " " + sv.TenSV).Contains(hoTen));
            }

            // Lọc theo MSSV
            if (!string.IsNullOrEmpty(mssv))
            {
                filteredSinhViens = filteredSinhViens.Where(sv => sv.MSSV.Contains(mssv));
            }

            // Trả về danh sách sinh viên đã lọc dưới dạng PartialView
            return PartialView("_SinhVienList", filteredSinhViens.ToList());
        }

        public string GetPhong(int sinhVienID)
        {
            var phongInfo = (from dk in db.DKPhongs
                             join p in db.Phongs on dk.PhongID equals p.PhongID
                             join t in db.ToaNhas on p.ToaNhaID equals t.ToaNhaID
                             where dk.SinhVienID == sinhVienID
                             orderby dk.NgayDK descending, dk.DKPhongID descending
                             select new { p.TenPhong, t.TenToaNha })
                             .FirstOrDefault();

            if (phongInfo != null)
            {
                return $"{phongInfo.TenToaNha}.{phongInfo.TenPhong}";
            }
            return "Chưa xếp";
        }

        public ActionResult XepPhong(int sinhVienID)
        {
            var viewModel = new XepPhongViewModel
            {
                SinhVienID = sinhVienID,
                ToaNhas = db.ToaNhas.Select(t => new SelectListItem
                {
                    Value = t.ToaNhaID.ToString(),
                    Text = t.TenToaNha
                }).ToList(),
                Phongs = new List<SelectListItem>() // Initialize as empty
            };

            return View(viewModel);
        }

        // POST: Admin/XepPhong
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult XepPhong(XepPhongViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Lấy thông tin phòng
                var phong = db.Phongs.FirstOrDefault(p => p.PhongID == model.SelectedPhongID);

                if (phong == null)
                {
                    TempData["ErrorMessage"] = "Không tồn tại phòng";
                    return RedirectToAction("Index", "SinhViens");
                }

                // Lấy số lượng người hiện tại đang ở trong phòng
                int soNguoiHienTai = db.DKPhongs.Count(dk => dk.PhongID == phong.PhongID && dk.NgayTra == null);

                // Kiểm tra số lượng người có vượt quá số lượng giường
                if (soNguoiHienTai >= phong.SoLuongGiuong)
                {
                    TempData["ErrorMessage"] = "Xếp phòng thất bại. Phòng đã đầy";
                    return RedirectToAction("Index", "SinhViens");
                }
                else
                {
                    var dkPhongCu = db.DKPhongs
                         .Where(dk => dk.SinhVienID == model.SinhVienID && dk.NgayTra == null)
                         .OrderByDescending(dk => dk.NgayDK)
                         .FirstOrDefault();

                    // Nếu tìm thấy bản ghi đăng ký phòng trước đó, cập nhật ngày trả
                    if (dkPhongCu != null)
                    {
                        dkPhongCu.NgayTra = DateTime.Now;
                    }

                    var dkPhong = new DKPhong
                    {
                        SinhVienID = model.SinhVienID,
                        PhongID = model.SelectedPhongID,
                        NgayDK = DateTime.Now
                    };

                    db.DKPhongs.Add(dkPhong);
                    db.SaveChanges();

                    TempData["SuccessMessage"] = "Xếp phòng thành công";
                    return RedirectToAction("Index", "SinhViens");
                }
            }

            // Nếu có lỗi, tải lại danh sách tòa nhà và phòng
            model.ToaNhas = db.ToaNhas.Select(t => new SelectListItem
            {
                Value = t.ToaNhaID.ToString(),
                Text = t.TenToaNha
            }).ToList();

            model.Phongs = db.Phongs.Where(p => p.ToaNhaID == model.SelectedToaNhaID).Select(p => new SelectListItem
            {
                Value = p.PhongID.ToString(),
                Text = p.TenPhong
            }).ToList();

            return View(model);
        }

        // Action lấy danh sách phòng dựa trên tòa nhà
        public JsonResult GetPhongsByToaNha(int toaNhaID)
        {
            var phongs = db.Phongs.Where(p => p.ToaNhaID == toaNhaID).Select(p => new
            {
                PhongID = p.PhongID,
                TenPhong = p.TenPhong
            }).ToList();

            return Json(phongs, JsonRequestBehavior.AllowGet);
        }


        // GET: Admin/SinhViens
        public ActionResult Index()
        {
            ViewBag.ToaNhas = new SelectList(db.ToaNhas, "ToaNhaID", "TenToaNha");

            // Initialize Phongs dropdown as empty
            ViewBag.Phongs = new SelectList(Enumerable.Empty<SelectListItem>(), "PhongID", "TenPhong");

            var sinhViens = db.SinhViens.Include(sv => sv.DKPhongs.Select(dp => dp.Phong.ToaNha)).ToList();
            return View(sinhViens);
        }

        // GET: Admin/SinhViens/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SinhVien sinhVien = db.SinhViens.Find(id);
            if (sinhVien == null)
            {
                return HttpNotFound();
            }
            return View(sinhVien);
        }

        // GET: Admin/SinhViens/Create
        public ActionResult Create()
        {
            ViewBag.TaiKhoanID = new SelectList(db.TaiKhoans, "TaiKhoanID", "TenDangNhap");
            return View();
        }

        // POST: Admin/SinhViens/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "SinhVienID,HoSV,TenSV,MSSV,Lop,GioiTinh,NgaySinh,NoiSinh,DiaChi,Email,SoDienThoai,SoCCCD,TaiKhoanID")] SinhVien sinhVien)
        {
            if (ModelState.IsValid)
            {
                db.SinhViens.Add(sinhVien);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.TaiKhoanID = new SelectList(db.TaiKhoans, "TaiKhoanID", "TenDangNhap", sinhVien.TaiKhoanID);
            return View(sinhVien);
        }

        // GET: Admin/SinhViens/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SinhVien sinhVien = db.SinhViens.Find(id);
            if (sinhVien == null)
            {
                return HttpNotFound();
            }
            ViewBag.TaiKhoanID = new SelectList(db.TaiKhoans, "TaiKhoanID", "TenDangNhap", sinhVien.TaiKhoanID);
            return View(sinhVien);
        }

        // POST: Admin/SinhViens/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "SinhVienID,HoSV,TenSV,MSSV,Lop,GioiTinh,NgaySinh,NoiSinh,DiaChi,Email,SoDienThoai,SoCCCD,TaiKhoanID")] SinhVien sinhVien)
        {
            if (ModelState.IsValid)
            {
                db.Entry(sinhVien).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.TaiKhoanID = new SelectList(db.TaiKhoans, "TaiKhoanID", "TenDangNhap", sinhVien.TaiKhoanID);
            return View(sinhVien);
        }

        // GET: Admin/SinhViens/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SinhVien sinhVien = db.SinhViens.Find(id);
            if (sinhVien == null)
            {
                return HttpNotFound();
            }
            return View(sinhVien);
        }

        // POST: Admin/SinhViens/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            SinhVien sinhVien = db.SinhViens.Find(id);
            db.SinhViens.Remove(sinhVien);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
