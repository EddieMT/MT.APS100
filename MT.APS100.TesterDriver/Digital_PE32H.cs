using System;
using System.Runtime.InteropServices;

namespace MT.APS100.TesterDriver
{
    public class pe32h : object //, System.IDisposable
    {
        private bool writeToConsole = false;

        //private System.Runtime.InteropServices.HandleRef _handle;

        //private bool _disposed = true;

        ~pe32h() { /*Dispose(false);*/  }

        //public pe32h(System.IntPtr Instrument_Handle)
        //{
        //    this._handle = new System.Runtime.InteropServices.HandleRef(this, Instrument_Handle);
        //    //this._disposed = false;
        //}

        public pe32h(bool verbose)
        {
            writeToConsole = verbose;
        }

        public int Initialize()
        {
            int status = -1;
            //long lstatus = -1;
            int serialNo = 0; // Digital module daughter board serial number

            if (writeToConsole) Console.WriteLine("Digital module is booting...");

            status = api();
            status = init();

            if (status <= 0)
            {
                if (writeToConsole) Console.WriteLine("Digital module did not boot.\n", serialNo);
                return status;
            }

            reset(1);
            cal_reset(1);
            status = cal_load_auto(1, "C://MerlinTest//System//Calibration//OpenATE//");
            set_pxi(1, 0);
            //long lstatus = lmload(1, 1, 0, filePath);

            serialNo = rd_pesno(1);
            if (writeToConsole) Console.WriteLine("Digital module serial number: {0}\n", serialNo);

            // Set channelformat to NRZ
            set_rz(1, 1, 0x00000000);
            set_ro(1, 1, 0x00000000);

            //-------------------- Timing --------------------
            /*
            long Ts = 8; // MIPI 25MHz
            long edge1 = 0;
            long edge2 = 0;

            // Period resolution is 5ns (Ts = 8 is 40ns or 25MHz)
            Digital.set_tp(1, 1, Ts);

            // CLK: start = 5ns, stop = 25ns
            edge1 = 1;
            edge2 = 51;
            set_tstart(1, 1, 1, edge1);
            set_tstop(1, 1, 1, edge2);

            // SDATA: start = 0ns, stop = 40ns
            edge1 = 0;
            edge2 = Ts;
            set_tstart(1, 2, 1, edge1);
            set_tstop(1, 2, 1, edge2);

            // VIO: start = 0ns, stop = 40ns
            edge1 = 0;
            edge2 = Ts;
            set_tstart(1, 3, 1, edge1);
            set_tstop(1, 3, 1, edge2);

            // TRIG: start = 0ns, stop = 40ns
            edge1 = 0;
            edge2 = Ts;
            set_tstart(1, 5, 1, edge1);
            set_tstop(1, 5, 1, edge2);

            //-------------------- Levels --------------------

            // CLK
            set_vil(1, 1, 0.0);
            set_vih(1, 1, 1.8);

            // SDATA
            set_vil(1, 2, 0.0);
            set_vih(1, 2, 1.8);

            // VIO
            set_vil(1, 3, 0.0);
            set_vih(1, 3, 1.8);

            // TRIG
            set_vil(1, 5, 0.0);
            set_vih(1, 5, 3.0);

            //-------------------- Misc. --------------------

            set_logmode(1, 1);
            set_logmode(1, 1);
            set_addsyn(1, 0);
            pmufv(1, 0, 0, 10);
            con_pmu(1, 0, 0);
            */
            set_logmode(1, 1);
            set_addsyn(1, 0);
            pmufv(1, 0, 0.0, 10);//board number, chip number, voltage, clampi  ALL CHANNEL
            con_pmu(1, 0, 0);


            return 0;
        }



    // PE32H Sequencer Functions --------------------------------------------------

    public pe32h()
        {
            int pInvokeResult = PInvoke.init();
            PInvoke.TestForError(pInvokeResult);
            //this._disposed = false;
        }

        public int api()
        {
            int pInvokeResult = PInvoke.api();
            PInvoke.TestForError(pInvokeResult);
            return pInvokeResult;
        }

        public int init()
        {
            int pInvokeResult = PInvoke.init();
            PInvoke.TestForError(pInvokeResult);
            return pInvokeResult;
        }

        public void reset(int bdno)
        {
            PInvoke.reset(bdno);
        }

        public void set_ftcnt(int bdno, int cnt)
        {
            PInvoke.set_ftcnt(bdno, cnt);
        }

        public void set_addbeg(int bdno, int add)
        {
            PInvoke.set_addbeg(bdno, add);
        }

        public void set_addend(int bdno, int add)
        {
            PInvoke.set_addend(bdno, add);
        }

        public void set_addsyn(int bdno, int add)
        {
            PInvoke.set_addsyn(bdno, add);
        }

