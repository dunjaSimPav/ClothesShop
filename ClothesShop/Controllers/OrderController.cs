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
using System.Threading.Tasks;
using System.IO;
using Stripe;
using Microsoft.Extensions.Configuration;
using Stripe.Checkout;

namespace ClothesShop.Controllers
{
    public class OrderController : Controller
    {
        private IOrderRepository _repository;
        private readonly IUserProfileRepository _userProfileRepository;
        private Cart _cart;

        private UserManager<IdentityUser> _userManager;
        private readonly IPaymentService _paymentService;
        private readonly ISessionManager _sessionManager;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        public OrderController(UserManager<IdentityUser> userMgr, IOrderRepository repoService, IUserProfileRepository userProfileRepository, Cart cartService,
            IPaymentService paymentService, ISessionManager sessionManager, IEmailService emailService, IConfiguration configuration)
        {
            _userManager = userMgr;
            _repository = repoService;
            _userProfileRepository = userProfileRepository;
            _cart = cartService;
            _paymentService = paymentService;
            _sessionManager = sessionManager;
            _emailService = emailService;
            _configuration = configuration;
        }

        [HttpPost]
        [Route("webhook")]
        public async Task<IActionResult> PaymentProcessingResult()
        {
            using var reader = new StreamReader(Request.Body);
            var json = await reader.ReadToEndAsync();
            var stripeEvent = EventUtility.ConstructEvent(json,
                Request.Headers["Stripe-Signature"],
                _configuration["Stripe:WebhookSecretKey"]);

            if (stripeEvent.Type == "checkout.session.completed")
            {
                var session = stripeEvent.Data.Object as Session;
                // TODO: Process further...
            }

            return Ok();
        }

        [HttpGet]
        [Route("order/edit/{orderId:int}")]
        public IActionResult EditOrder(int orderId)
        {
            var user = GetCurrentUser();

            if (user == null)
            {
                _sessionManager.SetByKey("MustBeLoggedIn", MessageConstants.ToDoThisOperationYouMustBeLoggedIn);
                return RedirectToAction("", "Home");
            }

            var userProfile = _userProfileRepository.UserProfiles.FirstOrDefault(p => p.AccountId == user.Id);

            if (userProfile == null)
            {
                _sessionManager.SetByKey("MustBeLoggedIn", MessageConstants.ToDoThisOperationYouMustBeLoggedIn);
                return RedirectToAction("", "Home");
            }

            var order = _repository.Orders.Where(x => x.OrderId == orderId && x.Shipped == false).FirstOrDefault();

            if (order == null)
            {
                _sessionManager.SetByKey("ValidOrderDoesNotExist", string.Format(MessageConstants.ValidOrderWithSubmittedIdDoesNotExist, orderId));
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
                _sessionManager.SetByKey("MustBeLoggedIn", MessageConstants.ToDoThisOperationYouMustBeLoggedIn);
                return RedirectToAction("", "Home");
            }

            var userProfile = _userProfileRepository.UserProfiles.FirstOrDefault(p => p.AccountId == user.Id);

            if (userProfile == null)
            {
                _sessionManager.SetByKey("MustBeLoggedIn", MessageConstants.ToDoThisOperationYouMustBeLoggedIn);
                return RedirectToAction("", "Home");
            }

            var order = _repository.Orders.Where(x => x.OrderId == updatedOrder.OrderId && x.Shipped == false).FirstOrDefault();

            if (order == null)
            {
                _sessionManager.SetByKey("ValidOrderDoesNotExist", string.Format(MessageConstants.ValidOrderWithSubmittedIdDoesNotExist, updatedOrder.OrderId));
                return RedirectToAction("OrdersByUser", "Order");
            }

            updatedOrder = _repository.UpdateOrder(updatedOrder);

            var content = EmailHelper.PrepareOrderEmail(updatedOrder, true);

            _emailService.SendEmail(updatedOrder.Email, $"Updated Order - {updatedOrder.Name} - {updatedOrder.Email}!", content);

            return RedirectToAction("OrdersByUser", "Order");
        }

        public IActionResult Checkout()
        {
            var user = GetCurrentUser();

            if (user == null)
            {
                _sessionManager.SetByKey("MustBeLoggedIn", MessageConstants.ToDoThisOperationYouMustBeLoggedIn);
                return RedirectToAction("", "Home");
            }

            var userProfile = _userProfileRepository.UserProfiles.FirstOrDefault(p => p.AccountId == user.Id);

            if (userProfile == null)
            {
                _sessionManager.SetByKey("MustBeLoggedIn", MessageConstants.ToDoThisOperationYouMustBeLoggedIn);
                return RedirectToAction("", "Home");
            }

            Order model = new Order()
            {
                UserProfile = userProfile
            };
            return View(model);
        }

