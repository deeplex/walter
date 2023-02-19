namespace Deeplex.Saverwalter.WebAPI.Services
{
    public static class Utils
    {
        public static string Euro(this double? d) => string.Format("{0:N2}€", d ?? 0);
        public static string Prozent(this double d) => string.Format("{0:N2}%", d);
        public static string Euro(this double d) => string.Format("{0:N2}€", d);
        public static string? Datum(this DateTime? d) => d?.ToString("dd.MM.yyyy") ?? null;
        public static string Datum(this DateTime d) => d.ToString("dd.MM.yyyy");
        public static string Zeit(this DateTime d) => d.ToString("dd.MM.yyyy HH:mm:ss");
        public static string? Zeit(this DateTime? d) => d?.ToString("dd.MM.yyyy HH:mm:ss") ?? null;
    }
}