        public void fstart(int bdno, int onoff)
        {
            PInvoke.fstart(bdno, onoff);
        }

        public int check_tprun(int bdno)
        {
            int pInvokeResult = PInvoke.check_tprun(bdno);
            PInvoke.TestForError(pInvokeResult);
            return pInvokeResult;
        }

        public int check_tpass(int bdno)
        {
            int pInvokeResult = PInvoke.check_tpass(bdno);
            PInvoke.TestForError(pInvokeResult);
            return pInvokeResult;
        }

        public void cycle(int bdno, int onoff)
        {
            PInvoke.cycle(bdno, onoff);
        }

        public void set_pxi(int bdno, int data)
        {
            PInvoke.set_pxi(bdno, data);
        }

        public uint rd_fccnt(int bdno)
        {
            uint pInvokeResult = PInvoke.rd_fccnt(bdno);
            PInvoke.TestForError((int)pInvokeResult);
            return pInvokeResult;
        }

        public void set_checkmode(int bdno, int onoff)
        {
            PInvoke.set_checkmode(bdno, onoff);
        }

        public void set_logmode(int bdno, int onoff)
        {
            PInvoke.set_logmode(bdno, onoff);
        }

        // PE32H Formatter Functions --------------------------------------------------

        public void set_tp(int bdno, int ts, int data)
        {
            PInvoke.set_tp(bdno, ts, data);
        }

        public void set_tstart(int bdno, int pno, int ts, int data)
        {
            PInvoke.set_tstart(bdno, pno, ts, data);
        }

        public void set_tstop(int bdno, int pno, int ts, int data)
        {
            PInvoke.set_tstop(bdno, pno, ts, data);
        }

        public void set_rz(int bdno, int fs, int data)
        {
            PInvoke.set_rz(bdno, fs, data);
        }

        public void set_ro(int bdno, int fs, int data)
        {
            PInvoke.set_ro(bdno, fs, data);
        }

        public int lmload(int begbdno, int boardwidth, int begadd, string patternfile)
        {
            int pInvokeResult = PInvoke.lmload(begbdno, boardwidth, begadd, patternfile);
            PInvoke.TestForError((int)pInvokeResult);
            return pInvokeResult;
        }

        // PE32H Pin Electronics Functions --------------------------------------------------

        public void set_vih(int bdno, int pno, double rv)
        {
            PInvoke.set_vih(bdno, pno, rv);
        }

        public void set_vil(int bdno, int pno, double rv)
        {
            PInvoke.set_vil(bdno, pno, rv);
        }

        public void set_voh(int bdno, int pno, double rv)
        {
            PInvoke.set_voh(bdno, pno, rv);
        }

        public void set_vol(int bdno, int pno, double rv)
        {
            PInvoke.set_vol(bdno, pno, rv);
        }

        public void pmufv(int bdno, int chip, double rv, double clampi)
        {
            PInvoke.pmufv(bdno, chip, rv, clampi);
        }

        public void pmufi(int bdno, int chip, double ri, double cvh, double cvl)
        {
            PInvoke.pmufi(bdno, chip, ri, cvh, cvl);
        }

        public void pmufir(int bdno, int cno, double ri, double cvh, double cvl, int rang)
        {
            PInvoke.pmufir(bdno, cno, ri, cvh, cvl, rang);
        }

        public void pmucv(int bdno, int cno, double cvh, double cvl)
        {
            PInvoke.pmucv(bdno, cno, cvh, cvl);
        }

        public void pmuci(int bdno, int cno, double cih, double cil)
        {
            PInvoke.pmuci(bdno, cno, cih, cil);
        }

        public void con_pmu(int bdno, int pno, int onoff)
        {
            PInvoke.con_pmu(bdno, pno, onoff);
        }

        public void con_pmus(int bdno, int pno, int onoff)
        {
            PInvoke.con_pmus(bdno, pno, onoff);
        }

        public void cpu_df(int bdno, int pno, int donoff, int fonoff)
        {
            PInvoke.cpu_df(bdno, pno, donoff, fonoff);
        }

        public void check_pmu(int bdno, int cno)
        {
            PInvoke.check_pmu(bdno, cno);
        }

        public double vmeas(int bdno, int pno)
        {
            return PInvoke.vmeas(bdno, pno);
        }

        public double imeas(int bdno, int pno)
        {
            return PInvoke.imeas(bdno, pno);
        }

        // PE32H Calibration Functions --------------------------------------------------

        public int cal_load(int bdno, string calfile)
        {
            int pInvokeResult = PInvoke.cal_load(bdno, calfile);
            PInvoke.TestForError(pInvokeResult);
            return pInvokeResult;
        }

        public int cal_load_auto(int bdno, string calfile)
        {
            int pInvokeResult = PInvoke.cal_load_auto(bdno, calfile);
            PInvoke.TestForError(pInvokeResult);
            return pInvokeResult;
        }

