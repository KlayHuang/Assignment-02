using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Assignment_02.Models;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Assignment_02.Controllers
{
    public class ProductsController : Controller
    {
        private readonly AdventureWorksLT2022Context _context;
        private readonly ILogger<ProductsController> _logger;
        
        public ProductsController(AdventureWorksLT2022Context context, ILogger<ProductsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Products
        public async Task<IActionResult> Index(string searchTermName, string searchTermProductNumber, string searchTermColor, int page)
        {
            try
            {
                if (page < 1)
                {
                    page = 1;
                }
                int pageSize = 10;

                IQueryable<Product> productsQuery = _context.Product;
                if (!string.IsNullOrEmpty(searchTermName))
                {
                    searchTermName = searchTermName.Trim();
                    productsQuery = productsQuery.Where(p => p.Name.Contains(searchTermName));
                    ViewBag.SearchTermName = searchTermName;
                }

                if (!string.IsNullOrEmpty(searchTermProductNumber))
                {
                    searchTermProductNumber = searchTermProductNumber.Trim();
                    productsQuery = productsQuery.Where(p => p.ProductNumber.Contains(searchTermProductNumber));
                    ViewBag.SearchTermProductNumber = searchTermProductNumber;
                }

                if (!string.IsNullOrEmpty(searchTermColor))
                {
                    searchTermColor = searchTermColor.Trim();
                    productsQuery = productsQuery.Where(p => p.Color.Contains(searchTermColor));
                    ViewBag.SearchTermColor = searchTermColor;
                }

                int totalProducts = await productsQuery.CountAsync();
                int totalPages = (int)Math.Ceiling((double)totalProducts / pageSize);

                if (page > totalPages && totalPages > 0)
                {
                    page = totalPages;
                }

                var products = await productsQuery
                    .OrderBy(p => p.ProductID)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;

                if (totalProducts == 0)
                    ViewBag.AlertMessage = "無符合搜尋條件的產品!!";

                return View(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
            return View("Error");
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Product
                .Include(p => p.ProductCategory)
                .Include(p => p.ProductModel)
                .FirstOrDefaultAsync(m => m.ProductID == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            ViewData["ProductCategoryID"] = new SelectList(_context.ProductCategory, "ProductCategoryID", "Name");
            ViewData["ProductModelID"] = new SelectList(_context.ProductModel, "ProductModelID", "Name");
            return View();
        }

        // POST: Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product)
        {
            if (ModelState.IsValid)
            {
                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProductCategoryID"] = new SelectList(_context.ProductCategory, "ProductCategoryID", "Name", product.ProductCategoryID);
            ViewData["ProductModelID"] = new SelectList(_context.ProductModel, "ProductModelID", "Name", product.ProductModelID);
            return View(product);
        }

        // GET: Products/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Product.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            ViewData["ProductCategoryID"] = new SelectList(_context.ProductCategory, "ProductCategoryID", "Name", product.ProductCategoryID);
            ViewData["ProductModelID"] = new SelectList(_context.ProductModel, "ProductModelID", "Name", product.ProductModelID);

            //image
            if (product.ThumbNailPhoto != null)
            {
                ViewData["ThumbnailPhotoBase64"] = Convert.ToBase64String(product.ThumbNailPhoto);
            }

            return View(product);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,  Product product, IFormFile ThumbNailPhoto = null)
        {
            if (id != product.ProductID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    product.ModifiedDate = DateTime.Now;

                    if (ThumbNailPhoto != null && ThumbNailPhoto.Length > 0)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            await ThumbNailPhoto.CopyToAsync(memoryStream);
                            product.ThumbNailPhoto = memoryStream.ToArray();
                        }
                        product.ThumbnailPhotoFileName = ThumbNailPhoto.FileName;
                    }
                    else
                    {
                        var existingProduct = await _context.Product.AsNoTracking().FirstOrDefaultAsync(p => p.ProductID == id);
                        if (existingProduct != null)
                        {
                            product.ThumbNailPhoto = existingProduct.ThumbNailPhoto;
                            product.ThumbnailPhotoFileName = existingProduct.ThumbnailPhotoFileName;
                        }
                    }

                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                    if (!ProductExists(product.ProductID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            else
            {
                /*
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var error in errors)
                {
                    Console.WriteLine(error.ErrorMessage);
                }
                */
                if (ModelState.ContainsKey("ThumbNailPhoto"))
                {
                    var thumbNailPhotoState = ModelState["ThumbNailPhoto"];
                    if (thumbNailPhotoState!.Errors.Any())
                    {
                        var existingProduct = await _context.Product.AsNoTracking().FirstOrDefaultAsync(p => p.ProductID == id);
                        if (existingProduct != null)
                        {
                            product.ThumbNailPhoto = existingProduct.ThumbNailPhoto;
                            product.ThumbnailPhotoFileName = existingProduct.ThumbnailPhotoFileName;
                        }
                    }
                }
            }
            ViewData["ProductCategoryID"] = new SelectList(_context.ProductCategory, "ProductCategoryID", "Name", product.ProductCategoryID);
            ViewData["ProductModelID"] = new SelectList(_context.ProductModel, "ProductModelID", "Name", product.ProductModelID);
            return View(product);
        }

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Product
                .Include(p => p.ProductCategory)
                .Include(p => p.ProductModel)
                .FirstOrDefaultAsync(m => m.ProductID == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Product.FindAsync(id);
            if (product != null)
            {
                _context.Product.Remove(product);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Product.Any(e => e.ProductID == id);
        }
    }

}
