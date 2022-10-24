using Deeplex.Saverwalter.Model;
using System;
using System.Linq;

namespace Deeplex.Saverwalter.ViewModels
{
    public class ZaehlerListViewModelEntry
    {
        public override string ToString()
        {
            return Kennnummer + ", " + Ort;
        }

        public Zaehler Entity;
        private Zaehlerstand LastStand;
        public int Id => Entity.ZaehlerId;
        public string Kennnummer => Entity.Kennnummer;
        public string TypString => Entity.Typ.ToString();
        // TODO remove i18n from viewmodels
        public string LastStandString => LastStand == null ? "Keine Angabe" : LastStand.Stand.ToString();
        public DateTime? Datum => LastStand == null ? null : LastStand.Datum;
        public int WohnungId => Entity.Wohnung?.WohnungId ?? 0;
        // TODO remove i18n from viewmodels
        public string Ort => Entity.Wohnung == null ? "Unbestimmter Ort" :
            AdresseViewModel.Anschrift(Entity.Adresse) + ", " + Entity.Wohnung.Bezeichnung;
        public Zaehler AllgemeinZaehler => Entity.Allgemeinzaehler;

        public ZaehlerListViewModelEntry(Zaehler z)
        {
            // TODO Handle Entity.Wohnung.Adresse != Entity.Adresse

            Entity = z;
            LastStand = z.Staende.OrderBy(e => e.Datum).LastOrDefault();
        }
    }
}