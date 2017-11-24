using System;

namespace MT.APS100.TesterDriver
{
    public class SwitchMatrix
    {
        private pe32h Digital = null;
        private bool writeToConsole = false;

        public SwitchMatrix(pe32h Digital_Handle)
        {
            Digital = Digital_Handle;
        }

        public SwitchMatrix(pe32h Digital_Handle, bool verbose)
        {
            Digital = Digital_Handle;
            writeToConsole = verbose;
        }

#if false // Switch Matrix v0.0
        public void Initialize()
        {
            // SW1, SW2, SW3, SW4 power levels
            Digital.set_vih(1, 32, 3.3);
            Digital.set_vil(1, 32, 0.0);

            Digital.cpu_df(1, 32, 1, 1); // Power on all switches

            // SW1 control levels
            Digital.set_vih(1, 24, 3.0);
            Digital.set_vil(1, 24, 0.0);

            // SW2 control levels
            Digital.set_vih(1, 26, 3.0);
            Digital.set_vil(1, 26, 0.0);

            Digital.set_vih(1, 28, 3.0);
            Digital.set_vil(1, 28, 0.0);

            // SW3 control levels
            Digital.set_vih(1, 25, 3.0);
            Digital.set_vil(1, 25, 0.0);

            Digital.set_vih(1, 27, 3.0);
            Digital.set_vil(1, 27, 0.0);

            Digital.set_vih(1, 29, 3.0);
            Digital.set_vil(1, 29, 0.0);

            Digital.set_vih(1, 31, 3.0);
            Digital.set_vil(1, 31, 0.0);

            // SW4 control levels
            Digital.set_vih(1, 30, 3.0);
            Digital.set_vil(1, 30, 0.0);

            // Initialize switch control states
            Digital.cpu_df(1, 24, 1, 0); // SW1_CTRL
            Digital.cpu_df(1, 26, 1, 0); // SW2_V1
            Digital.cpu_df(1, 28, 1, 0); // SW2_V2
            Digital.cpu_df(1, 25, 1, 0); // SW3_V4
            Digital.cpu_df(1, 27, 1, 0); // SW3_V3
            Digital.cpu_df(1, 29, 1, 0); // SW3_V2
            Digital.cpu_df(1, 31, 1, 0); // SW3_V1
            Digital.cpu_df(1, 30, 1, 0); // SW4_CTRL
        }

        public void ConnectSigGen(string srcSelect)
        {
            if (srcSelect == "SG1" || srcSelect == "SG2")
            {
                Digital.cpu_df(1, 24, 1, 0); // SW1_CTRL
            }
            else if (srcSelect == "Noise")
            {
                Digital.cpu_df(1, 24, 1, 1); // SW1_CTRL
            }
            else // Error
            {
                if (writeToConsole) Console.WriteLine("ERROR: Invalid RF source connection!");
            }
        }

        public void ConnectDigitizer(string measSelect)
        {
            if (measSelect == "D1")
            {
                Digital.cpu_df(1, 30, 1, 0); // SW4_CTRL
            }
            else if (measSelect == "CAL")
            {
                Digital.cpu_df(1, 30, 1, 1); // SW4_CTRL
            }
            else // Error
            {
                if (writeToConsole) Console.WriteLine("ERROR: Invalid RF measure connection!");
            }
        }

        public void ConnectSourcePath(string outSelect)
        {
            if (outSelect == "S1")
            {
                Digital.cpu_df(1, 26, 1, 0); // SW2_V1
                Digital.cpu_df(1, 28, 1, 0); // SW2_V2
            }
            else if (outSelect == "S2")
            {
                Digital.cpu_df(1, 26, 1, 1); // SW2_V1
                Digital.cpu_df(1, 28, 1, 1); // SW2_V2
            }
            else if (outSelect == "S3")
            {
                Digital.cpu_df(1, 26, 1, 0); // SW2_V1
                Digital.cpu_df(1, 28, 1, 1); // SW2_V2
            }
            else if (outSelect == "S4" || outSelect == "CAL")
            {
                Digital.cpu_df(1, 26, 1, 1); // SW2_V1
                Digital.cpu_df(1, 28, 1, 0); // SW2_V2
            }
            else // Error
            {
                if (writeToConsole) Console.WriteLine("ERROR: Invalid RF source connection!");
            }
        }

