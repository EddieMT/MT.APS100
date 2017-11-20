using System;
using System.ComponentModel;

namespace MT.APS100.Model
{
    public enum TransferMode
    {
        Default = 0,
        FTP = 1,
        Server = 2
    }

    public enum WorkMode
    {
        OPERATOR,
        MES
    }

    public enum ButtonStatus
    {
        status_s0,
        status_s1,
        status_s2,
        status_s3
    }

    public enum ResponseStatus
    {
        Success,
        Fail,
		Processing
    }

    public enum BinPassFail
    {
		[Description("P")]
        Pass = 'P',
		[Description("F")]
        Fail = 'F',
		[Description(" ")]
		Unknown = ' '
    }

	/// <summary>
	/// Used to indicate endian-ness
	/// </summary>
	public enum Endian
	{
		/// <summary>
		/// Unknown
		/// </summary>
		Unknown = 0,
		/// <summary>
		/// Big endian
		/// </summary>
		Big,
		/// <summary>
		/// Little Endian
		/// </summary>
		Little,
	}

    public enum FlowWorkMode
    {
        StopOnFail,
        ContinueOnFail,
        StopOnAllFail
    }

    public enum ZipMode
    {
        zip,
        gz,
        tgz
    }

    public enum ModulationType
    {
        TD_SCDMA,
        LTE_FDD,
        CW
    }
}