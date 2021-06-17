using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MVCCoreEStoreData;
using MVCCoreEStore.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using MVCCoreEStoreData.Infrastructure;
using PaymentBase;

namespace MVCCoreEStore
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDistributedMemoryCache();

            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(20);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            services.AddControllersWithViews();

            services.AddDbContext<AppDbContext>(options =>
            {
                switch (Configuration.GetValue<string>("Application:DbType"))
                {
                    case "MySql":
                        options.UseMySql(
                            Configuration.GetConnectionString("MySql"),
                            ServerVersion.AutoDetect(Configuration.GetConnectionString("MySql")),
                            x => x.MigrationsAssembly("MigrationsMySql")
                            );
                        break;
                    //case "Oracle":
                    //    options.UseSqlServer(Configuration.GetConnectionString("Default"));
                    //    break;
                    //case "Postgres":
                    //    options.UseSqlServer(Configuration.GetConnectionString("Default"));
                    //    break;
                    case "SqlServer":
                    default:
                        options.UseSqlServer(
                            Configuration.GetConnectionString("SqlServer"),
                            x => x.MigrationsAssembly("MigrationsSqlServer"));
                        break;
                }

                options.UseLazyLoadingProxies();
            });

            services.AddIdentity<User, Role>(options =>
            {
                options.Password.RequireDigit = Configuration.GetValue<bool>("Security:PasswordPolicy:RequireDigit");
                options.Password.RequiredLength = Configuration.GetValue<int>("Security:PasswordPolicy:RequiredLength");
                options.Password.RequireLowercase = Configuration.GetValue<bool>("Security:PasswordPolicy:RequireLowercase");
                options.Password.RequireNonAlphanumeric = Configuration.GetValue<bool>("Security:PasswordPolicy:RequireNonAlphanumeric");
                options.Password.RequireUppercase = Configuration.GetValue<bool>("Security:PasswordPolicy:RequireUppercase");

                options.SignIn.RequireConfirmedEmail = Configuration.GetValue<bool>("Security:PasswordPolicy:RequireConfirmedEmail");

                //options.Lockout.AllowedForNewUsers = false;
                //options.Lockout.MaxFailedAccessAttempts = 5;
                //options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            })
                .AddEntityFrameworkStores<AppDbContext>()
                .AddTokenProvider<DataProtectorTokenProvider<User>>(TokenOptions.DefaultProvider);

            services.AddScoped<IShoppingCartService, ShoppingCartService>();
            services.AddTransient<IMailMessageService, MailMessageService>();
            services.AddTransient<IPaymentSevice, PaymentService>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, AppDbContext context, RoleManager<Role> roleManager, UserManager<User> userManager)
        {

            context.Database.Migrate();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseStatusCodePagesWithReExecute("/error/{0}");

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseSession();

            var cultures = new List<CultureInfo> {
                                new CultureInfo("tr-TR")
                            };
            app.UseRequestLocalization(options =>
            {
                options.DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture("tr-TR");
                options.SupportedCultures = cultures;
                options.SupportedUICultures = cultures;
            });


            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "areas",
                    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

                endpoints.MapControllerRoute(
                    name: "product",
                    pattern: "p/{id}/{name}.html",
                    defaults: new { controller = "Home", action = "Product" }
                    );

                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

            });

            new[]
            {
                new Role { Name = "Administrators", FriendlyName = "Yöneticiler"},
                new Role { Name = "ProductAdministrators", FriendlyName = "Ürün Yöneticileri"},
                new Role { Name = "OrderAdministrators", FriendlyName = "Sipariş Yöneticileri"},
                new Role { Name = "Members", FriendlyName = "Üyeler"},
            }
            .ToList()
            .ForEach(role =>
            {
                roleManager.CreateAsync(role).Wait();
            });

            {
                var user = new User
                {
                    UserName = Configuration.GetValue<string>("Security:DefaultUser:UserName"),
                    Name = Configuration.GetValue<string>("Security:DefaultUser:Name")
                };
                user.EmailConfirmed = true;
                userManager.CreateAsync(user, Configuration.GetValue<string>("Security:DefaultUser:Password")).Wait();
                userManager.AddToRoleAsync(user, "Administrators").Wait();
            }

            if (env.IsDevelopment())
            {
                //{
                //    var user1 = new User
                //    {
                //        UserName = "productadmin",
                //        Name = "Product Admin"
                //    };
                //    userManager.CreateAsync(user1, "1234aA!").Wait();
                //    userManager.AddToRoleAsync(user1, "ProductAdministrators").Wait();
                //}
                //{
                //    var user2 = new User
                //    {
                //        UserName = "orderadmin",
                //        Name = "Order Admin"
                //    };
                //    userManager.CreateAsync(user2, "1234aA!").Wait();
                //    userManager.AddToRoleAsync(user2, "OrderAdministrators").Wait();
                //}
                //{
                //    var user3 = new User
                //    {
                //        UserName = "member",
                //        Name = "Mehmet Sarıçizmeli"
                //    };
                //    userManager.CreateAsync(user3, "1234aA!").Wait();
                //    userManager.AddToRoleAsync(user3, "Members").Wait();
                //}

            }



        }
    }
}
