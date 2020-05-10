﻿// <auto-generated />
using System;
using Deeplex.Saverwalter.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Deeplex.Saverwalter.Model.Migrations
{
    [DbContext(typeof(SaverwalterContext))]
    [Migration("20200510154255_InitialCreate")]
    partial class InitialCreate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
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

            modelBuilder.Entity("Deeplex.Saverwalter.Model.Allgemeinzaehler", b =>
                {
                    b.Property<int>("AllgemeinzaehlerId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Beschreibung")
                        .HasColumnType("TEXT");

                    b.HasKey("AllgemeinzaehlerId");

                    b.ToTable("Allgemeinzaehler");
                });

            modelBuilder.Entity("Deeplex.Saverwalter.Model.Garage", b =>
                {
                    b.Property<int>("GarageId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("BesitzerJuristischePersonId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Kennung")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("GarageId");

                    b.HasIndex("BesitzerJuristischePersonId");

                    b.ToTable("Garagen");
                });

            modelBuilder.Entity("Deeplex.Saverwalter.Model.JuristischePerson", b =>
                {
                    b.Property<int>("JuristischePersonId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Bezeichnung")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("JuristischePersonId");

                    b.ToTable("JuristischePersonen");
                });

            modelBuilder.Entity("Deeplex.Saverwalter.Model.KalteBetriebskostenRechnung", b =>
                {
                    b.Property<int>("KalteBetriebskostenRechnungId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<double>("Betrag")
                        .HasColumnType("REAL");

                    b.Property<int>("Jahr")
                        .HasColumnType("INTEGER");

                    b.Property<int>("KalteBetriebskostenpunktId")
                        .HasColumnType("INTEGER");

                    b.HasKey("KalteBetriebskostenRechnungId");

                    b.HasIndex("KalteBetriebskostenpunktId");

                    b.ToTable("KalteBetriebskostenRechnungen");
                });

            modelBuilder.Entity("Deeplex.Saverwalter.Model.KalteBetriebskostenpunkt", b =>
                {
                    b.Property<int>("KalteBetriebskostenpunktId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("AdresseId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Beschreibung")
                        .HasColumnType("TEXT");

                    b.Property<int>("Bezeichnung")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Schluessel")
                        .HasColumnType("INTEGER");

                    b.HasKey("KalteBetriebskostenpunktId");

                    b.HasIndex("AdresseId");

                    b.ToTable("KalteBetriebskosten");
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

                    b.HasKey("KontoId");

                    b.ToTable("Kontos");
                });

            modelBuilder.Entity("Deeplex.Saverwalter.Model.Mieter", b =>
                {
                    b.Property<int>("MieterId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("KontaktId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("VertragId")
                        .HasColumnType("INTEGER");

                    b.HasKey("MieterId");

                    b.HasIndex("KontaktId");

                    b.HasIndex("VertragId");

                    b.ToTable("MieterSet");
                });

            modelBuilder.Entity("Deeplex.Saverwalter.Model.MietobjektGarage", b =>
                {
                    b.Property<int>("MietobjektGarageId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("GarageId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("VertragId")
                        .HasColumnType("INTEGER");

                    b.HasKey("MietobjektGarageId");

                    b.HasIndex("GarageId");

                    b.HasIndex("VertragId");

                    b.ToTable("MietobjektGaragen");
                });

            modelBuilder.Entity("Deeplex.Saverwalter.Model.Vertrag", b =>
                {
                    b.Property<int>("rowid")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("AnsprechpartnerKontaktId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("Beginn")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("Ende")
                        .HasColumnType("TEXT");

                    b.Property<int>("Personenzahl")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Version")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasDefaultValue(0);

                    b.Property<Guid>("VertragId")
                        .HasColumnType("TEXT");

                    b.Property<int?>("WohnungId")
                        .HasColumnType("INTEGER");

                    b.HasKey("rowid");

                    b.HasAlternateKey("VertragId", "Version");

                    b.HasIndex("AnsprechpartnerKontaktId");

                    b.HasIndex("WohnungId");

                    b.ToTable("Vertraege");
                });

            modelBuilder.Entity("Deeplex.Saverwalter.Model.WarmeBetriebskostenRechnung", b =>
                {
                    b.Property<int>("WarmeBetriebskostenRechnungId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("AllgemeinzaehlerId")
                        .HasColumnType("INTEGER");

                    b.Property<double>("Betrag")
                        .HasColumnType("REAL");

                    b.Property<int>("Jahr")
                        .HasColumnType("INTEGER");

                    b.HasKey("WarmeBetriebskostenRechnungId");

                    b.HasIndex("AllgemeinzaehlerId");

                    b.ToTable("WarmeBetriebskostenRechnungen");
                });

            modelBuilder.Entity("Deeplex.Saverwalter.Model.Wohnung", b =>
                {
                    b.Property<int>("WohnungId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("AdresseId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("BesitzerJuristischePersonId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Bezeichnung")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<double>("Nutzflaeche")
                        .HasColumnType("REAL");

                    b.Property<double>("Wohnflaeche")
                        .HasColumnType("REAL");

                    b.HasKey("WohnungId");

                    b.HasIndex("AdresseId");

                    b.HasIndex("BesitzerJuristischePersonId");

                    b.ToTable("Wohnungen");
                });

            modelBuilder.Entity("Deeplex.Saverwalter.Model.Zaehler", b =>
                {
                    b.Property<int>("ZaehlerId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("Typ")
                        .HasColumnType("INTEGER");

                    b.Property<int>("WohnungId")
                        .HasColumnType("INTEGER");

                    b.HasKey("ZaehlerId");

                    b.HasIndex("WohnungId");

                    b.ToTable("ZaehlerSet");
                });

            modelBuilder.Entity("Deeplex.Saverwalter.Model.Zaehlergemeinschaft", b =>
                {
                    b.Property<int>("ZaehlergemeinschaftId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("AllgemeinzaehlerId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Typ")
                        .HasColumnType("INTEGER");

                    b.Property<int>("WohnungId")
                        .HasColumnType("INTEGER");

                    b.HasKey("ZaehlergemeinschaftId");

                    b.HasIndex("AllgemeinzaehlerId");

                    b.HasIndex("WohnungId");

                    b.ToTable("Zaehlergemeinschaften");
                });

            modelBuilder.Entity("Deeplex.Saverwalter.Model.Zaehlerstand", b =>
                {
                    b.Property<int>("ZaehlerstandId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("Datum")
                        .HasColumnType("TEXT");

                    b.Property<double>("Stand")
                        .HasColumnType("REAL");

                    b.Property<int>("ZaehlerId")
                        .HasColumnType("INTEGER");

                    b.HasKey("ZaehlerstandId");

                    b.HasIndex("ZaehlerId");

                    b.ToTable("Zaehlerstaende");
                });

            modelBuilder.Entity("Deeplex.Saverwalter.Model.Garage", b =>
                {
                    b.HasOne("Deeplex.Saverwalter.Model.JuristischePerson", "Besitzer")
                        .WithMany("Garagen")
                        .HasForeignKey("BesitzerJuristischePersonId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Deeplex.Saverwalter.Model.KalteBetriebskostenRechnung", b =>
                {
                    b.HasOne("Deeplex.Saverwalter.Model.KalteBetriebskostenpunkt", "KalteBetriebskostenpunkt")
                        .WithMany("Rechnungen")
                        .HasForeignKey("KalteBetriebskostenpunktId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Deeplex.Saverwalter.Model.KalteBetriebskostenpunkt", b =>
                {
                    b.HasOne("Deeplex.Saverwalter.Model.Adresse", "Adresse")
                        .WithMany("KalteBetriebskosten")
                        .HasForeignKey("AdresseId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Deeplex.Saverwalter.Model.Kontakt", b =>
                {
                    b.HasOne("Deeplex.Saverwalter.Model.Adresse", "Adresse")
                        .WithMany()
                        .HasForeignKey("AdresseId");
                });

            modelBuilder.Entity("Deeplex.Saverwalter.Model.Mieter", b =>
                {
                    b.HasOne("Deeplex.Saverwalter.Model.Kontakt", "Kontakt")
                        .WithMany()
                        .HasForeignKey("KontaktId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Deeplex.Saverwalter.Model.Vertrag", "Vertrag")
                        .WithMany("Mieter")
                        .HasForeignKey("VertragId")
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

                    b.HasOne("Deeplex.Saverwalter.Model.Vertrag", "Vertrag")
                        .WithMany("Garagen")
                        .HasForeignKey("VertragId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Deeplex.Saverwalter.Model.Vertrag", b =>
                {
                    b.HasOne("Deeplex.Saverwalter.Model.Kontakt", "Ansprechpartner")
                        .WithMany()
                        .HasForeignKey("AnsprechpartnerKontaktId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Deeplex.Saverwalter.Model.Wohnung", "Wohnung")
                        .WithMany("Vertraege")
                        .HasForeignKey("WohnungId");
                });

            modelBuilder.Entity("Deeplex.Saverwalter.Model.WarmeBetriebskostenRechnung", b =>
                {
                    b.HasOne("Deeplex.Saverwalter.Model.Allgemeinzaehler", "Allgemeinzaehler")
                        .WithMany("Rechnungen")
                        .HasForeignKey("AllgemeinzaehlerId")
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
                        .HasForeignKey("BesitzerJuristischePersonId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Deeplex.Saverwalter.Model.Zaehler", b =>
                {
                    b.HasOne("Deeplex.Saverwalter.Model.Wohnung", "Wohnung")
                        .WithMany("Zaehler")
                        .HasForeignKey("WohnungId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Deeplex.Saverwalter.Model.Zaehlergemeinschaft", b =>
                {
                    b.HasOne("Deeplex.Saverwalter.Model.Allgemeinzaehler", "Allgemeinzaehler")
                        .WithMany("Zaehlergemeinschaften")
                        .HasForeignKey("AllgemeinzaehlerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Deeplex.Saverwalter.Model.Wohnung", "Wohnung")
                        .WithMany("Zaehlergemeinschaften")
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