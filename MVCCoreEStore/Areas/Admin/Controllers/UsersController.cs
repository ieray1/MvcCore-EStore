using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MVCCoreEStoreData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MVCCoreEStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Administrators")]
    public class UsersController : Controller
    {
        private readonly AppDbContext context;
        private readonly UserManager<User> userManager;

        public UsersController(
            AppDbContext context,
            UserManager<User> userManager
            )
        {
            this.context = context;
            this.userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Roles"] = new SelectList(await context.Roles.ToListAsync(), "Name", "FriendlyName");
            var model = await context.Users.OrderBy(p => p.Name).ToListAsync();
            return View(model);
        }

        public async Task<IActionResult> ChangeRole(string id, string newRole)
        {
            var user = await userManager.FindByIdAsync(id);
            var roles = await userManager.GetRolesAsync(user);
            var admins = context.UserRoles.Count(p => p.RoleId == 1);
            if (admins < 2 && roles[0] == "Administrators")
            {
                TempData["error"] = "Sistemde en az bir yönetici olmalıdır";
            }
            else
            {
                await userManager.RemoveFromRoleAsync(user, roles[0]);
                await userManager.AddToRoleAsync(user, newRole);
                TempData["success"] = "Kullanıcı rolu başarıyla değiştirilmiştir.";
            }
            return RedirectToAction("Index");
        }
    }
}
