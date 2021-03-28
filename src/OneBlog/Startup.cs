using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.Extensions.WebEncoders;
using OneBlog.Configuration;
using OneBlog.Core.Services;
using OneBlog.Data;
using OneBlog.Data.Common;
using OneBlog.Data.Contracts;
using OneBlog.Helpers;
using OneBlog.Mvc;
using OneBlog.Services;
using System;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Unicode;

namespace OneBlog
{
    public class Startup
    {

        private IConfiguration _conf { get; }


        public Startup(IConfiguration conf)
        {
            //中文支持
            EncodingProvider provider = CodePagesEncodingProvider.Instance;
            Encoding.RegisterProvider(provider);
            _conf = conf;
        }

        public void ConfigureServices(IServiceCollection svcs)
        {
            //svcs.AddMvcDI();
            //AspNetCoreHelper.ConfigureServices(svcs);
            var builder = new ContainerBuilder();
            var setting = _conf.GetSection("DataSettings");
            svcs.Configure<AppSettings>(_conf.GetSection(nameof(AppSettings)));
            var dbProviderType = setting.GetValue<DataBaseType>(nameof(DataBaseType));
            var connectionString = setting.GetValue<string>("ConnectionString");

            svcs.AddEntityFrameworkCore<AppDbContext>(
                new DataBaseConnection() { ConnectionString = connectionString, DataBaseType = dbProviderType }
            , ServiceLifetime.Transient);

            //   svcs.AddEntityFrameworkSqlite()
            //.AddDbContext<AppDbContext>(options =>
            //{
            //    options.UseSqlite(connectionString);
            //    //options.UseMySqlLolita();
            //}, ServiceLifetime.Transient)
            //.AddUnitOfWork<AppDbContext>();

            svcs.AddUEditorServices(UploadProvider.Bos);
            svcs.AddSession();
            svcs.AddResponseCompression();
            svcs.Configure<GzipCompressionProviderOptions>(options => options.Level = System.IO.Compression.CompressionLevel.Optimal);
            svcs.AddResponseCompression(options =>
            {
                options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[]
                         {
                               "image/svg+xml",
                               "application/atom+xml"
                            }); ;
                options.Providers.Add<GzipCompressionProvider>();
            });

            //解决Multipart body length limit 134217728 exceeded
            svcs.Configure<FormOptions>(x =>
            {
                x.ValueLengthLimit = int.MaxValue;
                x.MultipartBodyLengthLimit = int.MaxValue;
                x.MultipartHeadersLengthLimit = int.MaxValue;
            });

            svcs.AddTransient<IMailService, MailService>();

            svcs.AddIdentity<AppUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 6;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
            }).AddEntityFrameworkStores<AppDbContext>();


            svcs.Configure<WebEncoderOptions>(options =>
            {
                options.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All);
            });

            svcs.Configure<RazorViewEngineOptions>(options =>
            {
                options.ViewLocationExpanders.Add(new ThemeViewLocationExpander());
            });


            svcs.AddSingleton<IUrlHelperFactory, UrlHelperFactory>();
            svcs.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            svcs.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            svcs.AddScoped<ICultureService, CultureService>();
            svcs.AddScoped<IPostService, PostService>();
            svcs.AddScoped<ITagService, TagService>();
            svcs.AddScoped<ICategoryService, CategoryService>();

            svcs.AddScoped<IViewRenderService, ViewRenderService>();
            svcs.AddScoped<IRolesRepository, RolesRepository>();
            svcs.AddScoped<IUsersRepository, UsersRepository>();

            svcs.AddTransient<DataMapper>();
            svcs.AddTransient<AppInitializer>();
            svcs.AddScoped<NavigationHelper>();
            svcs.AddTransient<ApplicationEnvironment>();

            // Supporting Live Writer (MetaWeblogAPI)
            //svcs.AddMetaWeblog<WeblogProvider>();

            // Add Caching Support
            svcs.AddMemoryCache(opt => opt.ExpirationScanFrequency = TimeSpan.FromMinutes(5));

            svcs.AddRazorPages();
            svcs.AddControllers().AddJsonOptions(o =>
            {
                o.JsonSerializerOptions.PropertyNamingPolicy = null;
                o.JsonSerializerOptions.DictionaryKeyPolicy = null;
                o.JsonSerializerOptions.PropertyNameCaseInsensitive = false;
            });

            //if (_env.IsProduction())
            //{
            //    mvcBuilder.AddMvcOptions(options => options.Filters.Add(new RequireOnlyDoaminAttribute() { Host = "dahuangshu.com" }));
            //}

            builder.Populate(svcs);
            IocContainer.RegisterAutofac(builder);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app,
                              IServiceScopeFactory scopeFactory, IOptions<AppSettings> appSettings, IWebHostEnvironment env)
        {

            app.UseResponseCompression();
            app.UseSession();
            // Add the following to the request pipeline only in development environment.
            if (env.IsDevelopment())
            {
                //loggerFactory.AddDebug(LogLevel.Information);
                app.UseDeveloperExceptionPage();
                //app.UseDatabaseErrorPage();
            }
            else
            {
                // Early so we can catch the StatusCode error
                app.UseStatusCodePagesWithReExecute("/Error/{0}");
                app.UseExceptionHandler("/Exception");
            }

            app.UseStaticFiles();
            // Support MetaWeblog API
            //app.UseMetaWeblog("/livewriter");
            // Keep track of Active # of users for Vanity Project
            app.UseMiddleware<ActiveUsersMiddleware>();

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                //endpoints.MapControllerRoute(
                //        name: "default",
                //        pattern: "{controller=Home}/{action=Index}/{id?}");

                endpoints.MapAreaControllerRoute(
                           name: "areas", "areas",
                           pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
                endpoints.MapControllers();
            });

            if (appSettings.Value.TestData)
            {
                using (var scope = scopeFactory.CreateScope())
                {
                    var initializer = scope.ServiceProvider.GetService<AppInitializer>();
                    initializer.SeedAsync().Wait();
                }
            }
        }
    }
}
