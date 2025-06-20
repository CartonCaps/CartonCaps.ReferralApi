namespace CartonCaps.ReferralApi.Models.DTO
{
	public class ReferralDto
	{
		public string ReferralCode { get; set; }
		public string ReferredEmailOrPhone { get; set; }
		public string Status { get; set; }    // Status name only
		public DateTime CreatedAt { get; set; }
	}
}
