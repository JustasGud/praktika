using Microsoft.Data.SqlClient;
using UtilityBillingSystem.Application.Abstractions;
using UtilityBillingSystem.Infrastructure.Db;

namespace UtilityBillingSystem.Infrastructure.Repositories;

public sealed class PricingRepository : IPricingRepository
{
    private readonly IDbConnectionFactory _factory;
    public PricingRepository(IDbConnectionFactory factory) => _factory = factory;

    public CurrentPriceRow? GetCurrentPrice(int communityServiceId)
    {
        using var con = _factory.CreateAppDb();
        con.Open();

        using var cmd = con.CreateCommand();
        cmd.CommandText = @"
SELECT TOP(1) Price, Currency, EffectiveFrom
FROM dbo.Prices
WHERE CommunityServiceId=@id AND EffectiveTo IS NULL
ORDER BY EffectiveFrom DESC;";
        cmd.Parameters.AddWithValue("@id", communityServiceId);

        using var r = cmd.ExecuteReader();
        if (!r.Read()) return null;

        return new CurrentPriceRow(
            Price: r.GetDecimal(0),
            Currency: r.GetString(1),
            EffectiveFrom: DateOnly.FromDateTime(r.GetDateTime(2))
        );
    }

    public void SetNewPrice(int communityServiceId, decimal price, string currency, DateOnly effectiveFrom)
    {
        if (price < 0) throw new InvalidOperationException("Kaina negali būti neigiama.");

        using var con = _factory.CreateAppDb();
        con.Open();

        using var tx = con.BeginTransaction();

        // uždarom seną kainą
        using (var close = con.CreateCommand())
        {
            close.Transaction = tx;
            close.CommandText = @"
UPDATE dbo.Prices
SET EffectiveTo = @to
WHERE CommunityServiceId=@id AND EffectiveTo IS NULL;";
            close.Parameters.AddWithValue("@to", effectiveFrom.ToDateTime(TimeOnly.MinValue).Date);
            close.Parameters.AddWithValue("@id", communityServiceId);
            close.ExecuteNonQuery();
        }

        // įrašom naują
        using (var ins = con.CreateCommand())
        {
            ins.Transaction = tx;
            ins.CommandText = @"
INSERT INTO dbo.Prices(CommunityServiceId, Price, Currency, EffectiveFrom, EffectiveTo)
VALUES (@id, @p, @cur, @from, NULL);";
            ins.Parameters.AddWithValue("@id", communityServiceId);
            ins.Parameters.AddWithValue("@p", price);
            ins.Parameters.AddWithValue("@cur", currency);
            ins.Parameters.AddWithValue("@from", effectiveFrom.ToDateTime(TimeOnly.MinValue).Date);

            try
            {
                ins.ExecuteNonQuery();
            }
            catch (SqlException ex) when (SqlExceptionHelper.IsUniqueViolation(ex))
            {
                throw new InvalidOperationException("Šiai paslaugai jau yra aktyvi kaina (patikrink galiojimo datas).");
            }
        }

        tx.Commit();
    }

    public List<ResidentServiceRow> GetResidentServicesWithPrices(int userId)
    {
        using var con = _factory.CreateAppDb();
        con.Open();

        using var cmd = con.CreateCommand();
        cmd.CommandText = @"
SELECT s.Name AS ServiceName,
       p.Price,
       p.Currency,
       p.EffectiveFrom
FROM dbo.Users u
JOIN dbo.CommunityServices cs ON cs.CommunityId = u.CommunityId
JOIN dbo.Services s ON s.Id = cs.ServiceId
JOIN dbo.Prices p ON p.CommunityServiceId = cs.Id
WHERE u.Id = @uid
  AND p.EffectiveTo IS NULL
ORDER BY s.Name;";
        cmd.Parameters.AddWithValue("@uid", userId);

        using var r = cmd.ExecuteReader();
        var list = new List<ResidentServiceRow>();
        while (r.Read())
        {
            list.Add(new ResidentServiceRow(
                ServiceName: r.GetString(0),
                Price: r.GetDecimal(1),
                Currency: r.GetString(2),
                EffectiveFrom: DateOnly.FromDateTime(r.GetDateTime(3))
            ));
        }
        return list;
    }
}
