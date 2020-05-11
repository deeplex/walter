using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Deeplex.Saverwalter.App.ViewModels
{
    public class VertragDetailViewModel : VertragDetailVersion
    {
        public ObservableProperty<ImmutableList<VertragDetailMieter>> Kontakte
            = new ObservableProperty<ImmutableList<VertragDetailMieter>>();

        public ObservableProperty<ImmutableList<VertragDetailVermieter>> JuristischePersonen
            = new ObservableProperty<ImmutableList<VertragDetailVermieter>>();

        public ObservableProperty<ImmutableList<VertragDetailWohnung>> AlleWohnungen
           = new ObservableProperty<ImmutableList<VertragDetailWohnung>>();

        public ObservableProperty<List<VertragVersionListViewModel>> Versionen { get; }
            = new ObservableProperty<List<VertragVersionListViewModel>>();

        public ObservableProperty<ImmutableList<VertragDetailKalteBetriebskosten>> KalteBetriebskosten { get; }
            = new ObservableProperty<ImmutableList<VertragDetailKalteBetriebskosten>>();

        public VertragDetailViewModel(Guid id)
            : this(App.Walter.Vertraege
                  .Include(v => v.Wohnung).ThenInclude(w => w.Besitzer)
                  .Include(v => v.Garagen)
                  .Where(v => v.VertragId == id).ToList())
        { }

        public VertragDetailViewModel(List<Vertrag> v)
            : base(v.OrderBy(vs => vs.Version).Last())
        {
            Versionen.Value = v.OrderBy(vs => vs.Version).Select(vs => new VertragVersionListViewModel(vs)).ToList();
            Beginn.Value = Versionen.Value.First().Beginn.Value;

            Kontakte.Value = App.Walter.Kontakte
                .Select(k => new VertragDetailMieter(k))
                .ToImmutableList();

            JuristischePersonen.Value = App.Walter.JuristischePersonen
                .Select(j => new VertragDetailVermieter(j))
                .ToImmutableList();

            AlleWohnungen.Value = App.Walter.Wohnungen
                .Select(w => new VertragDetailWohnung(w))
                .ToImmutableList();

            KalteBetriebskosten.Value = App.Walter.KalteBetriebskosten
                .Select(k => new VertragDetailKalteBetriebskosten(k, v.Last()))
                .ToImmutableList();
        }
    }

    public class VertragDetailVersion : BindableBase
    {
        public int Id { get; }
        public int Version { get; }
        public ObservableProperty<int> Personenzahl { get; } = new ObservableProperty<int>();
        public ObservableProperty<VertragDetailWohnung> Wohnung { get; }
            = new ObservableProperty<VertragDetailWohnung>();
        public ObservableProperty<DateTime> Beginn { get; } = new ObservableProperty<DateTime>();
        public ObservableProperty<DateTime?> Ende { get; } = new ObservableProperty<DateTime?>();
        public ObservableProperty<VertragDetailVermieter> Vermieter
            = new ObservableProperty<VertragDetailVermieter>();
        public ObservableProperty<ImmutableList<VertragDetailMieter>> Mieter
            = new ObservableProperty<ImmutableList<VertragDetailMieter>>();

        public string BeginnString => Beginn.Value.ToShortDateString();
        public string EndeString => Ende.Value is DateTime e ? e.ToShortDateString() : "";
        public bool HasEnde => Ende.Value is DateTime;

        public VertragDetailVersion(Vertrag v)
        {
            Id = v.rowid;
            Version = v.Version;
            Personenzahl.Value = v.Personenzahl;
            Wohnung.Value = v.Wohnung is Wohnung w ? new VertragDetailWohnung(w) : null;
            Mieter.Value = v.Mieter.Select(m => new VertragDetailMieter(m.Kontakt))
                .OrderBy(m => m.Name.Value.Length).Reverse() // From the longest to the smallest because of XAML I guess
                .ToImmutableList();
            Vermieter.Value = new VertragDetailVermieter(
                v.Wohnung is Wohnung ? v.Wohnung.Besitzer : v.Garagen.First().Garage.Besitzer);

            Ende.Value = v.Ende;
            Beginn.Value = v.Beginn;
        }
    }

    public class VertragDetailMieter
    {
        public int Id;
        public ObservableProperty<string> Name = new ObservableProperty<string>();

        public VertragDetailMieter(int id) : this(App.Walter.Kontakte.Find(id)) { }
        public VertragDetailMieter(Kontakt k)
        {
            Id = k.KontaktId;
            Name.Value = k.Vorname + " " + k.Nachname;
        }
    }

    public class VertragDetailVermieter
    {
        public int Id;
        public ObservableProperty<string> Name = new ObservableProperty<string>();

        public VertragDetailVermieter(JuristischePerson j)
        {
            Id = j.JuristischePersonId;
            Name.Value = j.Bezeichnung;
        }
    }

    public class VertragDetailWohnung
    {
        public int Id;
        public ObservableProperty<int> BesitzerId = new ObservableProperty<int>();
        public ObservableProperty<string> BezeichnungVoll = new ObservableProperty<string>();

        public VertragDetailWohnung(Wohnung w)
        {
            Id = w.WohnungId;
            BesitzerId.Value = w.Besitzer.JuristischePersonId;
            BezeichnungVoll.Value = Utils.Anschrift(w) + " - " + w.Bezeichnung;
        }
    }

    public class VertragDetailKalteBetriebskosten
    {
        public int Id { get; }
        public ObservableProperty<string> Bezeichnung { get; } = new ObservableProperty<string>();
        public ObservableProperty<string> Schluessel { get; } = new ObservableProperty<string>();
        public ObservableProperty<string> Anteil { get; } = new ObservableProperty<string>();
        public ObservableProperty<string> Beschreibung { get; } = new ObservableProperty<string>();

        public VertragDetailKalteBetriebskosten(KalteBetriebskostenpunkt k, Vertrag v)
        {
            string Percent(double d) => string.Format("{0:N2}%", d * 100);

            var w = v.Wohnung;
            var p = App.Walter.Vertraege
                .Where(a => a.Wohnung.AdresseId == w.AdresseId)
                .Where(a => a.Ende == null || a.Ende < DateTime.Today)
                .Sum(a => a.Personenzahl);

            Id = k.KalteBetriebskostenpunktId;
            Bezeichnung.Value = k.Bezeichnung.ToDescriptionString();
            Schluessel.Value = k.Schluessel.ToDescriptionString();
            switch (k.Schluessel)
            {
                case UmlageSchluessel.NachWohnflaeche:
                    Anteil.Value = Percent(w.Wohnflaeche / w.Adresse.Wohnungen.Sum(a => a.Wohnflaeche));
                    break;
                case UmlageSchluessel.NachNutzeinheit:
                    Anteil.Value = Percent(1.0 / w.Adresse.Wohnungen.Count());
                    break;
                case UmlageSchluessel.NachPersonenzahl:
                    Anteil.Value = Percent((double)v.Personenzahl / p);
                    break;
                case UmlageSchluessel.NachVerbrauch:
                    Anteil.Value = "n/a";
                    break;
            }
            Beschreibung.Value = k.Beschreibung;
        }
    }
}