        public void ConnectMeasurePath(string inSelect)
        {
            if (inSelect == "M1")
            {
                Digital.cpu_df(1, 25, 1, 0); // SW3_V4
                Digital.cpu_df(1, 27, 1, 0); // SW3_V3
                Digital.cpu_df(1, 29, 1, 0); // SW3_V2
                Digital.cpu_df(1, 31, 1, 0); // SW3_V1
            }
            else if (inSelect == "M2")
            {
                Digital.cpu_df(1, 25, 1, 1); // SW3_V4
                Digital.cpu_df(1, 27, 1, 0); // SW3_V3
                Digital.cpu_df(1, 29, 1, 0); // SW3_V2
                Digital.cpu_df(1, 31, 1, 0); // SW3_V1
            }
            else if (inSelect == "M3")
            {
                Digital.cpu_df(1, 25, 1, 0); // SW3_V4
                Digital.cpu_df(1, 27, 1, 1); // SW3_V3
                Digital.cpu_df(1, 29, 1, 0); // SW3_V2
                Digital.cpu_df(1, 31, 1, 0); // SW3_V1
            }
            else if (inSelect == "M4")
            {
                Digital.cpu_df(1, 25, 1, 1); // SW3_V4
                Digital.cpu_df(1, 27, 1, 1); // SW3_V3
                Digital.cpu_df(1, 29, 1, 0); // SW3_V2
                Digital.cpu_df(1, 31, 1, 0); // SW3_V1
            }
            else if (inSelect == "M5")
            {
                Digital.cpu_df(1, 25, 1, 0); // SW3_V4
                Digital.cpu_df(1, 27, 1, 0); // SW3_V3
                Digital.cpu_df(1, 29, 1, 1); // SW3_V2
                Digital.cpu_df(1, 31, 1, 0); // SW3_V1
            }
            else if (inSelect == "M6")
            {
                Digital.cpu_df(1, 25, 1, 1); // SW3_V4
                Digital.cpu_df(1, 27, 1, 0); // SW3_V3
                Digital.cpu_df(1, 29, 1, 1); // SW3_V2
                Digital.cpu_df(1, 31, 1, 0); // SW3_V1
            }
            else if (inSelect == "M7")
            {
                Digital.cpu_df(1, 25, 1, 0); // SW3_V4
                Digital.cpu_df(1, 27, 1, 1); // SW3_V3
                Digital.cpu_df(1, 29, 1, 1); // SW3_V2
                Digital.cpu_df(1, 31, 1, 0); // SW3_V1
            }
            else if (inSelect == "M8")
            {
                Digital.cpu_df(1, 25, 1, 1); // SW3_V4
                Digital.cpu_df(1, 27, 1, 1); // SW3_V3
                Digital.cpu_df(1, 29, 1, 1); // SW3_V2
                Digital.cpu_df(1, 31, 1, 0); // SW3_V1
            }
            else if (inSelect == "M9")
            {
                Digital.cpu_df(1, 25, 1, 0); // SW3_V4
                Digital.cpu_df(1, 27, 1, 0); // SW3_V3
                Digital.cpu_df(1, 29, 1, 0); // SW3_V2
                Digital.cpu_df(1, 31, 1, 1); // SW3_V1
            }
            else if (inSelect == "M10")
            {
                Digital.cpu_df(1, 25, 1, 1); // SW3_V4
                Digital.cpu_df(1, 27, 1, 0); // SW3_V3
                Digital.cpu_df(1, 29, 1, 0); // SW3_V2
                Digital.cpu_df(1, 31, 1, 1); // SW3_V1
            }
            else if (inSelect == "M11")
            {
                Digital.cpu_df(1, 25, 1, 0); // SW3_V4
                Digital.cpu_df(1, 27, 1, 1); // SW3_V3
                Digital.cpu_df(1, 29, 1, 0); // SW3_V2
                Digital.cpu_df(1, 31, 1, 1); // SW3_V1
            }
            else if (inSelect == "M12")
            {
                Digital.cpu_df(1, 25, 1, 1); // SW3_V4
                Digital.cpu_df(1, 27, 1, 1); // SW3_V3
                Digital.cpu_df(1, 29, 1, 0); // SW3_V2
                Digital.cpu_df(1, 31, 1, 1); // SW3_V1
            }
            else // Error
            {
                if (writeToConsole) Console.WriteLine("ERROR: Invalid RF measure connection!");
            }
        }
#endif

#if false // Switch Matrix v1.0 (PCB-000-000-001 REV -)  !!!***PCB MODE***!!!
        public void Initialize()
        {
            // SW1 - S7 power levels
            Digital.set_vih(1, 20, 3.3);
            Digital.set_vil(1, 20, 0.0);

            // SW1 control levels
            Digital.set_vih(1, 21, 3.0); // SW1_CTRL
            Digital.set_vil(1, 21, 0.0); // SW1_CTRL

            // SW2 control levels
            Digital.set_vih(1, 22, 3.0); // SW2_V1
            Digital.set_vil(1, 22, 0.0); // SW2_V1

            Digital.set_vih(1, 23, 3.0); // SW2_V2
            Digital.set_vil(1, 23, 0.0); // SW2_V2

            // SW3 control levels
            Digital.set_vih(1, 24, 3.0); // SW3_CTRL
            Digital.set_vil(1, 24, 0.0); // SW3_CTRL

            // SW4 control levels
            Digital.set_vih(1, 25, 3.0); // SW4_V1
            Digital.set_vil(1, 25, 0.0); // SW4_V1

            Digital.set_vih(1, 26, 3.0); // SW4_V2
            Digital.set_vil(1, 26, 0.0); // SW4_V2

            // SW5 control levels
            Digital.set_vih(1, 27, 3.0); // SW5_V1
            Digital.set_vil(1, 27, 0.0); // SW5_V1

            Digital.set_vih(1, 28, 3.0); // SW5_V2
            Digital.set_vil(1, 28, 0.0); // SW5_V2

            // SW6 control levels
            Digital.set_vih(1, 29, 3.0); // SW6_V1
            Digital.set_vil(1, 29, 0.0); // SW6_V1

            Digital.set_vih(1, 30, 3.0); // SW6_V2
            Digital.set_vil(1, 30, 0.0); // SW6_V2

            // SW7 control levels
            Digital.set_vih(1, 31, 3.0); // SW7_V1
            Digital.set_vil(1, 31, 0.0); // SW7_V1

            Digital.set_vih(1, 32, 3.0); // SW7_V2
            Digital.set_vil(1, 32, 0.0); // SW7_V2

            // Initialize switch control states
            Digital.cpu_df(1, 20, 1, 1); // SW1 - SW7 power
            Digital.cpu_df(1, 21, 1, 0); // SW1_CTRL
            Digital.cpu_df(1, 22, 1, 0); // SW2_V1
            Digital.cpu_df(1, 23, 1, 0); // SW2_V2
            Digital.cpu_df(1, 24, 1, 0); // SW3_CTRL
            Digital.cpu_df(1, 25, 1, 0); // SW4_V1
            Digital.cpu_df(1, 26, 1, 0); // SW4_V2
            Digital.cpu_df(1, 27, 1, 0); // SW5_V1
            Digital.cpu_df(1, 28, 1, 0); // SW5_V2
            Digital.cpu_df(1, 29, 1, 0); // SW6_V1
            Digital.cpu_df(1, 30, 1, 0); // SW6_V2
            Digital.cpu_df(1, 31, 1, 0); // SW7_V1
            Digital.cpu_df(1, 32, 1, 0); // SW7_V2
        }

