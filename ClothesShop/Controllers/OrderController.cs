using System.Linq;
using ClothesShop.Models;
using ClothesShop.Repository;
using Microsoft.AspNetCore.Mvc;
using ClothesShop.Infrastructure;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using ClothesShop.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using ClothesShop.Services;
using Microsoft.EntityFrameworkCore;

namespace ClothesShop.Controllers
{
    public class OrderController : Controller
    {
        private IOrderRepository repository;
        private readonly IUserProfileRepository userProfileRepository;
        private Cart cart;

        private UserManager<IdentityUser> userManager;
        private readonly ISessionManager sessionManager;
        private readonly IEmailService emailService;

        public OrderController(UserManager<IdentityUser> userMgr, IOrderRepository repoService, IUserProfileRepository userProfileRepository, Cart cartService,
            ISessionManager sessionManager, IEmailService emailService)
        {
            userManager = userMgr;
            repository = repoService;
            this.userProfileRepository = userProfileRepository;
            cart = cartService;
            this.sessionManager = sessionManager;
            this.emailService = emailService;
        }

        [HttpGet]
        [Route("order/edit/{orderId:int}")]
        public IActionResult EditOrder(int orderId)
        {
            var user = GetCurrentUser();

            if (user == null)
            {
                sessionManager.SetByKey("MustBeLoggedIn", MessageConstants.ToDoThisOperationYouMustBeLoggedIn);
                return RedirectToAction("", "Home");
            }

            var userProfile = userProfileRepository.UserProfiles.FirstOrDefault(p => p.AccountId == user.Id);

            if (userProfile == null)
            {
                sessionManager.SetByKey("MustBeLoggedIn", MessageConstants.ToDoThisOperationYouMustBeLoggedIn);
                return RedirectToAction("", "Home");
            }

            var order = repository.Orders.Where(x => x.OrderId == orderId && x.Shipped == false).FirstOrDefault();

            if (order == null)
            {
                sessionManager.SetByKey("ValidOrderDoesNotExist", string.Format(MessageConstants.ValidOrderWithSubmittedIdDoesNotExist, orderId));
                return RedirectToAction("OrdersByUser", "Home");
            }

            return View(order);
        }

        [HttpPost]
        [Route("order/edit")]
        public IActionResult EditOrderPost([FromForm] Order updatedOrder)
        {
            var user = GetCurrentUser();

            if (user == null)
            {
                sessionManager.SetByKey("MustBeLoggedIn", MessageConstants.ToDoThisOperationYouMustBeLoggedIn);
                return RedirectToAction("", "Home");
            }

            var userProfile = userProfileRepository.UserProfiles.FirstOrDefault(p => p.AccountId == user.Id);

            if (userProfile == null)
            {
                sessionManager.SetByKey("MustBeLoggedIn", MessageConstants.ToDoThisOperationYouMustBeLoggedIn);
                return RedirectToAction("", "Home");
            }

            var order = repository.Orders.Where(x => x.OrderId == updatedOrder.OrderId && x.Shipped == false).FirstOrDefault();

            if (order == null)
            {
                sessionManager.SetByKey("ValidOrderDoesNotExist", string.Format(MessageConstants.ValidOrderWithSubmittedIdDoesNotExist, updatedOrder.OrderId));
                return RedirectToAction("OrdersByUser", "Order");
            }

            updatedOrder = repository.UpdateOrder(updatedOrder);

            var content = EmailHelper.PrepareOrderEmail(updatedOrder, true);

            emailService.SendEmail(updatedOrder.Email, $"Updated Order - {updatedOrder.Name} - {updatedOrder.Email}!", content);

            return RedirectToAction("OrdersByUser", "Order");
        }

        public IActionResult Checkout()
        {
            var user = GetCurrentUser();

            if (user == null)
            {
                sessionManager.SetByKey("MustBeLoggedIn", MessageConstants.ToDoThisOperationYouMustBeLoggedIn);
                return RedirectToAction("", "Home");
            }

            var userProfile = userProfileRepository.UserProfiles.FirstOrDefault(p => p.AccountId == user.Id);

            if (userProfile == null)
            {
                sessionManager.SetByKey("MustBeLoggedIn", MessageConstants.ToDoThisOperationYouMustBeLoggedIn);
                return RedirectToAction("", "Home");
            }

            Order model = new Order()
            {
                UserProfile = userProfile
            };
            return View(model);
        }

