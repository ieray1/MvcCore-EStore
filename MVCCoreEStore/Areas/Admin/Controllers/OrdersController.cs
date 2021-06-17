using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MVCCoreEStore.Services;
using MVCCoreEStoreData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MVCCoreEStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Administrators, OrderAdministrators")]
    public class OrdersController : Controller
    {
        private readonly AppDbContext context;
        private readonly IMailMessageService mailMessageService;
        private readonly IWebHostEnvironment webHostEnvironment;

        public OrdersController(
            AppDbContext context,
            IMailMessageService mailMessageService,
            IWebHostEnvironment webHostEnvironment
            )
        {
            this.context = context;
            this.mailMessageService = mailMessageService;
            this.webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            var model = await context.Orders.Where(p => p.OrderState == OrderStates.New).OrderBy(p => p.Date).ToListAsync();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateOrder(int id, string shippingNumber)
        {
            var order = await context.Orders.FindAsync(id);

            order.OrderState = OrderStates.Shipped;
            order.ShippingNumber = shippingNumber;

            await context.SaveChangesAsync();

            TempData["success"] = "Sipariş gönderme işlemi başarıyla tamamlanmıştır.";

            var body = System.IO.File.ReadAllText(
                System.IO.Path.Combine(webHostEnvironment.WebRootPath, "content", "OrderShippedTemplate.html")
                );
            body = string.Format(body,
                    order.User.Name,
                    order.Id.ToString("00000000"),
                    order.GrandTotal.ToString("c2"),
                    shippingNumber);

            await mailMessageService.Send(order.User.UserName, "Siparişiniz Kargoya Verildi", body);

            return RedirectToAction("Index");
        }


    }
}
