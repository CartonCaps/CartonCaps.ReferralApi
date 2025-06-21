using CartonCaps.ReferralApi.Models;
using CartonCaps.ReferralApi.Models.DTO;
using CartonCaps.ReferralApi.Models.Enums;
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
		private static readonly Dictionary<int, List<DateTime>> _inviteTimestamps = new();
		private const int MAX_INVITES_PER_HOUR = 5;

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
				Status = ((ReferralStatus)r.ReferralStatusId).ToString(),
			}).ToList();
		}

		public async Task<ReferralOperationResult> UpdateSuccessfulReferralToRedeemedAsync(int newlyAddedUserId, string referralCode)
		{
			try
			{
				var referral = await _repository.UpdateSuccessfulReferralToRedeemedAsync(newlyAddedUserId, referralCode);
				return referral;
			}
			catch (Exception ex)
			{
				//if something fails duirng DB call log that here. 
				return new ReferralOperationResult
				{
					Success = false,
					Message = $"An error occurred while updating or retrieving the referral: {ex.Message}"
				};
			}
		}

		public async Task<ReferralOperationResult> CreateReferralInvite(int referrerId, string emailOrPhone, string channel, string referralCode)
		{
			
			try
			{
				(bool flowControl, ReferralOperationResult value) = await CanSendReferralInviteAsync(referrerId, emailOrPhone);

				if (!flowControl)
				{
					return value;
				}

				var referral = new Referrals
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

		public async Task<(bool flowControl, ReferralOperationResult value)> CanSendReferralInviteAsync(int referrerId, string emailOrPhone)
		{
			//Try and set max invite per hour like 5 or something in case someone tries to spam invites.
			if (!_inviteTimestamps.ContainsKey(referrerId))
				_inviteTimestamps[referrerId] = new List<DateTime>();

			var now = DateTime.UtcNow;
			_inviteTimestamps[referrerId] = _inviteTimestamps[referrerId]
				.Where(t => (now - t).TotalHours < 1).ToList();

			if (_inviteTimestamps[referrerId].Count >= MAX_INVITES_PER_HOUR)
			{
				return (flowControl: false, value: new ReferralOperationResult
				{
					Success = false,
					Message = "Invite limit exceeded. Please try again later."
				});
			}

			// Log this invite , so that we can track it.
			_inviteTimestamps[referrerId].Add(now);

			// Check if the user is trying to invite themselves.
			var userDetails = await _userRepository.GetUserById(referrerId);
			if (string.Equals(userDetails.EmailOrPhone, emailOrPhone, StringComparison.OrdinalIgnoreCase))
			{
				return (flowControl: false, value: new ReferralOperationResult
				{
					Success = false,
					Message = "Cannot invite yourself."
				});
			}

			return (flowControl: true, value: null);
		}
	}
}
