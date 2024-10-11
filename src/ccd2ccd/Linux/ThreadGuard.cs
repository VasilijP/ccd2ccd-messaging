using System.Runtime.InteropServices;

namespace ccd2ccd.Linux;

public class ThreadGuard
{
    private readonly ulong mask;

    private ThreadGuard(ulong mask)
    {
        this.mask = mask;
    }

    public static ThreadGuard GetInstance(IntPtr mask)
    {
        return new ThreadGuard((ulong)mask.ToInt64());
    }

    public void Guard()
    {
        IntPtr thread = pthread_self();
        IntPtr size = (IntPtr)Marshal.SizeOf(typeof(ulong));
        IntPtr maskPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(ulong)));
        Marshal.WriteInt64(maskPtr, (long)mask);

        int result = pthread_setaffinity_np(thread, size, maskPtr);
        Marshal.FreeHGlobal(maskPtr);

        if (result != 0)
        {
            throw new System.ComponentModel.Win32Exception(result);
        }
    }

    [DllImport("libc", SetLastError = true)]
    private static extern int pthread_setaffinity_np(IntPtr thread, IntPtr cpusetsize, IntPtr cpuset);

    [DllImport("libc")]
    private static extern IntPtr pthread_self();
}