        [HttpPost] // TODO: Finalize this
        public async Task<IActionResult> ProcessOrder(Order order)
        {
            var user = GetCurrentUser();

            if (user == null)
            {
                _sessionManager.SetByKey("MustBeLoggedIn", MessageConstants.ToDoThisOperationYouMustBeLoggedIn);
                return RedirectToAction("", "Home");
            }

            var userProfile = _userProfileRepository.UserProfiles.FirstOrDefault(p => p.AccountId == user.Id);

            if (userProfile == null)
            {
                _sessionManager.SetByKey("MustBeLoggedIn", MessageConstants.ToDoThisOperationYouMustBeLoggedIn);
                return RedirectToAction("", "Home");
            }

            if (_cart.Lines.Count() == 0)
            {
                ModelState.AddModelError("", "Sorry, your cart is empty!");
            }

            if (!ModelState.IsValid)
            {
                return RedirectToAction("Checkout", "Order");
            }

            order.Lines = _cart.Lines.Select(x => new CartLine() { Quantity = x.Quantity, Article = x.Article }).ToList();

            order.UserProfileId = userProfile.Id;

            var savedOrder = _repository.SaveOrder(order);
            _cart.Clear();

            var redirectUrl = await _paymentService.ProcessPayment(savedOrder,
                successUrl: Url.Action("Completed", "Order", savedOrder.OrderId, Request.Scheme),
                cancelUrl: Url.Action("Cancelled", "Order", savedOrder.OrderId, Request.Scheme));


            return Redirect(redirectUrl);

            //order = _repository.Orders
            //    .Include(x => x.Lines)
            //        .ThenInclude(x => x.Article)
            //            .ThenInclude(x => x.ArticleType)
            //    .AsNoTracking()
            //    .FirstOrDefault(x => x.OrderId == savedOrder.OrderId);

            //var content = EmailHelper.PrepareOrderEmail(order);

            //_emailService.SendEmail(order.Email, $"New Order - {order.Name} - {order.Email}!", content);
            //return RedirectToAction("Completed", "Order", new { orderId = order.OrderId });
        }

        [HttpGet]
        public IActionResult Cancelled(int orderId)
        {
            return RedirectToPage("/Cart");
        }

        [HttpPost]
        public IActionResult Checkout(Order order)
        {
            var user = GetCurrentUser();

            if (user == null)
            {
                _sessionManager.SetByKey("MustBeLoggedIn", MessageConstants.ToDoThisOperationYouMustBeLoggedIn);
                return RedirectToAction("", "Home");
            }

            var userProfile = _userProfileRepository.UserProfiles.FirstOrDefault(p => p.AccountId == user.Id);

            if (userProfile == null)
            {
                _sessionManager.SetByKey("MustBeLoggedIn", MessageConstants.ToDoThisOperationYouMustBeLoggedIn);
                return RedirectToAction("", "Home");
            }

            if (_cart.Lines.Count() == 0)
            {
                ModelState.AddModelError("", "Sorry, your cart is empty!");
            }

            if (ModelState.IsValid)
            {
                order.Lines = _cart.Lines;

                order.UserProfileId = userProfile.Id;

                var savedOrder = _repository.SaveOrder(order);
                _cart.Clear();
                
                order = _repository.Orders
                    .Include(x => x.Lines)
                        .ThenInclude(x => x.Article)
                            .ThenInclude(x => x.ArticleType)
                    .AsNoTracking()
                    .FirstOrDefault(x => x.OrderId == savedOrder.OrderId);

                var content = EmailHelper.PrepareOrderEmail(order);

                _emailService.SendEmail(order.Email, $"New Order - {order.Name} - {order.Email}!", content);
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
                _sessionManager.SetByKey("MustBeLoggedIn", MessageConstants.ToDoThisOperationYouMustBeLoggedIn);
                return RedirectToAction("", "Home");
            }

            var userProfile = _userProfileRepository.UserProfiles.FirstOrDefault(p => p.AccountId == user.Id);

            if (userProfile == null)
            {
                _sessionManager.SetByKey("MustBeLoggedIn", MessageConstants.ToDoThisOperationYouMustBeLoggedIn);
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
                _sessionManager.SetByKey("MustBeLoggedIn", MessageConstants.ToDoThisOperationYouMustBeLoggedIn);
                return RedirectToPage("/");
            }

            var userProfile = _userProfileRepository.UserProfiles.FirstOrDefault(p => p.AccountId == user.Id);

            if (userProfile == null)
            {
                _sessionManager.SetByKey("MustBeLoggedIn", MessageConstants.ToDoThisOperationYouMustBeLoggedIn);
                return RedirectToPage("/");
            }

            var orders = _repository.Orders.Where(x => x.UserProfileId == userProfile.Id);
            var viewModel = new OrdersByUserViewModel();
            viewModel.Orders = orders.Where(x => x.Shipped == false).ToList();
            viewModel.ShippedOrders = orders.Where(x => x.Shipped == true).ToList();
            return View(viewModel);
        }

        [HttpPost]
        [Authorize]
        public IActionResult Cancel([FromForm] int OrderId)
        {
            var order = _repository.Orders.Where(x => x.OrderId == OrderId).FirstOrDefault();

            if(order != null)
            {
                order.Canceled = true;
                order.Note = "Cancelled by the user";
                _repository.SaveOrder(order);
            }

            return RedirectToAction("OrdersByUser", "Order");
        }

        [HttpPost]
        [Authorize]
        public IActionResult ReturnToCart([FromForm] int OrderId)
        {
            var order = _repository.Remove(OrderId);

            if(order != null)
            {
                foreach(var line in order.Lines)
                {
                    _cart.AddItem(line.Article, line.Quantity);
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

            var user = _userManager.FindByEmailAsync(claimName).Result;

            var userByUsername = _userManager.FindByNameAsync(claimName).Result;

            if (userByUsername != null)
                return userByUsername;

            if (user != null)
                return user;

            return null;
        }
    }
}
