using Deeplex.Saverwalter.Model.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Deeplex.Saverwalter.Model
{
    public sealed class SaverwalterContext : DbContext
    {
        public DbSet<Adresse> Adressen { get; set; } = null!;
        public DbSet<Betriebskostenrechnung> Betriebskostenrechnungen { get; set; } = null!;
        public DbSet<Erhaltungsaufwendung> Erhaltungsaufwendungen { get; set; } = null!;
        public DbSet<Garage> Garagen { get; set; } = null!;
        public DbSet<HKVO> HKVO { get; set; } = null!;
        public DbSet<JuristischePerson> JuristischePersonen { get; set; } = null!;
        public DbSet<Konto> Kontos { get; set; } = null!;
        public DbSet<Miete> Mieten { get; set; } = null!;
        public DbSet<Mieter> MieterSet { get; set; } = null!;
        public DbSet<Mietminderung> Mietminderungen { get; set; } = null!;
        public DbSet<NatuerlichePerson> NatuerlichePersonen { get; set; } = null!;
        public DbSet<Umlage> Umlagen { get; set; } = null!;
        public DbSet<Umlagetyp> Umlagetypen { get; set; } = null!;
        public DbSet<Vertrag> Vertraege { get; set; } = null!;
        public DbSet<VertragVersion> VertragVersionen { get; set; } = null!;
        public DbSet<Wohnung> Wohnungen { get; set; } = null!;
        public DbSet<Zaehler> ZaehlerSet { get; set; } = null!;
        public DbSet<Zaehlerstand> Zaehlerstaende { get; set; } = null!;

        public DbSet<UserAccount> UserAccounts { get; set; } = null!;
        public DbSet<Pbkdf2PasswordCredential> Pbkdf2PasswordCredentials { get; set; } = null!;

        public override int SaveChanges()
        {
            var entries = ChangeTracker.Entries().Where(e => e.State == EntityState.Modified).ToList();
            foreach (var entry in entries)
            {
                if (entry.Metadata.FindProperty("LastModified") != null)
                {
                    entry.Property("LastModified").CurrentValue = DateTime.Now.ToUniversalTime();
                }
            }

            return base.SaveChanges();
        }

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

        // Careful postgres only:
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<JuristischePerson>().HasAlternateKey(person => person.PersonId);
            modelBuilder.Entity<NatuerlichePerson>().HasAlternateKey(person => person.PersonId);

            modelBuilder.Entity<Adresse>().Property(b => b.CreatedAt).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<Adresse>().Property(b => b.LastModified).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<Betriebskostenrechnung>().Property(b => b.CreatedAt).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<Betriebskostenrechnung>().Property(b => b.LastModified).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<Erhaltungsaufwendung>().Property(b => b.CreatedAt).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<Erhaltungsaufwendung>().Property(b => b.LastModified).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<Garage>().Property(b => b.CreatedAt).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<Garage>().Property(b => b.LastModified).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<HKVO>().Property(b => b.CreatedAt).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<HKVO>().Property(b => b.LastModified).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<JuristischePerson>().Property(b => b.CreatedAt).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<JuristischePerson>().Property(b => b.LastModified).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<Konto>().Property(b => b.CreatedAt).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<Konto>().Property(b => b.LastModified).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<Miete>().Property(b => b.CreatedAt).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<Miete>().Property(b => b.LastModified).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<Mieter>().Property(b => b.CreatedAt).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<Mieter>().Property(b => b.LastModified).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<Mietminderung>().Property(b => b.CreatedAt).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<Mietminderung>().Property(b => b.LastModified).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<NatuerlichePerson>().Property(b => b.CreatedAt).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<NatuerlichePerson>().Property(b => b.LastModified).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<Umlage>().Property(b => b.CreatedAt).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<Umlage>().Property(b => b.LastModified).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<Umlagetyp>().Property(b => b.CreatedAt).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<Umlagetyp>().Property(b => b.LastModified).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<Vertrag>().Property(b => b.CreatedAt).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<Vertrag>().Property(b => b.LastModified).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<VertragsBetriebskostenrechnung>()
                .Property(b => b.CreatedAt).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<VertragsBetriebskostenrechnung>()
                .Property(b => b.LastModified).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<Wohnung>().Property(b => b.CreatedAt).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<Wohnung>().Property(b => b.LastModified).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<Zaehler>().Property(b => b.CreatedAt).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<Zaehler>().Property(b => b.LastModified).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<Zaehlerstand>().Property(b => b.CreatedAt).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<Zaehlerstand>().Property(b => b.LastModified).HasDefaultValueSql("NOW()");

            base.OnModelCreating(modelBuilder);
            modelBuilder.HasPostgresExtension("uuid-ossp");
        }
    }
}