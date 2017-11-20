using System;
using System.Collections.Generic;

namespace MT.APS100.Model
{
    public class LotInfo
    {
        public UIInputItem TesterID { get; set; } = new UIInputItem();
        public UIInputItem ProgramName { get; set; } = new UIInputItem();
        public UIInputItem OperatorID { get; set; } = new UIInputItem();
        public UIInputItem CustomerID { get; set; } = new UIInputItem();
        public UIInputItem DeviceName { get; set; } = new UIInputItem();
        public UIInputItem CustomerLotNo { get; set; } = new UIInputItem();
        public UIInputItem SubLotNo { get; set; } = new UIInputItem();
        public UIInputItem ModeCode { get; set; } = new UIInputItem();
        public UIInputItem TestCode { get; set; } = new UIInputItem();
        public UIInputItem TestBinNo { get; set; } = new UIInputItem();
        public UIInputItem WaferVersion { get; set; } = new UIInputItem();
        public UIInputItem MotherFab { get; set; } = new UIInputItem();

        public DateTime SETUP_T { get; set; }
        public DateTime START_T { get; set; }
        public string TST_TEMP { get; set; } = string.Empty;
        public string FACIL_ID { get; set; } = string.Empty;
        public string HAND_ID { get; set; } = string.Empty;
        public string LOAD_ID { get; set; } = string.Empty;
        public string TSTR_TYP { get; set; } = string.Empty;
        public string EXEC_TYP { get; set; } = string.Empty;
        public string EXEC_VER { get; set; } = string.Empty;
        public char RTST_COD { get; set; } = 'N';
        public string JOB_REV { get; set; } = string.Empty;
        public string SETUP_ID { get; set; } = "PRODUCTION";
        public DateTime FINISH_T { get; set; }
        public char MODE_COD { get; set; } = 'P';
        public string TesterOSVersion { get; set; } = string.Empty;
    }

    public class UIInputItem
    {
        public string Value { get; set; }
        public string Pattern { get; set; }
        public string Name { get; set; }
        public string Index { get; set; }

        public override string ToString()
        {
            return this.Value;
        }
    }

    public class DatalogConfiguration
    {
        public string datalog_name_rule_1 { get; set; }
        public string datalog_name_rule_2 { get; set; }
    }

    //public class PackConfiguration
    //{
    //    public bool PackTestResult { get; set; }
    //    public ZipMode PackFileType { get; set; }
    //    public char PackFileSet { get; set; }
    //}

    public class UIInputItemConfiguration
    {
        public List<UIInputItem> listUIInputItems = new List<UIInputItem>();
    }

    public class TestCodeConvertConfiguration
    {
        public Dictionary<string, string> dictTestCode = new Dictionary<string, string>();
    }

    public class SpecialSTDFCondition
    {
        public Dictionary<string, string> dictSTDFVariable = new Dictionary<string, string>();
        public Dictionary<string, string> dictSTDFConstant = new Dictionary<string, string>();
    }

    public class Recipe
    {
        public DatalogConfiguration DatalogConfiguration = new DatalogConfiguration();
        //public PackConfiguration PackConfiguration = new PackConfiguration();
        public UIInputItemConfiguration UIInputItemConfiguration = new UIInputItemConfiguration();
        public TestCodeConvertConfiguration TestCodeConvertConfiguration = new TestCodeConvertConfiguration();
        public SpecialSTDFCondition SpecialSTDFCondition = new SpecialSTDFCondition();
    }
}