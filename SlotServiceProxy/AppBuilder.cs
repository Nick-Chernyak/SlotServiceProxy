using DryIoc.Microsoft.DependencyInjection;
using MediatR;
using Microsoft.OpenApi.Models;
using SlotServiceProxy.Domain;
using SlotServiceProxy.Infrastructure;
using SlotServiceProxy.Infrastructure.DependencyInjection;
using SlotServiceProxy.Middlewares;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace SlotServiceProxy;

public static class AppBuilder
{
    public static WebApplicationBuilder ConfigureBuilder(this WebApplicationBuilder builder)
    {
        
        var container = SlotsServiceProxyCompositionRoot.Build();
        builder.Host.UseServiceProviderFactory(new DryIocServiceProviderFactory(container));
        
        builder.Services.AddMediatR(AppDomain.CurrentDomain.GetAssemblies());
        builder.Services.RegisterMediatR();

        builder.Services.AddHttpContextAccessor();
        
        //Json options set null to avoid lower case property names in response.
        builder.Services.AddControllers().AddJsonOptions(opts => opts.JsonSerializerOptions.PropertyNamingPolicy = null);;

        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddSwaggerGen(swaggerGenOptions =>
        {
            swaggerGenOptions.DescribeAllParametersInCamelCase();
            
            swaggerGenOptions.EnableAnnotations();
            swaggerGenOptions.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "SlotServiceProxy API",
                Version = "v1",
            });
        });
        
        SetDraliaCredentials(builder.Configuration);
        
        return builder;
    }

    public static WebApplication ConfigureApplication(this WebApplication app)
    {
        app.UseMiddleware<ErrorHandlingMiddleware>();
        var isDevelopment = app.Environment.IsDevelopment();


        if (isDevelopment)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.DefaultModelExpandDepth(5);
                c.DefaultModelRendering(ModelRendering.Example);
                c.DisplayRequestDuration();
                c.DocExpansion(DocExpansion.List);
                c.EnableDeepLinking();
                c.ShowExtensions();
                c.ShowCommonExtensions();
                c.EnableValidator();
            });
        }

        app.UseHttpsRedirection();
        app.MapControllers();

        return app;
    }
    
    private static void SetDraliaCredentials(IConfiguration configuration)
    {
        var draliaCredentialsSection = configuration.GetSection(nameof(DummyConfigurator.DraliaCredentials));
        var draliaCredentials = draliaCredentialsSection.GetChildren()
            .ToDictionary(x => x.Key, x => x.Value);
        DummyConfigurator.DraliaCredentials = draliaCredentials;
    }
}