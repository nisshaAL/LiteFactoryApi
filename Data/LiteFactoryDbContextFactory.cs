using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace LiteFactoryApi.Data;

public sealed class LiteFactoryDbContextFactory : IDesignTimeDbContextFactory<LiteFactoryDbContext>
{
    public LiteFactoryDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<LiteFactoryDbContext>()
            .UseSqlite("Data Source=data/litefactory.db")
            .Options;

        return new LiteFactoryDbContext(options);
    }
}
