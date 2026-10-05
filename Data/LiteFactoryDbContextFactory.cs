using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace LiteFactoryApi.Data;

public sealed class LiteFactoryDbContextFactory : IDesignTimeDbContextFactory<LiteFactorySqliteDbContext>
{
    public LiteFactorySqliteDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<LiteFactorySqliteDbContext>()
            .UseSqlite("Data Source=data/litefactory.db")
            .Options;

        return new LiteFactorySqliteDbContext(options);
    }
}
