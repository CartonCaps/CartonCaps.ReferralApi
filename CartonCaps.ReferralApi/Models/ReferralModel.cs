namespace CartonCaps.ReferralApi.Models
{
	public class ReferralModel
	{
		public int Id { get; set; }
		public int ReferrerUserId { get; set; }
		public string ReferralCode { get; set; }
		public string? EmailOrPhone { get; set; }
		public DateTime ReferredDate { get; set; }
		public int ReferralStatusId { get; set; } // Foreign key to ReferralStatus
		public ReferralStatus Status { get; set; } 
	}
}
