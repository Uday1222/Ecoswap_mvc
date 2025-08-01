using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EcoSwap.Models;
using Ecoswap_mvc.Repository;
using Microsoft.AspNetCore.Mvc;

namespace EcoSwap.Controllers
{
    public class ItemController : Controller
    {
        private readonly IItemRepository _itemRepository;
        private readonly IChatMessageRepository _chatMessageRepository;

        public ItemController(IItemRepository itemRepository, IChatMessageRepository chatMessageRepository)
        {
            _itemRepository = itemRepository;
            _chatMessageRepository = chatMessageRepository;
        }

        public async Task<IActionResult> Index()
        {
            var items = await _itemRepository.GetAllItemsAsync();
            return View(items.OrderByDescending(i => i.DatePosted).ToList());
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(string title, string description, bool isGiveaway, decimal? price, IFormFile image)
        {
            // Get current user from session
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString))
            {
                return RedirectToAction("Login", "Account");
            }

            if (!int.TryParse(userIdString, out int userId))
            {
                return RedirectToAction("Login", "Account");
            }

            string imagePath = null;
            if (image != null && image.Length > 0)
            {
                var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "temp");
                if (!Directory.Exists(uploads))
                    Directory.CreateDirectory(uploads); 
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                var filePath = Path.Combine(uploads, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    image.CopyTo(stream);
                }
                imagePath = "/uploads/temp/" + fileName;
            }
            var item = new Item
            {
                Title = title,
                Description = description,
                ImagePath = imagePath,
                IsGiveaway = isGiveaway,
                Price = isGiveaway ? null : price,
                DatePosted = DateTime.Now,
                UserId = userId // Store the user who posted the item
            };
            await _itemRepository.CreateItemAsync(item);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var item = await _itemRepository.GetItemByIdAsync(id);
            if (item == null)
                return NotFound();
            return View(item);
        }
    }
}