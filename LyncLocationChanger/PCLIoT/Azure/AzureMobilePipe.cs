using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCLIoT.Azure
{
    public class AzureMobilePipe<T> : BindableBase
    {

        MobileServiceCollection<T, T> _serviceCollection;
        public MobileServiceCollection<T,T> serviceCollection
        {
            get { return _serviceCollection; }
            set { SetProperty(ref _serviceCollection, value); }
        }
        static IMobileServiceTable<T> table = AzureCore.MobileService.GetTable<T>();
        protected async Task<MobileServiceCollection<T, T>> refresh()
        {
            //var operation = PushNotificationChannelManager.CreatePushNotificationChannelForApplicationAsync();
            //operation.Completed += OnChannelCreationCompleted;
            try
            {
                // This code refreshes the entries in the list view by querying the TodoItems table.
                // The query excludes completed TodoItems
                serviceCollection = await table
                    .Where(a => true) // a.deviceType.Equals("Dock"))
                    .ToCollectionAsync();
            }
            catch (Exception)
            {
                return null;
            }
            return serviceCollection;
        }
        public async Task clearTable()
        {
            await clearTable(await table.ToListAsync());
        }
        protected async Task clearTable(List<T> list)
        {
            foreach (var row in list)
            {
                await remove(row);
            }
        }
        public async Task remove(T row)
        {
            await table.DeleteAsync(row);
            this.serviceCollection.Remove(row);
        }


        public async Task insert(T row)
        {
            // This code inserts a new TodoItem into the database. When the operation completes
            // and Mobile Services has assigned an Id, the item is added to the CollectionView
            //await refresh();
            await table.InsertAsync(row);
            serviceCollection.Add(row);
            //await SyncAsync(); // offline sync
        }
        protected async Task UpdateTable2(T row)
        {
            // Cannot work under this version (Azure MobilseServices UWP)


            // This code takes a freshly completed TodoItem and updates the database. When the MobileService 
            // responds, the item is removed from the list 
            await table.UpdateAsync(row);
            serviceCollection.Remove(row);

            //await SyncAsync(); // offline sync
        }

    }
}
