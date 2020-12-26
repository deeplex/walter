using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Deeplex.Saverwalter.App.ViewModels
{
    public sealed class KontaktListViewModel : BindableBase
    {
        public List<KontaktListEntry> Kontakte = new List<KontaktListEntry>();
        private KontaktListEntry mSelectedKontakt;
        public KontaktListEntry SelectedKontakt
        {
            get => mSelectedKontakt;
            set
            {
                mSelectedKontakt = value;
                RaisePropertyChangedAuto();
                RaisePropertyChanged(nameof(hasSelectedKontakt));
            }
        }
        public bool hasSelectedKontakt => SelectedKontakt != null;

        public KontaktListViewModel()
        {
            Kontakte = App.Walter.NatuerlichePersonen
                .Include(k => k.Adresse)
                .Select(k => new KontaktListEntry(k)).ToList();

            var jp = App.Walter.JuristischePersonen;
            foreach (var j in jp)
            {
                Kontakte.Add(new KontaktListEntry(j));
            }
        }
    }

    public sealed class KontaktListEntry
    {
        public Type Type { get; }
        public int Id { get; }
        public Guid Guid { get; }
        public string Vorname { get; }
        public string Name { get; }
        public string Anschrift { get; }
        public string Email { get; }
        public string Telefon { get; }
        public string Mobil { get; }

        public KontaktListEntry(JuristischePerson j) : this(j as IPerson)
        {
            Type = j.GetType();
            Id = j.JuristischePersonId;
            Vorname = "";
            Name = j.Bezeichnung;
        }

        public KontaktListEntry(NatuerlichePerson k) : this(k as IPerson)
        {
            Type = k.GetType();
            Id = k.NatuerlichePersonId;
            Vorname = k.Vorname ?? "";
            Name = k.Nachname;
        }

        private KontaktListEntry(IPerson p)
        {
            Guid = p.PersonId;
            Email = p.Email ?? "";
            Telefon = p.Telefon ?? "";
            Mobil = p.Mobil ?? "";
            Anschrift = p.Adresse is Adresse a ?
                a.Strasse + " " + a.Hausnummer + ", " +
                a.Postleitzahl + " " + a.Stadt : "";
        }
    }
}
