using Com.Common.Model;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Com.AuthProvider
{
    public class AppDBContext : IdentityDbContext<IdentityUser<int>, IdentityRole<int>, int>
    {
        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options)
        {

        }
        public virtual DbSet<ErrorLog> ErrorLog { get; set; }

        //builder.Services.AddScoped<IPasswordHasher<IdentityUser>, CustomPasswordHasher>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //update AspNetUsers set PasswordHash = 'AQAAAAIAAYagAAAAEIhab1z+JoHOpAoVV2Qb4tUiA15pCkjcj/pd4do+32J5wGbrONX0uHRHWxERkCd9oQ==', SecurityStamp = 'RN5DIMQTHRUNL4ACVAJQ7S3NXQCWTKZI' where id = 1

            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<IdentityUser<int>>().HasData(
                new IdentityUser<int>() { Id = 1,  UserName = "superadmin",  NormalizedUserName = "SUPERADMIN",  Email = "admin@test.com", NormalizedEmail = "ADMIN@TEST.COM", PasswordHash = "Admin@1234", SecurityStamp = "Head", ConcurrencyStamp = "Office", PhoneNumber = "1234567890", PhoneNumberConfirmed =false, TwoFactorEnabled = false, LockoutEnabled = false, AccessFailedCount = 0 }
                );
            modelBuilder.Entity<IdentityRole<int>>().HasData(
                new IdentityRole<int>() { Id = 1, Name = "SuperAdmin", NormalizedName = "SUPERADMIN", ConcurrencyStamp = "sdflkjsdflksdf" },
                new IdentityRole<int>() { Id = 2, Name = "Admin", NormalizedName = "ADMIN", ConcurrencyStamp = "asdasdasdas" },
                new IdentityRole<int>() { Id = 3, Name = "User", NormalizedName = "USER", ConcurrencyStamp = "asdasdasdaopipiis" },
                new IdentityRole<int>() { Id = 4, Name = "Reseller", NormalizedName = "RESELLER", ConcurrencyStamp = "asafsdfdasdaoppsdas" }
                );
            modelBuilder.Entity<IdentityUserRole<int>>().HasData(
                new IdentityUserRole<int>() { UserId = 1, RoleId =1 }
                );
        }
    }

    
}
