using Microsoft.EntityFrameworkCore;
using UserTask.Models;

namespace UserTask.Context
{
    public class MainContext : DbContext
    {
        public MainContext(DbContextOptions options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Role> Roles { get; set; }
    }
}
