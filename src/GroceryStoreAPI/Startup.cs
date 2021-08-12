using FluentValidation.AspNetCore;
using GroceryStoreAPI.Database;
using GroceryStoreAPI.Middleware;
using GroceryStoreAPI.Repositories;
using GroceryStoreAPI.Services;
using GroceryStoreAPI.Validators;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace GroceryStoreAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration) => Configuration = configuration;

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            string dbConnectionString = Configuration.GetConnectionString("GroceryStoreDb");
            services.AddScoped<IDbContext, GroceryStoreDbContext>(_ => new GroceryStoreDbContext(dbConnectionString));
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped(typeof(IService<>), typeof(Service<>));
            
            services.AddControllers(o => o.Filters.Add<ExceptionFilter>())
                .AddFluentValidation(c => c.RegisterValidatorsFromAssemblyContaining<CustomerDtoValidator>());
            services.AddHealthChecks();
            services.AddApiVersioning(c =>
            {
                c.DefaultApiVersion = new ApiVersion(1, 0);
                c.AssumeDefaultVersionWhenUnspecified = true;
                c.ReportApiVersions = true;
            });
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "GroceryStoreAPI", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "GroceryStoreAPI v1"));
            }

            app.UseHealthChecks("/status");
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}