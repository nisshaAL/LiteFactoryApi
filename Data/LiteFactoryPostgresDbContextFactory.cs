using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace LiteFactoryApi.Data;

public sealed class LiteFactoryPostgresDbContextFactory : IDesignTimeDbContextFactory<LiteFactoryPostgresDbContext>
{
    public LiteFactoryPostgresDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<LiteFactoryPostgresDbContext>()
            .UseNpgsql("Host=localhost;Database=litefactory_design_time;Username=litefactory_design_time;SSL Mode=Disable")
            .Options;

        return new LiteFactoryPostgresDbContext(options);
    }
}
