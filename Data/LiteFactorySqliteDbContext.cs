using Microsoft.EntityFrameworkCore;

namespace LiteFactoryApi.Data;

public sealed class LiteFactorySqliteDbContext(DbContextOptions<LiteFactorySqliteDbContext> options)
    : LiteFactoryDbContext(options);
