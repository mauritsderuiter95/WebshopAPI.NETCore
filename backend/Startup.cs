using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using backend.Services;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using backend.Models;
using Hangfire;
using Hangfire.Mongo;
using Microsoft.AspNetCore.Http;
using System.Configuration;

namespace backend
{
  public class Startup
  {
    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration)
    {
      Configuration = configuration;
      _configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
      var migrationOptions = new MongoMigrationOptions
      {
        Strategy = MongoMigrationStrategy.Migrate,
        BackupStrategy = MongoBackupStrategy.Collections
      };
      var storageOptions = new MongoStorageOptions
      {
        // ...
        MigrationOptions = migrationOptions,
        CheckConnection = false
      };

      //var connectionKey = "MONGODB_CONNECTION";

      string connection = Configuration.GetConnectionString("WrautomatenDb");

      services.AddHangfire(config => config.UseMongoStorage(connection, "Hangfire", storageOptions));

      services.AddCors(options =>
      {
        options.AddPolicy("AllowSome",
              builder =>
              {
                builder.WithOrigins("http://localhost:8081").AllowAnyMethod().AllowAnyHeader().AllowCredentials().SetPreflightMaxAge(TimeSpan.FromSeconds(3600));
                builder.WithOrigins("http://localhost:8082").AllowAnyMethod().AllowAnyHeader().AllowCredentials().SetPreflightMaxAge(TimeSpan.FromSeconds(3600));
                builder.WithOrigins("http://192.168.1.102:8081").AllowAnyMethod().AllowAnyHeader().AllowCredentials().SetPreflightMaxAge(TimeSpan.FromSeconds(3600));
                builder.WithOrigins("http://192.168.1.102:8082").AllowAnyMethod().AllowAnyHeader().AllowCredentials().SetPreflightMaxAge(TimeSpan.FromSeconds(3600));
                builder.WithOrigins("https://bestellen.wrautomaten.nl").AllowAnyMethod().AllowAnyHeader().AllowCredentials().SetPreflightMaxAge(TimeSpan.FromSeconds(3600));
                builder.WithOrigins("https://admin.wrautomaten.nl").AllowAnyMethod().AllowAnyHeader().AllowCredentials().SetPreflightMaxAge(TimeSpan.FromSeconds(3600));
                builder.WithOrigins("https://wr-automaten.nl").AllowAnyMethod().AllowAnyHeader().AllowCredentials().SetPreflightMaxAge(TimeSpan.FromSeconds(3600));
                builder.WithOrigins("https://www.wr-automaten.nl").AllowAnyMethod().AllowAnyHeader().AllowCredentials().SetPreflightMaxAge(TimeSpan.FromSeconds(3600));
                builder.WithOrigins("https://www.wrautomaten.nl").AllowAnyMethod().AllowAnyHeader().AllowCredentials().SetPreflightMaxAge(TimeSpan.FromSeconds(3600));
                builder.WithOrigins("https://wrautomaten.nl").AllowAnyMethod().AllowAnyHeader().AllowCredentials().SetPreflightMaxAge(TimeSpan.FromSeconds(3600));
                builder.WithOrigins("http://wrnuxt.azurewebsites.net").AllowAnyMethod().AllowAnyHeader().AllowCredentials().SetPreflightMaxAge(TimeSpan.FromSeconds(3600));
                builder.WithOrigins("https://wrnuxt.azurewebsites.net").AllowAnyMethod().AllowAnyHeader().AllowCredentials().SetPreflightMaxAge(TimeSpan.FromSeconds(3600));
              });
      });

      services.AddScoped<ProductService>();
      services.AddScoped<CartService>();
      services.AddScoped<OrderService>();
      services.AddScoped<UploadService>();
      services.AddScoped<MachineService>();
      services.AddScoped<VerificationService>();
      services.AddScoped<MailService>();
      services.AddScoped<SchedulerService>();
      services.AddScoped<CallBackService>();
      services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

      //services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

      services.AddControllers();

      // configure strongly typed settings objects
      var appSettingsSection = Configuration.GetSection("AppSettings");
      services.Configure<AppSettings>(appSettingsSection);

      // configure jwt authentication
      var appSettings = appSettingsSection.Get<AppSettings>();
      var key = Encoding.ASCII.GetBytes(appSettings.Secret);
      // var key = Encoding.ASCII.GetBytes(Configuration.GetValue<string>("SECRET"));

      services.AddAuthentication(x =>
      {
        x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
      })
      .AddJwtBearer(x =>
      {
        x.RequireHttpsMetadata = false;
        x.SaveToken = true;
        x.TokenValidationParameters = new TokenValidationParameters
        {
          ValidateIssuerSigningKey = true,
          IssuerSigningKey = new SymmetricSecurityKey(key),
          ValidateIssuer = false,
          ValidateAudience = false
        };
      });

      // configure DI for application services
      services.AddScoped<UserService>();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
      app.UseCors("AllowSome");

      if (env.IsDevelopment())
        app.UseDeveloperExceptionPage();
      else
        app.UseHsts();

      app.UseStaticFiles();

      app.UseRouting();

      app.UseDeveloperExceptionPage();
      //app.UseDatabaseErrorPage();

      //app.UseAuthentication();
      app.UseAuthorization();

      app.UseHttpsRedirection();
      // app.UseMvc();
      app.UseEndpoints(endpoints => endpoints.MapControllers());

      app.UseHangfireServer();
      app.UseHangfireDashboard("/hangfire", new DashboardOptions()
      {
        Authorization = new[] { new HangfireAuthorizationFilter() },
        AppPath = "https://www.wrautomaten.nl"
      });

      SchedulerService schedulerService = new SchedulerService(_configuration);
      schedulerService.Startup();
    }
  }
}
