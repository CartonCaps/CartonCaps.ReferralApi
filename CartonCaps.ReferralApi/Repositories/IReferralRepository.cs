using CartonCaps.ReferralApi.Models;
using CartonCaps.ReferralApi.Models.Responses;

namespace CartonCaps.ReferralApi.Repositories
{
	public interface IReferralRepository
	{
		Task<List<ReferralModel>> GetReferralsByUserIdAsync(int userId);
		Task<ReferralOperationResult> AddReferralInvite(ReferralModel referral);
		void UpdateReferralStatus(int referralId, int statusId);
	}
}
