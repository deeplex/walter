// Copyright (c) 2023-2025 Henrik S. Gaßmann, Kai Lawrence
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using Deeplex.Saverwalter.Model.Auth;
using Microsoft.EntityFrameworkCore;

namespace Deeplex.Saverwalter.Model
{
    public sealed class SaverwalterContext : DbContext
    {
        public DbSet<Abrechnungsresultat> Abrechnungsresultate { get; set; } = null!;
        public DbSet<Abrechnungsverzicht> Abrechnungsverzichte { get; set; } = null!;
        public DbSet<Adresse> Adressen { get; set; } = null!;
        public DbSet<Buchungskonto> Buchungskonten { get; set; } = null!;
        public DbSet<Buchungssatz> Buchungssaetze { get; set; } = null!;
        public DbSet<Buchungszeile> Buchungszeilen { get; set; } = null!;
        public DbSet<OffenerPostenAusgleich> OffenePostenAusgleiche { get; set; } = null!;
        public DbSet<KontaktMitgliedschaft> KontaktMitgliedschaften { get; set; } = null!;
        public DbSet<WohnungEigentuemer> WohnungEigentuemer { get; set; } = null!;
        public DbSet<Garage> Garagen { get; set; } = null!;
        public DbSet<GarageVertrag> GarageVertraege { get; set; } = null!;
        public DbSet<GarageVertragVersion> GarageVertragVersionen { get; set; } = null!;
        public DbSet<HKVO> HKVO { get; set; } = null!;
        public DbSet<UmlageVersion> UmlageVersionen { get; set; } = null!;
        public DbSet<WohnungVersion> WohnungVersionen { get; set; } = null!;
        public DbSet<Bankkonto> Bankkontos { get; set; } = null!;
        public DbSet<Mietminderung> Mietminderungen { get; set; } = null!;
        public DbSet<Kontakt> Kontakte { get; set; } = null!;
        public DbSet<Transaktion> Transaktionen { get; set; } = null!;
        public DbSet<Umlage> Umlagen { get; set; } = null!;
        public DbSet<Umlagetyp> Umlagetypen { get; set; } = null!;
        public DbSet<Vertrag> Vertraege { get; set; } = null!;
        public DbSet<VertragVersion> VertragVersionen { get; set; } = null!;
        public DbSet<Verwalter> VerwalterSet { get; set; } = null!;
        public DbSet<Wohnung> Wohnungen { get; set; } = null!;
        public DbSet<Zaehler> ZaehlerSet { get; set; } = null!;
        public DbSet<Zaehlerstand> Zaehlerstaende { get; set; } = null!;

        public DbSet<UserAccount> UserAccounts { get; set; } = null!;
        public DbSet<UserResetCredential> UserResetCredentials { get; set; } = null!;
        public DbSet<Pbkdf2PasswordCredential> Pbkdf2PasswordCredentials { get; set; } = null!;

        private void setLastModified()
        {
            var entries = ChangeTracker.Entries().Where(e => e.State == EntityState.Modified).ToList();
            foreach (var entry in entries)
            {
                if (entry.Metadata.FindProperty("LastModified") != null)
                {
                    entry.Property("LastModified").CurrentValue = DateTime.Now.ToUniversalTime();
                }
            }
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            setLastModified();
            return await base.SaveChangesAsync(cancellationToken);
        }

