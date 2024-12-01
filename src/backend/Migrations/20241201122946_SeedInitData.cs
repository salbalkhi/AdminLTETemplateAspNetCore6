using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.AspNetCore.Identity;
using System.Text;

#nullable disable

namespace Tadawi.Migrations
{
    /// <inheritdoc />
    public partial class SeedInitData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
            INSERT INTO [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'1', N'Admin', N'ADMIN', N'1');
            INSERT INTO [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'2', N'Client', N'Client', N'2');
            INSERT INTO [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'3', N'Vistor', N'Vistor', N'3');
            INSERT INTO [dbo].[AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'4', N'Stuff', N'STUFF', N'4');
        ");

            // Seed Data for User Table add admin user "admin" with password "admin"
            var passwordHasher = new PasswordHasher<IdentityUser>();
    var hashedPassword = passwordHasher.HashPassword(null, "admin");

    migrationBuilder.Sql($@"
        INSERT INTO [dbo].[AspNetUsers] ([Id], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount], [FirstName], [LastName]) 
        VALUES (N'1', N'admin', N'ADMIN', N'admin@example.com', N'ADMIN@EXAMPLE.COM', 1, '{hashedPassword}', N'{Guid.NewGuid()}', N'{Guid.NewGuid()}', NULL, 0, 0, NULL, 1, 0, N'Admin', N'User');
        
        INSERT INTO [dbo].[AspNetUserRoles] ([UserId], [RoleId]) VALUES (N'1', N'1');
    ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
            DELETE FROM [dbo].[AspNetRoles] WHERE [Id] = N'1';
            DELETE FROM [dbo].[AspNetRoles] WHERE [Id] = N'2';
            DELETE FROM [dbo].[AspNetRoles] WHERE [Id] = N'3';
            DELETE FROM [dbo].[AspNetRoles] WHERE [Id] = N'4';
        ");

            migrationBuilder.Sql(@"
        DELETE FROM [dbo].[AspNetUserRoles] WHERE [UserId] = N'1' AND [RoleId] = N'1';
        DELETE FROM [dbo].[AspNetUsers] WHERE [Id] = N'1';
    ");
        }
    }
}
