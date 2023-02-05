namespace Deeplex.Saverwalter.WebAPI.Services
{
    public static class Utils
    {
        public static string Euro(this double d) => string.Format("{0:N2}€", d);
        public static string Datum(this DateTime d) => d.ToString("dd.MM.yyyy");
    }
}
