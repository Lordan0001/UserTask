using Microsoft.EntityFrameworkCore;
using UserTask.Models;

namespace UserTask.Context
{
    /// <summary>
    /// Represents the main database context for the UserTask application.
    /// </summary>
    public class MainContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MainContext"/> class.
        /// </summary>
        /// <param name="options">The database context options.</param>
        public MainContext(DbContextOptions options) : base(options) { }

        /// <summary>
        /// Gets or sets the collection of User entities in the database.
        /// </summary>
        public DbSet<User> Users { get; set; }

        /// <summary>
        /// Gets or sets the collection of UserRole entities in the database.
        /// </summary>
        public DbSet<UserRole> UserRoles { get; set; }

        /// <summary>
        /// Gets or sets the collection of Role entities in the database.
        /// </summary>
        public DbSet<Role> Roles { get; set; }
    }
}
