using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ARSoft.Tools.Net.Dns;
using System.Net;
using JsonConfig;


namespace DNSProxy
{
    class Program
    {
        static IPAddress serverIP = IPAddress.IPv6Loopback;
        static List<DNSConfig> dnsConfigs = new List<DNSConfig>();

        static int queryCount = 0;
        static int processedCount = 0;
        static int onLineRulesCount = 0;

        static void Main(string[] args)
        {
            Console.WriteLine("");
            Console.WriteLine("DNSProxy");

            #region Config
            Console.WriteLine("+Config Load...");

            foreach (var item in Config.User.rules)
            {
                Console.WriteLine(item.Name);
                dnsConfigs.Add(new DNSConfig() { Name = item.Name, Domain = item.Domain, IsDefault = item.IsDefault, IP = item.IP, Sort = item.Sort });
            }

            Console.WriteLine("+Config Load...OK");

            System.Timers.Timer printTimer = new System.Timers.Timer();
            printTimer.Interval = 500;
            printTimer.Elapsed += new System.Timers.ElapsedEventHandler(PrintTimer_Elapsed);
            printTimer.Start();

            #region 手动添加配置 取消
            //dnsConfigs.Add(new DNSConfig() { Name = "Default", Domain = "", IsDefault = true, IP = "10.200.150.2" });//默认dns
            //dnsConfigs.Add(new DNSConfig() { Name = "HomeDNS", Domain = "*", IsDefault = false, IP = "192.168.10.254" }); //全匹配dns
            //dnsConfigs.Add(new DNSConfig() { Name = "XdfDNS.xdf", Domain = "xdf.cn", IsDefault = false, IP = "10.200.150.2" });//匹配域名xdf.n
            //dnsConfigs.Add(new DNSConfig() { Name = "XdfDNS.neworiental", Domain = "neworiental.org", IsDefault = false, IP = "10.200.150.2" });//匹配域名neworiental.org
            //dnsConfigs.Add(new DNSConfig() { Name = "r.i3long.cn", Domain = "r.i3long.cn", IsDefault = false, IP = "10.200.150.2" });//匹配域名r.i3long.cn
            #endregion


            #endregion

            using (DnsServer server = new DnsServer(System.Net.IPAddress.Any, 53, 53))
            {
                server.QueryReceived += OnQueryReceived;

                server.Start();

                Console.WriteLine("Press Enter to stop server");
                Console.ReadLine();
            }


        }

        static async Task OnQueryReceived(object sender, QueryReceivedEventArgs e)
        {
            queryCount++;

            DnsMessage query = e.Query as DnsMessage;

            if (query == null)
                return;

            DnsMessage response = query.CreateResponseInstance();
            //response.AnswerRecords.Clear();
            //response.AdditionalRecords.Clear();

            DnsQuestion question = response.Questions.FirstOrDefault();

            if (question == null || string.IsNullOrEmpty(question.Name.ToString()))
                return;

            #region rules match
            //默认配置 无任何规则 满足最基础网络配置
            var defaultConfig = dnsConfigs.Where(d => string.IsNullOrEmpty(d.Domain) && d.IsDefault).FirstOrDefault();


            //全匹配规则配置文件，默认流量走向
            var allInConfig = dnsConfigs.Where(d => d.Domain.Equals("*")).FirstOrDefault();

            //当前匹配到的规则
            DNSConfig currentConfig = null;

            var onlineruleList = dnsConfigs
                .Where(d => d.IsDefault == false && !d.Domain.Equals("*") && d.IsWork)
                .OrderByDescending(d => d.Sort)
                .ToList();

            onLineRulesCount = onlineruleList.Count();

            
            foreach (var item in onlineruleList)
            {
                if (question.Name.ToString().Contains(item.Domain))
                {
                    currentConfig = item;
                    break;
                }
            }


            //ARecord ip = new ARecord(question.Name, 60, IPAddress.Parse("192.168.1.1"));
            //response.AnswerRecords.Add(ip);

            if (currentConfig == null)
            {
                if (allInConfig != null && allInConfig.IsWork)
                {
                    currentConfig = allInConfig;
                    //Console.WriteLine(question.Name.ToString() + " No rules match! Set * Config! ");
                }
                else
                {
                    currentConfig = defaultConfig;
                    //Console.WriteLine(question.Name.ToString() + " No rules match! Set Default Config! ");
                }
            } 
            #endregion



            #region 获取解析
            try
            {
                DnsMessage dnsMessage = currentConfig.ServerInfo.Resolve(question.Name, question.RecordType);
                if ((dnsMessage == null) || ((dnsMessage.ReturnCode != ReturnCode.NoError) && (dnsMessage.ReturnCode != ReturnCode.NxDomain)))
                {
                    //Console.WriteLine("???????");
                    throw new Exception("DNS request failed");
                }
                else
                {
                    foreach (DnsRecordBase dnsRecord in dnsMessage.AnswerRecords)
                    {
                        try
                        {
                            ARecord aRecord = dnsRecord as ARecord;
                            if (aRecord != null)
                            {
                                //Console.WriteLine(question.Name.ToString() + " - " + currentConfig.Name + " " + aRecord.Address.ToString());
                            }
                        }
                        catch { }

                        response.AnswerRecords.Add(dnsRecord);
                    }
                }


                response.ReturnCode = dnsMessage.ReturnCode;

                processedCount++;

            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.Message);
                response.ReturnCode = ReturnCode.FormatError;
            } 
            #endregion


            e.Response = response;

        }




        static private void PrintTimer_Elapsed(object sender, EventArgs e)
        {
            ConsoleWriter.ConsoleWriteQuery("queryCount:" + queryCount);
            ConsoleWriter.ConsoleWriteProcessed("processedCount:" + processedCount);
            ConsoleWriter.ConsoleWriteOnlineRules("onLineRulesCount:" + onLineRulesCount);
            ConsoleWriter.ConsoleWriteDateTime();

            string str = string.Empty;
            foreach(var item in dnsConfigs )
            {
                str += item.Name + ":" + item.IsWork + ";";
            }

            ConsoleWriter.ConsoleWriteIsWork(str);
        }

    }


    
}
