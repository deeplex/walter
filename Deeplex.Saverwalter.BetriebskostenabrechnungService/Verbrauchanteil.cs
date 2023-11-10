using Deeplex.Saverwalter.Model;

namespace Deeplex.Saverwalter.BetriebskostenabrechnungService
{
    public sealed class VerbrauchAnteil
    {
        public Umlage Umlage { get; }
        public Dictionary<Zaehlereinheit, List<Verbrauch>> AlleZaehler { get; } = new();
        public Dictionary<Zaehlereinheit, double> AlleVerbrauch { get; } = new();
        public Dictionary<Zaehlereinheit, List<Verbrauch>> DieseZaehler { get; } = new();
        public Dictionary<Zaehlereinheit, double> DieseVerbrauch { get; } = new();
        public Dictionary<Zaehlereinheit, double> Anteil { get; } = new();

        public VerbrauchAnteil(Umlage umlage, Wohnung wohnung, Zeitraum zeitraum, List<Note> notes)
        {
            Umlage = umlage;
            var zaehlerGroups = umlage.Zaehler
                // if no Wohnung is attached => zaehler is allgemeinZaehler
                .Where(zaehler => zaehler.Wohnung != null)
                .GroupBy(zaehler => zaehler.Typ.ToUnit());

            foreach (var zaehlergroup in zaehlerGroups)
            {
                var unit = zaehlergroup.Key;

                AlleZaehler[unit] = new();
                AlleVerbrauch[unit] = new();
                DieseZaehler[unit] = new();
                DieseVerbrauch[unit] = new();

                foreach (var zaehler in zaehlergroup)
                {
                    if (zaehler.Staende.Count == 0)
                    {
                        continue;
                    }
                    // Done because the first entry should not be in the last month.
                    var daysToFirstZaehler = zaehler.Staende
                        .OrderBy(stand => stand.Stand)
                        .FirstOrDefault()?
                        .Datum
                        .DayNumber - zeitraum.Abrechnungsende.DayNumber;
                    // First Zählerstand should be at least in next to last month of Abrechnung
                    if (daysToFirstZaehler > -31)
                    {
                        continue;
                    }
                    var verbrauch = new Verbrauch(zaehler, zeitraum.Abrechnungsbeginn, zeitraum.Abrechnungsende, notes);
                    AlleZaehler[unit].Add(verbrauch);
                    AlleVerbrauch[unit] += verbrauch.Delta;

                    if (zaehler.Wohnung == wohnung)
                    {
                        var verbrauchMieter = new Verbrauch(zaehler, zeitraum.Nutzungsbeginn, zeitraum.Nutzungsende, notes);
                        DieseZaehler[unit].Add(verbrauchMieter);
                        DieseVerbrauch[unit] += verbrauch.Delta;
                    }
                }

                foreach (var entry in AlleZaehler)
                {
                    checkIfZaehlerstaendeValid(entry.Value, zeitraum.Abrechnungsbeginn, zeitraum.Abrechnungsende, notes);
                }
                foreach (var entry in DieseZaehler)
                {
                    checkIfZaehlerstaendeValid(entry.Value, zeitraum.Nutzungsbeginn, zeitraum.Nutzungsende, notes);
                }
            }

            foreach (var zaehlerTyp in DieseVerbrauch)
            {
                Anteil[zaehlerTyp.Key] = zaehlerTyp.Value / AlleVerbrauch[zaehlerTyp.Key];

                if (double.IsNaN(Anteil[zaehlerTyp.Key]))
                {
                    Anteil[zaehlerTyp.Key] = 0;
                }
            }
        }

