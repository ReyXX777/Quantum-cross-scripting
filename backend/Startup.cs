using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QuantumCrossScripting.Data;  // Replace with the appropriate namespace for ApplicationDbContext
using Swashbuckle.AspNetCore.SwaggerGen;

public class Startup
{
    // IConfiguration is injected via the constructor to access appsettings
    public IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        // Add DbContext with connection string from appsettings.json
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

        // Register controllers
        services.AddControllers();

        // Register Swagger services
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "QuantumCrossScripting API",
                Version = "v1"
            });
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        // Configure the HTTP request pipeline
        if (env.IsDevelopment())
        {
            // Use the Developer Exception Page in development
            app.UseDeveloperExceptionPage();
            
            // Enable Swagger in development environment
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "API V1");
                c.RoutePrefix = string.Empty; // Set Swagger UI to be served at the root
            });
        }
        else
        {
            // Handle production environment error page
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        // Other middleware
        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseAuthorization();

        // Map controllers
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}
