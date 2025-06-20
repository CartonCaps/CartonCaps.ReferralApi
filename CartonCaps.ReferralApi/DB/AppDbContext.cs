using CartonCaps.ReferralApi.Models;
using Microsoft.EntityFrameworkCore;

namespace CartonCaps.ReferralApi.DB
{
	public class AppDbContext : DbContext {
		public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

		public DbSet<ReferralModel> Referrals { get; set; }
		public DbSet<ReferralStatus> ReferralStatuses { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<ReferralModel>()
				.HasOne(r => r.Status)
				.WithMany()
				.HasForeignKey(r => r.ReferralStatusId);
		}
	}
}
