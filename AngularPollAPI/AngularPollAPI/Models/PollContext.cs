using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AngularPollAPI.Models
{
    public class PollContext:DbContext
    {
        public PollContext(DbContextOptions<PollContext> options) : base(options) { }
        public DbSet<User> Users { get; set; }
        public DbSet<Friend> Friends { get; set; }
        public DbSet<Poll> Polls { get; set; }
        public DbSet<PollAnswer> PollAnswers { get; set; }
        public DbSet<PollAnswerVote> PollAnswerVotes { get; set; }
        public DbSet<PollUser> PollUsers { get; set; }
        public DbSet<PollUserInvite> PollUserInvites { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder) 
        { 
            modelBuilder.Entity<User>().ToTable("User");
            modelBuilder.Entity<Friend>().ToTable("Friend");
            modelBuilder.Entity<Poll>().ToTable("Poll");
            modelBuilder.Entity<PollAnswer>().ToTable("PollAnswer");
            modelBuilder.Entity<PollAnswerVote>().ToTable("PollAnswerVote");
            modelBuilder.Entity<PollUser>().ToTable("PollUser");
            modelBuilder.Entity<PollUserInvite>().ToTable("PollUserInvite");
        }
    }
}
