using System;
using Microsoft.EntityFrameworkCore;


namespace gaton.Model;

public class PalyerContext : DbContext
{
    public PalyerContext(DbContextOptions<PalyerContext> options) : base(options)
    {
    }

    public DbSet<Player> Players { get; set; }
}
