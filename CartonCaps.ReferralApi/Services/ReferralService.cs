using CartonCaps.ReferralApi.Models;
using CartonCaps.ReferralApi.Models.DTO;
using CartonCaps.ReferralApi.Models.Responses;
using CartonCaps.ReferralApi.Repositories;

namespace CartonCaps.ReferralApi.Services
{
	public class ReferralService : IReferralService
	{
		private readonly IReferralRepository _repository;
		private readonly IUserRepository _userRepository;
		private readonly IReferralLinkService _referralLinkService;
		private readonly INotificationService _notificationService;

		public ReferralService(IReferralRepository referralRepository, IUserRepository userRepository, 
			IReferralLinkService referralLinkService, INotificationService notificationService)
		{
			_repository = referralRepository;
			_userRepository = userRepository;
			_referralLinkService = referralLinkService;
			_notificationService = notificationService;
		}
		public async Task<List<ReferralDto>> GetUserReferralsAsync(int userId)
		{
			var referrals = await _repository.GetReferralsByUserIdAsync(userId);

			// Map to DTOs
			return referrals.Select(r => new ReferralDto
			{
				ReferralCode = r.ReferralCode,
				ReferredEmailOrPhone = r.EmailOrPhone,
				CreatedAt = r.ReferredDate,
				Status = r.Status.StatusName
			}).ToList();
		}


		public async Task<ReferralOperationResult> CreateReferralInvite(int referrerId, string emailOrPhone, string channel, string referralCode)
		{
			try
			{
				var referral = new ReferralModel
				{
					ReferrerUserId = referrerId,
					ReferralCode = referralCode,
					EmailOrPhone = emailOrPhone,
					ReferredDate = DateTime.UtcNow,
					ReferralStatusId = 1
				};

				//First all the line to db.
				var result = _repository.AddReferralInvite(referral).Result;
				if (!result.Success)
					return result;

				var referralLink = await _referralLinkService.GenerateReferralLinkAsync(referralCode, channel);

				var message = $"You're invited to CartonCaps! Use this link to join: {referralLink}";

				bool notificationSent = channel.ToLower() switch
				{
					"sms" => _notificationService.SendSms(emailOrPhone, message),
					"email" => _notificationService.SendEmail(emailOrPhone, "You're Invited to CartonCaps", message),
					_ => false
				};

				if (!notificationSent)
				{					
					referral.ReferralStatusId = 4;
					_repository.UpdateReferralStatus(referral.Id, referral.ReferralStatusId);

					return new ReferralOperationResult
					{
						Success = false,
						Message = "Referral saved, but notification failed.",
						ReferralId = result.ReferralId
					};
				}

				return result;
			}
			catch (Exception ex)
			{
				// The exception can also be logged here if needed. 
				return new ReferralOperationResult
				{
					Success = false,
					Message = $"An error occurred while creating the referral invite: {ex.Message}"
				};
			}

		}


	}
}
