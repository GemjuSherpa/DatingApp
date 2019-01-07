using Microsoft.EntityFrameworkCore;
using DatingApp.API.Models;

namespace DatingApp.API.Data
{
  public class DataContext : DbContext
  {
    //constructor
    public DataContext(DbContextOptions<DataContext> options) : base(options) { }

    //Properties: Models
    public DbSet<Value> Values { get; set; }

    public DbSet<User> Users { get; set; }
    public DbSet<Photo> Photos { get; set; }
  }
}