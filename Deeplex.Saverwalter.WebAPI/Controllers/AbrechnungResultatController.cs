
using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.WebAPI.Services;
using Deeplex.Saverwalter.WebAPI.Services.DbServices;
using Microsoft.AspNetCore.Mvc;
using static Deeplex.Saverwalter.WebAPI.Controllers.AbrechnungsresultatController;
using static Deeplex.Saverwalter.WebAPI.Controllers.SelectionListController;
using static Deeplex.Saverwalter.WebAPI.Services.Utils;

namespace Deeplex.Saverwalter.WebAPI.Controllers
{
    [ApiController]
    [Route("api/abrechnungsresultate")]
    public class AbrechnungsresultatController : FileControllerBase<AbrechnungsresultatEntry, Guid, Abrechnungsresultat>
    {
        public class AusgleichsZahlungInfo
        {
            public DateOnly Datum { get; set; }
            public decimal Betrag { get; set; }
            public Guid BuchungssatzId { get; set; }
        }

        public class AbrechnungsresultatEntryBase
        {
            public Guid Id { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime LastModified { get; set; }
            public int Jahr { get; set; }
            public bool Abgesendet { get; set; }
            /// <summary>Positiv = Mieter muss nachzahlen, negativ = Vermieter erstattet.</summary>
            public decimal Saldo { get; set; }
            /// <summary>Abgerechneter Gesamtbetrag (Vorauszahlung + Saldo).</summary>
            public decimal Rechnungsbetrag { get; set; }
            /// <summary>Geleistete NK-Vorauszahlungen des Jahres (Haben auf dem NkBuchungskonto).</summary>
            public decimal Vorauszahlung { get; set; }
            /// <summary>|Saldo| minus per OPOS gedeckte Ausgleichszahlungen.</summary>
            public decimal OffenerBetrag { get; set; }
            /// <summary>True wenn kein offener Betrag verbleibt (Saldo 0 oder voll ausgeglichen).</summary>
            public bool Ausgeglichen { get; set; }
            /// <summary>Gebuchte Ausgleichszahlungen (Nachzahlungen/Erstattungen) per OPOS.</summary>
            public List<AusgleichsZahlungInfo> AusgleichsZahlungen { get; set; } = [];
            /// <summary>
            /// Bankkonto des Vermieters für den Zahlungsverkehr dieses Vertrags:
            /// das Bankkonto hinter dem Vertrags-Zahlungskonto, sonst das der
            /// aktuellen Wohnungs-Eigentümer. Vorbelegung für den Ausgleich
            /// (Zahler bei Erstattungen, Zahlungsempfänger bei Nachzahlungen).
            /// </summary>
            public int? VermieterBankkontoId { get; set; }
            public string? Notiz { get; set; }
            public Guid BuchungssatzId { get; set; }

            public Permissions Permissions { get; set; } = new Permissions();

            public AbrechnungsresultatEntryBase() { }
            public AbrechnungsresultatEntryBase(Abrechnungsresultat entity, Permissions permissions)
            {
                Id = entity.AbrechnungsresultatId;
                CreatedAt = entity.CreatedAt;
                LastModified = entity.LastModified;
                Jahr = entity.Buchungssatz.Buchungsdatum.Year;
                Abgesendet = entity.Abgesendet;
                Notiz = entity.Notiz;
                BuchungssatzId = entity.Buchungssatz.BuchungssatzId;

                var bkAbrKontoId = entity.Vertrag.BkAbrechnungsKonto.BuchungskontoId;

                // Saldo als Glattstellung auf dem BkAbrechnungsKonto:
                // Soll = Nachzahlung des Mieters (positiv), Haben = Guthaben (negativ).
                var saldoZeilen = entity.Buchungssatz.Buchungszeilen
                    .Where(z => z.Buchungskonto.BuchungskontoId == bkAbrKontoId)
                    .ToList();
                Saldo = saldoZeilen
                    .Sum(z => z.SollHaben == SollHaben.Soll ? z.Betrag : -z.Betrag);

                // Vorauszahlung = geleistete NK-Vorauszahlungen des Jahres (Haben auf
                // dem NkBuchungskonto), ohne die Glattstellungs-Zeile der Abrechnung
                // selbst; Rechnungsbetrag folgt aus dem Saldo.
                Vorauszahlung = entity.Vertrag.NkBuchungskonto.Buchungszeilen
                    .Where(z => z.SollHaben == SollHaben.Haben
                             && z.Buchungssatz.Buchungsjahr == Jahr
                             && z.Buchungssatz.BuchungssatzId != BuchungssatzId)
                    .Sum(z => z.Betrag);
                Rechnungsbetrag = Vorauszahlung + Saldo;

                // Ausgleich per OPOS auf der Saldo-Zeile:
                //   Nachzahlung (Soll-Forderung): gedeckt durch Zahlungs-Haben-Zeilen.
                //   Guthaben (Haben-Verbindlichkeit): gedeckt durch Auszahlungs-Soll-Zeilen.
                decimal gedeckt = 0;
                var nachzahlungsZeile = saldoZeilen.FirstOrDefault(z => z.SollHaben == SollHaben.Soll);
                var guthabenZeile = saldoZeilen.FirstOrDefault(z => z.SollHaben == SollHaben.Haben);
                if (nachzahlungsZeile != null)
                {
                    foreach (var ausgleich in nachzahlungsZeile.AlsSollZeile)
                    {
                        gedeckt += ausgleich.HabenZeile.Betrag;
                        AusgleichsZahlungen.Add(new AusgleichsZahlungInfo
                        {
                            Datum = ausgleich.HabenZeile.Buchungssatz.Buchungsdatum,
                            Betrag = ausgleich.HabenZeile.Betrag,
                            BuchungssatzId = ausgleich.HabenZeile.Buchungssatz.BuchungssatzId
                        });
                    }
                }
                else if (guthabenZeile != null)
                {
                    foreach (var ausgleich in guthabenZeile.AlsHabenZeile)
                    {
                        gedeckt += ausgleich.SollZeile.Betrag;
                        AusgleichsZahlungen.Add(new AusgleichsZahlungInfo
                        {
                            Datum = ausgleich.SollZeile.Buchungssatz.Buchungsdatum,
                            Betrag = ausgleich.SollZeile.Betrag,
                            BuchungssatzId = ausgleich.SollZeile.Buchungssatz.BuchungssatzId
                        });
                    }
                }

                AusgleichsZahlungen = [.. AusgleichsZahlungen.OrderBy(z => z.Datum)];
                OffenerBetrag = Math.Max(0, Math.Abs(Saldo) - gedeckt);
                Ausgeglichen = Math.Abs(Saldo) - gedeckt <= 0.005m;

                Permissions = permissions;
            }
        }

