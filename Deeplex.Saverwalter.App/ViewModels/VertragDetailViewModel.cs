using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Toolkit.Uwp.UI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Deeplex.Saverwalter.App.ViewModels
{
    public class VertragDetailViewModel : VertragDetailVersion
    {
        public ObservableProperty<List<VertragDetailKontakt>> Kontakte
            = new ObservableProperty<List<VertragDetailKontakt>>();

        public ObservableProperty<List<VertragVersionListViewModel>> Versionen { get; }
            = new ObservableProperty<List<VertragVersionListViewModel>>();

        public VertragDetailViewModel(Guid id)
            : this(App.Walter.Vertraege
                  .Include(v => v.Wohnung).ThenInclude(w => w.Besitzer)
                  .Include(v => v.Garagen)
                  .Where(v => v.VertragId == id).ToList()) { }

        public VertragDetailViewModel(List<Vertrag> v)
            : base(v.OrderBy(vs => vs.Version).Last())
        {
            Versionen.Value = v.OrderBy(vs => vs.Version).Select(vs => new VertragVersionListViewModel(vs)).ToList();
            Beginn.Value = Versionen.Value.First().Beginn.Value;

            Kontakte.Value = App.Walter.Kontakte
                .Select(k => new VertragDetailKontakt(k))
                .ToList();
        }
    }

    public class VertragDetailKontakt
    {
        public int Id;
        public ObservableProperty<string> Name = new ObservableProperty<string>();

        public VertragDetailKontakt(Kontakt k)
        {
            Id = k.KontaktId;
            Name.Value = k.Vorname + " " + k.Nachname;
        }
    }

    public class VertragDetailVersion
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

        public ObservableProperty<List<VertragDetailMieter>> Mieter
            = new ObservableProperty<List<VertragDetailMieter>>();

        public string BeginnString => Beginn.Value.ToShortDateString();
        public string EndeString => Ende.Value is DateTime e ? e.ToShortDateString() : "";
        public bool HasEnde => Ende.Value is DateTime;

        public VertragDetailVersion(Vertrag v)
        {
            Id = v.rowid;
            Version = v.Version;
            Personenzahl.Value = v.Personenzahl;
            Wohnung.Value = v.Wohnung is Wohnung w? new VertragDetailWohnung(w) : null;
            Mieter.Value = v.Mieter.Select(m => new VertragDetailMieter(m.Kontakt)).ToList();
            Vermieter.Value = new VertragDetailVermieter(
                v.Wohnung is Wohnung ? v.Wohnung.Besitzer : v.Garagen.First().Garage.Besitzer);

            Ende.Value = v.Ende;
            Beginn.Value = v.Beginn;
        }
    }

    public class VertragDetailMieter
    {
        public int Id;
        public ObservableProperty<Anrede> Anrede = new ObservableProperty<Anrede>();
        public ObservableProperty<string> Vorname = new ObservableProperty<string>();
        public ObservableProperty<string> Nachname = new ObservableProperty<string>();

        public VertragDetailMieter(Kontakt k)
        {
            Id = k.KontaktId;
            Anrede.Value = k.Anrede;
            Vorname.Value = k.Vorname;
            Nachname.Value = k.Nachname;
        }
    }

    public class VertragDetailVermieter
    {
        public int Id;
        public ObservableProperty<string> Bezeichnung = new ObservableProperty<string>();

        public VertragDetailVermieter(JuristischePerson j)
        {
            Id = j.JuristischePersonId;
            Bezeichnung.Value = j.Bezeichnung;
        }
    }

    public class VertragDetailWohnung
    {
        public int Id;
        public ObservableProperty<string> Bezeichnung = new ObservableProperty<string>();
        public ObservableProperty<string> Anschrift = new ObservableProperty<string>();

        public VertragDetailWohnung(Wohnung w)
        {
            Id = w.WohnungId;
            Bezeichnung.Value = w.Bezeichnung;
            Anschrift.Value = Utils.Anschrift(w);
        }
    }
}

