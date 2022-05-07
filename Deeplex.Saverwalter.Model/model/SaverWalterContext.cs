using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace Deeplex.Saverwalter.Model
{
    public sealed class SaverwalterContext : DbContext
    {
        private bool mPreconfigured = false;

        public DbSet<Adresse> Adressen { get; set; } = null!;
        public DbSet<AdresseAnhang> AdresseAnhaenge { get; set; } = null!;
        public DbSet<Anhang> Anhaenge { get; set; } = null!;
        public DbSet<Betriebskostenrechnung> Betriebskostenrechnungen { get; set; } = null!;
        public DbSet<BetriebskostenrechnungAnhang> BetriebskostenrechnungAnhaenge { get; set; } = null!;
        public DbSet<BetriebskostenrechnungsGruppe> Betriebskostenrechnungsgruppen { get; set; } = null!;
        public DbSet<Erhaltungsaufwendung> Erhaltungsaufwendungen { get; set; } = null!;
        public DbSet<ErhaltungsaufwendungAnhang> ErhaltungsaufwendungAnhaenge { get; set; } = null!;
        public DbSet<Garage> Garagen { get; set; } = null!;
        public DbSet<GarageAnhang> GarageAnhaenge { get; set; } = null!;
        public DbSet<JuristischePerson> JuristischePersonen { get; set; } = null!;
        public DbSet<JuristischePersonenMitglied> JuristischePersonenMitglieder { get; set; } = null!;
        public DbSet<JuristischePersonAnhang> JuristischePersonAnhaenge { get; set; } = null!;
        public DbSet<Konto> Kontos { get; set; } = null!;
        public DbSet<KontoAnhang> KontoAnhaenge { get; set; } = null!;
        public DbSet<Miete> Mieten { get; set; } = null!;
        public DbSet<MieteAnhang> MieteAnhaenge { get; set; } = null!;
        public DbSet<Mieter> MieterSet { get; set; } = null!;
        public DbSet<MietMinderung> MietMinderungen { get; set; } = null!;
        public DbSet<MietMinderungAnhang> MietMinderungAnhaenge { get; set; } = null!;
        public DbSet<MietobjektGarage> MietobjektGaragen { get; set; } = null!;
        public DbSet<NatuerlichePerson> NatuerlichePersonen { get; set; } = null!;
        public DbSet<NatuerlichePersonAnhang> NatuerlichePersonAnhaenge { get; set; } = null!;
        public DbSet<Vertrag> Vertraege { get; set; } = null!;
        public DbSet<VertragAnhang> VertragAnhaenge { get; set; } = null!;
        public DbSet<Wohnung> Wohnungen { get; set; } = null!;
        public DbSet<WohnungAnhang> WohnungAnhaenge { get; set; } = null!;
        public DbSet<Zaehler> ZaehlerSet { get; set; } = null!;
        public DbSet<ZaehlerAnhang> ZaehlerAnhaenge { get; set; } = null!;
        public DbSet<Zaehlerstand> Zaehlerstaende { get; set; } = null!;
        public DbSet<ZaehlerstandAnhang> ZaehlerstandAnhaenge { get; set; } = null!;

        public SaverwalterContext()
            : base()
        {
        }
        public SaverwalterContext(DbContextOptions<SaverwalterContext> options)
            : base(options)
        {
            mPreconfigured = true;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            if (!mPreconfigured)
            {
                options.UseSqlite("Data Source=walter.db");
            }
        }

        public IPerson FindPerson(Guid PersonId)
        {
            var left = JuristischePersonen.SingleOrDefault(j => PersonId == j.PersonId);
            if (left != null)
            {
                return left;
            }
            return NatuerlichePersonen.SingleOrDefault(n => PersonId == n.PersonId);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Vertrag>()
                .HasKey(v => v.rowid);
            modelBuilder.Entity<Vertrag>()
                .HasAlternateKey("VertragId", "Version");
            modelBuilder.Entity<Vertrag>()
                .Property(v => v.Version);

            modelBuilder.Entity<JuristischePerson>()
                .HasAlternateKey(jp => jp.PersonId);
            modelBuilder.Entity<NatuerlichePerson>()
                .HasAlternateKey(np => np.PersonId);
        }
    }
}