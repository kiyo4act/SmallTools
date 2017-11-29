using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilitiesLib
{
    public static class LocalResources
    {
        public const string m_gstrEventLogSource = "AutoHostsUpdateService";

        public const string m_gstrAutoHostsUpdateServiceRegistryPath = @"System\CurrentControlSet\Services\AutoHostsUpdateService\Parameters";
        public const string m_gstrExternalHostsTargetRegistryName = "ExternalHostsTarget";
        public const string m_gstrInternalHostsTargetRegistryName = "InternalHostsTarget";
        public const string m_gstrNetworkAuthenticationUsernameName = "NetworkUsername";
        public const string m_gstrNetworkAuthenticationPasswordName = "NetworkPassword";

        public const string m_gstrIsUpdateInternalHostsRegistryName = "UpdateInternalHosts";
        public const string m_gstrIsApplyInExternalHostsRegistryName = "ApplyInExternalHosts";
        public const string m_gstrIsNetworkAuthenticationName = "NetworkAuthentication";

        public const string m_gstrExternalHostsTargetRegistryDefaultValue = @"C:\Hosts";
        public const string m_gstrInternalHostsTargetRegistryDefaultValue = @"C:\Windows\System32\drivers\etc\Hosts";

        public const uint m_gintDefaultEnabledValue = 1;
        public const uint m_gintDefaultDisabledValue = 0;
    }

    public class XmlResources
    {
        public string InternalHostsTarget { get; set; }
        public string ExternalHostsTarget { get; set; }
        public int UpdateInternalHosts { get; set; }
        public int ApplyInExternalHosts { get; set; }
        public int NetworkAuthentication { get; set; }
        public string NetworkUsername { get; set; }
        public byte[] NetworkPassword { get; set; }

        public XmlResources()
        {

        }

        public XmlResources(
            string internalHostsTarget,
            string externalHostsTarget,
            int updateInternalHosts,
            int applyInExternalHosts,
            int networkAuthentication,
            string networkUsername,
            byte[] networkPassword)
        {
            InternalHostsTarget = internalHostsTarget;
            ExternalHostsTarget = externalHostsTarget;
            UpdateInternalHosts = updateInternalHosts;
            ApplyInExternalHosts = applyInExternalHosts;
            NetworkAuthentication = networkAuthentication;
            NetworkUsername = networkUsername;
            NetworkPassword = networkPassword;
        }
    }
}
