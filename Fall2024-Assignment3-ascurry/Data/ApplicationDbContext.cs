﻿using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Fall2024_Assignment3_ascurry.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Fall2024_Assignment3_ascurry.Models.Movie> Movie { get; set; } = default!;

    public DbSet<Fall2024_Assignment3_ascurry.Models.Actor> Actor { get; set; } = default!;

    public DbSet<Fall2024_Assignment3_ascurry.Models.MovieActor> MovieActor { get; set; } = default!;
}

