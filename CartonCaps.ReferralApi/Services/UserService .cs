using CartonCaps.ReferralApi.Repositories;

namespace CartonCaps.ReferralApi.Services
{
	public class UserService : IUserService
	{
		private readonly IUserRepository _userRepository;

		public Task<string> GetReferralCode(int userId)
		{
			return _userRepository.GetReferralCodeByUserId(userId);
		}
	}
}