        public void ConnectSigGen(string srcSelect)
        {
            if (srcSelect == "SG1" || srcSelect == "SG2")
            {
                Digital.cpu_df(1, 21, 1, 0); // SW1_CTRL
            }
            else if (srcSelect == "Noise")
            {
                Digital.cpu_df(1, 21, 1, 1); // SW1_CTRL
            }
            else // Error
            {
                if (writeToConsole) Console.WriteLine("ERROR: Invalid RF source connection!");
            }
        }

        public void ConnectSourcePath(string outSelect)
        {
            if (outSelect == "S1")
            {
                Digital.cpu_df(1, 22, 1, 0); // SW2_V1
                Digital.cpu_df(1, 23, 1, 0); // SW2_V2

                Digital.cpu_df(1, 24, 1, 0); // SW3_CTRL
            }
            else if (outSelect == "S2")
            {
                Digital.cpu_df(1, 22, 1, 1); // SW2_V1
                Digital.cpu_df(1, 23, 1, 1); // SW2_V2

                Digital.cpu_df(1, 24, 1, 0); // SW3_CTRL
            }
            else if (outSelect == "S3")
            {
                Digital.cpu_df(1, 22, 1, 0); // SW2_V1
                Digital.cpu_df(1, 23, 1, 1); // SW2_V2

                Digital.cpu_df(1, 24, 1, 0); // SW3_CTRL
            }
            else if (outSelect == "S4")
            {
                Digital.cpu_df(1, 22, 1, 1); // SW2_V1
                Digital.cpu_df(1, 23, 1, 0); // SW2_V2

                Digital.cpu_df(1, 24, 1, 0); // SW3_CTRL
            }
            else if (outSelect == "CAL")
            {
                Digital.cpu_df(1, 22, 1, 1); // SW2_V1
                Digital.cpu_df(1, 23, 1, 0); // SW2_V2

                Digital.cpu_df(1, 24, 1, 1); // SW3_CTRL
            }
            else // Error
            {
                if (writeToConsole) Console.WriteLine("ERROR: Invalid RF source path connection!");
            }
        }

