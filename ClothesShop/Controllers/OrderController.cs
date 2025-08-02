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
using System;
using ClothesShop.Enums;
using System.Diagnostics;

namespace ClothesShop.Controllers
{
    [Route("[controller]")]
    public class OrderController : Controller
    {
        private IOrderRepository _repository;
        private readonly IUserProfileRepository _userProfileRepository;
        private Cart _cart;

        private readonly Localizer L;

        private UserManager<IdentityUser> _userManager;
        private readonly IPaymentService _paymentService;
        private readonly ISessionManager _sessionManager;
        private readonly IConfiguration _configuration;

        public OrderController(UserManager<IdentityUser> userMgr, IOrderRepository repoService, IUserProfileRepository userProfileRepository, Cart cartService,
            IPaymentService paymentService, ISessionManager sessionManager, IConfiguration configuration,
            Localizer l)
        {
            _userManager = userMgr;
            _repository = repoService;
            _userProfileRepository = userProfileRepository;
            _cart = cartService;
            _paymentService = paymentService;
            _sessionManager = sessionManager;
            _configuration = configuration;
            L = l;
        }

        private const string PaymentSucceededEvent = "payment_intent.succeeded";
        private const string PaymentCanceledEvent = "payment_intent.canceled";
        private const string PaymentFailedEvent = "payment_intent.payment_failed";

        public const string PaymentFailedStatus = "requires_payment_method";
        private const string PaymentCanceledStatus = "canceled";
        private const string PaymentSucceededStatus = "succeeded";

        [HttpPost]
        [Route("webhook")]
        public async Task<IActionResult> PaymentProcessingResult()
        {
            using var reader = new StreamReader(Request.Body);
            var json = await reader.ReadToEndAsync();
            var stripeEvent = EventUtility.ConstructEvent(json,
                Request.Headers["Stripe-Signature"],
                _configuration["Stripe:WebhookSecretKey"]);

            string[] validPaymentTypes = [
                PaymentSucceededEvent, PaymentFailedEvent, PaymentCanceledEvent
            ];

            string[] validPaymentStatuses = [
                PaymentFailedStatus,
                PaymentSucceededStatus,
                PaymentCanceledStatus
            ];

            string paymentStatus = validPaymentTypes.FirstOrDefault(x => x.Equals(stripeEvent.Type, StringComparison.OrdinalIgnoreCase));

            if (string.IsNullOrEmpty(paymentStatus))
            {
                return BadRequest();
            }

            var session = stripeEvent.Data.Object as PaymentIntent;

            if (!validPaymentStatuses.Any(x => x.Equals(session.Status, StringComparison.OrdinalIgnoreCase)))
            {
                return BadRequest();
            }

            if (!session.Metadata.TryGetValue("orderId", out string orderIdStr)
                || !int.TryParse(orderIdStr, out int orderId)
                || orderId < 1)
            {
                return BadRequest();
            }


            var orderFromDb = await _repository.GetOrderById(orderId);
            if (orderFromDb == null)
            {
                return BadRequest();
            }

            Func<string, PaymentStatus> getPaymentStatusFromString = (string status) => status.ToLower() switch
            {
                PaymentCanceledStatus => PaymentStatus.Cancelled,
                PaymentSucceededStatus => PaymentStatus.Paid,
                PaymentFailedStatus => PaymentStatus.Failed,
                _ => PaymentStatus.Pending
            };

            PaymentStatus orderStatus = getPaymentStatusFromString(session.Status);

            if (orderStatus == PaymentStatus.Pending)
            {
                // Nema potrebe za prevodjenjem, debug poruka za Stripe servis
                Debug.WriteLine($"Primljen je nepoznat status porudzbine, naplata nije uspela. Koristi se status: {orderStatus}");
            }

            orderFromDb.Status = orderStatus;
            _repository.UpdateOrder(orderFromDb);

            return Ok(orderFromDb);
        }


