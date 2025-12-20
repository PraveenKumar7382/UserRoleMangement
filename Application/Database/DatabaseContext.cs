using Microsoft.EntityFrameworkCore;
using System;
using Application.Models;

namespace Application.Database
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserSession> UserSessions { get; set; }
    }
}
