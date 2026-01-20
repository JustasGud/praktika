using Microsoft.Data.SqlClient;

namespace UtilityBillingSystem.Infrastructure.Db;

public static class SqlExceptionHelper
{
    // 2601 / 2627 -> duplicate key
    public static bool IsUniqueViolation(SqlException ex) => ex.Number is 2601 or 2627;
}