        private static void checkIfZaehlerstaendeValid(
            List<Verbrauch> verbraeuche,
            DateOnly beginn,
            DateOnly ende,
            List<Note> notes)
        {
            var thresholdOfDaysBeforeNotOkay = 14;

            foreach (var verbrauch in verbraeuche)
            {
                if (verbrauch.Enddatum <= verbrauch.Anfangsdatum)
                {
                    notes.Add($"Enddatum von {verbrauch.Zaehler.Kennnummer} ist kleiner oder gleich dem Anfang der Zählung " +
                        $"({verbrauch.Anfangsdatum.ToString("dd.MM.yyyy")} - {verbrauch.Enddatum.ToString("dd.MM.yyyy")})", Severity.Error);
                }
                else if (verbrauch.Enddatum.DayNumber < (ende.DayNumber - thresholdOfDaysBeforeNotOkay))
                {
                    var candidates = verbraeuche.Where(other_verbrauch =>
                        other_verbrauch.Zaehler != verbrauch.Zaehler &&
                        other_verbrauch.Anfangsdatum == verbrauch.Enddatum &&
                        other_verbrauch.Zaehler.Typ == verbrauch.Zaehler.Typ &&
                        other_verbrauch.Zaehler.Staende?.OrderBy(stand => stand.Datum).FirstOrDefault()?.Datum == other_verbrauch.Anfangsdatum &&
                        verbrauch.Zaehler.Wohnung == other_verbrauch.Zaehler.Wohnung);

                    if (candidates.Count() > 1)
                    {
                        var ersatz_string = string.Join(", ", candidates.Select(verbrauch => verbrauch.Zaehler.Kennnummer));
                        notes.Add($"Mehr als einen Ersatz für {verbrauch.Zaehler.Kennnummer} gefunden: {ersatz_string}", Severity.Error);
                    }

                    var ersatz = candidates.FirstOrDefault();

                    if (ersatz != null)
                    {
                        notes.Add($"Zählerwechsel erkannt am {verbrauch.Enddatum.ToString("dd.MM.yyyy")} von Zähler " +
                            $"{verbrauch.Zaehler.Kennnummer} auf {ersatz.Zaehler.Kennnummer} ({ersatz.Zaehler.Typ})",
                            Severity.Info);
                    }
                    else
                    {
                        notes.Add(
                            $"Kein Endstand für Zähler {verbrauch.Zaehler.Kennnummer} ({verbrauch.Zaehler.Typ}) gefunden. " +
                            $"Letzter gültiger Zählerstand ist vom {verbrauch.Enddatum.ToString("dd.MM.yyyy")}, was {ende.DayNumber - verbrauch.Enddatum.DayNumber}" +
                            $" Tage vor Nutzungsende ist.",
                            Severity.Error);
                    }
                }

                if (verbrauch.Anfangsdatum > beginn && verbrauch.Anfangsdatum != verbrauch.Enddatum)
                {
                    var candidates = verbraeuche.Where(other_verbrauch =>
                        other_verbrauch.Anfangsdatum != other_verbrauch.Enddatum &&
                        other_verbrauch.Zaehler != verbrauch.Zaehler &&
                        other_verbrauch.Enddatum == verbrauch.Anfangsdatum &&
                        other_verbrauch.Zaehler.Typ == verbrauch.Zaehler.Typ &&
                        verbrauch.Zaehler.Wohnung == other_verbrauch.Zaehler.Wohnung);

                    if (candidates.Count() > 1)
                    {
                        var ersatz_string = string.Join(", ", candidates.Select(verbrauch => verbrauch.Zaehler.Kennnummer));
                        notes.Add($"Mehr als einen Ersatz für {verbrauch.Zaehler.Kennnummer} gefunden: {ersatz_string}", Severity.Error);
                    }

                    var vorgaenger = candidates.FirstOrDefault();

                    if (vorgaenger != null)
                    {
                        // Wurde bereits beim abgelösten Teil klar gemacht.
                    }
                    else
                    {
                        notes.Add(
                            $"Kein Anfangsstand für Zähler {verbrauch.Zaehler.Kennnummer} ({verbrauch.Zaehler.Typ}) gefunden. " +
                            $"Erster gültiger Zählerstand ist vom {verbrauch.Anfangsdatum}, was nach Nutzungsbeginn ist.",
                            Severity.Error);
                    }
                }
            }

        }

        public static List<VerbrauchAnteil> GetVerbrauchAnteile(
            List<Umlage> umlagen,
            Vertrag vertrag,
            Zeitraum zeitraum,
            List<Note> notes)
        {
            return umlagen
                .Where(umlage => umlage.Zaehler.Count > 0)
                .Select(umlage => new VerbrauchAnteil(umlage, vertrag.Wohnung, zeitraum, notes))
                .ToList();
        }
    }
}
