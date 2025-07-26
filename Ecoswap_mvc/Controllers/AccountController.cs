using Microsoft.AspNetCore.Mvc;
using Ecoswap_mvc.Models;
using Ecoswap_mvc.Repository;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Ecoswap_mvc.Controllers
{
    //commit
    public class AccountController : Controller
    {
        private readonly IUserRepository _userRepository;

        public AccountController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var isValid = await _userRepository.ValidateUserAsync(model.Username, model.Password);
                if (isValid)
                {
                    var user = await _userRepository.GetUserByUsernameAsync(model.Username);
                    await _userRepository.UpdateLastLoginAsync(user.Id);

                    // Store user info in session
                    HttpContext.Session.SetString("UserId", user.Id.ToString());
                    HttpContext.Session.SetString("Username", user.Username);
                    HttpContext.Session.SetString("FullName", user.FullName ?? user.Username);

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Invalid username or password");
                }
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if username already exists
                if (await _userRepository.UsernameExistsAsync(model.Username))
                {
                    ModelState.AddModelError("Username", "Username already exists");
                    return View(model);
                }

                // Check if email already exists
                if (await _userRepository.EmailExistsAsync(model.Email))
                {
                    ModelState.AddModelError("Email", "Email already exists");
                    return View(model);
                }

                var user = new User
                {
                    Username = model.Username,
                    Email = model.Email,
                    Password = model.Password,
                    FullName = model.FullName,
                    PhoneNumber = model.PhoneNumber
                };

                await _userRepository.CreateUserAsync(user);

                // Auto-login after registration
                HttpContext.Session.SetString("UserId", user.Id.ToString());
                HttpContext.Session.SetString("Username", user.Username);
                HttpContext.Session.SetString("FullName", user.FullName ?? user.Username);

                return RedirectToAction("Index", "Home");
            }
            return View(model);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
} 