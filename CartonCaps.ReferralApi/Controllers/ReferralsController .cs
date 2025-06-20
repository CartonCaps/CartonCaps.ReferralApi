using CartonCaps.ReferralApi.Models;
using CartonCaps.ReferralApi.Models.DTO;
using CartonCaps.ReferralApi.Services;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CartonCaps.ReferralApi.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ReferralsController : ControllerBase
	{
		private readonly IReferralService _service;
		private readonly IUserService _userRepository;
		private readonly IReferralLinkService _referralLinkService;


		public ReferralsController(IReferralService referralService, IUserService userservice, IReferralLinkService referralLinkServie )
		{
			_service = referralService;
			_userRepository = userservice;
			_referralLinkService = referralLinkServie;
		}



		// GET: api/<ReferralsController>
		[HttpGet]
		public IEnumerable<string> Get()
		{
			return new string[] { "value1", "value2" };
		}

		// GET api/<ReferralsController>/5
		[HttpGet("{id}")]
		public string Get(int id)
		{
			return "value";
		}

		// POST api/<ReferralsController>
		[HttpPost]
		public void Post([FromBody] string value)
		{
		}

		// PUT api/<ReferralsController>/5
		[HttpPut("{id}")]
		public void Put(int id, [FromBody] string value)
		{
		}

		// DELETE api/<ReferralsController>/5
		[HttpDelete("{id}")]
		public void Delete(int id)
		{
		}

		/// <summary>
		/// This endpoint retrieves a list of referrals for a specific user. I am assuming one user has same referal code that they 
		/// can share with others and is unique to that user.
		/// </summary>
		/// <param name="userId"></param>
		/// <returns></returns>
		[HttpPost("referalslist")]
		public async Task<IActionResult> GetReferrals([FromQuery] int userId)
		{
			// I would assume this validation was also done on the UI side but this is just to be sure.
			//If there was no userId , it does not even need to make this call. 
			if (userId <= 0)
			{
				return BadRequest("Invalid userId. It must be a positive integer.");
			}


			var referrals = await _service.GetUserReferralsAsync(userId);

			if (referrals == null || referrals.Count == 0)
				return NotFound($"No referrals found for user ID: {userId}");

			return Ok(referrals);
		}

		[HttpPost("invite")]
		public async Task<IActionResult> InviteFriend([FromBody] ReferralInviteRequest request)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var referralCode = await _userRepository.GetReferralCode(request.ReferrerUserId);
			var result = await _service.CreateReferralInvite(request.ReferrerUserId, request.EmailOrPhone, request.Channel, referralCode);
			if (!result.Success)
				return StatusCode(500, result.Message);


			return Ok(new { status = "logged" });
		}

	}
}
