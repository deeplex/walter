using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;
using System;
using System.Linq;
using System.Text;

namespace Deeplex.Saverwalter.Model
{
    /// <summary>A replacement for <see cref="NpgsqlSqlGenerationHelper"/>
    /// to convert PascalCaseCsharpyIdentifiers to alllowercasenames.
    /// So table and column names with no embedded punctuation
    /// get generated with no quotes or delimiters.</summary>
    public class NpgsqlSqlGenerationLowercasingHelper : NpgsqlSqlGenerationHelper
    {
        //Don't lowercase ef's migration table
        const string dontAlter = "__EFMigrationsHistory";
        static string Customize(string input) => input == dontAlter ? input : input.ToLower();
        public NpgsqlSqlGenerationLowercasingHelper(RelationalSqlGenerationHelperDependencies dependencies)
            : base(dependencies) { }
        public override string DelimitIdentifier(string identifier)
            => base.DelimitIdentifier(Customize(identifier));
        public override void DelimitIdentifier(StringBuilder builder, string identifier)
            => base.DelimitIdentifier(builder, Customize(identifier));
    }

    public sealed class SaverwalterContext : DbContext
    {
        private bool mPreconfigured = false;

        public DbSet<Adresse> Adressen { get; set; } = null!;
        public DbSet<Anhang> Anhaenge { get; set; } = null!;
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
            options.ReplaceService<ISqlGenerationHelper, NpgsqlSqlGenerationLowercasingHelper>();

            if (!mPreconfigured)
            {
                //options.UseSqlite("Data Source=walter.db");
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
            modelBuilder.Entity<JuristischePerson>()
                .HasAlternateKey(jp => jp.PersonId);
            modelBuilder.Entity<NatuerlichePerson>()
                .HasAlternateKey(np => np.PersonId);

            base.OnModelCreating(modelBuilder);
            modelBuilder.HasPostgresExtension("uuid-ossp");
        }
    }
}