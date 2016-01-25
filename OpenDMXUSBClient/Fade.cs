using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace OpenDMXUSBClient
{
    public class Fade
    {
        public int channel;
        public int[] values;
        public double milliseconds;
        private Thread thread;
        public Fade(string[] parameters)
        {
            values = new int[parameters.Length - 2];
            for (int i = 2; i < parameters.Length; i++)
            {

                values[i-2] = 0;
                if (!int.TryParse(parameters[i], out values[i-2]))
                {
                    return;
                }

                if (values[i-2] > 255 || values[i-2] < 0)
                {
                    return;
                }
            }
            int port = 0;
            if (!int.TryParse(parameters[0], out port))
            {
                return;
            }
            if (port > 512 || port < 1)
            {
                return;
            }
            int time = 0;
            if (!int.TryParse(parameters[1], out time))
            {
                return;
            }
            if (time < 1)
            {
                return;
            }

            this.channel = port;
            this.milliseconds = time;
            thread = new Thread(DoFade);
            for (int i = 0; i < DmxClient.fades.Count; i++)
            {
                if (DmxClient.fades[i] != null && DmxClient.fades[i].channel == channel)
                {
                    DmxClient.fades[i].Abort();
                }
            }
            DmxClient.fades.Add(this);
            thread.Start();
        }
        void DoFade()
        {
            Console.WriteLine("New fade on channel " + channel);
            bool run = true;
            int lastValue = values[0];
            int currentPart = 0;
            DateTime startTime = DateTime.Now;
            double elapsedTime;
            double millisecondsPerPart = milliseconds / (values.Length - 1);
            double percentage;
            OpenDMX.setDmxValue(channel, BitConverter.GetBytes(lastValue)[0]);
            while (run)
            {
                elapsedTime = DateTime.Now.Subtract(startTime).TotalMilliseconds;
                currentPart = (int)(elapsedTime / millisecondsPerPart);
                if (currentPart < values.Length-1)
                {
                    percentage = (elapsedTime / millisecondsPerPart) % 1;
                    int v = (int)((values[currentPart + 1] - values[currentPart]) * percentage + values[currentPart]);
                    if (v != lastValue)
                    {
                        lastValue = v;
                        OpenDMX.setDmxValue(channel, BitConverter.GetBytes(lastValue)[0]);
                    }
                }
                else
                {
                    run = false;
                }
            }
            Abort();
        }
        public void Abort()
        {
            if (DmxClient.fades.Contains(this))
            {
                DmxClient.fades.Remove(this);
            }
            Console.WriteLine("Fade on channel " + channel + " ended");            
            thread.Abort();            
        }
    }
}
