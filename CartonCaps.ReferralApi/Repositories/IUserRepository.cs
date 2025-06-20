namespace CartonCaps.ReferralApi.Repositories
{
	public interface IUserRepository
	{
		Task<string> GetReferralCodeByUserId(int userId);
	}
}
