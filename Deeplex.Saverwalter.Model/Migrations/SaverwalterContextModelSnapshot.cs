﻿// <auto-generated />
using System;
using Deeplex.Saverwalter.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Deeplex.Saverwalter.Model.Migrations
{
    [DbContext(typeof(SaverwalterContext))]
    partial class SaverwalterContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.3");

            modelBuilder.Entity("Deeplex.Saverwalter.Model.Adresse", b =>
                {
                    b.Property<int>("AdresseId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Hausnummer")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Notiz")
                        .HasColumnType("TEXT");

                    b.Property<string>("Postleitzahl")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Stadt")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Strasse")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("AdresseId");

                    b.ToTable("Adressen");
                });

            modelBuilder.Entity("Deeplex.Saverwalter.Model.Betriebskostenrechnung", b =>
                {
                    b.Property<int>("BetriebskostenrechnungId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Beschreibung")
                        .HasColumnType("TEXT");

                    b.Property<double>("Betrag")
                        .HasColumnType("REAL");

                    b.Property<int>("BetreffendesJahr")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("Datum")
                        .HasColumnType("TEXT");

                    b.Property<string>("Notiz")
                        .HasColumnType("TEXT");

                    b.Property<int>("Schluessel")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Typ")
                        .HasColumnType("INTEGER");

                    b.HasKey("BetriebskostenrechnungId");

                    b.ToTable("Betriebskostenrechnungen");
                });

            modelBuilder.Entity("Deeplex.Saverwalter.Model.Betriebskostenrechnungsgruppe", b =>
                {
                    b.Property<int>("BetriebskostenrechnungsgruppeId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("RechnungBetriebskostenrechnungId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("WohnungId")
                        .HasColumnType("INTEGER");

                    b.HasKey("BetriebskostenrechnungsgruppeId");

                    b.HasIndex("RechnungBetriebskostenrechnungId");

                    b.HasIndex("WohnungId");

                    b.ToTable("Betriebskostenrechnungsgruppen");
                });

            modelBuilder.Entity("Deeplex.Saverwalter.Model.Garage", b =>
                {
                    b.Property<int>("GarageId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("AdresseId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("BesitzerJuristischePersonId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Kennung")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Notiz")
                        .HasColumnType("TEXT");

                    b.HasKey("GarageId");

                    b.HasIndex("AdresseId");

                    b.HasIndex("BesitzerJuristischePersonId");

                    b.ToTable("Garagen");
                });

            modelBuilder.Entity("Deeplex.Saverwalter.Model.JuristischePerson", b =>
                {
                    b.Property<int>("JuristischePersonId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int?>("AdresseId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Bezeichnung")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Email")
                        .HasColumnType("TEXT");

                    b.Property<string>("Fax")
                        .HasColumnType("TEXT");

                    b.Property<string>("Mobil")
                        .HasColumnType("TEXT");

                    b.Property<string>("Notiz")
                        .HasColumnType("TEXT");

                    b.Property<string>("Telefon")
                        .HasColumnType("TEXT");

                    b.HasKey("JuristischePersonId");

                    b.HasIndex("AdresseId");

                    b.ToTable("JuristischePersonen");
                });

            modelBuilder.Entity("Deeplex.Saverwalter.Model.JuristischePersonenMitglied", b =>
                {
                    b.Property<int>("JuristischePersonenMitgliedId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("JuristischePersonId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("KontaktId")
                        .HasColumnType("INTEGER");

                    b.HasKey("JuristischePersonenMitgliedId");

                    b.HasIndex("JuristischePersonId");

                    b.HasIndex("KontaktId");

                    b.ToTable("JuristischePersonenMitglied");
                });

            modelBuilder.Entity("Deeplex.Saverwalter.Model.Kontakt", b =>
                {
                    b.Property<int>("KontaktId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int?>("AdresseId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Anrede")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Email")
                        .HasColumnType("TEXT");

                    b.Property<string>("Fax")
                        .HasColumnType("TEXT");

                    b.Property<string>("Mobil")
                        .HasColumnType("TEXT");

                    b.Property<string>("Nachname")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Notiz")
                        .HasColumnType("TEXT");

                    b.Property<string>("Telefon")
                        .HasColumnType("TEXT");

                    b.Property<string>("Vorname")
                        .HasColumnType("TEXT");

                    b.HasKey("KontaktId");

                    b.HasIndex("AdresseId");

                    b.ToTable("Kontakte");
                });

            modelBuilder.Entity("Deeplex.Saverwalter.Model.Konto", b =>
                {
                    b.Property<int>("KontoId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Bank")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Iban")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Notiz")
                        .HasColumnType("TEXT");

                    b.HasKey("KontoId");

                    b.ToTable("Kontos");
                });

            modelBuilder.Entity("Deeplex.Saverwalter.Model.MietMinderung", b =>
                {
                    b.Property<int>("MietMinderungId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("Beginn")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("Ende")
                        .HasColumnType("TEXT");

                    b.Property<double>("Minderung")
                        .HasColumnType("REAL");

                    b.Property<string>("Notiz")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("VetragId")
                        .HasColumnType("TEXT");

                    b.HasKey("MietMinderungId");

                    b.ToTable("MietMinderungen");
                });

            modelBuilder.Entity("Deeplex.Saverwalter.Model.Miete", b =>
                {
                    b.Property<int>("MieteId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<double?>("Betrag")
                        .HasColumnType("REAL");

                    b.Property<DateTime>("BetreffenderMonat")
                        .HasColumnType("TEXT");

                    b.Property<string>("Notiz")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("VertragId")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("Zahlungsdatum")
                        .HasColumnType("TEXT");

                    b.HasKey("MieteId");

                    b.ToTable("Mieten");
                });

            modelBuilder.Entity("Deeplex.Saverwalter.Model.Mieter", b =>
                {
                    b.Property<int>("MieterId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("KontaktId")
                        .HasColumnType("INTEGER");

                    b.Property<Guid>("VertragId")
                        .HasColumnType("TEXT");

                    b.HasKey("MieterId");

                    b.HasIndex("KontaktId");

                    b.ToTable("MieterSet");
                });

            modelBuilder.Entity("Deeplex.Saverwalter.Model.MietobjektGarage", b =>
                {
                    b.Property<int>("MietobjektGarageId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("GarageId")
                        .HasColumnType("INTEGER");

                    b.Property<Guid>("VertragId")
                        .HasColumnType("TEXT");

                    b.HasKey("MietobjektGarageId");

                    b.HasIndex("GarageId");

                    b.ToTable("MietobjektGaragen");
                });

            modelBuilder.Entity("Deeplex.Saverwalter.Model.Vertrag", b =>
                {
                    b.Property<int>("rowid")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int?>("AnsprechpartnerId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("Beginn")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("Ende")
                        .HasColumnType("TEXT");

                    b.Property<double>("KaltMiete")
                        .HasColumnType("REAL");

                    b.Property<string>("Notiz")
                        .HasColumnType("TEXT");

                    b.Property<int>("Personenzahl")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Version")
                        .HasColumnType("INTEGER");

                    b.Property<string>("VersionsNotiz")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("VertragId")
                        .HasColumnType("TEXT");

                    b.Property<int>("WohnungId")
                        .HasColumnType("INTEGER");

                    b.HasKey("rowid");

                    b.HasAlternateKey("VertragId", "Version");

                    b.HasIndex("AnsprechpartnerId");

                    b.HasIndex("WohnungId");

                    b.ToTable("Vertraege");
                });

            modelBuilder.Entity("Deeplex.Saverwalter.Model.Wohnung", b =>
                {
                    b.Property<int>("WohnungId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("AdresseId")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("BesitzerId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Bezeichnung")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Notiz")
                        .HasColumnType("TEXT");

                    b.Property<int>("Nutzeinheit")
                        .HasColumnType("INTEGER");

                    b.Property<double>("Nutzflaeche")
                        .HasColumnType("REAL");

                    b.Property<double>("Wohnflaeche")
                        .HasColumnType("REAL");

                    b.HasKey("WohnungId");

                    b.HasIndex("AdresseId");

                    b.HasIndex("BesitzerId");

                    b.ToTable("Wohnungen");
                });

            modelBuilder.Entity("Deeplex.Saverwalter.Model.Zaehler", b =>
                {
                    b.Property<int>("ZaehlerId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Kennnummer")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Notiz")
                        .HasColumnType("TEXT");

                    b.Property<int>("Typ")
                        .HasColumnType("INTEGER");

                    b.Property<int>("WohnungId")
                        .HasColumnType("INTEGER");

                    b.HasKey("ZaehlerId");

                    b.HasIndex("WohnungId");

                    b.ToTable("ZaehlerSet");
                });

            modelBuilder.Entity("Deeplex.Saverwalter.Model.Zaehlerstand", b =>
                {
                    b.Property<int>("ZaehlerstandId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("Datum")
                        .HasColumnType("TEXT");

                    b.Property<string>("Notiz")
                        .HasColumnType("TEXT");

                    b.Property<double>("Stand")
                        .HasColumnType("REAL");

                    b.Property<int>("ZaehlerId")
                        .HasColumnType("INTEGER");

                    b.HasKey("ZaehlerstandId");

                    b.HasIndex("ZaehlerId");

                    b.ToTable("Zaehlerstaende");
                });

            modelBuilder.Entity("Deeplex.Saverwalter.Model.Betriebskostenrechnungsgruppe", b =>
                {
                    b.HasOne("Deeplex.Saverwalter.Model.Betriebskostenrechnung", "Rechnung")
                        .WithMany("Gruppen")
                        .HasForeignKey("RechnungBetriebskostenrechnungId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Deeplex.Saverwalter.Model.Wohnung", "Wohnung")
                        .WithMany("Betriebskostenrechnungsgruppen")
                        .HasForeignKey("WohnungId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Deeplex.Saverwalter.Model.Garage", b =>
                {
                    b.HasOne("Deeplex.Saverwalter.Model.Adresse", "Adresse")
                        .WithMany("Garagen")
                        .HasForeignKey("AdresseId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Deeplex.Saverwalter.Model.JuristischePerson", "Besitzer")
                        .WithMany("Garagen")
                        .HasForeignKey("BesitzerJuristischePersonId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Deeplex.Saverwalter.Model.JuristischePerson", b =>
                {
                    b.HasOne("Deeplex.Saverwalter.Model.Adresse", "Adresse")
                        .WithMany()
                        .HasForeignKey("AdresseId");
                });

            modelBuilder.Entity("Deeplex.Saverwalter.Model.JuristischePersonenMitglied", b =>
                {
                    b.HasOne("Deeplex.Saverwalter.Model.JuristischePerson", "JuristischePerson")
                        .WithMany("Mitglieder")
                        .HasForeignKey("JuristischePersonId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Deeplex.Saverwalter.Model.Kontakt", "Kontakt")
                        .WithMany("JuristischePersonen")
                        .HasForeignKey("KontaktId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Deeplex.Saverwalter.Model.Kontakt", b =>
                {
                    b.HasOne("Deeplex.Saverwalter.Model.Adresse", "Adresse")
                        .WithMany("Kontakte")
                        .HasForeignKey("AdresseId");
                });

            modelBuilder.Entity("Deeplex.Saverwalter.Model.Mieter", b =>
                {
                    b.HasOne("Deeplex.Saverwalter.Model.Kontakt", "Kontakt")
                        .WithMany()
                        .HasForeignKey("KontaktId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Deeplex.Saverwalter.Model.MietobjektGarage", b =>
                {
                    b.HasOne("Deeplex.Saverwalter.Model.Garage", "Garage")
                        .WithMany()
                        .HasForeignKey("GarageId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Deeplex.Saverwalter.Model.Vertrag", b =>
                {
                    b.HasOne("Deeplex.Saverwalter.Model.Kontakt", "Ansprechpartner")
                        .WithMany()
                        .HasForeignKey("AnsprechpartnerId");

                    b.HasOne("Deeplex.Saverwalter.Model.Wohnung", "Wohnung")
                        .WithMany("Vertraege")
                        .HasForeignKey("WohnungId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Deeplex.Saverwalter.Model.Wohnung", b =>
                {
                    b.HasOne("Deeplex.Saverwalter.Model.Adresse", "Adresse")
                        .WithMany("Wohnungen")
                        .HasForeignKey("AdresseId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Deeplex.Saverwalter.Model.JuristischePerson", "Besitzer")
                        .WithMany("Wohnungen")
                        .HasForeignKey("BesitzerId");
                });

            modelBuilder.Entity("Deeplex.Saverwalter.Model.Zaehler", b =>
                {
                    b.HasOne("Deeplex.Saverwalter.Model.Wohnung", "Wohnung")
                        .WithMany("Zaehler")
                        .HasForeignKey("WohnungId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Deeplex.Saverwalter.Model.Zaehlerstand", b =>
                {
                    b.HasOne("Deeplex.Saverwalter.Model.Zaehler", "Zaehler")
                        .WithMany("Staende")
                        .HasForeignKey("ZaehlerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