        [HttpPost]
        public IActionResult Checkout(Order order)
        {
            var user = GetCurrentUser();

            if (user == null)
            {
                sessionManager.SetByKey("MustBeLoggedIn", MessageConstants.ToDoThisOperationYouMustBeLoggedIn);
                return RedirectToAction("", "Home");
            }

            var userProfile = userProfileRepository.UserProfiles.FirstOrDefault(p => p.AccountId == user.Id);

            if (userProfile == null)
            {
                sessionManager.SetByKey("MustBeLoggedIn", MessageConstants.ToDoThisOperationYouMustBeLoggedIn);
                return RedirectToAction("", "Home");
            }

            if (cart.Lines.Count() == 0)
            {
                ModelState.AddModelError("", "Sorry, your cart is empty!");
            }

            if (ModelState.IsValid)
            {
                order.Lines = cart.Lines;

                order.UserProfileId = userProfile.Id;

                var savedOrder = repository.SaveOrder(order);
                cart.Clear();
                
                order = repository.Orders
                    .Include(x => x.Lines)
                        .ThenInclude(x => x.Article)
                            .ThenInclude(x => x.ArticleType)
                    .AsNoTracking()
                    .FirstOrDefault(x => x.OrderId == savedOrder.OrderId);

                var content = EmailHelper.PrepareOrderEmail(order);

                emailService.SendEmail(order.Email, $"New Order - {order.Name} - {order.Email}!", content);
                return RedirectToAction("Completed", "Order", new { orderId = order.OrderId});
            }
            else
                return View();
        }

        [HttpGet]
        public IActionResult Completed(int orderId)
        {
            var user = GetCurrentUser();

            if (user == null)
            {
                sessionManager.SetByKey("MustBeLoggedIn", MessageConstants.ToDoThisOperationYouMustBeLoggedIn);
                return RedirectToAction("", "Home");
            }

            var userProfile = userProfileRepository.UserProfiles.FirstOrDefault(p => p.AccountId == user.Id);

            if (userProfile == null)
            {
                sessionManager.SetByKey("MustBeLoggedIn", MessageConstants.ToDoThisOperationYouMustBeLoggedIn);
                return RedirectToAction("", "Home");
            }

            Order model = new Order()
            {
                OrderId = orderId,
                UserProfile = userProfile,
            };
            return View("Completed", model);
        }

        [HttpGet("order/ordersByUser")]
        public IActionResult OrdersByUser()
        {
            var user = GetCurrentUser();
            
            if (user == null)
            {
                sessionManager.SetByKey("MustBeLoggedIn", MessageConstants.ToDoThisOperationYouMustBeLoggedIn);
                return RedirectToPage("/");
            }

            var userProfile = userProfileRepository.UserProfiles.FirstOrDefault(p => p.AccountId == user.Id);

            if (userProfile == null)
            {
                sessionManager.SetByKey("MustBeLoggedIn", MessageConstants.ToDoThisOperationYouMustBeLoggedIn);
                return RedirectToPage("/");
            }

            var orders = repository.Orders.Where(x => x.UserProfileId == userProfile.Id);
            var viewModel = new OrdersByUserViewModel();
            viewModel.Orders = orders.Where(x => x.Shipped == false).ToList();
            viewModel.ShippedOrders = orders.Where(x => x.Shipped == true).ToList();
            return View(viewModel);
        }

        [HttpPost]
        [Authorize]
        public IActionResult Cancel([FromForm] int OrderId)
        {
            var order = repository.Orders.Where(x => x.OrderId == OrderId).FirstOrDefault();

            if(order != null)
            {
                order.Canceled = true;
                order.Note = "Cancelled by the user";
                repository.SaveOrder(order);
            }

            return RedirectToAction("OrdersByUser", "Order");
        }

        [HttpPost]
        [Authorize]
        public IActionResult ReturnToCart([FromForm] int OrderId)
        {
            var order = repository.Remove(OrderId);

            if(order != null)
            {
                foreach(var line in order.Lines)
                {
                    cart.AddItem(line.Article, line.Quantity);
                }
            }

            return RedirectToPage("/Cart");
        }

        private IdentityUser GetCurrentUser()
        {
            string claimName = User.FindFirstValue(ClaimTypes.Name);

            if (string.IsNullOrEmpty(claimName))
            {
                return null;
            }

            var user = userManager.FindByEmailAsync(claimName).Result;

            var userByUsername = userManager.FindByNameAsync(claimName).Result;

            if (userByUsername != null)
                return userByUsername;

            if (user != null)
                return user;

            return null;
        }
    }
}
