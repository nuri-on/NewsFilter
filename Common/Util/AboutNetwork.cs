using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Connectivity;

namespace NewsFilter.Common.Util
{
    class AboutNetwork
    {
        /// <summary>
        /// 네트워크 연결 유무
        /// </summary>
        /// <returns></returns>
        public static bool IsInternet()
        {
            ConnectionProfile connections = NetworkInformation.GetInternetConnectionProfile();
            bool internet = connections != null && connections.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.InternetAccess;
            return internet;
        }

        /// <summary>
        /// 사용자 IP 정보
        /// </summary>
        /// <returns></returns>
        public static string GetDeviceUserIP()
        {
            string returnIP = "192.168.0.1";

            var icp = NetworkInformation.GetInternetConnectionProfile();

            if (icp != null && icp.NetworkAdapter != null)
            {
                var hostname = NetworkInformation.GetHostNames()
                    .SingleOrDefault(
                        hn => hn.IPInformation != null && hn.IPInformation.NetworkAdapter != null
                            && hn.IPInformation.NetworkAdapter.NetworkAdapterId == icp.NetworkAdapter.NetworkAdapterId
                    );
                if (hostname != null)
                {
                    returnIP = hostname.CanonicalName;
                }
            }
            return returnIP;
        }
    }
}
