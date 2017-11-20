namespace MT.APS100.Model
{
    public class EnvironmentConfig
    {
        public TransferMode TransferMode = TransferMode.Default;
        public string ProductionWorkspace { get; set; }
        public ServerIP ServerDataIP { get; set; } = new ServerIP();
        public ServerIP ServerProgIP { get; set; } = new ServerIP();
        public string LocalDlogDir { get; set; }
        public string LocalProgDir { get; set; }
        public string ServerDlogDir { get; set; }
        public string ServerProgDir { get; set; }
        public string ServerMESFileDir { get; set; }
        public string TesterType { get; set; } = string.Empty;
        public bool UIClearProg { get; set; }
        public bool StartClearProg { get; set; }
        public bool ExitClearProg { get; set; }
        public bool AlarmM01 { get; set; }
        public bool AlarmM02 { get; set; }
        public bool AlarmM03 { get; set; }
        public bool AlarmM04 { get; set; }
    }

    public class ServerIP
    {
        public string IP { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
    }
}