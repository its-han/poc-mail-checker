using Microsoft.EntityFrameworkCore;

namespace MyBackgroundService;

public class CheckerDbContext: DbContext
{
    public CheckerDbContext(DbContextOptions<CheckerDbContext> options) : base(options)
    {
        
    }
    
    public DbSet<ProducerEntity> Producer { get; set; }
    public DbSet<ConsumerEntity> Consumer { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<ProducerEntity>() 
            .HasIndex(p => p.Uuid)
            .IsUnique(false);  
        
        modelBuilder.Entity<ConsumerEntity>() 
            .HasIndex(p => p.Uuid)
            .IsUnique(false);  
    }
}