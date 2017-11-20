using System;
using System.ComponentModel;

namespace MT.APS100.Service
{
    public static class MyExtension
    {
        public static string ToDescription(this Enum value, bool nameInstend = true)
        {
            DescriptionAttribute attribute = Attribute.GetCustomAttribute(value.GetType().GetField(value.ToString()), typeof(DescriptionAttribute)) as DescriptionAttribute;
            return attribute == null ? string.Empty : attribute.Description;
        }
    }

    public static class FileStructure
    {
        public const string HANDLER_DLL_FILE_PATH = @"C:\MerlinTest\DLL\TTL.dll";
        public const string ENVIRONMENT_CONFIG_FILE_PATH = @"C:\MerlinTest\Imports\env_config.txt";
        public const string USERCAL_DIR = @"C:\MerlinTest\UserCal\";
    }
}
