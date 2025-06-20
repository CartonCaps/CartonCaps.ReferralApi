using CartonCaps.ReferralApi.DB;
using CartonCaps.ReferralApi.Models;
using CartonCaps.ReferralApi.Models.Responses;
using Microsoft.EntityFrameworkCore;

namespace CartonCaps.ReferralApi.Repositories
{
	public class ReferralRepository : IReferralRepository
	{
		private readonly AppDbContext _context;

		public ReferralRepository(AppDbContext context)
		{
			_context = context;
		}
		private readonly List<ReferralModel> _referrals = new();

		public async Task<List<ReferralModel>> GetReferralsByUserIdAsync(int userId)
		{
			return await _context.Referrals
				.Include(r => r.Status)
				.Where(r => r.ReferrerUserId == userId)
				.ToListAsync();
		}

		public async Task<ReferralOperationResult> AddReferralInvite(ReferralModel referral)
		{
			// In real time this would be primary done by the database. Since we are not 
			//really using a database here, we will just simulate the ID generation.
			referral.Id = _referrals.Count + 1;
			_referrals.Add(referral);
			return new ReferralOperationResult
			{
				Success = true,
				ReferralId = referral.Id,
				Message = "Referral successfully added."
			};
		}


		public void UpdateReferralStatus(int referralId, int statusId)
		{
			var referral = _referrals.FirstOrDefault(r => r.Id == referralId);
			if (referral != null)
			{
				referral.ReferralStatusId = statusId;
			}
		}

	}
	

	

}

