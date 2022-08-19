using Deeplex.Saverwalter.Model;
using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using System;

namespace Deeplex.Saverwalter.ViewModels
{
    public class VertragDetailViewModelVersion : DetailViewModel<VertragVersion>, IDetailViewModel
    {
        public override string ToString() => "Vertragsversion"; // TODO

        public int Id => Entity.VertragVersionId;
        public SavableProperty<double> Grundmiete;
        public SavableProperty<int> Personenzahl;
        public SavableProperty<WohnungListViewModelEntry> Wohnung { get; protected set; }
        public SavableProperty<DateTimeOffset> Beginn;
        public SavableProperty<DateTimeOffset?> Ende;
        public SavableProperty<string> Notiz;
        public SavableProperty<KontaktListViewModelEntry> Ansprechpartner { get; protected set; }

        public RelayCommand RemoveDate;

        public VertragDetailViewModelVersion(INotificationService ns, IWalterDbService db): base(ns, db)
        {
            RemoveDate = new RelayCommand(_ => Ende = null, _ => Ende != null);
        }

        public override void SetEntity(VertragVersion v)
        {
            Entity = v;

            Grundmiete = new(this, v.Grundmiete);
            Personenzahl = new(this, v.Personenzahl);
            Beginn = new(this, v.Beginn);
            Notiz = new(this, v.Notiz);
        }

        public override void checkForChanges()
        {
            NotificationService.outOfSync =
                Personenzahl.Value != Entity.Personenzahl ||
                Grundmiete.Value != Entity.Grundmiete ||
                Beginn.Value != Entity.Beginn ||
                Notiz.Value != Entity.Notiz;
        }

        public void versionSave()
        {
            Entity.Beginn = Beginn.Value.DateTime;
            Entity.Notiz = Notiz.Value;
            Entity.Personenzahl = Personenzahl.Value;
            Entity.Grundmiete = Grundmiete.Value;

            save();
        }
    }

}
