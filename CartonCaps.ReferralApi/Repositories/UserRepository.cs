namespace CartonCaps.ReferralApi.Repositories
{
	public class UserRepository : IUserRepository
	{
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
	}
}
