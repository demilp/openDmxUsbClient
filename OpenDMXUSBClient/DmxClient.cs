using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenDMXUSBClient
{
    public class DmxClient
    {
        public TCPClientManager client;
        public static List<Fade> fades;
        public DmxClient()
        {
            client = new TCPClientManager();
            client.CommandReceivedEvent += OnData;
            client.ConnectEvent += OnConnect;
            client.Initialize(ConfigurationManager.AppSettings["ip"], int.Parse(ConfigurationManager.AppSettings["port"]), ConfigurationManager.AppSettings["delimitador"]);
            fades = new List<Fade>();
            OpenDMX.start();

        }
        public void OnConnect(object sender, ConnectEventArgs args)
        {
            client.SendData("{\"type\":\"register\", \"data\":\"dmx\", \"tag\":\"tool\"}");
        }
        public void OnData(object sender, CommandEventArgs args)
        {
            try {
                string[] p = args.comando.Split('|');
                if (p.Length < 2 || p.Length == 3)
                {
                    return;
                }
                if (p.Length == 2)
                {
                    SetChannel(p);
                }
                else
                {
                    new Fade(p);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            
        }
        void SetChannel(string[] p)
        {
            int value = 0;
            if (!int.TryParse(p[1], out value))
            {
                return;
            }

            if (value > 255 || value < 0)
            {
                return;
            }
            int port = 0;
            if (!int.TryParse(p[0], out port))
            {
                return;
            }
            if (port > 512 || port < 1)
            {
                return;
            }
            byte[] intBytes = BitConverter.GetBytes(value);
            for (int i = 0; i < fades.Count; i++)
            {
                if (DmxClient.fades[i] != null && fades[i].channel == port)
                {
                    fades[i].Abort();
                }
            }
            OpenDMX.setDmxValue(port, intBytes[0]);
            Console.WriteLine("Channel "+port+" set on "+value);
        }
        public void Close()
        {
            OpenDMX.done = true;
            OpenDMX.stop();
            client.Close();
            for (int i = 0; i < fades.Count; i++)
            {
                fades[i].Abort();
            }            
        }
    }
}