        public void ConnectMeasurePath(string measSelect)
        {
            if (measSelect == "M1")
            {
                Digital.cpu_df(1, 25, 1, 0); // SW4_V1
                Digital.cpu_df(1, 26, 1, 0); // SW4_V2

                Digital.cpu_df(1, 31, 1, 0); // SW7_V1
                Digital.cpu_df(1, 32, 1, 1); // SW7_V2
            }
            else if (measSelect == "M2")
            {
                Digital.cpu_df(1, 25, 1, 1); // SW4_V1
                Digital.cpu_df(1, 26, 1, 1); // SW4_V2

                Digital.cpu_df(1, 31, 1, 0); // SW7_V1
                Digital.cpu_df(1, 32, 1, 1); // SW7_V2
            }
            else if (measSelect == "M3")
            {
                Digital.cpu_df(1, 25, 1, 0); // SW4_V1
                Digital.cpu_df(1, 26, 1, 1); // SW4_V2

                Digital.cpu_df(1, 31, 1, 0); // SW7_V1
                Digital.cpu_df(1, 32, 1, 1); // SW7_V2
            }
            else if (measSelect == "M4")
            {
                Digital.cpu_df(1, 25, 1, 1); // SW4_V1
                Digital.cpu_df(1, 26, 1, 0); // SW4_V2

                Digital.cpu_df(1, 31, 1, 0); // SW7_V1
                Digital.cpu_df(1, 32, 1, 1); // SW7_V2
            }
            else if (measSelect == "M5")
            {
                Digital.cpu_df(1, 27, 1, 0); // SW5_V1
                Digital.cpu_df(1, 28, 1, 0); // SW5_V2

                Digital.cpu_df(1, 31, 1, 1); // SW7_V1
                Digital.cpu_df(1, 32, 1, 0); // SW7_V2
            }
            else if (measSelect == "M6")
            {
                Digital.cpu_df(1, 27, 1, 1); // SW5_V1
                Digital.cpu_df(1, 28, 1, 1); // SW5_V2

                Digital.cpu_df(1, 31, 1, 1); // SW7_V1
                Digital.cpu_df(1, 32, 1, 0); // SW7_V2
            }
            else if (measSelect == "M7")
            {
                Digital.cpu_df(1, 27, 1, 0); // SW5_V1
                Digital.cpu_df(1, 28, 1, 1); // SW5_V2

                Digital.cpu_df(1, 31, 1, 1); // SW7_V1
                Digital.cpu_df(1, 32, 1, 0); // SW7_V2
            }
            else if (measSelect == "M8")
            {
                Digital.cpu_df(1, 27, 1, 1); // SW5_V1
                Digital.cpu_df(1, 28, 1, 0); // SW5_V2

                Digital.cpu_df(1, 31, 1, 1); // SW7_V1
                Digital.cpu_df(1, 32, 1, 0); // SW7_V2
            }
            else if (measSelect == "M9")
            {
                Digital.cpu_df(1, 29, 1, 0); // SW6_V1
                Digital.cpu_df(1, 30, 1, 0); // SW6_V2

                Digital.cpu_df(1, 31, 1, 0); // SW7_V1
                Digital.cpu_df(1, 32, 1, 0); // SW7_V2
            }
            else if (measSelect == "M10")
            {
                Digital.cpu_df(1, 29, 1, 1); // SW6_V1
                Digital.cpu_df(1, 30, 1, 1); // SW6_V2

                Digital.cpu_df(1, 31, 1, 0); // SW7_V1
                Digital.cpu_df(1, 32, 1, 0); // SW7_V2
            }
            else if (measSelect == "M11")
            {
                Digital.cpu_df(1, 29, 1, 0); // SW6_V1
                Digital.cpu_df(1, 30, 1, 1); // SW6_V2

                Digital.cpu_df(1, 31, 1, 0); // SW7_V1
                Digital.cpu_df(1, 32, 1, 0); // SW7_V2
            }
            else if (measSelect == "M12")
            {
                Digital.cpu_df(1, 29, 1, 1); // SW6_V1
                Digital.cpu_df(1, 30, 1, 0); // SW6_V2

                Digital.cpu_df(1, 31, 1, 0); // SW7_V1
                Digital.cpu_df(1, 32, 1, 0); // SW7_V2
            }
            else if (measSelect == "CAL")
            {
                Digital.cpu_df(1, 31, 1, 1); // SW7_V1
                Digital.cpu_df(1, 32, 2, 1); // SW7_V2
            }
            else // Error
            {
                if (writeToConsole) Console.WriteLine("ERROR: Invalid RF measure path connection!");
            }
        }
#endif

#if true // Switch Matrix v1.0 (PCB-000-000-001 REV -)  !!!***BOX MODE***!!!
        public int Initialize()
        {
            try
            {
                // SW1 - SW7 power levels
                Digital.set_vih(1, 20, 3.3);
                Digital.set_vil(1, 20, 0.0);

                // SW1 control levels
                Digital.set_vih(1, 21, 3.0); // SW1_CTRL
                Digital.set_vil(1, 21, 0.0); // SW1_CTRL

                // SW2 control levels
                Digital.set_vih(1, 22, 3.0); // SW2_V1
                Digital.set_vil(1, 22, 0.0); // SW2_V1

                Digital.set_vih(1, 23, 3.0); // SW2_V2
                Digital.set_vil(1, 23, 0.0); // SW2_V2

                // SW3 control levels
                Digital.set_vih(1, 24, 3.0); // SW3_CTRL
                Digital.set_vil(1, 24, 0.0); // SW3_CTRL

                // SW4 control levels
                Digital.set_vih(1, 25, 3.0); // SW4_V1
                Digital.set_vil(1, 25, 0.0); // SW4_V1

                Digital.set_vih(1, 26, 3.0); // SW4_V2
                Digital.set_vil(1, 26, 0.0); // SW4_V2

                // SW5 control levels
                Digital.set_vih(1, 27, 3.0); // SW5_V1
                Digital.set_vil(1, 27, 0.0); // SW5_V1

                Digital.set_vih(1, 28, 3.0); // SW5_V2
                Digital.set_vil(1, 28, 0.0); // SW5_V2

                // SW6 control levels
                Digital.set_vih(1, 29, 3.0); // SW6_V1
                Digital.set_vil(1, 29, 0.0); // SW6_V1

                Digital.set_vih(1, 30, 3.0); // SW6_V2
                Digital.set_vil(1, 30, 0.0); // SW6_V2

                // SW7 control levels
                Digital.set_vih(1, 31, 3.0); // SW7_V1
                Digital.set_vil(1, 31, 0.0); // SW7_V1

                Digital.set_vih(1, 32, 3.0); // SW7_V2
                Digital.set_vil(1, 32, 0.0); // SW7_V2

                // Initialize switch control states
                Digital.cpu_df(1, 20, 1, 1); // SW1 - SW7 power
                Digital.cpu_df(1, 21, 1, 0); // SW1_CTRL
                Digital.cpu_df(1, 22, 1, 0); // SW2_V1
                Digital.cpu_df(1, 23, 1, 0); // SW2_V2
                Digital.cpu_df(1, 24, 1, 0); // SW3_CTRL
                Digital.cpu_df(1, 25, 1, 0); // SW4_V1
                Digital.cpu_df(1, 26, 1, 0); // SW4_V2
                Digital.cpu_df(1, 27, 1, 0); // SW5_V1
                Digital.cpu_df(1, 28, 1, 0); // SW5_V2
                Digital.cpu_df(1, 29, 1, 0); // SW6_V1
                Digital.cpu_df(1, 30, 1, 0); // SW6_V2
                Digital.cpu_df(1, 31, 1, 0); // SW7_V1
                Digital.cpu_df(1, 32, 1, 0); // SW7_V2

                return 0;
            }
            catch
            {
                Console.WriteLine("Switch Matrix did not boot\n");
                return -1;
            }
        }

