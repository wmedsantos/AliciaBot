using Microsoft.EntityFrameworkCore;

namespace AlicIA.Infrastructure.Persistence;

public class AlicIADbContext : DbContext
{
    public AlicIADbContext(DbContextOptions<AlicIADbContext> options) : base(options)
    {
    }
}