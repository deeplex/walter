using Deeplex.Saverwalter.Model;

namespace Deeplex.Saverwalter.BetriebskostenabrechnungService
{
    public sealed class Verbrauch
    {
        public Zaehler Zaehler;
        public double Delta;

        public Verbrauch(Zaehler zaehler, Zeitraum zeitraum, List<Note> notes)
        {
            Zaehler = zaehler;
            var endstand = GetZaehlerEndStand(zaehler, zeitraum.Nutzungsende);
            var anfangsstand = GetZaehlerAnfangsStand(zaehler, zeitraum.Nutzungsbeginn);
            if (anfangsstand == null)
            {
                notes.Add(new Note($"Keinen Anfangsstand für Zähler {zaehler.Kennnummer} gefunden.", Severity.Error));
            }
            if (endstand == null)
            {
                notes.Add(new Note($"Keinen Endstand für Zähler {zaehler.Kennnummer} gefunden.", Severity.Error));
            }

            if (anfangsstand != null && endstand != null)
            {
                Delta = endstand.Stand - anfangsstand.Stand;
            }
            else if (anfangsstand == null && endstand?.Stand == 0)
            {
                Delta = 0;
            }
            else
            {
                Delta = 0;
            }
        }

        private static Zaehlerstand? GetZaehlerEndStand(Zaehler zaehler, DateOnly nutzungsende)
        {
            return zaehler.Staende
                    .OrderBy(s => s.Datum)
                    .LastOrDefault(l => l.Datum <= nutzungsende && (nutzungsende.DayNumber - l.Datum.DayNumber) < 30);
        }

        private static Zaehlerstand? GetZaehlerAnfangsStand(Zaehler zaehler, DateOnly nutzungsbeginn)
        {
            return zaehler.Staende
                .OrderBy(s => s.Datum)
                .FirstOrDefault(l => Math.Abs(l.Datum.DayNumber - nutzungsbeginn.AddDays(-1).DayNumber) < 30);
        }


    }
}
