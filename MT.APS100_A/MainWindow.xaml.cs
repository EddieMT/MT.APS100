using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.IO;
using System.Threading;
using MT.APS100.Model;
using MT.APS100.Service;
using System.ComponentModel;
using System.Configuration;
using System.Reflection;

namespace MT.APS100_A
{
    public delegate bool OKCallback();

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //TODO: Any exception, should we check handler and application to make sure they are unloaded?

        private EventWaitHandle handle;
        private BackgroundWorker backgroundWorker;
        private string from;

        private ObservableCollection<DisplayTestResult> colTestResult = new ObservableCollection<DisplayTestResult>();
        private ObservableCollection<BinResult> colHardBinResult = new ObservableCollection<BinResult>();
        private ObservableCollection<BinResult> colSoftBinResult = new ObservableCollection<BinResult>();
        private List<PartResult> fullPartResult = new List<PartResult>();
        private Dictionary<LotInfo, List<PartResult>> fulllot = new Dictionary<LotInfo, List<PartResult>>();
        private uint siteNumber;
        private EnvironmentConfig enviConfig;
        private LotInfo lotInfo;
        private string uilogPath = string.Empty;
        private string flowPath = string.Empty;
        private uint partID = 0;
        private string _ttl = string.Empty;
        private string _productionworkspace = string.Empty;
        private string _subcon = string.Empty;
        private string _password = string.Empty;
        private MT.APS100.Model.Configuration configuration;
        private List<TestLimit> limits;
        private List<DisplayTestResult> lastRoundTestResults = new List<DisplayTestResult>();
        private List<PartResult> lastRoundPartResults = new List<PartResult>();

        private UIService uiService;
        private HandlerService handlerService;
        private FlowService flowService;
        private DataService dataService;
        private Importconfig configService;

        private int systemCount = 0;
        private BasicInfo currentBasicInfo = new BasicInfo();
        private string lastTestCode = string.Empty;
        private string lastProgramName = string.Empty;

