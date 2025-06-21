using CartonCaps.ReferralApi.Models;
using CartonCaps.ReferralApi.Models.Responses;
using CartonCaps.ReferralApi.Repositories;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CartonCaps.ReferralApi.Services.Tests
{
	[TestClass]
	public class ReferralServiceTests
	{
		private Mock<IReferralRepository> _referralRepoMock;
		private Mock<IUserRepository> _userRepoMock;
		private Mock<IReferralLinkService> _linkServiceMock;
		private Mock<INotificationService> _notificationServiceMock;
		private ReferralService _service;


		[TestInitialize]
		public void Setup()
		{
			_referralRepoMock = new Mock<IReferralRepository>();
			_userRepoMock = new Mock<IUserRepository>();
			_linkServiceMock = new Mock<IReferralLinkService>();
			_notificationServiceMock = new Mock<INotificationService>();

			_service = new ReferralService(
				_referralRepoMock.Object,
				_userRepoMock.Object,
				_linkServiceMock.Object,
				_notificationServiceMock.Object);
		}


		[TestMethod]
		public async Task GetUserReferralsAsync_ReturnsMappedDtos()
		{
			var referrals = new List<Referrals>
	    {
		new Referrals { ReferralCode = "ABC123", EmailOrPhone = "user@example.com", ReferredDate = DateTime.UtcNow, ReferralStatusId = 1 }
	    };

			_referralRepoMock.Setup(r => r.GetReferralsByUserIdAsync(1)).ReturnsAsync(referrals);

			var result = await _service.GetUserReferralsAsync(1);

			Assert.AreEqual(1, result.Count);
			Assert.AreEqual("ABC123", result[0].ReferralCode);
			Assert.AreEqual("Pending", result[0].Status); 
		}

		[TestMethod]
		public async Task UpdateSuccessfulReferralToRedeemedAsync_Success_ReturnsResult()
		{
			_referralRepoMock.Setup(r => r.UpdateSuccessfulReferralToRedeemedAsync(2, "REF123"))
				.ReturnsAsync(new ReferralOperationResult { Success = true });

			var result = await _service.UpdateSuccessfulReferralToRedeemedAsync(2, "REF123");

			Assert.IsTrue(result.Success);
		}

		[TestMethod]
		public async Task UpdateSuccessfulReferralToRedeemedAsync_Exception_ReturnsFailure()
		{
			_referralRepoMock.Setup(r => r.UpdateSuccessfulReferralToRedeemedAsync(It.IsAny<int>(), It.IsAny<string>()))
				.ThrowsAsync(new Exception("DB failure"));

			var result = await _service.UpdateSuccessfulReferralToRedeemedAsync(2, "REF123");

			Assert.IsFalse(result.Success);
			Assert.IsTrue(result.Message.Contains("DB failure"));
		}

		[TestMethod]
		public async Task CreateReferralInvite_SmsChannel_SuccessfulFlow()
		{
			_referralRepoMock.Setup(r => r.AddReferralInvite(It.IsAny<Referrals>()))
				.ReturnsAsync(new ReferralOperationResult { Success = true, ReferralId = 10 });

			_linkServiceMock.Setup(l => l.GenerateReferralLinkAsync("REF123", "sms"))
				.ReturnsAsync("https://mock.link");

			_notificationServiceMock.Setup(n => n.SendSms("1234567890", It.IsAny<string>())).Returns(true);

			var result = await _service.CreateReferralInvite(1, "1234567890", "sms", "REF123");

			Assert.IsTrue(result.Success);
			Assert.AreEqual(10, result.ReferralId);
		}
		[TestMethod]
		public async Task CreateReferralInvite_NotificationFails_ReturnsFailureAndUpdatesStatus()
		{
			_referralRepoMock.Setup(r => r.AddReferralInvite(It.IsAny<Referrals>()))
				.ReturnsAsync(new ReferralOperationResult { Success = true, ReferralId = 99 });

			_linkServiceMock.Setup(l => l.GenerateReferralLinkAsync("REF123", "email"))
				.ReturnsAsync("https://mock.link");

			_notificationServiceMock.Setup(n => n.SendEmail("someone@example.com", It.IsAny<string>(), It.IsAny<string>()))
				.Returns(false);

			ReferralOperationResult result = await _service.CreateReferralInvite(1, "someone@example.com", "email", "REF123");

			Assert.IsFalse(result.Success);
			Assert.AreEqual("Referral saved, but notification failed.", result.Message);
		}

		[TestMethod]
		public async Task CreateReferralInvite_WhenExceptionThrown_ReturnsFailure()
		{
			_referralRepoMock.Setup(r => r.AddReferralInvite(It.IsAny<Referrals>()))
				.Throws(new Exception("Simulated exception"));

			var result = await _service.CreateReferralInvite(1, "email@example.com", "email", "REF123");

			Assert.IsFalse(result.Success);
			Assert.IsTrue(result.Message.Contains("Simulated exception"));
		}


	}
}
