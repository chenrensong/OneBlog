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
using Microsoft.EntityFrameworkCore.Providers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.Extensions.WebEncoders;
using OneBlog.Configuration;
using OneBlog.Core.Services;
using OneBlog.Data;
using OneBlog.Data.Common;
using OneBlog.Data.Contracts;
using OneBlog.Helpers;
using OneBlog.Logger;
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

        private IHostingEnvironment _env { get; }
        private IConfiguration _conf { get; }


        public Startup(IHostingEnvironment env, IConfiguration conf)
        {
            //中文支持
            EncodingProvider provider = CodePagesEncodingProvider.Instance;
            Encoding.RegisterProvider(provider);
            _env = env;
            _conf = conf;
        }

        public IServiceProvider ConfigureServices(IServiceCollection svcs)
        {
            //svcs.AddMvcDI();
            //AspNetCoreHelper.ConfigureServices(svcs);
            var builder = new ContainerBuilder();
            var setting = _conf.GetSection("DataSettings");
            svcs.Configure<AppSettings>(_conf.GetSection(nameof(AppSettings)));
            var dbProviderType = setting.GetValue<DbProviderType>(nameof(DbProviderType));
            var connectionString = setting.GetValue<string>("ConnectionString");
            DataProviderFactory.Configure(dbProviderType, connectionString);

            svcs.AddUEditorServices(UploadProvider.Bos);
            /////Node支持
            //svcs.AddNodeServices();
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

            if (_env.IsDevelopment())
            {
                svcs.AddTransient<IMailService, LoggingMailService>();
            }
            else
            {
                svcs.AddTransient<IMailService, MailService>();
            }

            svcs.AddDbContext<AppDbContext>(ServiceLifetime.Scoped);

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


            svcs.AddUnitOfWork<AppDbContext>();

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
            //// Add MVC to the container
            var mvcBuilder = svcs.AddMvc();
            mvcBuilder.AddJsonOptions(r =>
            {
                r.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver();
                r.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
            });
            //var mvcCore = svcs.AddMvcCore();
            //mvcBuilder.AddJsonOptions(opts => opts.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver());
            //mvcCore.AddJsonFormatters(options => options.ContractResolver = new CamelCasePropertyNamesContractResolver());
            // Add Https - renable once Azure Certs work
            if (_env.IsProduction())
            {
                mvcBuilder.AddMvcOptions(options => options.Filters.Add(new RequireOnlyDoaminAttribute() { Host = "dahuangshu.com" }));
            }

            builder.Populate(svcs);
            return IocContainer.RegisterAutofac(builder);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app,
                              ILoggerFactory loggerFactory,
                              IMailService mailService,
                              IServiceScopeFactory scopeFactory, IOptions<AppSettings> appSettings)
        {

            app.UseResponseCompression();
            app.UseSession();
            // Add the following to the request pipeline only in development environment.
            if (_env.IsDevelopment())
            {
                loggerFactory.AddDebug(LogLevel.Information);
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                // Support logging to email
                loggerFactory.AddEmail(mailService, LogLevel.Critical);
                loggerFactory.AddConsole(LogLevel.Error);

                // Early so we can catch the StatusCode error
                app.UseStatusCodePagesWithReExecute("/Error/{0}");
                app.UseExceptionHandler("/Exception");
            }

            app.UseStaticFiles();
            // Support MetaWeblog API
            //app.UseMetaWeblog("/livewriter");
            // Keep track of Active # of users for Vanity Project
            app.UseMiddleware<ActiveUsersMiddleware>();

            app.UseAuthentication();

            app.UseMvc();

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
