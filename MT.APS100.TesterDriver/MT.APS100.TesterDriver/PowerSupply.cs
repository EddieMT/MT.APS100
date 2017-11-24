using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ivi.Visa.Interop;

namespace MT.APS100.TesterDriver
{
    public class PowerSupply
    {
        public FormattedIO488 io488PS;

        private string N6700C_Resource;

        public PowerSupply(string N6700C_Resource)
        {
            io488PS = new FormattedIO488();

            this.N6700C_Resource = N6700C_Resource;
        }

        public int Initialize()
        {
            try
            {
                ResourceManager rmPS = new ResourceManager();
                string queryResult = "";
                string[] systemInfo = null;

                Console.WriteLine("Power supply is booting...");

                try
                {
                    io488PS.IO = (IMessage)rmPS.Open(N6700C_Resource, AccessMode.NO_LOCK, 200000, "");
                    Console.WriteLine("Power supply booted successfully");
                }
                catch
                {
                    Console.WriteLine("Power supply did not boot\n");
                    return -1;
                }

                io488PS.IO.Timeout = 5000; // Timeout set to 5sec

                //Logging(" > Check selected application (:INST:SEL? ) " + thisXapp);
                io488PS.IO.WriteString("*IDN?");
                queryResult = io488PS.ReadString();
                if (queryResult.IndexOf("\n") != -1)
                {
                    queryResult = queryResult.Remove(queryResult.IndexOf("\n", 2));
                    systemInfo = queryResult.Split(',');

                    Console.WriteLine("Power supply model: {0}", systemInfo[1]);
                    Console.WriteLine("Power supply serial number: {0}", systemInfo[2]);
                    Console.WriteLine("Power supply firmware revision: {0}\n", systemInfo[3]);
                }

                // Set power supply defaults
                io488PS.IO.WriteString("*RST; *WAI;"); // Reset
                io488PS.IO.WriteString("OUTP OFF, (@1:4);");
                io488PS.IO.WriteString("VOLT:RANG 8, (@1:4);");
                io488PS.IO.WriteString("VOLT 0, (@1:4);");
                io488PS.IO.WriteString("CURR 0.05, (@1:4);");

                return 0;
            }
            catch
            {
                Console.WriteLine("Power supply booted successfully, Set power supply defaults fail\n");
                return -1;
            }
        }

        public int IOWriteString(string buffer)
        {
            return io488PS.IO.WriteString(buffer);
        }

        public void IOClose()
        {
            io488PS.IO.Close();
        }
    }
}
