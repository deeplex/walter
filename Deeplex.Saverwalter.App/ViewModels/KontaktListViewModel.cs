using Deeplex.Saverwalter.Model;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Deeplex.Saverwalter.App.ViewModels
{
    public class KontaktListViewModel
    {
        public List<KontaktListEntry> Kontakte = new List<KontaktListEntry>();
        public ObservableProperty<KontaktListEntry> SelectedKontakt
            = new ObservableProperty<KontaktListEntry>();

        public KontaktListViewModel()
        {
            Kontakte = App.Walter.Kontakte
                .Include(k => k.Adresse)
                .Select(k => new KontaktListEntry(k)).ToList();

            var jp = App.Walter.JuristischePersonen;
            foreach (var j in jp)
            {
                Kontakte.Add(new KontaktListEntry(j));
            }
        }
    }

    public class KontaktListEntry
    {
        public Type Type { get; }
        public int Id { get; }
        public string Vorname { get; }
        public string Name { get; }
        public string Anschrift { get; }
        public string Email { get; }
        public string Telefon { get; }
        public string Mobil { get; }

        public KontaktListEntry(JuristischePerson j)
        {
            Type = j.GetType();
            Id = j.JuristischePersonId;
            Vorname = "";
            Name = j.Bezeichnung;
            Email = j.Email ?? "";
            Telefon = j.Telefon ?? "";
            Mobil = j.Mobil ?? "";
            Anschrift = j.Adresse is Adresse a ?
                a.Strasse + " " + a.Hausnummer + ", " +
                a.Postleitzahl + " " + a.Stadt : "";
        }

        public KontaktListEntry(Kontakt k)
        {
            Type = k.GetType();
            Id = k.KontaktId;
            Vorname = k.Vorname ?? "";
            Name = k.Nachname ?? "";
            Email = k.Email ?? "";
            Telefon = k.Telefon ?? "";
            Mobil = k.Mobil ?? "";
            Anschrift = k.Adresse is Adresse a ?
                a.Strasse + " " + a.Hausnummer + ", " +
                a.Postleitzahl + " " + a.Stadt : "";
        }
    }
}
