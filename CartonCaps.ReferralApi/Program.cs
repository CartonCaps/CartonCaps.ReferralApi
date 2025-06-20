using CartonCaps.ReferralApi.DB;
using CartonCaps.ReferralApi.Models;
using CartonCaps.ReferralApi.Repositories;
using CartonCaps.ReferralApi.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
	options.UseInMemoryDatabase("ReferralDb"));

builder.Services.AddScoped<IReferralRepository, ReferralRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IReferralService, ReferralService>();
builder.Services.AddHttpClient<IReferralLinkService, ReferralLinkService>();
builder.Services.AddScoped<INotificationService, NotificationService>();



var app = builder.Build();


//Seeding data here to get the experience of having a populated database

using (var scope = app.Services.CreateScope())
{
	var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

	var statuses = new List<ReferralStatus>
	{
		new ReferralStatus { Id = 1, StatusName = "Pending"  },
		new ReferralStatus { Id = 2, StatusName = "Redeemed" },
		new ReferralStatus { Id = 3, StatusName = "Registered"},
		new ReferralStatus { Id = 4, StatusName = "notification_failed"}

	};

	var referrals = new List<ReferralModel>
	{
		new ReferralModel
		{
			Id = 1,
		ReferrerUserId = 100,
		ReferralCode = "REF-USER123",
		EmailOrPhone = "friend1@example.com",
		ReferredDate = DateTime.UtcNow.AddDays(-3),
		ReferralStatusId = 1, // Pending
        Status = statuses[0]
		},
		new ReferralModel
		{
			Id = 2,
		ReferrerUserId = 100,
		ReferralCode = "REF-USER123",
		EmailOrPhone = "friend2@example.com",
		ReferredDate = DateTime.UtcNow.AddDays(-2),
		ReferralStatusId = 2, // Registered
        Status = statuses[1]
		},
		new ReferralModel
	{
		Id = 3,
		ReferrerUserId = 101,
		ReferralCode = "REF-USER123",
		EmailOrPhone = "friend3@example.com",
		ReferredDate = DateTime.UtcNow.AddDays(-1),
		ReferralStatusId = 3, // Rewarded
        Status = statuses[2]
	},
	new ReferralModel
	{
		Id = 4,
		ReferrerUserId = 101,
		ReferralCode = "REF-USER456",
		EmailOrPhone = "colleague@example.com",
		ReferredDate = DateTime.UtcNow.AddDays(-5),
		ReferralStatusId = 3,
		Status = statuses[2]
	}
	};

	context.ReferralStatuses.AddRange(statuses);
	context.Referrals.AddRange(referrals);
	context.SaveChanges();
}



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
