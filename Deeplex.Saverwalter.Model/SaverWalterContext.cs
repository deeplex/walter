// Copyright (c) 2023-2024 Henrik S. Gaßmann, Kai Lawrence
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
        public DbSet<Adresse> Adressen { get; set; } = null!;
        public DbSet<Betriebskostenrechnung> Betriebskostenrechnungen { get; set; } = null!;
        public DbSet<Erhaltungsaufwendung> Erhaltungsaufwendungen { get; set; } = null!;
        public DbSet<Garage> Garagen { get; set; } = null!;
        public DbSet<HKVO> HKVO { get; set; } = null!;
        public DbSet<Konto> Kontos { get; set; } = null!;
        public DbSet<Miete> Mieten { get; set; } = null!;
        public DbSet<Mietminderung> Mietminderungen { get; set; } = null!;
        public DbSet<Kontakt> Kontakte { get; set; } = null!;
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

            modelBuilder.Entity<HKVO>().HasOne(u => u.Heizkosten).WithOne(u => u.HKVO);
            modelBuilder.Entity<HKVO>().HasOne(u => u.Betriebsstrom).WithMany(u => u.HKVOs);

            modelBuilder.Entity<Vertrag>().HasOne(u => u.Ansprechpartner).WithMany(u => u.VerwaltetVertraege);
            modelBuilder.Entity<Vertrag>().HasMany(u => u.Mieter).WithMany(u => u.Mietvertraege);

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
            modelBuilder.Entity<Kontakt>().Property(b => b.CreatedAt).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<Kontakt>().Property(b => b.LastModified).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<Konto>().Property(b => b.CreatedAt).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<Konto>().Property(b => b.LastModified).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<Miete>().Property(b => b.CreatedAt).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<Miete>().Property(b => b.LastModified).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<Mietminderung>().Property(b => b.CreatedAt).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<Mietminderung>().Property(b => b.LastModified).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<Umlage>().Property(b => b.CreatedAt).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<Umlage>().Property(b => b.LastModified).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<Umlagetyp>().Property(b => b.CreatedAt).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<Umlagetyp>().Property(b => b.LastModified).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<Vertrag>().Property(b => b.CreatedAt).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<Vertrag>().Property(b => b.LastModified).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<Verwalter>().Property(b => b.CreatedAt).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<Verwalter>().Property(b => b.LastModified).HasDefaultValueSql("NOW()");
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

            modelBuilder.Entity<UserAccount>().Property(b => b.CreatedAt).HasDefaultValueSql("NOW()");
            modelBuilder.Entity<UserAccount>().Property(b => b.LastModified).HasDefaultValueSql("NOW()");

            modelBuilder.HasPostgresExtension("uuid-ossp");
            base.OnModelCreating(modelBuilder);
        }
    }
}
