using EdisonTalk.MongoProxy.Core;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// MongoDB Config Injection
    /// </summary>
    public static IServiceCollection AddMongoProxy(this IServiceCollection services, IConfiguration configuration)
    {
        if (!configuration.GetSection(nameof(MongoDatabaseConfigs)).Exists())
            return services;

        services.Configure<MongoDatabaseConfigs>(configuration.GetSection(nameof(MongoDatabaseConfigs)));
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<MongoDatabaseConfigs>>().Value);
        services.AddSingleton<IMongoDbConnection, MongoDbConnection>();
        services.AddScoped<IMongoDbContext, MongoDbContext>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
