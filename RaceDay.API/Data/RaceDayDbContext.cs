using Microsoft.EntityFrameworkCore;
using RaceDay.API.Models;

namespace RaceDay.API.Data;

public class RaceDayDbContext : DbContext
{
    public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Organiser> Organisers => Set<Organiser>();
    public DbSet<Participant> Participants => Set<Participant>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<EventEnrolment> EventEnrolments => Set<EventEnrolment>();
    public DbSet<Result> Results => Set<Result>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // User
        modelBuilder.Entity<User>().HasIndex(u => u.Username).IsUnique();
        modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
        modelBuilder.Entity<User>().HasCheckConstraint("CK_User_Role", "[Role] IN ('Organiser', 'Participant')");

        // Organiser
        modelBuilder.Entity<Organiser>()
            .HasOne(o => o.User).WithOne(u => u.Organiser)
            .HasForeignKey<Organiser>(o => o.UserID).OnDelete(DeleteBehavior.Cascade);

        // Participant
        modelBuilder.Entity<Participant>()
            .HasOne(p => p.User).WithOne(u => u.Participant)
            .HasForeignKey<Participant>(p => p.UserID).OnDelete(DeleteBehavior.Cascade);

        // Event
        modelBuilder.Entity<Event>()
            .HasOne(e => e.Organiser).WithMany(o => o.Events)
            .HasForeignKey(e => e.OrganiserID).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Event>().HasCheckConstraint("CK_Event_Type", "[EventType] IN ('Run', 'Walk', 'Cycle')");

        // Category
        modelBuilder.Entity<Category>()
            .HasOne(c => c.Event).WithMany(e => e.Categories)
            .HasForeignKey(c => c.EventID).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Category>().HasCheckConstraint("CK_Category_Type", "[CategoryType] IN ('Age', 'Distance', 'Other') OR [CategoryType] IS NULL");

        // EventEnrolment
        modelBuilder.Entity<EventEnrolment>()
            .HasOne(ee => ee.Participant).WithMany(p => p.EventEnrolments)
            .HasForeignKey(ee => ee.ParticipantID).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<EventEnrolment>()
            .HasOne(ee => ee.Event).WithMany(e => e.EventEnrolments)
            .HasForeignKey(ee => ee.EventID).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<EventEnrolment>()
            .HasOne(ee => ee.Category).WithMany(c => c.EventEnrolments)
            .HasForeignKey(ee => ee.CategoryID).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<EventEnrolment>().HasIndex(ee => new { ee.ParticipantID, ee.EventID }).IsUnique();
        modelBuilder.Entity<EventEnrolment>().HasCheckConstraint("CK_Enrolment_Status", "[Status] IN ('Pending', 'Confirmed', 'Cancelled', 'Completed')");
        modelBuilder.Entity<EventEnrolment>().HasCheckConstraint("CK_Enrolment_PaymentStatus", "[PaymentStatus] IN ('Pending', 'Paid', 'Refunded')");

        // Result
        modelBuilder.Entity<Result>()
            .HasOne(r => r.Enrolment).WithOne(ee => ee.Result)
            .HasForeignKey<Result>(r => r.EnrolmentID).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Result>().HasIndex(r => r.EnrolmentID).IsUnique();
        modelBuilder.Entity<Result>().HasCheckConstraint("CK_Result_Status", "[Status] IN ('Not Started', 'In Progress', 'Finished', 'DNF', 'DQ')");
    }
}