        public class AbrechnungsresultatEntry : AbrechnungsresultatEntryBase
        {
            private Abrechnungsresultat Entity { get; } = null!;
            public SelectionEntry? Vertrag { get; set; } = null!;

            public AbrechnungsresultatEntry() : base() { }
            public AbrechnungsresultatEntry(Abrechnungsresultat entity, Permissions permissions)
                : base(entity, permissions)
            {
                Entity = entity;
                var v = entity.Vertrag;
                var a = v.Wohnung.Adresse?.Anschrift ?? "Unbekannte Anschrift";
                Vertrag = new(v.VertragId, a + " - " + v.Wohnung.Bezeichnung);
            }
        }


        private readonly ILogger<AbrechnungsresultatController> _logger;
        protected override AbrechnungsresultatDbService DbService { get; }

        public AbrechnungsresultatController(
            ILogger<AbrechnungsresultatController> logger,
            AbrechnungsresultatDbService service,
            HttpClient httpClient) : base(logger, httpClient)
        {
            DbService = service;
            _logger = logger;
        }

        [HttpGet("vertrag/{vertragId}/jahr/{jahr}")]
        public Task<ActionResult<AbrechnungsresultatEntry>> GetAbrechnungsResultatFromVertrag(int vertragId, int jahr)
        {
            return DbService.Get(User!, vertragId, jahr);

        }

        [HttpGet("{id}")]
        public Task<ActionResult<AbrechnungsresultatEntry>> GetAbrechnungsResultat(Guid id)
        {
            return DbService.Get(User!, id);
        }

        [HttpPut("{id}")]
        public Task<ActionResult<AbrechnungsresultatEntry>> PutAbrechnungsResultat(Guid id, AbrechnungsresultatEntry entry)
        {
            return DbService.Put(User!, id, entry);
        }

        [HttpDelete("{id}")]
        public Task<ActionResult> DeleteAbrechnungsResultat(Guid id)
        {
            return DbService.Delete(User!, id);
        }
    }
}
