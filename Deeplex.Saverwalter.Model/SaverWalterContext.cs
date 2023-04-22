using Deeplex.Saverwalter.Model.Auth;
using Microsoft.EntityFrameworkCore;

namespace Deeplex.Saverwalter.Model
{
    public sealed class SaverwalterContext : DbContext
    {
        public DbSet<Adresse> Adressen { get; set; } = null!;
        public DbSet<Betriebskostenrechnung> Betriebskostenrechnungen { get; set; } = null!;
        public DbSet<Erhaltungsaufwendung> Erhaltungsaufwendungen { get; set; } = null!;
        public DbSet<Garage> Garagen { get; set; } = null!;
        public DbSet<JuristischePerson> JuristischePersonen { get; set; } = null!;
        public DbSet<Konto> Kontos { get; set; } = null!;
        public DbSet<Miete> Mieten { get; set; } = null!;
        public DbSet<Mieter> MieterSet { get; set; } = null!;
        public DbSet<Mietminderung> Mietminderungen { get; set; } = null!;
        public DbSet<NatuerlichePerson> NatuerlichePersonen { get; set; } = null!;
        public DbSet<Umlage> Umlagen { get; set; } = null!;
        public DbSet<Vertrag> Vertraege { get; set; } = null!;
        public DbSet<VertragVersion> VertragVersionen { get; set; } = null!;
        public DbSet<Wohnung> Wohnungen { get; set; } = null!;
        public DbSet<Zaehler> ZaehlerSet { get; set; } = null!;
        public DbSet<Zaehlerstand> Zaehlerstaende { get; set; } = null!;

        public DbSet<UserAccount> UserAccounts { get; set; } = null!;
        public DbSet<Pbkdf2PasswordCredential> Pbkdf2PasswordCredentials { get; set; } = null!;

        public SaverwalterContext(DbContextOptions<SaverwalterContext> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options
                .UseLazyLoadingProxies()
                .UseSnakeCaseNamingConvention();

        }

        public IPerson FindPerson(Guid PersonId)
        {

            if (JuristischePersonen.SingleOrDefault(j => j.PersonId == PersonId) is JuristischePerson j)
            {
                return j;
            }
            else if (NatuerlichePersonen.SingleOrDefault(n => n.PersonId == PersonId) is NatuerlichePerson n)
            {
                return n;
            }
            else
            {
                throw new ArgumentException("FindPerson: PersonId does not belong to a NatuerlichePerson or JuristischePerson");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<JuristischePerson>().HasAlternateKey(person => person.PersonId);
            modelBuilder.Entity<NatuerlichePerson>().HasAlternateKey(person => person.PersonId);

            base.OnModelCreating(modelBuilder);
            modelBuilder.HasPostgresExtension("uuid-ossp");
        }
    }
}