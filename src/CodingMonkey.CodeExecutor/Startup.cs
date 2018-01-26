
namespace CodingMonkey.CodeExecutor
{
    using System.IdentityModel.Tokens.Jwt;
    using System.IO;

    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    using Serilog;
    using Newtonsoft.Json.Serialization;
    using CodingMonkey.CodeExecutor.Configuration;
    using IdentityServer4.AccessTokenValidation;

    public class Startup
    {
        public IConfiguration Configuration { get; set; }

        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            string applicationPath = env.ContentRootPath;
            Configuration = configuration;

            if(env.IsDevelopment() || env.IsStaging())
            {
                // Create SeriLog
                Log.Logger = new LoggerConfiguration()
                                    .MinimumLevel.Debug()
                                    .WriteTo.RollingFile(Path.Combine(applicationPath, "log_{Date}.txt"))
                                    .CreateLogger();
            }
            else
            {
                //TODO: Sort out production logging
            }
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<IdentityServerConfig>(
                config =>
                {
                    config.Authority = Configuration["IdentityServer:Authority"];
                    config.ScopeName = Configuration["IdentityServer:ScopeName"];
                    config.ScopeSecret = Configuration["IdentityServer:ScopeSecret"];
                });

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
                    .AddIdentityServerAuthentication(options =>
                    {
                        options.Authority = this.Configuration["IdentityServer:Authority"];
                        options.ApiName = this.Configuration["IdentityServer:ScopeName"];
                        options.ApiSecret = this.Configuration["IdentityServer:ScopeSecret"];
                        //options.AutomaticChallenge = true;
                        // Todo: Do not require HTTP while in development. Change to true on release
                        options.RequireHttpsMetadata = false;
                    });

            // Change JSON serialisation to use property names!
            // See: https://weblog.west-wind.com/posts/2016/Jun/27/Upgrading-to-ASPNET-Core-RTM-from-RC2

            services.AddMvc()
                    .AddJsonOptions(opt =>
                    {
                        var resolver = opt.SerializerSettings.ContractResolver;

                        if (resolver != null)
                        {
                            var res = resolver as DefaultContractResolver;
                            res.NamingStrategy = null; // This removes camel casing
                        }
                    });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
            loggerFactory.AddSerilog();

            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseMvc();
        }
    }
}
