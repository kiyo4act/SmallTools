using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.Threading.Tasks;
using UtilitiesLib;

namespace AutoHostsUpdateService
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();
        }

        private void serviceProcessInstaller_AfterInstall(object sender, InstallEventArgs e)
        {

        }

        private void serviceInstaller_AfterInstall(object sender, InstallEventArgs e)
        {
            // Set default Registry value (InstallUtill)

            // ExternalHostsTarget
            RegistryUtility.FRegistrySetValue(Microsoft.Win32.RegistryHive.LocalMachine, LocalResources.MGstrAutoHostsUpdateServiceRegistryPath, LocalResources.MGstrExternalHostsTargetRegistryName, LocalResources.MGstrExternalHostsTargetRegistryDefaultValue);
            // InternalHostsTarget
            RegistryUtility.FRegistrySetValue(Microsoft.Win32.RegistryHive.LocalMachine, LocalResources.MGstrAutoHostsUpdateServiceRegistryPath, LocalResources.MGstrInternalHostsTargetRegistryName, LocalResources.MGstrInternalHostsTargetRegistryDefaultValue);
            // ApplyInExternalHosts (Enabled)
            RegistryUtility.FRegistrySetDwordValue(Microsoft.Win32.RegistryHive.LocalMachine, LocalResources.MGstrAutoHostsUpdateServiceRegistryPath, LocalResources.MGstrIsApplyInExternalHostsRegistryName, LocalResources.MGintDefaultEnabledValue);
            // UpdateInternalHosts (Enabled)
            RegistryUtility.FRegistrySetDwordValue(Microsoft.Win32.RegistryHive.LocalMachine, LocalResources.MGstrAutoHostsUpdateServiceRegistryPath, LocalResources.MGstrIsUpdateInternalHostsRegistryName, LocalResources.MGintDefaultEnabledValue);
            // NetworkAuthentication (Disabled)
            RegistryUtility.FRegistrySetDwordValue(Microsoft.Win32.RegistryHive.LocalMachine, LocalResources.MGstrAutoHostsUpdateServiceRegistryPath, LocalResources.MGstrIsNetworkAuthenticationName, LocalResources.MGintDefaultDisabledValue);
            // NetworkAuthenticationUsername (Empty)
            RegistryUtility.FRegistrySetValue(Microsoft.Win32.RegistryHive.LocalMachine, LocalResources.MGstrAutoHostsUpdateServiceRegistryPath, LocalResources.MGstrNetworkAuthenticationUsernameName, String.Empty);
            // NetworkAuthenticationPassword (Empty)
            RegistryUtility.FRegistrySetValue(Microsoft.Win32.RegistryHive.LocalMachine, LocalResources.MGstrAutoHostsUpdateServiceRegistryPath, LocalResources.MGstrNetworkAuthenticationPasswordName, String.Empty);
        }
    }
}
