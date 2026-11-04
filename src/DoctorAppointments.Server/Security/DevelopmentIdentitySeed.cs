using Microsoft.AspNetCore.Identity;

namespace DoctorAppointments.Server.Security;

public static class DevelopmentIdentitySeed
{
    public static async Task RunAsync(WebApplication app)
    {
        var email = app.Configuration["DevelopmentAdmin:Email"]?.Trim();
        var password = app.Configuration["DevelopmentAdmin:Password"];

        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException(
                "Set DevelopmentAdmin:Email and DevelopmentAdmin:Password in user secrets.");
        }

        await using var scope = app.Services.CreateAsyncScope();

        var roles = scope.ServiceProvider
            .GetRequiredService<RoleManager<IdentityRole>>();

        var users = scope.ServiceProvider
            .GetRequiredService<UserManager<IdentityUser>>();

        foreach (var role in new[] { "Admin", "Doctor", "Patient" })
        {
            if (!await roles.RoleExistsAsync(role))
                RequireSuccess(await roles.CreateAsync(new IdentityRole(role)));
        }

        var admin = await users.FindByEmailAsync(email);

        if (admin is null)
        {
            admin = new IdentityUser
            {
                UserName = email,
                Email = email
            };

            RequireSuccess(await users.CreateAsync(admin, password));
        }
        else if (!await users.IsInRoleAsync(admin, "Admin") &&
                 !await users.CheckPasswordAsync(admin, password))
        {
            throw new InvalidOperationException(
                "An account already uses the development admin email. Choose a fresh email.");
        }

        if (!await users.IsInRoleAsync(admin, "Admin"))
            RequireSuccess(await users.AddToRoleAsync(admin, "Admin"));
    }

    private static void RequireSuccess(IdentityResult result)
    {
        if (result.Succeeded)
            return;

        var errors = string.Join(
            " | ", result.Errors.Select(error => error.Description));

        throw new InvalidOperationException(errors);
    }
}