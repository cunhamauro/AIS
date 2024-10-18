using AIS.Data;
using AIS.Data.Entities;
using AIS.Data.Repositories;
using AIS.Helpers;
using AIS.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Syncfusion.Licensing;
using System;
using System.Text;

namespace AIS
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
            services.AddIdentity<User, IdentityRole>(cfg =>
            {
                cfg.Tokens.AuthenticatorTokenProvider = TokenOptions.DefaultAuthenticatorProvider;
                cfg.SignIn.RequireConfirmedEmail = true;
                cfg.User.RequireUniqueEmail = true;

                // Configure Password settings
                cfg.Password.RequireDigit = true;
                cfg.Password.RequireLowercase = true;
                cfg.Password.RequireUppercase = true;
                cfg.Password.RequiredLength = 6;
                cfg.Password.RequireNonAlphanumeric = false;
                cfg.Password.RequiredUniqueChars = 1;

                // Configure Lockout settings
                cfg.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15); // Account locked out for 15 minutes after 3 failed attempts
                cfg.Lockout.MaxFailedAccessAttempts = 3; // Number of failed attempts before lockout
                cfg.Lockout.AllowedForNewUsers = true; // Lockout is allowed for new users
            })
                .AddDefaultTokenProviders()
                .AddEntityFrameworkStores<DataContext>();

            // Authentication Token
            services.AddAuthentication().AddCookie().AddJwtBearer(cfg =>
            {
                cfg.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = this.Configuration["Tokens:Issuer"],
                    ValidAudience = this.Configuration["Tokens:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(this.Configuration["Tokens:Key"]))
                };
            });

            services.AddDbContext<DataContext>(cfg =>
            {
                cfg.UseSqlServer(this.Configuration.GetConnectionString("AIS_Connection"));
            });

            services.AddTransient<SeedDatabase>();

            // Services
            services.AddHttpClient<ICountriesAPIService>(client =>
            {
                client.BaseAddress = new Uri("https://restcountries.com");
            });
            services.AddHttpClient();
            services.AddScoped<ICountriesAPIService, CountriesAPIService>();
            services.AddScoped<IAirportsAPIService, AirportsAPIService>();
            services.AddScoped<IAircraftAvailabilityService, AircraftAvailabilityService>();
            services.AddScoped<IImagesAPIService, ImagesAPIService>();
            services.AddHostedService<FlightCleanupService>();

            // Repositories
            services.AddScoped<IAircraftRepository, AircraftRepository>();
            services.AddScoped<IAirportRepository, AirportRepository>();
            services.AddScoped<IFlightRepository, FlightRepository>();
            services.AddScoped<ITicketRepository, TicketRepository>();
            services.AddScoped<ITicketRecordRepository, TicketRecordRepository>();
            services.AddScoped<IFlightRecordRepository, FlightRecordRepository>();

            // Helpers
            services.AddScoped<IUserHelper, UserHelper>();
            services.AddScoped<IImageHelper, ImageHelper>();
            services.AddScoped<IConverterHelper, ConverterHelper>();
            services.AddScoped<IMailHelper, MailHelper>();
            services.AddScoped<IPdfHelper, PdfHelper>();
            services.AddScoped<IQrCodeHelper, QrCodeHelper>();

            string syncfusionLicenseKey = Configuration[@"Syncfusion:LicenseKey"];
            SyncfusionLicenseProvider.RegisterLicense(syncfusionLicenseKey);

            services.ConfigureApplicationCookie(options =>
            {
                //options.LoginPath = "/Account/NotAuthorized";
                options.AccessDeniedPath = "/Account/NotAuthorized";

                // Check the users security stamp
                options.Events.OnValidatePrincipal = async context =>
                {
                    var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<User>>();
                    var signInManager = context.HttpContext.RequestServices.GetRequiredService<SignInManager<User>>();
                    var user = await userManager.GetUserAsync(context.Principal);

                    if (user == null)
                    {
                        // And logout that user if its no longer valid
                        await signInManager.SignOutAsync();
                        context.RejectPrincipal();
                    }
                };
            });

            services.AddControllersWithViews();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Shared/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseStatusCodePagesWithReExecute("/Home/PageNotFound");

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
