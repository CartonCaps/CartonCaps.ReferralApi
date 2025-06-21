using CartonCaps.ReferralApi.DB;
using CartonCaps.ReferralApi.Models;

namespace CartonCaps.ReferralApi.Repositories
{
	public class UserRepository : IUserRepository
	{
		private readonly AppDbContext _context;

		public UserRepository(AppDbContext context)
		{
			_context = context;
		}
		private readonly Dictionary<int, string> _userReferralCodes = new()
       {
	    { 101, "REF101ABC" },
	    { 102, "REF102XYZ" },
	    { 103, "REF103MNO" },
		{100, "REF100DEF" }
	   };


		
		public async Task<string> GetReferralCodeByUserId(int userId)
		{
			// In real -world applications, this would be a database call and this would be tied to the user profile.
			return  _userReferralCodes.TryGetValue(userId, out var code) ? code : throw new Exception("Referral code not found for user");
		}

		public Task<UserReferralProfile?> GetUserByReferralCodeAsync(string referralCode)
		{
			var user = _context.UserRefProfiles.FirstOrDefault(u => u.ReferralCode.Equals(referralCode, StringComparison.OrdinalIgnoreCase));
			return Task.FromResult(user);
		}
	}
}
