using ARSoft.Tools.Net.Dns;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace DNSProxy
{
    class DNSConfig
    {
        public DNSConfig()
        {
            InitTimer();
        }
        private DnsClient _dnsClient;
        /// <summary>
        /// 规则名字
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 匹配域名
        /// </summary>
        public string Domain { get; set; }

        /// <summary>
        /// 排序字段
        /// </summary>
        public int Sort { get; set; }

        /// <summary>
        /// 获取服务信息
        /// </summary>
        public DnsClient ServerInfo
        {
            get
            {
                if (_dnsClient == null)
                {
                    SetDNSServerInfo();

                }
                return _dnsClient;
            }
        }

        /// <summary>
        /// 默认规则标识
        /// </summary>
        public bool IsDefault { get; set; }

        /// <summary>
        /// 设置服务器信息
        /// </summary>
        /// <param name="iP">IP address</param>
        /// <param name="querytimeout">10</param>
        private void SetDNSServerInfo()
        {
            if (string.IsNullOrEmpty(IP))
                IP = "119.29.29.29"; //119.29.29.29 DNSPOD;  8.8.8.8 google

            _dnsClient = new DnsClient(IPAddress.Parse(IP), QueryTimeout);
        }


        [DefaultValue(2)]
        public int QueryTimeout { get; set; }

        public string IP { get; set; }

        /// <summary>
        /// 服务是否正常连通（ping机制定时检测）
        /// </summary>
        [DefaultValue(true)]
        public bool IsWork { get; set; }


        #region Ping Timer

        private System.Timers.Timer serverTimer = new System.Timers.Timer();
        private void InitTimer()
        {
            IsWork = true;
            serverTimer.Interval = 5000 + new Random().Next(1000);
            serverTimer.Elapsed += new System.Timers.ElapsedEventHandler(ServerTimer_Elapsed);
            serverTimer.Start();

        }

        private void ServerTimer_Elapsed(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(IP))
                return;

            Ping PNG = new Ping();
            PingReply PNGRPLY = PNG.Send(IP, 2000);

            if (PNGRPLY.Status == IPStatus.Success)
            {
                if (!IsWork)
                    Console.WriteLine(Name + " Working");

                IsWork = true;
            }
            else
            {
                IsWork = false;
                Console.WriteLine(Name + " Can't Working!!!!");
            }

        }
        #endregion

    }
}
