namespace Chess.Data.Seeding
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using Chess.Common.Constants;
    using Chess.Data.Models;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.DependencyInjection;

    internal class RolesSeeder : ISeeder
    {
        public async Task SeedAsync(ChessDbContext dbContext, IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<RoleEntity>>();

            await SeedRoleAsync(roleManager, CommonConstants.AdministratorRoleName);
        }

        private static async Task SeedRoleAsync(RoleManager<RoleEntity> roleManager, string roleName)
        {
            var role = await roleManager.FindByNameAsync(roleName);
            if (role == null)
            {
                var result = await roleManager.CreateAsync(new RoleEntity(roleName));
                if (!result.Succeeded)
                {
                    throw new Exception(string.Join(Environment.NewLine, result.Errors.Select(e => e.Description)));
                }
            }
        }
    }
}
