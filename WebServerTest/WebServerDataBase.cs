using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;


namespace WebServerTest
{
    class WebServerDataBase : DbContext
    {
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<UserProfile> UserProfiles { get; set; } = null!;
        public WebServerDataBase()
        {
            //Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=helloapp.db");
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasOne(u => u.Profile).WithOne(p => p.User).HasForeignKey<UserProfile>(p => p.UserId);
        }
    }

    internal class User
    {
        public int Id { get; set; }
        public string login { get; set; }
        public string password { get; set; }
        public string Identifier { get; set; }
        public UserProfile Profile { get; set; }

    }
    internal class UserProfile
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Lastname { get; set; }
        public int Age { get; set; }
        public string City { get; set; }
        public string Language { get; set; }
        public string Bio { get; set; }
        public DateTime Birthday { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
    }
}
