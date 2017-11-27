using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MT.APS100.TesterDriver;

namespace MT.APS100.Service
{
    public class TesterService
    {
        public void UserCal(string ProgramDir)
        {
            Tester tester = new Tester();
            tester.UserCal(ProgramDir);
        }
    }
}
