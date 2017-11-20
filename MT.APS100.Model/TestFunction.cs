using System.Collections.Generic;
namespace MT.APS100.Model
{
    public class TestFunction
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public List<FunctionParameter> Parameters = new List<FunctionParameter>();
    }

    public class FunctionParameter
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
    }
}