        public override int SaveChanges()
        {
            setLastModified();
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

        // Careful postgres only:
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasSequence<long>("buchungsnummer_seq").StartsAt(1).IncrementsBy(1);
            modelBuilder.Entity<Buchungssatz>()
                .Property(b => b.Buchungsnummer)
                .HasDefaultValueSql("nextval('buchungsnummer_seq')")
                .ValueGeneratedOnAdd();


            modelBuilder.Entity<HKVO>().HasOne(u => u.Heizkosten).WithMany(u => u.HeizkostenHKVOs).HasForeignKey(u => u.HeizkostenId);
            modelBuilder.Entity<HKVO>().HasOne(u => u.Betriebsstrom).WithMany(u => u.BetriebsstromHKVOs);
            modelBuilder.Entity<HKVO>()
                .HasMany(h => h.AllgemeinWaermeZaehler)
                .WithMany()
                .UsingEntity<Dictionary<string, object>>(
                    "HkvoAllgemeinWaermeZaehler",
                    r => r.HasOne<Zaehler>().WithMany().HasForeignKey("ZaehlerId").OnDelete(DeleteBehavior.Cascade),
                    l => l.HasOne<HKVO>().WithMany().HasForeignKey("HkvoId").OnDelete(DeleteBehavior.Cascade),
                    j =>
                    {
                        j.HasKey("HkvoId", "ZaehlerId");
                        j.ToTable("hkvo_allgemein_waerme_zaehler");
                    });

            modelBuilder.Entity<Vertrag>().HasOne(u => u.Ansprechpartner).WithMany(u => u.VerwaltetVertraege);
            modelBuilder.Entity<Vertrag>().HasMany(u => u.Mieter).WithMany(u => u.Mietvertraege);

            modelBuilder.Entity<OffenerPostenAusgleich>()
                .HasOne(o => o.SollZeile)
                .WithMany(z => z.AlsSollZeile)
                .HasForeignKey("SollZeileId")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();
            modelBuilder.Entity<OffenerPostenAusgleich>()
                .HasOne(o => o.HabenZeile)
                .WithMany(z => z.AlsHabenZeile)
                .HasForeignKey("HabenZeileId")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            modelBuilder.Entity<Bankkonto>()
                .HasOne(b => b.BuchungsKonto)
                .WithMany()
                .HasForeignKey("BuchungsKontoId")
                .IsRequired();
            modelBuilder.Entity<Bankkonto>()
                .HasMany(b => b.Besitzer)
                .WithMany()
                .UsingEntity<Dictionary<string, object>>(
                    "BankkontoKontakt",
                    r => r.HasOne<Kontakt>().WithMany().HasForeignKey("KontaktId").OnDelete(DeleteBehavior.Cascade),
                    l => l.HasOne<Bankkonto>().WithMany().HasForeignKey("BankkontoId").OnDelete(DeleteBehavior.Cascade),
                    j =>
                    {
                        j.HasKey("BankkontoId", "KontaktId").HasName("pk_bankkonto_besitzer");
                        j.HasIndex("KontaktId").HasDatabaseName("ix_bankkonto_besitzer_kontakt_id");
                        j.ToTable("bankkonto_besitzer");
                    });

            modelBuilder.Entity<Buchungssatz>()
                .HasOne(b => b.Transaktion)
                .WithMany(t => t.Buchungssaetze)
                .HasForeignKey("TransaktionId")
                .IsRequired(false);

            modelBuilder.Entity<Buchungssatz>()
                .HasOne(b => b.StornoVon)
                .WithOne(b => b.StornoNach)
                .HasForeignKey<Buchungssatz>("StornoVonId")
                .IsRequired(false);
            modelBuilder.Entity<Buchungssatz>()
                .HasIndex(b => new { b.Buchungsjahr, b.Buchungsnummer })
                .IsUnique();

            modelBuilder.Entity<Vertrag>()
                .HasOne(v => v.MietBuchungskonto)
                .WithMany()
                .HasForeignKey("MietBuchungskontoId")
                .IsRequired();
            modelBuilder.Entity<Vertrag>()
                .HasOne(v => v.NkBuchungskonto)
                .WithMany()
                .HasForeignKey("NkBuchungskontoId")
                .IsRequired();
            modelBuilder.Entity<Vertrag>()
                .HasOne(v => v.BkAbrechnungsKonto)
                .WithMany()
                .HasForeignKey("BkAbrechnungsKontoId")
                .IsRequired();

            modelBuilder.Entity<Wohnung>()
                .HasOne(w => w.MietErtragskonto)
                .WithMany()
                .HasForeignKey("MietErtragskontoId")
                .IsRequired();
            modelBuilder.Entity<Wohnung>()
                .HasOne(w => w.AufwandsKonto)
                .WithMany()
                .HasForeignKey("AufwandsKontoId")
                .IsRequired();

            modelBuilder.Entity<Kontakt>()
                .HasOne(k => k.VerbindlichkeitsKonto)
                .WithMany()
                .HasForeignKey("VerbindlichkeitsKontoId")
                .IsRequired(false);

            modelBuilder.Entity<KontaktMitgliedschaft>()
                .HasOne(m => m.JuristischePerson)
                .WithMany(k => k.AlsJuristischePerson)
                .HasForeignKey("JuristischePersonId")
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<KontaktMitgliedschaft>()
                .HasOne(m => m.Mitglied)
                .WithMany(k => k.AlsMitglied)
                .HasForeignKey("MitgliedId")
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<WohnungEigentuemer>()
                .HasOne(e => e.Wohnung)
                .WithMany(w => w.Eigentuemer)
                .HasForeignKey("WohnungId")
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<WohnungEigentuemer>()
                .HasOne(e => e.Kontakt)
                .WithMany(k => k.EigentuemerIn)
                .HasForeignKey("KontaktId")
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Umlage>()
                .HasOne(u => u.NkVerrechnungsKonto)
                .WithMany()
                .HasForeignKey("NkVerrechnungsKontoId")
                .IsRequired();
            modelBuilder.Entity<Umlage>()
                .HasOne(u => u.ZahlungsKonto)
                .WithMany()
                .HasForeignKey("ZahlungsKontoId")
                .IsRequired();

            modelBuilder.Entity<Vertrag>()
                .HasOne(v => v.ZahlungsKonto)
                .WithMany()
                .HasForeignKey("ZahlungsKontoId")
                .IsRequired();

            modelBuilder.Entity<Garage>()
                .HasOne(g => g.Ertragskonto)
                .WithMany()
                .HasForeignKey("ErtragskontoId")
                .IsRequired();
            modelBuilder.Entity<GarageVertrag>()
                .HasOne(g => g.MietBuchungskonto)
                .WithMany()
                .HasForeignKey("MietBuchungskontoId")
                .IsRequired();
            modelBuilder.Entity<GarageVertrag>()
                .HasOne(g => g.ZahlungsKonto)
                .WithMany()
                .HasForeignKey("ZahlungsKontoId")
                .IsRequired();
            modelBuilder.Entity<GarageVertrag>()
                .HasMany(g => g.Mieter)
                .WithMany()
                .UsingEntity<Dictionary<string, object>>(
                    "GarageVertragKontakt",
                    r => r.HasOne<Kontakt>().WithMany().HasForeignKey("KontaktId").OnDelete(DeleteBehavior.Cascade),
                    l => l.HasOne<GarageVertrag>().WithMany().HasForeignKey("GarageVertragId").OnDelete(DeleteBehavior.Cascade),
                    j =>
                    {
                        j.HasKey("GarageVertragId", "KontaktId").HasName("pk_garage_vertrag_mieter");
                        j.HasIndex("KontaktId").HasDatabaseName("ix_garage_vertrag_mieter_kontakt_id");
                        j.ToTable("garage_vertrag_mieter");
                    });

            modelBuilder.Entity<Abrechnungsresultat>().Property(b => b.CreatedAt).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<Abrechnungsresultat>().Property(b => b.LastModified).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<Abrechnungsverzicht>().Property(b => b.CreatedAt).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<Abrechnungsverzicht>().Property(b => b.LastModified).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<Abrechnungsverzicht>()
                .HasOne(v => v.Vertrag)
                .WithMany()
                .HasForeignKey("VertragId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
            modelBuilder.Entity<Abrechnungsverzicht>()
                .HasIndex("VertragId", "Jahr")
                .IsUnique();
            modelBuilder.Entity<Adresse>().Property(b => b.CreatedAt).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<Adresse>().Property(b => b.LastModified).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<Garage>().Property(b => b.CreatedAt).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<Garage>().Property(b => b.LastModified).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<GarageVertrag>().Property(b => b.CreatedAt).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<GarageVertrag>().Property(b => b.LastModified).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<GarageVertragVersion>().Property(b => b.CreatedAt).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<GarageVertragVersion>().Property(b => b.LastModified).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<HKVO>().Property(b => b.CreatedAt).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<HKVO>().Property(b => b.LastModified).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<Kontakt>().Property(b => b.CreatedAt).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<Kontakt>().Property(b => b.LastModified).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<Bankkonto>().Property(b => b.CreatedAt).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<Bankkonto>().Property(b => b.LastModified).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<Mietminderung>().Property(b => b.CreatedAt).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<Mietminderung>().Property(b => b.LastModified).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<Transaktion>().Property(b => b.CreatedAt).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<Transaktion>().Property(b => b.LastModified).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<Umlage>().Property(b => b.CreatedAt).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<Umlage>().Property(b => b.LastModified).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<UmlageVersion>().Property(b => b.CreatedAt).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<UmlageVersion>().Property(b => b.LastModified).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<WohnungVersion>().Property(b => b.CreatedAt).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<WohnungVersion>().Property(b => b.LastModified).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<Umlagetyp>().Property(b => b.CreatedAt).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<Umlagetyp>().Property(b => b.LastModified).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<Vertrag>().Property(b => b.CreatedAt).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<Vertrag>().Property(b => b.LastModified).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<Verwalter>().Property(b => b.CreatedAt).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<Verwalter>().Property(b => b.LastModified).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<Wohnung>().Property(b => b.CreatedAt).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<Wohnung>().Property(b => b.LastModified).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<Zaehler>().Property(b => b.CreatedAt).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<Zaehler>().Property(b => b.LastModified).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<Zaehlerstand>().Property(b => b.CreatedAt).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<Zaehlerstand>().Property(b => b.LastModified).HasDefaultValueSql("NOW()");

            modelBuilder.Entity<Buchungskonto>().Property(b => b.CreatedAt).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<Buchungskonto>().Property(b => b.LastModified).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<Buchungssatz>().Property(b => b.CreatedAt).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<Buchungssatz>().Property(b => b.LastModified).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<Buchungszeile>().Property(b => b.CreatedAt).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<Buchungszeile>().Property(b => b.LastModified).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<KontaktMitgliedschaft>().Property(b => b.CreatedAt).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<KontaktMitgliedschaft>().Property(b => b.LastModified).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<WohnungEigentuemer>().Property(b => b.CreatedAt).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<WohnungEigentuemer>().Property(b => b.LastModified).HasDefaultValueSql("NOW()");

            modelBuilder.Entity<UserAccount>().Property(b => b.CreatedAt).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<UserAccount>().Property(b => b.LastModified).HasDefaultValueSql("NOW()");

            modelBuilder.HasPostgresExtension("uuid-ossp");
            base.OnModelCreating(modelBuilder);
        }
    }
}
