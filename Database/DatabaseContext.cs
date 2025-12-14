using Microsoft.EntityFrameworkCore;
using System;
using UserRoleMangement.Models;

namespace UserRoleMangement.Database
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
    }
}
