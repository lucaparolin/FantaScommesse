using System.Security.Cryptography;
using System.Text;
using FantaScommesse.Models.Entities;
using FantaScommesse.Repositories;

namespace FantaScommesse.Services;

public class ParticipationService : IParticipationService
{
    private readonly IParticipationRepository _participationRepository;
    private readonly ISeasonRepository _seasonRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IReferralRepository _referralRepository;

    public ParticipationService(
        IParticipationRepository participationRepository,
        ISeasonRepository seasonRepository,
        IPaymentRepository paymentRepository,
        IReferralRepository referralRepository)
    {
        _participationRepository = participationRepository;
        _seasonRepository = seasonRepository;
        _paymentRepository = paymentRepository;
        _referralRepository = referralRepository;
    }

    public async Task<Participation> EnrollUserInSeasonAsync(long userId, int seasonId, long? referrerParticipationId = null)
    {
        // Check if already enrolled
        var existing = await _participationRepository.GetByUserAndSeasonAsync(userId, seasonId);
        if (existing != null) throw new InvalidOperationException("User already enrolled in this season");

        var season = await _seasonRepository.GetByIdAsync(seasonId);
        if (season == null) throw new InvalidOperationException("Season not found");
        if (season.IsClosed) throw new InvalidOperationException("Season is closed");
        if (DateTime.UtcNow > season.SignupDeadline) throw new InvalidOperationException("Signup deadline passed");

        // Create participation
        var participation = new Participation
        {
            UserId = userId,
            SeasonId = seasonId
        };

        var participationId = await _participationRepository.CreateAsync(participation, userId.ToString());
        participation.ParticipationId = participationId;

        // Handle referral if provided
        if (referrerParticipationId.HasValue)
        {
            var referral = new Referral
            {
                ReferrerParticipationId = referrerParticipationId.Value,
                ReferredParticipationId = participationId,
                DiscountEur = season.ReferralDiscount
            };
            await _referralRepository.CreateAsync(referral, userId.ToString());
        }

        return participation;
    }

    public async Task<bool> GeneratePaymentPlanAsync(long participationId, string paymentPlan, string username)
    {
        var participation = await _participationRepository.GetByIdAsync(participationId);
        if (participation == null) return false;

        var season = await _seasonRepository.GetByIdAsync(participation.SeasonId);
        if (season == null) return false;

        // Calculate discount from referrals
        var referrals = await _referralRepository.GetByReferrerIdAsync(participationId);
        var totalDiscount = referrals.Sum(r => r.DiscountEur);

        var baseFee = season.BaseFeeEur - totalDiscount;
        if (baseFee < 0) baseFee = 0;

        // Generate payments based on plan
        var payments = paymentPlan.ToUpperInvariant() switch
        {
            "FULL" => new[] { new { Amount = baseFee, DueDate = DateTime.UtcNow.AddDays(7) } },
            "2RATE" => new[]
            {
                new { Amount = baseFee / 2, DueDate = DateTime.UtcNow.AddDays(7) },
                new { Amount = baseFee / 2, DueDate = DateTime.UtcNow.AddDays(37) }
            },
            "4RATE" => new[]
            {
                new { Amount = baseFee / 4, DueDate = DateTime.UtcNow.AddDays(7) },
                new { Amount = baseFee / 4, DueDate = DateTime.UtcNow.AddDays(37) },
                new { Amount = baseFee / 4, DueDate = DateTime.UtcNow.AddDays(67) },
                new { Amount = baseFee / 4, DueDate = DateTime.UtcNow.AddDays(97) }
            },
            _ => throw new ArgumentException("Invalid payment plan")
        };

        foreach (var payment in payments)
        {
            var paymentEntity = new Payment
            {
                ParticipationId = participationId,
                ExpectedEur = payment.Amount,
                PaidEur = 0,
                DueOnUtc = payment.DueDate
            };
            await _paymentRepository.CreateAsync(paymentEntity, username);
        }

        return true;
    }

    public async Task<string> GenerateReferralCodeAsync(long participationId)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes($"{participationId}-{Guid.NewGuid()}"));
        return Convert.ToBase64String(hash)[..12].Replace("+", "").Replace("/", "").ToUpperInvariant();
    }

    public async Task<Participation?> GetByReferralCodeAsync(string referralCode)
    {
        // Simplified: store referral codes in a cache or separate table in production
        return null;
    }
}
