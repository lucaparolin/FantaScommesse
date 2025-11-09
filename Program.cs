using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = Encoding.UTF8.GetBytes(jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT Secret not configured"));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(secretKey),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// Add HttpContextAccessor for audit tracking
builder.Services.AddHttpContextAccessor();

// Register repositories (ADO.NET-based)
builder.Services.AddScoped<FantaScommesse.Repositories.IUserRepository, FantaScommesse.Repositories.UserRepository>();
builder.Services.AddScoped<FantaScommesse.Repositories.IRoundRepository, FantaScommesse.Repositories.RoundRepository>();
builder.Services.AddScoped<FantaScommesse.Repositories.IMatchRepository, FantaScommesse.Repositories.MatchRepository>();
builder.Services.AddScoped<FantaScommesse.Repositories.IPredictionRepository, FantaScommesse.Repositories.PredictionRepository>();
builder.Services.AddScoped<FantaScommesse.Repositories.ISeasonRepository, FantaScommesse.Repositories.SeasonRepository>();
builder.Services.AddScoped<FantaScommesse.Repositories.IParticipationRepository, FantaScommesse.Repositories.ParticipationRepository>();
builder.Services.AddScoped<FantaScommesse.Repositories.IPaymentRepository, FantaScommesse.Repositories.PaymentRepository>();
builder.Services.AddScoped<FantaScommesse.Repositories.IReferralRepository, FantaScommesse.Repositories.ReferralRepository>();

// Register services
builder.Services.AddScoped<FantaScommesse.Services.IAuthService, FantaScommesse.Services.AuthService>();
builder.Services.AddScoped<FantaScommesse.Services.IScoringService, FantaScommesse.Services.ScoringService>();
builder.Services.AddScoped<FantaScommesse.Services.IParticipationService, FantaScommesse.Services.ParticipationService>();
builder.Services.AddScoped<FantaScommesse.Services.INotificationService, FantaScommesse.Services.NotificationService>();
builder.Services.AddScoped<FantaScommesse.Services.IExternalApiService, FantaScommesse.Services.ExternalApiService>();

// Register validators
builder.Services.AddScoped<FantaScommesse.Validators.IPredictionValidator, FantaScommesse.Validators.PredictionValidator>();

// Register utilities
builder.Services.AddScoped<FantaScommesse.Utilities.CsvImporter>();

// Register background jobs
builder.Services.AddHostedService<FantaScommesse.BackgroundJobs.DeadlineReminderJob>();

// Register HTTP client factory
builder.Services.AddHttpClient();

// CORS policy (for API endpoints)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseMiddleware<FantaScommesse.Middleware.ErrorHandlingMiddleware>();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

// Map controller routes
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

// Map API endpoints
app.MapControllers();

app.Run();
