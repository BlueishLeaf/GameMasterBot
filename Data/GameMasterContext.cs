﻿using System;
using GameMasterBot.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace GameMasterBot.Data
{
    public class GameMasterContext : DbContext
    {
        public DbSet<Guild> Guilds => Set<Guild>();
        public DbSet<User> Users => Set<User>();
        public DbSet<Campaign> Campaigns => Set<Campaign>();
        public DbSet<Session> Sessions => Set<Session>();

        protected override void OnConfiguring(DbContextOptionsBuilder options) =>
            options.UseMySql(
                $"server={Environment.GetEnvironmentVariable("DB_HOST")};" +
                $"database={Environment.GetEnvironmentVariable("DB_NAME")};" +
                $"user={Environment.GetEnvironmentVariable("DB_USER")};" +
                $"password={Environment.GetEnvironmentVariable("DB_PASSWORD")}",
                ServerVersion.FromString("10.4.8-mariadb")
            ).UseLazyLoadingProxies();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // Don't generate IDs for Users and Guilds, use the ID provided by Discord
            builder.Entity<User>().Property(u => u.Id).ValueGeneratedNever();
            builder.Entity<Guild>().Property(g => g.Id).ValueGeneratedNever();
            
            // CampaignUser ManyToMany Resolver
            builder.Entity<CampaignUser>().HasKey(cu => new { cu.CampaignId, cu.UserId });
            builder.Entity<CampaignUser>()
                .HasOne(cu => cu.Campaign)
                .WithMany(c => c.CampaignUsers)
                .HasForeignKey(cu => cu.CampaignId);
            builder.Entity<CampaignUser>()
                .HasOne(cu => cu.User)
                .WithMany(u => u.CampaignUsers)
                .HasForeignKey(cu => cu.UserId);

            // GuildUser ManyToMany Resolver
            builder.Entity<GuildUser>().HasKey(gu => new { gu.GuildId, gu.UserId });
            builder.Entity<GuildUser>()
                .HasOne(gu => gu.Guild)
                .WithMany(g => g.GuildUsers)
                .HasForeignKey(gu => gu.GuildId);
            builder.Entity<GuildUser>()
                .HasOne(gu => gu.User)
                .WithMany(u => u.GuildUsers)
                .HasForeignKey(gu => gu.UserId);
            
            // Session Timestamp Index
            builder.Entity<Session>().HasIndex(s => s.Timestamp);
        } 
    }
}