        [HttpGet]
        [Route("order/edit/{orderId:int}")]
        public async Task<IActionResult> EditOrder(int orderId)
        {
            var user = GetCurrentUser();

            if (user == null)
            {
                _sessionManager.SetByKey("MustBeLoggedIn", L[MessageConstants.ToDoThisOperationYouMustBeLoggedIn]);
                return RedirectToAction("", "Home");
            }

            var userProfile = _userProfileRepository.UserProfiles.FirstOrDefault(p => p.AccountId == user.Id);

            if (userProfile == null)
            {
                _sessionManager.SetByKey("MustBeLoggedIn", L[MessageConstants.ToDoThisOperationYouMustBeLoggedIn]);
                return RedirectToAction("", "Home");
            }

            var order = await _repository.GetOrderById(orderId);

            if (order == null || order.Status != PaymentStatus.Pending)
            {
                _sessionManager.SetByKey("ValidOrderDoesNotExist", string.Format(L[MessageConstants.ValidOrderWithSubmittedIdDoesNotExist], orderId));
                return RedirectToAction("OrdersByUser", "Home");
            }

            return View(order);
        }

        [HttpPost]
        [Route("order/edit")]
        public async Task<IActionResult> EditOrderPost([FromForm] Order updatedOrder)
        {
            var user = GetCurrentUser();

            if (user == null)
            {
                _sessionManager.SetByKey("MustBeLoggedIn", L[MessageConstants.ToDoThisOperationYouMustBeLoggedIn]);
                return RedirectToAction("", "Home");
            }

            var userProfile = _userProfileRepository.UserProfiles.FirstOrDefault(p => p.AccountId == user.Id);

            if (userProfile == null)
            {
                _sessionManager.SetByKey("MustBeLoggedIn", L[MessageConstants.ToDoThisOperationYouMustBeLoggedIn]);
                return RedirectToAction("", "Home");
            }

            var order = await _repository.GetOrderById(updatedOrder.OrderId);

            if (order == null || order.Status != PaymentStatus.Pending)
            {
                _sessionManager.SetByKey("ValidOrderDoesNotExist", string.Format(L[MessageConstants.ValidOrderWithSubmittedIdDoesNotExist], updatedOrder.OrderId));
                return RedirectToAction("OrdersByUser", "Order");
            }

            updatedOrder = _repository.UpdateOrder(updatedOrder);

            return RedirectToAction("OrdersByUser", "Order");
        }

        [HttpGet("Checkout")]
        public IActionResult Checkout()
        {
            var user = GetCurrentUser();

            if (user == null)
            {
                _sessionManager.SetByKey("MustBeLoggedIn", L[MessageConstants.ToDoThisOperationYouMustBeLoggedIn]);
                return RedirectToAction("", "Home");
            }

            var userProfile = _userProfileRepository.UserProfiles.FirstOrDefault(p => p.AccountId == user.Id);

            if (userProfile == null)
            {
                _sessionManager.SetByKey("MustBeLoggedIn", L[MessageConstants.ToDoThisOperationYouMustBeLoggedIn]);
                return RedirectToAction("", "Home");
            }

            Order model = new Order()
            {
                UserProfile = userProfile
            };
            return View(model);
        }

        [HttpGet("pay/{orderId:int}")]
        public async Task<IActionResult> ProcessOrderGet(int orderId)
        {
            var user = GetCurrentUser();

            if (user == null)
            {
                _sessionManager.SetByKey("MustBeLoggedIn", L[MessageConstants.ToDoThisOperationYouMustBeLoggedIn]);
                return RedirectToAction("", "Home");
            }

            string failureUrl = Url.Action("StripeUnavailable", "Order");

            try
            {
                var order = await _repository.GetOrderById(orderId);

                (PaymentStatus status, string redirectUrl) = await _paymentService.ProcessPayment(order,
                    successUrl: Url.Action("OrderCompletedWithQueryParam", "Order", new { orderId = order.OrderId }, Request.Scheme),
                    cancelUrl: Url.Action("Cancelled", "Order", new { orderId = order.OrderId }, Request.Scheme),
                    failureUrl: failureUrl);

                return Redirect(redirectUrl);
            }
            catch (Exception ex)
            {
                return Redirect(failureUrl);
            }
        }