        public void cal_reset(int bdno)
        {
            PInvoke.cal_reset(bdno);
        }

        // PE32H User I/O Functions --------------------------------------------------

        public int rd_pesno(int bdno)
        {
            int pInvokeResult = PInvoke.rd_pesno(bdno);
            PInvoke.TestForError(pInvokeResult);
            return pInvokeResult;
        }

        public double get_temp(int bdno, int cno)
        {
            double pInvokeResult = PInvoke.get_temp(bdno, cno);
            //PInvoke.TestForError(pInvokeResult);
            return pInvokeResult;
        }

        

        private class PInvoke
        {
            // PE32H Sequencer Functions --------------------------------------------------

            [DllImport("PE32.dll", EntryPoint = "pe32_api", CallingConvention = CallingConvention.Cdecl)]
            public static extern int api();

            [DllImport("PE32.dll", EntryPoint = "pe32_init", CallingConvention = CallingConvention.Cdecl)]
            public static extern int init();

            [DllImport("PE32.dll", EntryPoint = "pe32_reset", CallingConvention = CallingConvention.Cdecl)]
            public static extern void reset(int bdno);

            [DllImport("PE32.dll", EntryPoint = "pe32_set_ftcnt", CallingConvention = CallingConvention.Cdecl)]
            public static extern void set_ftcnt(int bdno, int cnt);

            [DllImport("PE32.dll", EntryPoint = "pe32_set_addbeg", CallingConvention = CallingConvention.Cdecl)]
            public static extern void set_addbeg(int bdno, int add);

            [DllImport("PE32.dll", EntryPoint = "pe32_set_addend", CallingConvention = CallingConvention.Cdecl)]
            public static extern void set_addend(int bdno, int add);

            [DllImport("PE32.dll", EntryPoint = "pe32_set_addsyn", CallingConvention = CallingConvention.Cdecl)]
            public static extern void set_addsyn(int bdno, int add);

            [DllImport("PE32.dll", EntryPoint = "pe32_fstart", CallingConvention = CallingConvention.Cdecl)]
            public static extern void fstart(int bdno, int onoff);

            [DllImport("PE32.dll", EntryPoint = "pe32_check_tprun", CallingConvention = CallingConvention.Cdecl)]
            public static extern int check_tprun(int bdno);

            [DllImport("PE32.dll", EntryPoint = "pe32_check_tpass", CallingConvention = CallingConvention.Cdecl)]
            public static extern int check_tpass(int bdno);

            [DllImport("PE32.dll", EntryPoint = "pe32_cycle", CallingConvention = CallingConvention.Cdecl)]
            public static extern void cycle(int bdno, int onoff);

            [DllImport("PE32.dll", EntryPoint = "pe32_set_pxi", CallingConvention = CallingConvention.Cdecl)]
            public static extern void set_pxi(int bdno, int data);

            [DllImport("PE32.dll", EntryPoint = "pe32_rd_fccnt", CallingConvention = CallingConvention.Cdecl)]
            public static extern uint rd_fccnt(int bdno);

            [DllImport("PE32.dll", EntryPoint = "pe32_set_checkmode", CallingConvention = CallingConvention.Cdecl)]
            public static extern void set_checkmode(int bdno, int onoff);

            [DllImport("PE32.dll", EntryPoint = "pe32_set_logmode", CallingConvention = CallingConvention.Cdecl)]
            public static extern void set_logmode(int bdno, int onoff);

            // PE32H Formatter Functions --------------------------------------------------

            [DllImport("PE32.dll", EntryPoint = "pe32_set_tp", CallingConvention = CallingConvention.Cdecl)]
            public static extern void set_tp(int bdno, int ts, int data);

            [DllImport("PE32.dll", EntryPoint = "pe32_set_tstart", CallingConvention = CallingConvention.Cdecl)]
            public static extern void set_tstart(int bdno, int pno, int ts, int data);

            [DllImport("PE32.dll", EntryPoint = "pe32_set_tstop", CallingConvention = CallingConvention.Cdecl)]
            public static extern void set_tstop(int bdno, int pno, int ts, int data);

            [DllImport("PE32.dll", EntryPoint = "pe32_set_rz", CallingConvention = CallingConvention.Cdecl)]
            public static extern void set_rz(int bdno, int fs, int data);

            [DllImport("PE32.dll", EntryPoint = "pe32_set_ro", CallingConvention = CallingConvention.Cdecl)]
            public static extern void set_ro(int bdno, int fs, int data);

            [DllImport("PE32.dll", EntryPoint = "pe32_lmload", CallingConvention = CallingConvention.Cdecl)]
            public static extern int lmload(int begbdno, int boardwidth, int begadd, string patternfile);

