using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using System.Windows;

namespace HenuNetToolNF
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private string netName = "以太网";
        private string ipAddress;
        private string ipMask;
        private string ipGate;
        List<string> interfaces = new List<string>();
        public MainWindow()
        {
            InitializeComponent();
            getIpAddress();
            getInterface();
            inputIpAddress.Text = ipAddress;
            inputMask.Text = ipMask;
            inputGate.Text = ipGate;
            ifList.ItemsSource = interfaces;
        }

        public void getIpAddress()
        {
            ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection nics = mc.GetInstances();
            foreach (ManagementObject nic in nics)
            {
                //循环获取本地计算机IP、MAC、Subnet、Default GateWay
                if (Convert.ToBoolean(nic["ipEnabled"]) == true)
                {
                    try
                    {
                        string mac = nic["MacAddress"].ToString();//Mac地址
                        string ip = (nic["IPAddress"] as String[])[0];//IP地址
                        string ipsubnet = (nic["IPSubnet"] as String[])[0];//子网掩码
                        string ipgateway = (nic["DefaultIPGateway"] as String[])[0];//默认网关
                        if (ip.StartsWith("172.20."))
                        {
                            ipAddress = ip;
                            ipMask = ipsubnet;
                            ipGate = ipgateway;
                            return;
                        }
                    }
                    catch (Exception e)
                    {
                        ipAddress = "获取ip信息出错";
                        ipMask = "可能因为你已经修改过DHCP,无法自动配置";
                        ipGate = "请手动设置";
                    }

                }
            }
            if (ipAddress == null)
            {
                ipAddress = "你现在的ip不属于172.20.0.0/16";
                ipMask = "可能因为你已经修改过DHCP";
                ipGate = "本程序无法读取原来的配置，请手动设置";
            }
        }

        private void getInterface()
        {
            NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface adapter in adapters)
            {
                interfaces.Add(adapter.Name);
            }
        }

        private string applyConfig()
        {
            if (!checkConfig())
            {
                return "请检查配置！！！\n";
            }

            string str = "netsh int ipv4 set interface \"" + netName + "\" dhcpstaticipcoexistence=enabled";
            str += "\nnetsh int ipv4 add address \"" + netName + "\" " + ipAddress + " " + ipMask + " " + ipGate + "";
            str += "\nroute add 172.20.0.0 mask 255.255.0.0 " + ipGate + " metric 1";
            Process p = new Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.CreateNoWindow = true;
            p.Start();
            p.StandardInput.WriteLine(str + "&exit");
            p.StandardInput.AutoFlush = true;
            string strOuput = p.StandardOutput.ReadToEnd();
            p.WaitForExit();
            p.Close();
            return strOuput;
        }

        private Boolean checkConfig()
        {
            if (!ipAddress.StartsWith("172.20.")||!IsIP(ipAddress))
            {
                logBox.Inlines.Add("ip地址非法，应为172.20开头\n");
                return false;
            }
            if (!ipGate.StartsWith("172.20.") || !IsIP(ipGate))
            {
                logBox.Inlines.Add("网关地址非法，应为172.20开头\n");
                return false;
            }
            if (!ipMask.Equals("255.255.255.0") || !IsIP(ipMask))
            {
                logBox.Inlines.Add("子网掩码非法，应为255.255.255.0\n");
                return false;
            }
            if (ifList.SelectedIndex == -1)
            {
                logBox.Inlines.Add("未选择网卡\n");
                return false;
            }
            else
            {
                netName = interfaces[ifList.SelectedIndex];
            }
            return true;
        }

        public static bool IsIP(string ip)
        {
            //判断是否为IP
            return Regex.IsMatch(ip, @"^((2[0-4]\d|25[0-5]|[01]?\d\d?)\.){3}(2[0-4]\d|25[0-5]|[01]?\d\d?)$");
        }

        private void checkNet_Click(object sender, RoutedEventArgs e)
        {
            logBox.Inlines.Add("开始获取ip\n");
            getIpAddress();
            logBox.Inlines.Add(ipAddress);
        }

        private void btnApply_Click(object sender, RoutedEventArgs e)
        {
            logBox.Inlines.Add("开始应用设置\n");
            ipAddress = inputIpAddress.Text;
            ipMask = inputMask.Text;
            ipGate = inputGate.Text;
            string result = applyConfig();
            logBox.Inlines.Add(result+"\n");
        }

    }
}