        public void ConnectSigGen(string srcSelect)
        {
            if (srcSelect == "SG1" || srcSelect == "SG2")
            {
                Digital.cpu_df(1, 21, 1, 0); // SW1_CTRL
            }
            else if (srcSelect == "Noise")
            {
                Digital.cpu_df(1, 21, 1, 1); // SW1_CTRL
            }
            else // Error
            {
                if (writeToConsole) Console.WriteLine("ERROR: Invalid RF source connection!");
            }
        }

        public void ConnectSourcePath(string outSelect)
        {
            if (outSelect == "S1")
            {
                Digital.cpu_df(1, 22, 1, 0); // SW2_V1
                Digital.cpu_df(1, 23, 1, 0); // SW2_V2

                Digital.cpu_df(1, 24, 1, 0); // SW3_CTRL
            }
            else if (outSelect == "S2")
            {
                Digital.cpu_df(1, 22, 1, 1); // SW2_V1
                Digital.cpu_df(1, 23, 1, 1); // SW2_V2

                Digital.cpu_df(1, 24, 1, 0); // SW3_CTRL
            }
            else if (outSelect == "S3")
            {
                Digital.cpu_df(1, 22, 1, 0); // SW2_V1
                Digital.cpu_df(1, 23, 1, 1); // SW2_V2

                Digital.cpu_df(1, 24, 1, 0); // SW3_CTRL
            }
            else if (outSelect == "S4")
            {
                Digital.cpu_df(1, 22, 1, 1); // SW2_V1
                Digital.cpu_df(1, 23, 1, 0); // SW2_V2

                Digital.cpu_df(1, 24, 1, 0); // SW3_CTRL
            }
            else if (outSelect == "CAL")
            {
                Digital.cpu_df(1, 22, 1, 1); // SW2_V1
                Digital.cpu_df(1, 23, 1, 0); // SW2_V2

                Digital.cpu_df(1, 24, 1, 1); // SW3_CTRL
            }
            else // Error
            {
                if (writeToConsole) Console.WriteLine("ERROR: Invalid RF source path connection!");
            }
        }