            // PE32H Pin Electronics Functions --------------------------------------------------

            [DllImport("PE32.dll", EntryPoint = "pe32_set_vih", CallingConvention = CallingConvention.Cdecl)]
            public static extern void set_vih(int bdno, int pno, double rv);

            [DllImport("PE32.dll", EntryPoint = "pe32_set_vil", CallingConvention = CallingConvention.Cdecl)]
            public static extern void set_vil(int bdno, int pno, double rv);

            [DllImport("PE32.dll", EntryPoint = "pe32_set_voh", CallingConvention = CallingConvention.Cdecl)]
            public static extern void set_voh(int bdno, int pno, double rv);

            [DllImport("PE32.dll", EntryPoint = "pe32_set_vol", CallingConvention = CallingConvention.Cdecl)]
            public static extern void set_vol(int bdno, int pno, double rv);

            [DllImport("PE32.dll", EntryPoint = "pe32_pmufv", CallingConvention = CallingConvention.Cdecl)]
            public static extern void pmufv(int bdno, int chip, double rv, double clampi);

            [DllImport("PE32.dll", EntryPoint = "pe32_pmufi", CallingConvention = CallingConvention.Cdecl)]
            public static extern void pmufi(int bdno, int chip, double ri, double cvh, double cvl);

            [DllImport("PE32.dll", EntryPoint = "pe32_pmufir", CallingConvention = CallingConvention.Cdecl)]
            public static extern void pmufir(int bdno, int cno, double ri, double cvh, double cvl, int rang);

            [DllImport("PE32.dll", EntryPoint = "pe32_pmucv", CallingConvention = CallingConvention.Cdecl)]
            public static extern void pmucv(int bdno, int cno, double cvh, double cvl);

            [DllImport("PE32.dll", EntryPoint = "pe32_pmuci", CallingConvention = CallingConvention.Cdecl)]
            public static extern void pmuci(int bdno, int cno, double cih, double cil);

            [DllImport("PE32.dll", EntryPoint = "pe32_con_pmu", CallingConvention = CallingConvention.Cdecl)]
            public static extern void con_pmu(int bdno, int pno, int onoff);

            [DllImport("PE32.dll", EntryPoint = "pe32_con_pmus", CallingConvention = CallingConvention.Cdecl)]
            public static extern void con_pmus(int bdno, int pno, int onoff);

            [DllImport("PE32.dll", EntryPoint = "pe32_cpu_df", CallingConvention = CallingConvention.Cdecl)]
            public static extern void cpu_df(int bdno, int pno, int donoff, int fonoff);

            [DllImport("PE32.dll", EntryPoint = "pe32_check_pmu", CallingConvention = CallingConvention.Cdecl)]
            public static extern void check_pmu(int bdno, int cno);

            [DllImport("PE32.dll", EntryPoint = "pe32_vmeas", CallingConvention = CallingConvention.Cdecl)]
            public static extern double vmeas(int bdno, int pno);

            [DllImport("PE32.dll", EntryPoint = "pe32_imeas", CallingConvention = CallingConvention.Cdecl)]
            public static extern double imeas(int bdno, int pno);

            // PE32H Calibration Functions --------------------------------------------------

            [DllImport("PE32.dll", EntryPoint = "pe32_cal_load", CallingConvention = CallingConvention.Cdecl)]
            public static extern int cal_load(int bdno, string calfile);

            [DllImport("PE32.dll", EntryPoint = "pe32_cal_load_auto", CallingConvention = CallingConvention.Cdecl)]
            public static extern int cal_load_auto(int bdno, string calfile);

            [DllImport("PE32.dll", EntryPoint = "pe32_cal_reset", CallingConvention = CallingConvention.Cdecl)]
            public static extern void cal_reset(int bdno);

            // PE32H User I/O Functions --------------------------------------------------

            [DllImport("PE32.dll", EntryPoint = "pe32_rd_pesno", CallingConvention = CallingConvention.Cdecl)]
            public static extern int rd_pesno(int bdno);

            [DllImport("PE32.dll", EntryPoint = "pe32_get_temp", CallingConvention = CallingConvention.Cdecl)]
            public static extern double get_temp(int bdno, int cno);



            public static int TestForError(int status)
            {
                if (status < 0)
                {
                    //PInvoke.ThrowError(status);
                    Console.WriteLine("ERROR: Improper digital module command usage!");
                }
                return status;
            }

            //public static int ThrowError(System.Runtime.InteropServices.HandleRef handle, int code)
            //{
            //    System.Text.StringBuilder msg = new System.Text.StringBuilder(256);
            //    PInvoke.error_message(handle, code, msg);
            //    throw new System.Runtime.InteropServices.ExternalException(msg.ToString(), code);
            //}
        }
    }

    public class PE32H_Constants
    {

    }
}
