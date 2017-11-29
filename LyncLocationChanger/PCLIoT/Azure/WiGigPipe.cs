using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCLIoT.Azure
{
    public class WiGigPipe : AzureMobileUserPipe<WiGigDevice>
    {
        public WiGigPipe(string username) : base(username)
        {

        }
        protected string detectConnection(string connectedMAC, MobileServiceCollection<WiGigDevice, WiGigDevice> devices)
        {
            if (devices == null) return "";
            foreach (var device in devices)
            {
                if (device.connect && device.primaryMacAddress != connectedMAC)
                {
                    return device.primaryMacAddress;
                }
            }
            return "";
        }

        protected string detectDisconnect(string connectedMAC, MobileServiceCollection<WiGigDevice, WiGigDevice> devices)
        {
            if (devices == null) return "";
            foreach (var device in devices)
            {
                if (device.primaryMacAddress == connectedMAC && !device.connect)
                {
                    return device.primaryMacAddress;
                }
            }
            return "";
        }

        public async Task ConnectToDock(string MAC)
        {
            var devices = await refreshUserTable();
            await clearUserTable();
            var target = devices.First();
            target.connect = true;
            await insert(target);
        }

        public async Task DisconnectFromDock(string MAC)
        {
            var devices = await refreshUserTable();
            await clearUserTable();
            var target = devices.First();
            target.connect = false;
            await insert(target);
        }

        public async Task ConnectToDockSmart(string MAC)
        {
            var target = serviceCollection.First();
            if (target == null) return;
            target.connect = true;            
            await base.UpdateTable2(target);
            await refreshUserTable();
        }

        public async Task DisconnectFromDockSmart(string MAC)
        {
            var items = await refresh();
            if (items == null) return;
            var target = items.First();            
            target.connect = false;
            await base.UpdateTable2(target);
        }

        public async Task UpdateTable(List<WiGigDevice> devices)
        {

  //          this.devices_local = devices;
            //MobileServiceCollection<WiGigDevice, WiGigDevice> items = null;
            //string connectedMAC = "";
       //     this.serviceCollection = await refreshUserTable(username);
            // all clear

            if(this.serviceCollection != null)
            {
                // delete disappeared docks
                foreach (var item in this.serviceCollection)
                {
                    var matched = devices.Find(s => s.primaryMacAddress == item.primaryMacAddress);
                    if (matched == null)
                    {
                        await remove(item);
                    }
                }
            }
            else
            {
                foreach (var device in devices)
                {
                    await insert(device);
                }
            }
            foreach (var device in devices)
            {
                var matched = from item in this.serviceCollection
                              where item.primaryMacAddress == device.primaryMacAddress
                              select item;
                if (matched == null || matched.Count() == 0)
                {
                    await insert(device);
                }
            }

            var dups = new List<WiGigDevice>();
            foreach (var device in devices)
            {
                var duplicated = from dup in dups
                                 where dup.primaryMacAddress == device.primaryMacAddress
                                 select dup;
                if (duplicated != null)
                {
                    await remove(device);
                }
            }

            //// delete all and reserve connected flag
            //foreach (var item in items)
            //{
            //    if (item.connect) connectedMAC = item.primaryMacAddress;
            //    await wigigTable.DeleteAsync(item);
            //    items.Remove(item);
            //}

            ////// copy device list
            //foreach (var device in devices)
            //{
            //    if (device.primaryMacAddress == connectedMAC) device.connect = true;
            //    await wigigTable.InsertAsync(device);
            //    items.Add(device);
            //}
            return;
        }

    }
}