        [HttpPost("ProcessOrder")]
        public async Task<IActionResult> ProcessOrder(Order order)
        {
            var user = GetCurrentUser();

            if (user == null)
            {
                _sessionManager.SetByKey("MustBeLoggedIn", L[MessageConstants.ToDoThisOperationYouMustBeLoggedIn]);
                return RedirectToAction("", "Home");
            }

            var userProfile = _userProfileRepository.UserProfiles.FirstOrDefault(p => p.AccountId == user.Id);

            if (userProfile == null)
            {
                _sessionManager.SetByKey("MustBeLoggedIn", L[MessageConstants.ToDoThisOperationYouMustBeLoggedIn]);
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

            string failureUrl = Url.Action("StripeUnavailable", "Order");
            try
            {
                order.Lines = _cart.Lines.Select(x => new OrderLine() { 
                    Quantity = x.Quantity, 
                    ArticleId = x.ArticleId, 
                    Price = x.Price, 
                }).ToList();

                order.UserProfileId = userProfile.Id;

                var savedOrder = _repository.SaveOrder(order);
                _cart.Clear();

                (PaymentStatus status, string redirectUrl) = await _paymentService.ProcessPayment(savedOrder,
                    successUrl: Url.Action("OrderCompletedWithQueryParam", "Order", new { orderId = savedOrder.OrderId }, Request.Scheme),
                    cancelUrl: Url.Action("Cancelled", "Order", new { orderId = savedOrder.OrderId }, Request.Scheme),
                    failureUrl: failureUrl);
                
                return Redirect(redirectUrl);
            }
            catch (Exception ex)
            {
                return Redirect(failureUrl);
            }
        }

        [HttpGet("StripeUnavailable")]
        public IActionResult StripeUnavailable()
        {
            return View("StripeUnavailable");
        }

        [HttpGet("Cancelled")]
        public async Task<IActionResult> Cancelled([FromQuery] int orderId)
        {
            var user = GetCurrentUser();

            if (user == null)
            {
                _sessionManager.SetByKey("MustBeLoggedIn", L[MessageConstants.ToDoThisOperationYouMustBeLoggedIn]);
                return RedirectToAction("", "Home");
            }

            var userProfile = _userProfileRepository.UserProfiles.FirstOrDefault(p => p.AccountId == user.Id);

            if (userProfile == null)
            {
                _sessionManager.SetByKey("MustBeLoggedIn", L[MessageConstants.ToDoThisOperationYouMustBeLoggedIn]);
                return RedirectToAction("", "Home");
            }

            var order = await _repository.GetOrderById(orderId);

            if (order == null || order.Status != PaymentStatus.Pending)
            {
                _sessionManager.SetByKey("ValidOrderDoesNotExist", string.Format(L[MessageConstants.ValidOrderWithSubmittedIdDoesNotExist], orderId));
                return RedirectToAction("OrdersByUser", "Order");
            }

            order.Status = PaymentStatus.Cancelled;
            _repository.UpdateOrder(order);

            return View(order.OrderId);
        }

        [HttpPost("Checkout")]
        public async Task<IActionResult> Checkout(Order order)
        {
            var user = GetCurrentUser();

            if (user == null)
            {
                _sessionManager.SetByKey("MustBeLoggedIn", L[MessageConstants.ToDoThisOperationYouMustBeLoggedIn]);
                return RedirectToAction("", "Home");
            }

            var userProfile = _userProfileRepository.UserProfiles.FirstOrDefault(p => p.AccountId == user.Id);

            if (userProfile == null)
            {
                _sessionManager.SetByKey("MustBeLoggedIn", L[MessageConstants.ToDoThisOperationYouMustBeLoggedIn]);
                return RedirectToAction("", "Home");
            }

            if (_cart.Lines.Count() == 0)
            {
                ModelState.AddModelError("", L["Sorry, your cart is empty!"]);
            }

            if (ModelState.IsValid)
            {
                order.Lines = _cart.Lines.Select(x => new OrderLine()
                {
                    Quantity = x.Quantity,
                    ArticleId = x.ArticleId,
                    Price = x.Price,
                }).ToList();

                order.UserProfileId = userProfile.Id;

                var savedOrder = _repository.SaveOrder(order);
                _cart.Clear();
                
                order = await _repository.GetOrderById(order.OrderId);
                
                return RedirectToAction("Completed", "Order", new { orderId = order.OrderId });
            }
            else
                return View();
        }

        [HttpGet("Completed/{orderId:int}")]
        public IActionResult OrderCompleted(int orderId)
        {
            return OrderCompletedProcessor(orderId);
        }

        [HttpGet("Completed")]
        public IActionResult OrderCompletedWithQueryParam([FromQuery] int orderId)
        {
            return OrderCompletedProcessor(orderId);
        }

        private IActionResult OrderCompletedProcessor(int orderId)
        {
            var user = GetCurrentUser();

            if (user == null)
            {
                _sessionManager.SetByKey("MustBeLoggedIn", L[MessageConstants.ToDoThisOperationYouMustBeLoggedIn]);
                return RedirectToAction("", "Home");
            }

            var userProfile = _userProfileRepository.UserProfiles.FirstOrDefault(p => p.AccountId == user.Id);

            if (userProfile == null)
            {
                _sessionManager.SetByKey("MustBeLoggedIn", L[MessageConstants.ToDoThisOperationYouMustBeLoggedIn]);
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
        public async Task<IActionResult> OrdersByUser()
        {
            var user = GetCurrentUser();
            
            if (user == null)
            {
                _sessionManager.SetByKey("MustBeLoggedIn", L[MessageConstants.ToDoThisOperationYouMustBeLoggedIn]);
                return RedirectToPage("/");
            }

            var userProfile = _userProfileRepository.UserProfiles.FirstOrDefault(p => p.AccountId == user.Id);

            if (userProfile == null)
            {
                _sessionManager.SetByKey("MustBeLoggedIn", L[MessageConstants.ToDoThisOperationYouMustBeLoggedIn]);
                return RedirectToPage("/");
            }

            var orders = await _repository.GetOrdersByUser(userProfile.Id);
            var viewModel = new OrdersByUserViewModel();
            viewModel.Orders = orders.Where(x => x.Shipped == false).ToList();
            viewModel.ShippedOrders = orders.Where(x => x.Shipped == true).ToList();
            return View(viewModel);
        }

        [HttpPost("Cancel")]
        [Authorize(Roles = "User, Administrator")]
        public async Task<IActionResult> CancelOrder([FromForm] int OrderId)
        {
            var user = GetCurrentUser();

            if (user == null)
            {
                _sessionManager.SetByKey("MustBeLoggedIn", L[MessageConstants.ToDoThisOperationYouMustBeLoggedIn]);
                return RedirectToPage("/");
            }

            var userProfile = _userProfileRepository.UserProfiles.FirstOrDefault(p => p.AccountId == user.Id);

            if (userProfile == null)
            {
                _sessionManager.SetByKey("MustBeLoggedIn", L[MessageConstants.ToDoThisOperationYouMustBeLoggedIn]);
                return RedirectToPage("/");
            }

            var order = await _repository.GetOrderById(OrderId);

            if (userProfile.Id != order.UserProfileId && !HttpContext.User.IsInRole("Administrator"))
            {
                _sessionManager.SetByKey("MustBeLoggedIn", L[MessageConstants.ToDoThisOperationYouMustBeLoggedIn]);
                return RedirectToPage("/");
            }

            if (order != null)
            {
                order.Status = PaymentStatus.Cancelled;
                order.Note = L["Canceled by the user"];
                _repository.UpdateOrder(order);
            }

            return RedirectToAction("OrdersByUser", "Order");
        }

        [HttpPost("ReturnToCart")]
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
