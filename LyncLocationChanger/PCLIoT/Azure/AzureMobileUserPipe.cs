using Microsoft.WindowsAzure.MobileServices;
using PCLIoT.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PCLIoT.Azure
{
    public class AzureMobileUserPipe<T>
        : AzureMobilePipe<T>
        where T : UserTable
    {
        protected string username { get; set; }
        public AzureMobileUserPipe(string username) 
        {
            this.username = username;
            refresh();
        }

        public async Task clearUserTable()
        {
            var listforuser = await refreshUserTable();
            if(listforuser != null && listforuser.Count() > 0)
            await clearTable(listforuser);
        }

        public async Task<List<T>> refreshUserTable()
        {
            List<T> filteredList = null;
            try
            {
                // This code refreshes the entries in the list view by querying the TodoItems table.
                // The query excludes completed TodoItems
                await refresh();
                filteredList = serviceCollection.Where(a => true).ToList(); // a.username == username0
            }
            catch (Exception)
            {
                return filteredList;
            }
            return filteredList;
        }

    }
}
