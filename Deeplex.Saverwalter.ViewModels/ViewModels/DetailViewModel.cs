using Deeplex.Saverwalter.Services;
using Deeplex.Utils.ObjectModel;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Deeplex.Saverwalter.ViewModels
{
    public interface IDetailViewModel
    {
        string ToString();
        void checkForChanges();
        RelayCommand Save { get; }
        AsyncRelayCommand Delete { get; }
        IWalterDbService WalterDbService { get; }
        INotificationService NotificationService { get; }
    }

    public abstract class DetailViewModel<T> : BindableBase, IDetailViewModel where T : class
    {
        public DetailViewModel(INotificationService ns, IWalterDbService db)
        {
            WalterDbService = db;
            NotificationService = ns;

            Delete = new(async _ => { await delete(); }, _ => true);
        }

        protected async Task<bool> delete()
        {
            if (await NotificationService.Confirmation())
            {
                WalterDbService.ctx.Remove(Entity);
                WalterDbService.SaveWalter();
                return true;
            }
            return false;
        }

        // Could be protected, but Vertrag needs public for its Versionen. Maybe something can be done about that.
        public T Entity { get; protected set; }
        public abstract void SetEntity(T e);
        public abstract override string ToString();
        public RelayCommand Save { get; protected set; }
        public AsyncRelayCommand Delete { get; set; }
        public IWalterDbService WalterDbService { get; protected set; }
        public INotificationService NotificationService { get; protected set; }
        public abstract void checkForChanges();

        protected void save()
        {
            if (isInitialized)
            {
                WalterDbService.ctx.Update(Entity);
            }
            else
            {
                WalterDbService.ctx.Add(Entity);
            }

            WalterDbService.SaveWalter();
            RaisePropertyChanged(nameof(isInitialized));
            checkForChanges();
        }
        public bool isInitialized
        {
            get
            {
                if (Entity != null && WalterDbService.ctx.Entry(Entity) is EntityEntry entry)
                {
                    return entry.Metadata
                        .FindPrimaryKey()
                        .Properties
                        .Count(e =>
                        {
                            var currentValue = entry.Property(e.Name).CurrentValue;
                            if (currentValue is int i)
                            {
                                return i != default(int);
                            }
                            else if (currentValue is Guid g)
                            {
                                return g != default(Guid);
                            }
                            else
                            {
                                return false;
                            }
                        }) > 0;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}
