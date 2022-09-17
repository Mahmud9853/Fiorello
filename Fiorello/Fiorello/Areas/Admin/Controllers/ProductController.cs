using Fiorello.DAL;
using Fiorello.Helpers;
using Fiorello.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Fiorello.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _env;

        public ProductController(AppDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }
        public async Task<IActionResult> Index()
        {
            List<Product> products =await _db.Products.ToListAsync();
            return View(products);
        }
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _db.Category.ToListAsync();
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product,int categoryId)
        {
            ViewBag.Categories = await _db.Category.ToListAsync();

            if (!ModelState.IsValid)
            {
                return View();
            }
            bool IsExist = await _db.Products.AnyAsync(x=>x.Name==product.Name);
            if (IsExist)
            {
                ModelState.AddModelError("Name", "Error Name");
                return View();

            }
            if (product.Photo == null)
            {
                ModelState.AddModelError("Photo", "Error");
                return View();
            }
            if (!product.Photo.IsImage())
            {
                ModelState.AddModelError("Photo", "Error Photo");
                return View();
            }
            //if (!slider.Photo.IsOlder1Mb())
            //{
            //    ModelState.AddModelError("Photo", "MAX 5Mb");
            //    return View();
            //}
            string folder = Path.Combine(_env.WebRootPath, "img");
            product.Image = await product.Photo.SaveFileAsync(folder);

            product.CategoryId = categoryId;

            await _db.Products.AddAsync(product);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Update(int? id)
        {
            ViewBag.Categories = await _db.Category.ToListAsync();
            if (id == null)
            {
                return NotFound();
            }
            Product dbProducts = await _db.Products.FirstOrDefaultAsync(x => x.Id == id);
            if (dbProducts == null)
            {
                return BadRequest();
            }
            return View(dbProducts);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int? id,Product product,int categoryId)
        {
            ViewBag.Categories = await _db.Category.ToListAsync();
            if (id==null)
            {
                return NotFound();

            }
            Product dbProducts = await _db.Products.FirstOrDefaultAsync(x => x.Id == id);
            if (dbProducts==null)
            {
                return BadRequest();

            }
            if (!ModelState.IsValid)
            {
                return View(dbProducts);
            }
            bool IsExist =await _db.Products.AnyAsync(x=>x.Name == product.Name && x.Id !=id);
            if (IsExist)
            {
                ModelState.AddModelError("Name", "Error Name");
                return View(dbProducts);
            }
            if (product.Photo != null)
            {
                if (!product.Photo.IsImage())
                {
                    ModelState.AddModelError("Photo", "Error Photo");
                    return View(dbProducts);
                }
                string folder = Path.Combine(_env.WebRootPath, "img");
                dbProducts.Image = await product.Photo.SaveFileAsync(folder);
            }
            dbProducts.CategoryId = categoryId;
            dbProducts.Name = product.Name;
            dbProducts.Price = product.Price;
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Activity(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            Product product = await _db.Products.FirstOrDefaultAsync(x => x.Id == id);
            if (product==null)
            {
                return BadRequest();
            }
            if (product.IsDeactive)
            {
                product.IsDeactive = false;
            }
            else
            {
                product.IsDeactive = true;
            }
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Detail(int? id)
        {
            ViewBag.Categories = await _db.Category.ToListAsync();
            if (id == null)
            {
                return NotFound();

            }
            Product product = await _db.Products.FirstOrDefaultAsync(x=>x.Id==id);
            if (product == null)
            {
                return BadRequest();
            }
            return View(product);
        }
    }
}
