using System.Net;
using HomeHub.SpotifySort.Extensions;
using HomeHub.SystemUtils.Extensions;
using HomeHub.Web.Configuration;
using HomeHub.Web.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Email;

namespace HomeHub.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            // Configuring Serilog. TODO -> Find better place for this than a constructor.
            var serilogOptions = new SerilogOptions();
            Configuration.Bind("SerilogOptions", serilogOptions);

            var networkCredential = new NetworkCredential(
                serilogOptions.FromEmail,
                serilogOptions.FromEmailPassword);

            var emailConnectionInfo = new EmailConnectionInfo
            {
                Port = serilogOptions.Port,
                FromEmail = serilogOptions.FromEmail,
                ToEmail = serilogOptions.ToEmail,
                EnableSsl = serilogOptions.EnableSsl,
                EmailSubject = serilogOptions.EmailSubject,
                MailServer = serilogOptions.SmtpServer,
                NetworkCredentials = networkCredential
            };

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .WriteTo.Email(emailConnectionInfo, restrictedToMinimumLevel: LogEventLevel.Error)
                .CreateLogger();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.UseSpotifySorterBackgroundService(Configuration);
            services.UseSystemUtils(Configuration);
            services.AddSpaStaticFiles(configuration: options => { options.RootPath = "wwwroot/frontend"});
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
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

            // Configuring App.
            app.UseMiddleware<IpAuthenticationMiddleware>(Configuration["ipAccess:whitelist"]);
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });

            // https://www.freecodecamp.org/news/how-to-build-an-spa-with-vuejs-and-c-using-net-core/
            app.UseSpaStaticFiles();
            app.UseSpa(configuration: builder =>
            {
                if (env.IsDevelopment())
                {
                    builder.UseProxyToSpaDevelopmentServer("http://localhost:8080");
                }
            });
        }
    }
}
