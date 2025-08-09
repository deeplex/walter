
using Microsoft.AspNetCore.Mvc;

using Deeplex.Saverwalter.Model;
using static Deeplex.Saverwalter.WebAPI.Services.Utils;
using static Deeplex.Saverwalter.WebAPI.Controllers.AbrechnungsresultatController;
using Deeplex.Saverwalter.WebAPI.Services;
using Deeplex.Saverwalter.WebAPI.Services.ControllerService;
using static Deeplex.Saverwalter.WebAPI.Controllers.Services.SelectionListController;

namespace Deeplex.Saverwalter.WebAPI.Controllers
{
    [ApiController]
    [Route("api/abrechnungsresultate")]
    public class AbrechnungsresultatController : FileControllerBase<AbrechnungsresultatEntry, Guid, Abrechnungsresultat>
    {
        protected override WalterDbServiceBase<AbrechnungsresultatEntry, Guid, Abrechnungsresultat> DbService => throw new NotImplementedException();

        public class AbrechnungsresultatEntryBase
        {
            public Guid Id { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime LastModified { get; set; }
            public int Jahr { get; set; }
            public double Kaltmiete { get; set; }
            public double Vorauszahlung { get; set; }
            public double Rechnungsbetrag { get; set; }
            public double Minderung { get; set; }
            public bool Abgesendet { get; set; }
            public double Saldo { get; set; }
            public string? Notiz { get; set; }

            public Permissions Permissions { get; set; } = new Permissions();

            public AbrechnungsresultatEntryBase() { }
            public AbrechnungsresultatEntryBase(Abrechnungsresultat entity, Permissions permissions)
            {
                Id = entity.AbrechnungsresultatId;
                CreatedAt = entity.CreatedAt;
                LastModified = entity.LastModified;
                Jahr = entity.Jahr;
                Kaltmiete = entity.Kaltmiete;
                Vorauszahlung = entity.Vorauszahlung;
                Rechnungsbetrag = entity.Rechnungsbetrag;
                Minderung = entity.Minderung;
                Abgesendet = entity.Abgesendet;
                Saldo = entity.Saldo;
                Notiz = entity.Notiz;

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
        private AbrechnungsresultatDbService Service { get; }

        public AbrechnungsresultatController(
            ILogger<AbrechnungsresultatController> logger,
            AbrechnungsresultatDbService service,
            HttpClient httpClient) : base(logger, httpClient)
        {
            Service = service;
            _logger = logger;
        }

        [HttpGet("{id}")]
        public Task<ActionResult<AbrechnungsresultatEntry>> GetAbrechnungsResultat(Guid id)
        {
            return Service.Get(User!, id);
        }

        [HttpPut("{id}")]
        public Task<ActionResult<AbrechnungsresultatEntry>> PutAbrechnungsResultat(Guid id, AbrechnungsresultatEntry entry)
        {
            return Service.Put(User!, id, entry);
        }

        [HttpDelete("{id}")]
        public Task<ActionResult> DeleteAbrechnungsResultat(Guid id)
        {
            return Service.Delete(User!, id);
        }
    }
}