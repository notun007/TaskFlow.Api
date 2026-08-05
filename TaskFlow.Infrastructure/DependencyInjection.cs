using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Oracle.EntityFrameworkCore;
using TaskFlow.Application.Abstractions;
using TaskFlow.Application.Tasks;
using TaskFlow.Infrastructure.Identity;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<TaskFlowDbContext>(options => options.UseOracle(
            configuration.GetConnectionString("Oracle"),
            oracleOptions => oracleOptions.UseOracleSQLCompatibility(OracleSQLCompatibility.DatabaseVersion19)));
        services.AddIdentityCore<ApplicationUser>(options => { options.User.RequireUniqueEmail = true; }).AddRoles<Microsoft.AspNetCore.Identity.IdentityRole<Guid>>().AddEntityFrameworkStores<TaskFlowDbContext>();
        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<TaskFlowDbContext>());
        services.AddScoped<ITaskService, TaskService>();
        return services;
    }
}
