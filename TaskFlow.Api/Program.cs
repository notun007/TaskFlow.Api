using TaskFlow.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;
using TaskFlow.Infrastructure.Identity;

var builder = WebApplication.CreateBuilder(args);

// Avoid the Windows Event Log provider in local/console hosting. A logging
// permission failure must never turn an otherwise valid API request into 500.
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddControllers().AddJsonOptions(options =>
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddHealthChecks();
builder.Services.AddInfrastructure(builder.Configuration);
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is required.");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});
builder.Services.AddCors(options => options.AddPolicy("Web", policy => policy.WithOrigins(builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? ["http://localhost:4200"]).AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    var administratorEmail = app.Configuration["BootstrapAdministrator"]?.Trim();
    if (!string.IsNullOrWhiteSpace(administratorEmail))
    {
        using var scope = app.Services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        const string administratorRole = "Administrator";
        if (!await roleManager.RoleExistsAsync(administratorRole))
        {
            var roleResult = await roleManager.CreateAsync(new IdentityRole<Guid>(administratorRole));
            if (!roleResult.Succeeded)
                throw new InvalidOperationException($"Administrator role could not be created: {string.Join(", ", roleResult.Errors.Select(error => error.Description))}");
        }

        var administrator = await userManager.FindByEmailAsync(administratorEmail);
        if (administrator is null)
            app.Logger.LogWarning("Bootstrap administrator account {Email} does not exist.", administratorEmail);
        else if (!await userManager.IsInRoleAsync(administrator, administratorRole))
        {
            var assignmentResult = await userManager.AddToRoleAsync(administrator, administratorRole);
            if (!assignmentResult.Succeeded)
                throw new InvalidOperationException($"Administrator role could not be assigned: {string.Join(", ", assignmentResult.Errors.Select(error => error.Description))}");
            app.Logger.LogInformation("Development administrator role assigned to {Email}.", administratorEmail);
        }
    }
}
app.UseCors("Web");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");
app.Run();
