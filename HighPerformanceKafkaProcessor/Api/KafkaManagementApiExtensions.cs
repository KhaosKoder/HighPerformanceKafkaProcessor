using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace HighPerformanceKafkaProcessor.Api
{
    public static class KafkaManagementApiExtensions
    {
        public static IServiceCollection AddKafkaManagementApi(this IServiceCollection services)
        {
            services.AddControllers()
                    .AddApplicationPart(typeof(KafkaManagementController).Assembly)
                    .AddControllersAsServices();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Kafka Management API",
                    Version = "v1",
                    Description = "API for managing Kafka topic group consumers"
                });
            });

            return services;
        }

        public static IApplicationBuilder UseKafkaManagementApi(this IApplicationBuilder app)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Kafka Management API V1");
            });

            return app;
        }
    }
}