        public MainWindow()
        {
            InitializeComponent();

            _ttl = FileStructure.HANDLER_DLL_FILE_PATH;
            _subcon = ConfigurationManager.AppSettings["SubCon"];

            backgroundWorker = new BackgroundWorker();
            backgroundWorker.WorkerReportsProgress = true;
            backgroundWorker.WorkerSupportsCancellation = true;
            backgroundWorker.DoWork += new DoWorkEventHandler(backgroundWorker_DoWork);
            backgroundWorker.ProgressChanged += new ProgressChangedEventHandler(backgroundWorker_ProgressChanged);
            backgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorker_RunWorkerCompleted);
        }

        #region events
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                uiService = new UIService();
                enviConfig = uiService.GetEnviConfig();

                handlerService = new HandlerService(_ttl);

                uiService.CreateWorkspace();

                SetButtonSatus(ButtonStatus.status_s1);

                uiService.SaveUILog("Login", new MTResponse() { ResponseStatus = ResponseStatus.Success, Message = "Window_Loaded : Login success" });
            }
            catch (Exception ex)
            {
                if (ex is FileLoadException)
                {

                }
                else
                {
                    uiService.SaveUILog("Login", new MTResponse() { ResponseStatus = ResponseStatus.Fail, Message = "Window_Loaded : " + ex.Message });

                }

                MessageBox.Show(ex.Message);
                from = "Window_Loaded";
                this.Close();
            }
        }

        private void btnStartLot_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MessageBox.Show("Do you want to start lot？", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    SetButtonSatus(ButtonStatus.status_s0);

                    lotInfo = new LotInfo();

                    txtLotInfo.Inlines.Clear();
                    txtProgramLocation.Text = string.Empty;
                    txtDataLocation.Text = string.Empty;

                    LotinfoMES liMES = new LotinfoMES(lotInfo);
                    liMES.OKCallback += LotinfoMES_OKCallback;
                    if (liMES.ShowDialog().Value)
                    {
                        lotInfo.TestCode.Value = lastTestCode;
                        LotinfoDlg liDLG = new LotinfoDlg(lotInfo, currentBasicInfo);
                        liDLG.OKCallback += LotinfoDlg_OKCallback;
                        if (!liDLG.ShowDialog().Value)
                        {
                            SetButtonSatus(ButtonStatus.status_s1);
                        }
                    }
                    else
                    {
                        SetButtonSatus(ButtonStatus.status_s1);
                    }
                }
                uiService.SaveUILog("StartLot", new MTResponse() { ResponseStatus = ResponseStatus.Success, Message = "btnStartLot_Click : StartLot success" });
            }
            catch (Exception ex)
            {
                uiService.SaveUILog("StartLot", new MTResponse() { ResponseStatus = ResponseStatus.Fail, Message = "btnStartLot_Click : " + ex.Message });
            }
        }

        private void btnSaveTestResult_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MessageBox.Show("Do you want to save test result？", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    from = "btnSaveTestResult_Click";
                    backgroundWorker.CancelAsync();

                    Thread exit = new Thread(backgroundWorker_exit);
                    exit.Start();
                }
            }
            catch (Exception ex)
            {
                uiService.SaveUILog("SaveTestResult", new MTResponse() { ResponseStatus = ResponseStatus.Fail, Message = "btnSaveTestResult_Click : " + ex.Message });
            }
        }

        private void btnFullLotEnd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MessageBox.Show("Do you want to lot end？", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    SetButtonSatus(ButtonStatus.status_s0);

                    Thread threadStatus = new Thread(new ParameterizedThreadStart(UpdateStatus));
                    threadStatus.Start(Tuple.Create("正在检查config文件...", 1, 20));

                    if (!dataService.CheckConfigExist())
                    {
                        MessageBox.Show("alarm U04:config is not exist ，please call MFG leader !");
                        SetButtonSatus(ButtonStatus.status_s3);
                        return;
                    }

                    threadStatus.Abort();
                    threadStatus = new Thread(new ParameterizedThreadStart(UpdateStatus));
                    threadStatus.Start(Tuple.Create("正在检查转换summary文件...", 21, 40));

                    if (!dataService.CheckCSVExist())
                    {
                        MessageBox.Show("alarm U05:summary is not exist ，please call MFG leader !");
                        SetButtonSatus(ButtonStatus.status_s3);
                        return;
                    }

                    threadStatus.Abort();
                    threadStatus = new Thread(new ParameterizedThreadStart(UpdateStatus));
                    threadStatus.Start(Tuple.Create("正在存储汇总及过账summary文件...", 41, 70));

                    dataService.SaveFullLot(fulllot, limits);

                    threadStatus.Abort();
                    threadStatus = new Thread(new ParameterizedThreadStart(UpdateStatus));
                    threadStatus.Start(Tuple.Create("存储完成...", 71, 100));

                    fulllot = new Dictionary<LotInfo, List<PartResult>>();
                    systemCount = 0;
                    lastTestCode = string.Empty;
                    lastProgramName = string.Empty;
                    configuration = null;

                    SetButtonSatus(ButtonStatus.status_s1);
                    grdLocation.Visibility = Visibility.Visible;
                    grdStatus.Visibility = Visibility.Collapsed;

                    uiService.SaveUILog("FullLotEnd", new MTResponse() { ResponseStatus = ResponseStatus.Success, Message = "btnFullLotEnd_Click : FullLot success" });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                uiService.SaveUILog("FullLotEnd", new MTResponse() { ResponseStatus = ResponseStatus.Fail, Message = "btnFullLotEnd_Click : " + ex.Message });
                SetButtonSatus(ButtonStatus.status_s3);
                grdLocation.Visibility = Visibility.Visible;
                grdStatus.Visibility = Visibility.Collapsed;
            }
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Close();
            }
            catch (Exception ex)
            {
                uiService.SaveUILog("Exit", new MTResponse() { ResponseStatus = ResponseStatus.Fail, Message = "btnExit_Click : " + ex.Message });
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (from != "Window_Loaded")
            {
                if (MessageBox.Show("Do you want to Exit？", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    from = "btnExit_Click";
                    backgroundWorker.CancelAsync();

                    Thread exit = new Thread(backgroundWorker_exit);
                    exit.Start();
                }
                else
                {
                    e.Cancel = true;
                }
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            try
            {
                if (from != "Window_Loaded")
                {
                    Thread.Sleep(5000);
                    uiService.DeleteWorkspace("ExitClearProg");
                    uiService.SaveUILog("Exit", new MTResponse() { ResponseStatus = ResponseStatus.Success, Message = "Window_Closed : Exit success" });
                }
            }
            catch (Exception ex)
            {
                uiService.SaveUILog("Exit", new MTResponse() { ResponseStatus = ResponseStatus.Fail, Message = "Window_Closed : " + ex.Message });
            }
        }

        private void Btn_Configuration_Click(object sender, RoutedEventArgs e)
        {
            MTConfig config = new MTConfig();
            config.ShowDialog();
        }

        private void Btn_CreateProject_Click(object sender, RoutedEventArgs e)
        {
            ProjectCreation projectcreation = new ProjectCreation();
            projectcreation.ShowDialog();
        }

        private void btnEnvironment_Click(object sender, RoutedEventArgs e)
        {
            if (systemCount > 0)
            {
                MessageBox.Show("批次量产中，不能修改环境参数。请等待本批次完全结束后，再进行修改！");
                return;
            }

            if (MessageBox.Show("修改环境参数会导致存储方式或路径产生变化，修改完毕后需重启界面，是否确认修改？", "确认", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                Environment environment = new Environment();
                if (environment.ShowDialog().HasValue)
                {
                    if (environment.DialogResult.Value)
                    {
                        MessageBox.Show("环境参数修改完毕，需要重启界面，请退出界面！");
                        SetButtonSatus(ButtonStatus.status_s0);
                    }
                }
            }
        }

        private void txtLastNDeviceTested_LostFocus(object sender, RoutedEventArgs e)
        {
            int allTested = fullPartResult.Count;
            int lastNTested = 0;
            int.TryParse(txtLastNDeviceTested.Text, out lastNTested);
            if (allTested > lastNTested)
            {
                var list = fullPartResult.OrderByDescending(x => x.PartID).Take(lastNTested);
                int lastNPassed = list.Count(x => x.isSuccess == true);
                int lastNFailed = list.Count(x => x.isSuccess == false);
                double lastNYield = (double)lastNPassed / (double)lastNTested * 100;
                txtLastNPassCount.Text = lastNPassed.ToString();
                txtLastNFailCount.Text = lastNFailed.ToString();
                txtLastNYield.Text = string.Format("{0:N3}%", lastNYield);
            }
            else
            {
                int allPassed = fullPartResult.Count(x => x.isSuccess == true);
                int allFailed = fullPartResult.Count(x => x.isSuccess == false);
                double allYield = (allTested == 0) ? 0 : (double)allPassed / (double)allTested * 100;
                txtLastNPassCount.Text = allPassed.ToString();
                txtLastNFailCount.Text = allFailed.ToString();
                txtLastNYield.Text = string.Format("{0:N3}%", allYield);
            }
        }

        private void tbcSummary_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            e.Handled = true;
            if (e.Source is TabControl)
            {
                RefreshYield();
                RefreshDataGridBinResult();
            }
        }

        private void tbcDlog_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            e.Handled = true;
            if (e.Source is TabControl)
            {
                RefreshYield();
                RefreshDataGridBinResult();
                RefreshDataGridTestResult(lastRoundTestResults);
                RefreshGridTestResult(lastRoundPartResults);
            }
        }

        private bool isManual = false;
        private int loop = 1;
        private int indextime = 100;
        private void btnManualTest_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(txtLoop.Text, out loop))
            {
                MessageBox.Show("Loop must be integer!");
                return;
            }
            if (!int.TryParse(txtIndextime.Text, out indextime))
            {
                MessageBox.Show("Index time must be integer!");
                return;
            }
            btnManualTest.IsEnabled = false;
            isManual = true;
            handle.Set();
        }

        private void btnStopManualTest_Click(object sender, RoutedEventArgs e)
        {
            isManual = false;
        }
        #endregion

        #region private methods
        private void InitializeDataGrid(uint siteNumber)
        {
            InitializeDataGridTestResult(siteNumber);

            InitializeDataGridHardBinResul(siteNumber);

            InitializeDataGridSoftBinResul(siteNumber);
        }

        private void InitializeDataGridTestResult(uint siteNumber)
        {
            dgrTestResult.IsReadOnly = true;
            //dgrTestResult.IsHitTestVisible = false;
            //Style style = new Style();
            //style.TargetType = typeof(DataGridCell);

            //Setter bg = new Setter();
            //bg.Property = DataGridCell.BackgroundProperty;
            //bg.Value = Brushes.Cyan;
            //style.Setters.Add(bg);

            DataGridTextColumn column = new DataGridTextColumn();
            column.Header = "Test #";
            column.Binding = new Binding("TestNumber");
            column.DisplayIndex = 0;
            //column.CellStyle = style;
            column.IsReadOnly = true;
            dgrTestResult.Columns.Add(column);

            column = new DataGridTextColumn();
            column.Header = "Test Name";
            column.Binding = new Binding("TestName");
            column.DisplayIndex = 1;
            //column.CellStyle = style;
            column.IsReadOnly = true;
            dgrTestResult.Columns.Add(column);

            column = new DataGridTextColumn();
            column.Header = "Low Limit";
            column.Binding = new Binding("FTLower");
            column.DisplayIndex = 2;
            //column.CellStyle = style;
            column.IsReadOnly = true;
            dgrTestResult.Columns.Add(column);

            column = new DataGridTextColumn();
            column.Header = "High Limit";
            column.Binding = new Binding("FTUpper");
            column.DisplayIndex = 3;
            //column.CellStyle = style;
            column.IsReadOnly = true;
            dgrTestResult.Columns.Add(column);

            column = new DataGridTextColumn();
            column.Header = "Unit";
            column.Binding = new Binding("Unit");
            column.DisplayIndex = 4;
            //column.CellStyle = style;
            column.IsReadOnly = true;
            dgrTestResult.Columns.Add(column);

            for (int i = 0; i < siteNumber; i++)
            {
                column = new DataGridTextColumn();
                column.Header = "Site" + (i + 1).ToString();
                column.IsReadOnly = true;
                //dgrTestResult.Columns.Add(column);

                Style st = new Style();
                st.TargetType = typeof(DataGridCell);

                //DataTrigger dt = new DataTrigger();
                //Binding DataTriggerBinding = new Binding(string.Format("PFFlags[{0}]", i));
                //DataTriggerBinding.Mode = BindingMode.Default;
                //DataTriggerBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;

                //dt.Binding = DataTriggerBinding;
                //dt.Value = "True";
                //Setter DataTriggerSetter = new Setter();
                //DataTriggerSetter.Property = DataGridCell.BackgroundProperty;
                //DataTriggerSetter.Value = Brushes.OrangeRed;

                //dt.Setters.Add(DataTriggerSetter);
                //st.Triggers.Add(dt);

                DataTrigger dt2 = new DataTrigger();
                Binding DataTriggerBinding2 = new Binding(string.Format("PFFlags[{0}]", i));
                DataTriggerBinding2.Mode = BindingMode.Default;
                DataTriggerBinding2.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;

                dt2.Binding = DataTriggerBinding2;
                dt2.Value = "False";
                Setter DataTriggerSetter2 = new Setter();
                DataTriggerSetter2.Property = DataGridCell.BackgroundProperty;
                DataTriggerSetter2.Value = Brushes.Red;

                dt2.Setters.Add(DataTriggerSetter2);
                st.Triggers.Add(dt2);

                //column.ElementStyle = st;
                // dgrTestResult.CellStyle = st;

                Binding binding = new Binding(string.Format("DisplayResults[{0}]", i));
                column.Binding = binding;
                column.CellStyle = st;
                //column.ElementStyle = Style;
                dgrTestResult.Columns.Add(column);
                //MultiBinding binding = new MultiBinding
                //{
                //    Converter = new ValueConverter()
                //};
                //binding.Bindings.Add(new Binding(string.Format("DisplayResults[{0}]", i)));
                //binding.Bindings.Add(new Binding(string.Format("PFFlags[{0}]", i)));
                //binding.NotifyOnSourceUpdated = true;
            }
            dgrTestResult.ItemsSource = colTestResult;
        }

        private void InitializeDataGridHardBinResul(uint siteNumber)
        {
            DataGridTextColumn column = new DataGridTextColumn();
            column.Header = "BinID";
            column.Binding = new Binding("BinNum");
            dgrHardBins.Columns.Add(column);

            column = new DataGridTextColumn();
            column.Header = "BinName";
            column.Binding = new Binding("BinName");
            dgrHardBins.Columns.Add(column);

            for (int i = 0; i < siteNumber; i++)
            {
                column = new DataGridTextColumn();
                column.Header = "Site" + (i + 1).ToString();
                column.Binding = new Binding(string.Format("SubTotal[{0}]", i));
                dgrHardBins.Columns.Add(column);

                column = new DataGridTextColumn();
                column.Header = "Yield" + (i + 1).ToString();
                column.Binding = new Binding(string.Format("SubYield[{0}]", i));
                dgrHardBins.Columns.Add(column);
            }

            column = new DataGridTextColumn();
            column.Header = "Total";
            column.Binding = new Binding("Total");
            dgrHardBins.Columns.Add(column);

            column = new DataGridTextColumn();
            column.Header = "Yield";
            column.Binding = new Binding("Yield");
            dgrHardBins.Columns.Add(column);

            dgrHardBins.ItemsSource = colHardBinResult;
        }

        private void InitializeDataGridSoftBinResul(uint siteNumber)
        {
            DataGridTextColumn column = new DataGridTextColumn();
            column.Header = "BinID";
            column.Binding = new Binding("BinNum");
            dgrSoftBins.Columns.Add(column);

            column = new DataGridTextColumn();
            column.Header = "BinName";
            column.Binding = new Binding("BinName");
            dgrSoftBins.Columns.Add(column);

            column = new DataGridTextColumn();
            column.Header = "HardBinName";
            column.Binding = new Binding("HardBinName");
            dgrSoftBins.Columns.Add(column);

            for (int i = 0; i < siteNumber; i++)
            {
                column = new DataGridTextColumn();
                column.Header = "Site" + (i + 1).ToString();
                column.Binding = new Binding(string.Format("SubTotal[{0}]", i));
                dgrSoftBins.Columns.Add(column);

                column = new DataGridTextColumn();
                column.Header = "Yield" + (i + 1).ToString();
                column.Binding = new Binding(string.Format("SubYield[{0}]", i));
                dgrSoftBins.Columns.Add(column);
            }

            column = new DataGridTextColumn();
            column.Header = "Total";
            column.Binding = new Binding("Total");
            dgrSoftBins.Columns.Add(column);

            column = new DataGridTextColumn();
            column.Header = "Yield";
            column.Binding = new Binding("Yield");
            dgrSoftBins.Columns.Add(column);

            dgrSoftBins.ItemsSource = colSoftBinResult;
        }

        private void Release()
        {
            if (handle != null)
            {
                handlerService.Stop();
                handlerService.Unload();
                handle.Dispose();
                handle = null;

                flowService.Unload();
            }
        }

        private bool LotinfoMES_OKCallback()
        {
            try
            {
                txtProgramLocation.Text = Path.Combine(enviConfig.LocalProgDir, lotInfo.ProgramName.ToString());
                if (!Directory.Exists(txtProgramLocation.Text))
                {
                    MessageBox.Show("alarm M10:Program不存在，请确认后重新输入 !");
                    return false;
                }

                uiService.SaveUILog("Download", new MTResponse() { ResponseStatus = ResponseStatus.Success, Message = "LotinfoMES_OKCallback : Download success" });
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("System error: " + ex.Message + " Please contact the developer!");
                uiService.SaveUILog("Download", new MTResponse() { ResponseStatus = ResponseStatus.Fail, Message = "LotinfoMES_OKCallback : " + ex.Message });
                return false;
            }
        }

        private bool LotinfoDlg_OKCallback()
        {
            try
            {
                if (systemCount > 0)
                {
                    bool check20 = true;
                    foreach (var pi in lotInfo.GetType().GetProperties())
                    {
                        if (pi.GetValue(lotInfo).GetType() == typeof(UIInputItem)
                            && pi.GetValue(lotInfo).ToString().Contains(" "))
                        {
                            check20 = false;
                            break;
                        }
                    }
                    if (!check20)
                    {
                        MessageBox.Show("alarm M11:批次信息存在异常字符, 如空格等，请确认后重新输入 !");
                        return false;
                    }

                    bool check23 = (lastProgramName != string.Empty) ? (lastProgramName == lotInfo.ProgramName.ToString()) : true;
                    if (!check23)
                    {
                        MessageBox.Show("alarm M12:程序名前后不一致，请确认后重新输入 !");
                        return false;
                    }

                    if (fulllot.Any(x => (x.Key.TestCode.ToString() == lotInfo.TestCode.ToString() && x.Key.SubLotNo.ToString() == lotInfo.SubLotNo.ToString())))
                    {
                        MessageBox.Show(string.Format("alarm M13:子批次{0}中已存在{1}数据， 请确认后重新输入 !", lotInfo.SubLotNo.ToString(), lotInfo.TestCode.ToString()));
                        return false;
                    }
                }

                fullPartResult = new List<PartResult>();
                partID = 0;
                flowPath = Path.Combine(txtProgramLocation.Text, "Flow", lotInfo.ProgramName.ToString() + ".flw");

                if (configuration == null)
                {
                    configService = new Importconfig();
                    configuration = configService.ImportConfigurationData(flowPath);
                    siteNumber = (uint)configuration.NumberOfSites;
                    InitializeDataGrid(siteNumber);
                }

                flowService = new FlowService(flowPath, siteNumber,
                                                configuration.StopOnAllFail ? FlowWorkMode.StopOnAllFail :
                                                configuration.StopOnFail ? FlowWorkMode.StopOnFail : FlowWorkMode.ContinueOnFail);
                limits = flowService.GetLimits();
                flowService.processCal(configuration.UserCalibration, configuration.CalibrationExpiration);

                dataService = new DataService(enviConfig, lotInfo);
                txtDataLocation.Text = dataService.GetDataLocation();
                dataService.SaveHeader();
                if (flowService.Load() != 0)
                {
                    MessageBox.Show("Program initial failed!");
                    return false;
                }
                handlerService.Load();
                //EventWaitHandleSecurity security = new EventWaitHandleSecurity();
                //string user = Environment.UserDomainName + "\\" + Environment.UserName;
                //EventWaitHandleAccessRule rule = new EventWaitHandleAccessRule(user, EventWaitHandleRights.Synchronize | EventWaitHandleRights.FullControl, AccessControlType.Allow);
                //security.AddAccessRule(rule);
                //bool createNew;
                //handle = new EventWaitHandle(false, EventResetMode.AutoReset, "SOTSignal", out createNew, security);
                handle = new EventWaitHandle(false, EventResetMode.AutoReset, "SOTSignal");
                if (handle != null)
                {
                    backgroundWorker.RunWorkerAsync();
                }
                else
                {
                    throw new Exception("Handler loading failed!");
                }
                handlerService.Start();

                lastTestCode = lotInfo.TestCode.ToString();
                lastProgramName = lotInfo.ProgramName.ToString();
                systemCount += 1;

                SetButtonSatus(ButtonStatus.status_s2);

                //print lotinfo
                foreach (var pi in lotInfo.GetType().GetProperties())
                {
                    if (pi.GetValue(lotInfo).GetType() == typeof(UIInputItem))
                    {
                        txtLotInfo.Inlines.Add(new Run(string.Format("{0} : {1}", pi.Name, pi.GetValue(lotInfo).ToString())));
                        txtLotInfo.Inlines.Add(new LineBreak());
                    }
                }

                uiService.SaveUILog("OK", new MTResponse() { ResponseStatus = ResponseStatus.Success, Message = "LotinfoDlg_OKCallback : OK success. Start test" });
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("System error: " + ex.Message + " Please contact the developer!");
                uiService.SaveUILog("OK", new MTResponse() { ResponseStatus = ResponseStatus.Fail, Message = "LotinfoDlg_OKCallback : " + ex.Message });
                return false;
            }
        }

        private void SetButtonSatus(ButtonStatus buttonStatus)
        {
            btnStartLot.IsEnabled = (buttonStatus == ButtonStatus.status_s1 || buttonStatus == ButtonStatus.status_s3);
            btnSaveTestResult.IsEnabled = buttonStatus == ButtonStatus.status_s2;
            btnFullLotEnd.IsEnabled = (buttonStatus == ButtonStatus.status_s3);
            btnExit.IsEnabled = (buttonStatus == ButtonStatus.status_s1 || buttonStatus == ButtonStatus.status_s2 || buttonStatus == ButtonStatus.status_s3);

            btnManualTest.IsEnabled = buttonStatus == ButtonStatus.status_s2;
        }

        private void backgroundWorker_exit()
        {
            Thread.Sleep(3000);
            if (handle != null)
            {
                handle.Set();
            }
        }

        private void RefreshYield(bool refresh = false)
        {
            if (limits == null)
                return;

            if ((((TabItem)tbcDlog.SelectedItem).Header.ToString() == "Yield"
                && ((TabItem)tbcSummary.SelectedItem).Header.ToString() == "All")
                || refresh)
            {
                int allTested = fullPartResult.Count;
                int allPassed = fullPartResult.Count(x => x.isSuccess == true);
                int allFailed = fullPartResult.Count(x => x.isSuccess == false);
                double allYield = (allTested == 0) ? 0 : (double)allPassed / (double)allTested * 100;
                txtAllDeviceTested.Text = allTested.ToString();
                txtAllPassCount.Text = allPassed.ToString();
                txtAllFailCount.Text = allFailed.ToString();
                txtAllYield.Text = string.Format("{0:N3}%", allYield);
                //
                if (allTested > 100)
                {
                    var list = fullPartResult.OrderByDescending(x => x.PartID).Take(100);
                    int lastOPassed = list.Count(x => x.isSuccess == true);
                    int lastOFailed = list.Count(x => x.isSuccess == false);
                    double lastOYield = (double)lastOPassed;
                    txtLastOPassCount.Text = lastOPassed.ToString();
                    txtLastOFailCount.Text = lastOFailed.ToString();
                    txtLastOYield.Text = string.Format("{0:N3}%", lastOYield);
                }
                else
                {
                    txtLastOPassCount.Text = allPassed.ToString();
                    txtLastOFailCount.Text = allFailed.ToString();
                    txtLastOYield.Text = string.Format("{0:N3}%", allYield);
                }
                //
                int lastNTested = 0;
                int.TryParse(txtLastNDeviceTested.Text, out lastNTested);
                if (allTested > lastNTested)
                {
                    var list = fullPartResult.OrderByDescending(x => x.PartID).Take(lastNTested);
                    int lastNPassed = list.Count(x => x.isSuccess == true);
                    int lastNFailed = list.Count(x => x.isSuccess == false);
                    double lastNYield = (double)lastNPassed / (double)lastNTested * 100;
                    txtLastNPassCount.Text = lastNPassed.ToString();
                    txtLastNFailCount.Text = lastNFailed.ToString();
                    txtLastNYield.Text = string.Format("{0:N3}%", lastNYield);
                }
                else
                {
                    txtLastNPassCount.Text = allPassed.ToString();
                    txtLastNFailCount.Text = allFailed.ToString();
                    txtLastNYield.Text = string.Format("{0:N3}%", allYield);
                }
            }
        }

        private void RefreshDataGridBinResult(bool refresh = false)
        {
            if (limits == null)
                return;

            int[] siteTotal = new int[siteNumber];
            for (int i = 0; i < siteNumber; i++)
            {
                siteTotal[i] = fullPartResult.Count(x => x.SiteNumber == (i + 1));
            }
            int total = fullPartResult.Count;
            //
            if ((((TabItem)tbcDlog.SelectedItem).Header.ToString() == "Yield"
                && ((TabItem)tbcSummary.SelectedItem).Header.ToString() == "HardBins")
                || refresh)
            {
                colHardBinResult.Clear();
                var grpHBlimits = limits.GroupBy(x => new { x.HardBinName, x.HardBinNumber, x.HardBinPF });
                foreach (var limit in grpHBlimits)
                {
                    BinResult br = new BinResult();
                    br.BinNum = (ushort)limit.Key.HardBinNumber;
                    br.BinName = limit.Key.HardBinName;
                    int binTotal = 0;
                    int[] subTotal = new int[siteNumber];
                    string[] subYield = new string[siteNumber];
                    for (int i = 0; i < siteNumber; i++)
                    {
                        subTotal[i] = fullPartResult.Count(x => (x.SiteNumber == (i + 1) && x.HardBin.BinNum == br.BinNum && x.HardBin.BinName == br.BinName));
                        double percent = siteTotal[i] == 0 ? 0 : (double)subTotal[i] / (double)siteTotal[i] * 100;
                        subYield[i] = string.Format("{0:N3}%", percent);
                        binTotal += subTotal[i];
                    }
                    br.SubTotal = subTotal.ToList();
                    br.SubYield = subYield.ToList();
                    br.Total = binTotal;
                    br.Yield = string.Format("{0:N3}%", (total == 0 ? 0 : (double)binTotal / (double)total * 100));
                    colHardBinResult.Add(br);
                }
            }
            //
            if ((((TabItem)tbcDlog.SelectedItem).Header.ToString() == "Yield"
                && ((TabItem)tbcSummary.SelectedItem).Header.ToString() == "SoftBins")
                || refresh)
            {
                colSoftBinResult.Clear();
                var grpSBlimits = limits.GroupBy(x => new { x.SoftBinName, x.SoftBinNumber, x.SoftBinPF });
                foreach (var limit in grpSBlimits)
                {
                    BinResult br = new BinResult();
                    br.BinNum = (ushort)limit.Key.SoftBinNumber;
                    br.BinName = limit.Key.SoftBinName;
                    br.HardBinName = limit.First().HardBinName;
                    int binTotal = 0;
                    int[] subTotal = new int[siteNumber];
                    string[] subYield = new string[siteNumber];
                    for (int i = 0; i < siteNumber; i++)
                    {
                        subTotal[i] = fullPartResult.Count(x => (x.SiteNumber == (i + 1) && x.SoftBin.BinNum == br.BinNum && x.SoftBin.BinName == br.BinName));
                        double percent = siteTotal[i] == 0 ? 0 : (double)subTotal[i] / (double)siteTotal[i] * 100;
                        subYield[i] = string.Format("{0:N3}%", percent);
                        binTotal += subTotal[i];
                    }
                    br.SubTotal = subTotal.ToList();
                    br.SubYield = subYield.ToList();
                    br.Total = binTotal;
                    br.Yield = string.Format("{0:N3}%", (total == 0 ? 0 : (double)binTotal / (double)total * 100));
                    colSoftBinResult.Add(br);
                }
            }
        }

        private void RefreshDataGridTestResult(List<DisplayTestResult> list, bool refresh = false)
        {
            if (limits == null)
                return;

            if (((TabItem)tbcDlog.SelectedItem).Header.ToString() == "Device Data" || refresh)
            {
                colTestResult.Clear();
                list.ForEach(x => colTestResult.Add(x));
                //dgrTestResult.Dispatcher.Invoke(new Action(() =>
                //{
                //    dgrTestResult.Items.Refresh();
                //}));
            }
        }

        private void RefreshGridTestResult(List<PartResult> list, bool refresh = false)
        {
            if (limits == null)
                return;

            if (((TabItem)tbcDlog.SelectedItem).Header.ToString() == "Device Data" || refresh)
            {
                foreach (var item in list)
                {
                    byte siteNum = item.SiteNumber;
                    SolidColorBrush color = item.isSuccess ? Brushes.Green : Brushes.Red;
                    Label lblSite = GetControlObject<Label>("lblSite" + siteNum);
                    lblSite.Background = color;
                    Label lblSBin = GetControlObject<Label>("lblSBin" + siteNum);
                    lblSBin.Background = color;
                    lblSBin.Content = item.SoftBin.BinNum;
                    Label lblHBin = GetControlObject<Label>("lblHBin" + siteNum);
                    lblHBin.Background = color;
                    lblHBin.Content = item.HardBin.BinNum;
                    Label lblTime = GetControlObject<Label>("lblTime" + siteNum);
                    lblTime.Background = color;
                    lblTime.Content = item.Duration;
                }
            }
        }

        private T GetControlObject<T>(string controlName)
        {
            try
            {
                Type type = this.GetType();
                FieldInfo fieldInfo = type.GetField(controlName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (fieldInfo != null)
                {
                    T obj = (T)fieldInfo.GetValue(this);
                    return obj;
                }
                else
                {
                    return default(T);
                }
            }
            catch (Exception)
            {
                return default(T);
            }
        }

        private void UpdateStatus(object obj)
        {
            try
            {
                if (grdLocation.Visibility == Visibility.Visible)
                {
                    grdLocation.Dispatcher.Invoke(() => grdLocation.Visibility = Visibility.Collapsed);
                    grdStatus.Dispatcher.Invoke(() => grdStatus.Visibility = Visibility.Visible);
                }

                var status = (Tuple<string, int, int>)obj;
                for (int i = status.Item2; i <= status.Item3; i++)
                {
                    lblStatus.Dispatcher.Invoke(() => lblStatus.Content = status.Item1);
                    pgbStatus.Dispatcher.Invoke(() => pgbStatus.Value = i);
                    if (i == 100)
                    {
                        grdLocation.Dispatcher.Invoke(() => grdLocation.Visibility = Visibility.Visible);
                        grdStatus.Dispatcher.Invoke(() => grdStatus.Visibility = Visibility.Collapsed);
                    }
                    else
                    {
                        Thread.Sleep(2);
                    }
                }
            }
            catch
            {
                grdLocation.Dispatcher.Invoke(() => grdLocation.Visibility = Visibility.Visible);
                grdStatus.Dispatcher.Invoke(() => grdStatus.Visibility = Visibility.Collapsed);
            }
        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                List<DisplayTestResult> displayTestResults;
                List<PartResult> partResults;
                List<int> binning;

                while (!backgroundWorker.CancellationPending)
                {
                    handle.WaitOne();
                    if (isManual)
                    {
                        for (int i = 0; i < loop; i++)
                        {
                            if (!backgroundWorker.CancellationPending && isManual)
                            {
                                displayTestResults = flowService.Start();
                                partResults = flowService.ResultParser(flowService.ResultParser(displayTestResults), ref partID);
                                dataService.SaveDetails(partResults);
                                backgroundWorker.ReportProgress(1, Tuple.Create(displayTestResults, partResults));
                                lastRoundTestResults = displayTestResults;
                                lastRoundPartResults = partResults;
                                Thread.Sleep(indextime);
                            }
                            else
                            {
                                break;
                            }
                        }
                        backgroundWorker.ReportProgress(3);
                    }
                    else
                    {
                        if (!backgroundWorker.CancellationPending)
                        {
                            displayTestResults = flowService.Start();
                            partResults = flowService.ResultParser(flowService.ResultParser(displayTestResults), ref partID);
                            dataService.SaveDetails(partResults);
                            backgroundWorker.ReportProgress(1, Tuple.Create(displayTestResults, partResults));
                            lastRoundTestResults = displayTestResults;
                            lastRoundPartResults = partResults;
                            Thread.Sleep(100);
                            binning = new List<int>();
                            partResults.ForEach(x => binning.Add(x.HardBin.BinNum));
                            handlerService.EOT(binning);
                        }
                    }
                }
            }
            finally
            {
                Release();
            }
        }

        private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == 3)
            {
                RefreshDataGridTestResult(lastRoundTestResults, true);
                RefreshGridTestResult(lastRoundPartResults, true);
                RefreshYield(true);
                RefreshDataGridBinResult(true);
                isManual = false;
                btnManualTest.IsEnabled = true;
            }
            else
            {
                var res = (Tuple<List<DisplayTestResult>, List<PartResult>>)e.UserState;

                RefreshDataGridTestResult(res.Item1);

                RefreshGridTestResult(res.Item2);

                fullPartResult.AddRange(res.Item2);

                RefreshYield();

                RefreshDataGridBinResult();
            }
        }

        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show("System error: " + e.Error.Message + " Please contact the developer!");
                uiService.SaveUILog("StartLot", new MTResponse() { ResponseStatus = ResponseStatus.Fail, Message = "backgroundWorker_DoWork : " + e.Error.Message });
                SetButtonSatus(ButtonStatus.status_s0);
            }
            else
            {
                try
                {
                    if (from == "btnSaveTestResult_Click")
                    {
                        SetButtonSatus(ButtonStatus.status_s0);

                        if (fullPartResult.Count <= 0)
                        {
                            SetButtonSatus(ButtonStatus.status_s3);
                            //imgLOGO.Visibility = Visibility.Visible;
                            //gpbManualSettings.Visibility = Visibility.Collapsed;
                            //chkManual.IsChecked = false;
                            return;
                        }

                        Thread threadStatus = new Thread(new ParameterizedThreadStart(UpdateStatus));
                        threadStatus.Start(Tuple.Create("正在汇总并存储数据...", 1, 50));
                        dataService.SaveSummary(fullPartResult, limits);
                        uiService.SaveUILog("SaveTestResult", new MTResponse() { ResponseStatus = ResponseStatus.Processing, Message = "backgroundWorker_RunWorkerCompleted : SaveSummary done" });

                        threadStatus.Abort();
                        threadStatus = new Thread(new ParameterizedThreadStart(UpdateStatus));
                        threadStatus.Start(Tuple.Create("正在存储config文件...", 51, 100));
                        dataService.SaveConfig();
                        uiService.SaveUILog("SaveTestResult", new MTResponse() { ResponseStatus = ResponseStatus.Processing, Message = "backgroundWorker_RunWorkerCompleted : SaveConfig done" });

                        RefreshDataGridTestResult(lastRoundTestResults, true);
                        RefreshGridTestResult(lastRoundPartResults, true);
                        RefreshYield(true);
                        RefreshDataGridBinResult(true);

                        if (!string.IsNullOrEmpty(lotInfo.TestCode.ToString())
                            && (lotInfo.TestCode.ToString().StartsWith("P")
                            || lotInfo.TestCode.ToString().StartsWith("R")
                            || lotInfo.TestCode.ToString().StartsWith("p")
                            || lotInfo.TestCode.ToString().StartsWith("r")))
                            fulllot.Add(lotInfo, fullPartResult);

                        SetButtonSatus(ButtonStatus.status_s3);
                        //imgLOGO.Visibility = Visibility.Visible;
                        //gpbManualSettings.Visibility = Visibility.Collapsed;
                        //chkManual.IsChecked = false;
                        grdLocation.Visibility = Visibility.Visible;
                        grdStatus.Visibility = Visibility.Collapsed;

                        uiService.SaveUILog("SaveTestResult", new MTResponse() { ResponseStatus = ResponseStatus.Success, Message = "backgroundWorker_RunWorkerCompleted : SaveTestResult success" });
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("System error: " + e.Error.Message + " Please contact the developer!");
                    uiService.SaveUILog("SaveTestResult", new MTResponse() { ResponseStatus = ResponseStatus.Fail, Message = "backgroundWorker_RunWorkerCompleted : " + ex.Message });
                    SetButtonSatus(ButtonStatus.status_s0);
                    //imgLOGO.Visibility = Visibility.Visible;
                    //gpbManualSettings.Visibility = Visibility.Collapsed;
                    //chkManual.IsChecked = false;
                    grdLocation.Visibility = Visibility.Visible;
                    grdStatus.Visibility = Visibility.Collapsed;
                }
            }
        }
        #endregion
    }
}
