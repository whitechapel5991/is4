using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace api
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
            Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;

            services.AddControllers();

            //services
            //    .AddMvcCore()
            //    .AddAuthorization()
            //    .SetCompatibilityVersion(CompatibilityVersion.Latest);

            // accepts any access token issued by identity server
            services.AddAuthentication("Bearer")
                .AddJwtBearer("Bearer", options =>
                {
                    options.Authority = Configuration["Authority"] ?? "http://localhost:5000";
                    //options.RequireHttpsMetadata = false;
                    //options.Audience = "api";
                    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                    {
                        ValidateAudience = false,
                    };
                });

            // adds an authorization policy to make sure the token is for scope 'api1'
            services.AddAuthorization(options =>
            {
                options.AddPolicy("ApiScope", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim("scope", "api.read");
                });
            });

            //services.AddAuthentication("Bearer")
            //    .AddIdentityServerAuthentication("Bearer", options =>
            //    {
            //        options.Authority = Configuration["Authority"] ?? "https://localhost:5000";
            //        options.ApiName = "api";
            //    });

            //services.AddAuthorization();

            services.AddCors(setup =>
            {
                setup.AddDefaultPolicy(policy =>
                {
                    policy
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowAnyOrigin();
                });
            });
            //services.AddAuthorization(options =>
            //{
            //    options.AddPolicy("ApiScope", policy =>
            //    {
            //        policy.RequireAuthenticatedUser();
            //        policy.RequireClaim("scope", "identity.api");
            //    });
            //});
            //services.AddCors(setup =>
            //{
            //    setup.AddDefaultPolicy(policy =>
            //    {
            //        policy
            //            .AllowAnyHeader()
            //            .AllowAnyMethod()
            //            .AllowAnyOrigin();
            //    });
            //});


        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseCors();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers()
                    .RequireAuthorization("ApiScope");
            });

        }
    }
}
