using ClientsApp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace ClientsApp.Data
{
    public class ClientsDbContext : DbContext
    {
        
        public ClientsDbContext(DbContextOptions<ClientsDbContext> options) : base(options)
        {
        }
        public DbSet<Client> Client { get; set; }
        public DbSet<Log> Log { get; set; }

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    modelBuilder.Entity<Log>()
        //        .Property(b => b.TimeStamp)
        //        .HasDefaultValueSql("getdate()");
        //}
    }


    public static class DbSetExtensions
    {
        public static T AddIfNotExists<T>(this DbSet<T> dbSet, T entity, Expression<Func<T, bool>> predicate = null) where T : class, new()
        {
            var exists = predicate != null ? dbSet.Any(predicate) : dbSet.Any();    
            return !exists ? dbSet.Add(entity).Entity : null;
        }
    }
}
