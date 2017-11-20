using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MT.APS100.Model;
using System.IO;

namespace MT.APS100.Service
{
    public class ProgramService
    {
        public string ProgramName;
        public Configuration Configuration;
        public List<TestLimit> Limit;

        private FlowService flowService;

        public ProgramService(string programDirectory, string programName)
        {
            string configurationPath = Path.Combine(programDirectory, "Config", programName + ".csv");
            string limitPath = Path.Combine(programDirectory, "Limit", programName + ".csv");
            string dllPath = Path.Combine(programDirectory, "Debug", programName + ".dll");
            string flowPath = Path.Combine(programDirectory, "Flow", programName + ".flw");

            ProgramName = programName;
            //Configuration
            //Limit

            flowService = new FlowService(programName, dllPath, flowPath);
        }

        public void Load()
        {
            string calPath = Path.Combine(FileStructure.USERCAL_DIR, ProgramName + ".csv");
            flowService.Load(calPath);
        }

        public void Unload()
        {
            flowService.Unload_New();
        }

        public void Start()
        {

        }
    }
}
