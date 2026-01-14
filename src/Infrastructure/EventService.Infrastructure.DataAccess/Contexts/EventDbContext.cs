using EventService.Application.Models.Artists;
using EventService.Application.Models.Categories;
using EventService.Application.Models.Events;
using EventService.Application.Models.Organizers;
using EventService.Application.Models.Schemes;
using EventService.Application.Models.Venues;
using Microsoft.EntityFrameworkCore;

namespace EventService.Infrastructure.DataAccess.Contexts;

public class EventDbContext : DbContext
{
    public EventDbContext(DbContextOptions<EventDbContext> options)
        : base(options)
    {
    }

    public DbSet<EventEntity> Events => Set<EventEntity>();

    public DbSet<Artist> Artists => Set<Artist>();

    public DbSet<Venue> Venues => Set<Venue>();

    public DbSet<HallScheme> HallSchemes => Set<HallScheme>();

    public DbSet<Category> Categories => Set<Category>();

    public DbSet<Organizer> Organizers => Set<Organizer>();

    public DbSet<EventOrganizer> EventOrganizers => Set<EventOrganizer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureEvent(modelBuilder);
        ConfigureArtist(modelBuilder);
        ConfigureVenue(modelBuilder);
        ConfigureHallScheme(modelBuilder);
        ConfigureCategory(modelBuilder);
        ConfigureOrganizer(modelBuilder);
    }

    private static void ConfigureEvent(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EventEntity>(builder =>
        {
            builder.ToTable("events");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.HasOne(e => e.Category)
                .WithMany(c => c.Events)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.Venue)
                .WithMany()
                .HasForeignKey(e => e.VenueId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureArtist(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Artist>(builder =>
        {
            builder.ToTable("artists");
            builder.HasKey(a => a.Id);

            builder.Property(a => a.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder
                .HasMany(a => a.Events)
                .WithMany(e => e.Artists)
                .UsingEntity(j =>
                    j.ToTable("event_artists"));
        });
    }

    private static void ConfigureVenue(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Venue>(builder =>
        {
            builder.ToTable("venues");
            builder.HasKey(v => v.Id);

            builder.Property(v => v.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(v => v.Address)
                .IsRequired()
                .HasMaxLength(400);
        });
    }

    private static void ConfigureHallScheme(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<HallScheme>(builder =>
        {
            builder.ToTable("hall_schemes");
            builder.HasKey(h => h.Id);

            builder.HasOne(h => h.Venue)
                .WithMany(v => v.HallSchemes)
                .HasForeignKey(h => h.VenueId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureCategory(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(builder =>
        {
            builder.ToTable("categories");
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(150);
        });
    }

    private static void ConfigureOrganizer(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Organizer>(builder =>
        {
            builder.ToTable("organizers");
            builder.HasKey(o => o.Id);
            builder.Property(o => o.Name).IsRequired();

            modelBuilder.Entity<EventOrganizer>(eo =>
            {
                eo.ToTable("event_organizers");

                eo.HasKey(x => x.Id);

                eo.HasOne(x => x.EventEntity)
                    .WithMany(e => e.Organizers)
                    .HasForeignKey(x => x.EventId)
                    .OnDelete(DeleteBehavior.Cascade);

                eo.HasOne(x => x.Organizer)
                    .WithMany(o => o.Events)
                    .HasForeignKey(x => x.OrganizerId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        });
    }
}