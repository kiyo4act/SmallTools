using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilitiesLib
{
    public static class LocalResources
    {
        public const string MGstrEventLogSource = "AutoHostsUpdateService";

        public const string MGstrAutoHostsUpdateServiceRegistryPath = @"Software\AutoHostsUpdate";
        public const string MGstrExternalHostsTargetRegistryName = "ExternalHostsTarget";
        public const string MGstrInternalHostsTargetRegistryName = "InternalHostsTarget";
        public const string MGstrNetworkAuthenticationUsernameName = "NetworkUsername";
        public const string MGstrNetworkAuthenticationPasswordName = "NetworkPassword";

        public const string MGstrIsUpdateInternalHostsRegistryName = "UpdateInternalHosts";
        public const string MGstrIsApplyInExternalHostsRegistryName = "ApplyInExternalHosts";
        public const string MGstrIsNetworkAuthenticationName = "NetworkAuthentication";

        public const string MGstrExternalHostsTargetRegistryDefaultValue = @"C:\Hosts";
        public const string MGstrInternalHostsTargetRegistryDefaultValue = @"C:\Windows\System32\drivers\etc\Hosts";

        public const uint MGintDefaultEnabledValue = 1;
        public const uint MGintDefaultDisabledValue = 0;
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