        public void ConnectMeasurePath(string measSelect)
        {
            if (measSelect == "M1")
            {
                Digital.cpu_df(1, 25, 1, 0); // SW4_V1
                Digital.cpu_df(1, 26, 1, 0); // SW4_V2

                Digital.cpu_df(1, 31, 1, 0); // SW7_V1
                Digital.cpu_df(1, 32, 1, 1); // SW7_V2
            }
            else if (measSelect == "M2")
            {
                Digital.cpu_df(1, 27, 1, 0); // SW5_V1
                Digital.cpu_df(1, 28, 1, 0); // SW5_V2

                Digital.cpu_df(1, 31, 1, 1); // SW7_V1
                Digital.cpu_df(1, 32, 1, 0); // SW7_V2
            }
            else if (measSelect == "M3")
            {
                Digital.cpu_df(1, 29, 1, 0); // SW6_V1
                Digital.cpu_df(1, 30, 1, 0); // SW6_V2

                Digital.cpu_df(1, 31, 1, 0); // SW7_V1
                Digital.cpu_df(1, 32, 1, 0); // SW7_V2
            }
            else if (measSelect == "M4")
            {
                Digital.cpu_df(1, 25, 1, 1); // SW4_V1
                Digital.cpu_df(1, 26, 1, 1); // SW4_V2

                Digital.cpu_df(1, 31, 1, 0); // SW7_V1
                Digital.cpu_df(1, 32, 1, 1); // SW7_V2
            }
            else if (measSelect == "M5")
            {
                Digital.cpu_df(1, 27, 1, 1); // SW5_V1
                Digital.cpu_df(1, 28, 1, 1); // SW5_V2

                Digital.cpu_df(1, 31, 1, 1); // SW7_V1
                Digital.cpu_df(1, 32, 1, 0); // SW7_V2
            }
            else if (measSelect == "M6")
            {
                Digital.cpu_df(1, 29, 1, 1); // SW6_V1
                Digital.cpu_df(1, 30, 1, 1); // SW6_V2

                Digital.cpu_df(1, 31, 1, 0); // SW7_V1
                Digital.cpu_df(1, 32, 1, 0); // SW7_V2
            }
            else if (measSelect == "M7")
            {
                Digital.cpu_df(1, 25, 1, 0); // SW4_V1
                Digital.cpu_df(1, 26, 1, 1); // SW4_V2

                Digital.cpu_df(1, 31, 1, 0); // SW7_V1
                Digital.cpu_df(1, 32, 1, 1); // SW7_V2
            }
            else if (measSelect == "M8")
            {
                Digital.cpu_df(1, 27, 1, 0); // SW5_V1
                Digital.cpu_df(1, 28, 1, 1); // SW5_V2

                Digital.cpu_df(1, 31, 1, 1); // SW7_V1
                Digital.cpu_df(1, 32, 1, 0); // SW7_V2
            }
            else if (measSelect == "M9")
            {
                Digital.cpu_df(1, 29, 1, 0); // SW6_V1
                Digital.cpu_df(1, 30, 1, 1); // SW6_V2

                Digital.cpu_df(1, 31, 1, 0); // SW7_V1
                Digital.cpu_df(1, 32, 1, 0); // SW7_V2
            }
            else if (measSelect == "M10")
            {
                Digital.cpu_df(1, 25, 1, 1); // SW4_V1
                Digital.cpu_df(1, 26, 1, 0); // SW4_V2

                Digital.cpu_df(1, 31, 1, 0); // SW7_V1
                Digital.cpu_df(1, 32, 1, 1); // SW7_V2
            }
            else if (measSelect == "M11")
            {
                Digital.cpu_df(1, 27, 1, 1); // SW5_V1
                Digital.cpu_df(1, 28, 1, 0); // SW5_V2

                Digital.cpu_df(1, 31, 1, 1); // SW7_V1
                Digital.cpu_df(1, 32, 1, 0); // SW7_V2
            }
            else if (measSelect == "M12")
            {
                Digital.cpu_df(1, 29, 1, 1); // SW6_V1
                Digital.cpu_df(1, 30, 1, 0); // SW6_V2

                Digital.cpu_df(1, 31, 1, 0); // SW7_V1
                Digital.cpu_df(1, 32, 1, 0); // SW7_V2
            }
            else if (measSelect == "CAL")
            {
                Digital.cpu_df(1, 31, 1, 1); // SW7_V1
                Digital.cpu_df(1, 32, 2, 1); // SW7_V2
            }
            else // Error
            {
                if (writeToConsole) Console.WriteLine("ERROR: Invalid RF measure path connection!");
            }
        }
#endif
    }
}
