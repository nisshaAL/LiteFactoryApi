using Microsoft.EntityFrameworkCore;

namespace LiteFactoryApi.Data;

public sealed class LiteFactoryPostgresDbContext(DbContextOptions<LiteFactoryPostgresDbContext> options)
    : LiteFactoryDbContext(options);
