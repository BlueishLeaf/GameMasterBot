using System;
using GameMasterBot.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace GameMasterBot.Data
{
    public class GameMasterBotContext : DbContext
    {
        public DbSet<Guild> Guilds => Set<Guild>();
        public DbSet<User> Users => Set<User>();
        public DbSet<Campaign> Campaigns => Set<Campaign>();
        public DbSet<Session> Sessions => Set<Session>();

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
            var uri = new Uri(databaseUrl ?? string.Empty);
            var userInfoSplit = uri.UserInfo.Split(':');
            var username = userInfoSplit[0];
            var password = userInfoSplit[1];
            var database = uri.Segments[1];
            
            var connectionString =
                $"Host={uri.Host};" +
                $"Port={uri.Port};" +
                $"Database={database};" +
                $"Username={username};" +
                $"Password={password};" +
                "Sslmode=Prefer;" +
                "Trust Server Certificate=true;";

            options.UseNpgsql(connectionString);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // Discord ID Indices
            builder.Entity<Guild>().HasIndex(s => s.DiscordId);
            builder.Entity<User>().HasIndex(s => s.DiscordId);
            
            // Session Timestamp Index
            builder.Entity<Session>().HasIndex(s => s.Timestamp);
            
            // Set up AutoIncludes
            builder.Entity<Campaign>().Navigation(c => c.GameMaster).AutoInclude();
            builder.Entity<Campaign>().Navigation(c => c.Players).AutoInclude();
            builder.Entity<CampaignPlayer>().Navigation(cp => cp.User).AutoInclude();
            builder.Entity<GameMaster>().Navigation(gm => gm.User).AutoInclude();
            builder.Entity<Session>().Navigation(s => s.Campaign).AutoInclude();
        } 
    }
}