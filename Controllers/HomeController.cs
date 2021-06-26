using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDbApp.Models;
using MongoDbApp.Services;
using MongoDbApp.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MongoDbApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ProductService _db;

        public HomeController(ProductService service)
        {
            _db = service;
        }

        public async Task<IActionResult> Index(FilterViewModel filter)
        {
            IEnumerable<Product> products = await _db.GetProducts(filter);
            IndexViewModel viewModel = new(filter, products);

            return View(viewModel);
        }

        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(Product product)
        {
            if (ModelState.IsValid is true)
            {
                await _db.Create(product);
                return RedirectToAction("Index");
            }

            return View(product);
        }

        public async Task<IActionResult> Edit(string id)
        {
            Product product = await _db.GetProduct(id);

            if (product is null) return NotFound();

            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Product product)
        {
            if (ModelState.IsValid is true)
            {
                await _db.Update(product);
                return RedirectToAction("Index");
            }

            return View(product);
        }

        public async Task<IActionResult> Delete(string id)
        {
            await _db.Remove(id);

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteImage(string id)
        {
            await _db.DeleteImage(id);

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> GetImage(string id)
        {
            var image = await _db.GetImage(id);
            if (image is null) return NotFound();

            return File(image, "image/png");
        }

        public async Task<IActionResult> AttachImage(string id)
        {
            Product product = await _db.GetProduct(id);
            if (product is null) return NotFound();

            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> AttachImage(string id, IFormFile uploadedFile)
        {
            if (uploadedFile is not null)
            {
                await _db.StoreImage(id, uploadedFile.OpenReadStream(), uploadedFile.FileName);
            }

            return RedirectToAction("Index");
        }
    }
}