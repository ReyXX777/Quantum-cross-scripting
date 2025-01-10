using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using QuantumCrossScripting.Data;  // Replace with the appropriate namespace for ApplicationDbContext
using System.IO;
using System.Reflection;

public class Startup
{
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
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "QuantumCrossScripting API",
                Version = "v1",
                Description = "API for QuantumCrossScripting application",
                Contact = new OpenApiContact
                {
                    Name = "Support",
                    Email = "support@quantumcrossscripting.com"
                }
            });

            // Include XML comments in Swagger documentation
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            c.IncludeXmlComments(xmlPath);
        });

        // Add CORS policy
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAllOrigins", builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyHeader()
                       .AllowAnyMethod();
            });
        });

        // Add health checks
        services.AddHealthChecks();

        // Add response compression
        services.AddResponseCompression();
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
        app.UseCors("AllowAllOrigins");
        app.UseAuthorization();
        app.UseResponseCompression();

        // Serve static files
        app.UseStaticFiles();

        // Map health checks
        app.UseHealthChecks("/health");

        // Map controllers
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}
