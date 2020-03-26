using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;


namespace DatingApp.API.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }
        public DbSet<Value> Values { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Photo> Photos { get; set; }
        public DbSet<like> Likes { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            //  primary key for table like
            builder.Entity<like>().HasKey(k => new { k.LikerId, k.LikeeId });

            // relationsheb bettwen tables Like and User 
            builder.Entity<like>()
                .HasOne(u => u.Likee)
                .WithMany(u => u.Likers)
                .HasForeignKey(u => u.LikeeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<like>()
                 .HasOne(u => u.Liker)
                 .WithMany(u => u.Likees)
                 .HasForeignKey(u => u.LikerId)
                 .OnDelete(DeleteBehavior.Restrict);



        }

    }
}