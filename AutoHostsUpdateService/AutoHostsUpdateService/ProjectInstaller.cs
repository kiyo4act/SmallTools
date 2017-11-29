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
            RegistryUtility.FRegistrySetValue(Microsoft.Win32.RegistryHive.LocalMachine, LocalResources.m_gstrAutoHostsUpdateServiceRegistryPath, LocalResources.m_gstrExternalHostsTargetRegistryName, LocalResources.m_gstrExternalHostsTargetRegistryDefaultValue);
            // InternalHostsTarget
            RegistryUtility.FRegistrySetValue(Microsoft.Win32.RegistryHive.LocalMachine, LocalResources.m_gstrAutoHostsUpdateServiceRegistryPath, LocalResources.m_gstrInternalHostsTargetRegistryName, LocalResources.m_gstrInternalHostsTargetRegistryDefaultValue);
            // ApplyInExternalHosts (Enabled)
            RegistryUtility.FRegistrySetDwordValue(Microsoft.Win32.RegistryHive.LocalMachine, LocalResources.m_gstrAutoHostsUpdateServiceRegistryPath, LocalResources.m_gstrIsApplyInExternalHostsRegistryName, LocalResources.m_gintDefaultEnabledValue);
            // UpdateInternalHosts (Enabled)
            RegistryUtility.FRegistrySetDwordValue(Microsoft.Win32.RegistryHive.LocalMachine, LocalResources.m_gstrAutoHostsUpdateServiceRegistryPath, LocalResources.m_gstrIsUpdateInternalHostsRegistryName, LocalResources.m_gintDefaultEnabledValue);
            // NetworkAuthentication (Disabled)
            RegistryUtility.FRegistrySetDwordValue(Microsoft.Win32.RegistryHive.LocalMachine, LocalResources.m_gstrAutoHostsUpdateServiceRegistryPath, LocalResources.m_gstrIsNetworkAuthenticationName, LocalResources.m_gintDefaultDisabledValue);
            // NetworkAuthenticationUsername (Empty)
            RegistryUtility.FRegistrySetValue(Microsoft.Win32.RegistryHive.LocalMachine, LocalResources.m_gstrAutoHostsUpdateServiceRegistryPath, LocalResources.m_gstrNetworkAuthenticationUsernameName, String.Empty);
            // NetworkAuthenticationPassword (Empty)
            RegistryUtility.FRegistrySetValue(Microsoft.Win32.RegistryHive.LocalMachine, LocalResources.m_gstrAutoHostsUpdateServiceRegistryPath, LocalResources.m_gstrNetworkAuthenticationPasswordName, String.Empty);
        }
    }
}
