using ShopDongY.Models;

namespace ShopDongY.Data
{
    public static class DataSeeder
    {
        public static void SeedRoles(ShopDongYContext context)
        {
            if (!context.Roles.Any())
            {
                var roles = new List<RoleModel>
                {
                    new RoleModel { RoleName = "Admin" },
                    new RoleModel { RoleName = "Customer" }
                };

                context.Roles.AddRange(roles);
                context.SaveChanges();
            }
        }
    }
}
