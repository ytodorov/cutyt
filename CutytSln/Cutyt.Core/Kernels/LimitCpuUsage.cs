using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Cutyt.Core.Kernels
{
    public class LimitCpuUsage
    {

        [DllImport("kernel32.dll", EntryPoint = "CreateJobObjectW", CharSet = CharSet.Unicode)]
        public static extern IntPtr CreateJobObject(SecurityAttributes JobAttributes, string lpName);

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool AssignProcessToJobObject(IntPtr hJob, IntPtr hProcess);

        [DllImport("kernel32.dll")]
        public static extern bool SetInformationJobObject(IntPtr hJob, JOBOBJECTINFOCLASS JobObjectInfoClass, IntPtr lpJobObjectInfo, uint cbJobObjectInfoLength);

        public class SecurityAttributes
        {

            public int nLength;
            public IntPtr pSecurityDescriptor;
            public bool bInheritHandle;

            public SecurityAttributes()
            {
                this.bInheritHandle = true;
                this.nLength = 0;
                this.pSecurityDescriptor = IntPtr.Zero;
            }
        }

        public enum JOBOBJECTINFOCLASS
        {
            JobObjectAssociateCompletionPortInformation = 7,
            JobObjectBasicLimitInformation = 2,
            JobObjectBasicUIRestrictions = 4,
            JobObjectEndOfJobTimeInformation = 6,
            JobObjectExtendedLimitInformation = 9,
            JobObjectSecurityLimitInformation = 5,
            JobObjectCpuRateControlInformation = 15
        }

        [StructLayout(LayoutKind.Explicit)]
        //[CLSCompliant(false)]
        public struct JOBOBJECT_CPU_RATE_CONTROL_INFORMATION
        {
            [FieldOffset(0)]
            public UInt32 ControlFlags;
            [FieldOffset(4)]
            public UInt32 CpuRate;
            [FieldOffset(4)]
            public UInt32 Weight;
        }

        public enum CpuFlags
        {
            JOB_OBJECT_CPU_RATE_CONTROL_ENABLE = 0x00000001,
            JOB_OBJECT_CPU_RATE_CONTROL_WEIGHT_BASED = 0x00000002,
            JOB_OBJECT_CPU_RATE_CONTROL_HARD_CAP = 0x00000004
        }
    }
}