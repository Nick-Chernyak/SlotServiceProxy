using System.Reflection;
using DryIoc.Microsoft.DependencyInjection;
using MediatR;
using Microsoft.OpenApi.Models;
using SlotServiceProxy.Domain;
using SlotServiceProxy.Infrastructure.DependencyInjection;
using SlotServiceProxy.Middlewares;
using Swashbuckle.AspNetCore.Filters;
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

            swaggerGenOptions.ExampleFilters();
            swaggerGenOptions.IncludeXmlComments(XmlCommentsFilePath("Application"));
            //Default => means Web project.
            swaggerGenOptions.IncludeXmlComments(XmlCommentsFilePath());
            swaggerGenOptions.EnableAnnotations();
            swaggerGenOptions.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "SlotServiceProxy API",
                Version = "v1",
                Description = "API for fetching doctor (facility) calendar and reserving appointments " +
                              "(slots) for patients. This API is a proxy for the 'slot services API's' " +
                              "(used Dralia tech test API in this version).",
                Contact = new OpenApiContact
                {
                    Name = "Nick Charniak",
                    Email = "mikola.charniak@gmail.com"
                }
            });
        });
        builder.Services.AddSwaggerExamplesFromAssemblies(Assembly.GetEntryAssembly());
        
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
                c.DefaultModelExpandDepth(-1);
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
    
    //Very simple configuration for this test task. Not production code for sure.
    //In a case if we have more than one slot service, we should use some kind of strategy pattern with configuration.
    private static void SetDraliaCredentials(IConfiguration configuration)
    {
        var draliaCredentialsSection = configuration.GetSection(nameof(DummyConfigurator.DraliaCredentials));
        var draliaCredentials = draliaCredentialsSection.GetChildren()
            .ToDictionary(x => x.Key, x => x.Value);
        DummyConfigurator.DraliaCredentials = draliaCredentials;
    }

    private static string XmlCommentsFilePath(string? projectName = null)
    {
        var basicPath = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}");
        return projectName is null ? $"{basicPath}.xml" : $"{basicPath}.{projectName}.xml";
    }
}