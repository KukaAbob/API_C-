using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using WebApplication2;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// ���������� ������������ � Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ����������� � ���� ������
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddSingleton<DataBaseService>(sp =>
{
	var logger = sp.GetRequiredService<ILogger<DataBaseService>>();
	return new DataBaseService(connectionString, logger);
});

// ��������� JWT
var key = Encoding.UTF8.GetBytes("fdsgiuasfogewnrIURibnwfeszidscfqweqfxs");
builder.Services.AddAuthentication(options =>
{
	options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
	options.RequireHttpsMetadata = false;
	options.SaveToken = true;
	options.TokenValidationParameters = new TokenValidationParameters
	{
		ValidateIssuer = false,
		ValidateAudience = false,
		ValidateLifetime = true,
		ValidateIssuerSigningKey = true,
		IssuerSigningKey = new SymmetricSecurityKey(key)
	};

	// ����������� ������ ��������������
	options.Events = new JwtBearerEvents
	{
		OnAuthenticationFailed = context =>
		{
			Console.WriteLine($"Authentication failed: {context.Exception.Message}");
			return Task.CompletedTask;
		}
	};
});

// CORS �������� ��� ���������� ���� ��������
builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowAll", builder =>
	{
		builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
	});
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.UseDeveloperExceptionPage();
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

app.UseAuthentication();  // ��������� JWT ��������������
app.UseAuthorization();

app.MapControllers();
app.Run();
