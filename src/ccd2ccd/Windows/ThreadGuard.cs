using System.Collections.Concurrent;
using System.Runtime.InteropServices;

namespace ccd2ccd.Windows;

public class ThreadGuard
{
    // Import SetThreadAffinityMask from kernel32.dll
    [DllImport("kernel32.dll")]
    private static extern IntPtr SetThreadAffinityMask(IntPtr hThread, IntPtr dwThreadAffinityMask);

    // Import GetCurrentThread from kernel32.dll
    [DllImport("kernel32.dll")]
    private static extern IntPtr GetCurrentThread();
    
    // Static dictionary to pool instances of ThreadGuard by affinity mask
    private static readonly ConcurrentDictionary<IntPtr, ThreadGuard> GuardPool = new();
    
    private readonly IntPtr mask;

    // Private constructor to prevent direct instantiation
    private ThreadGuard(IntPtr mask) 
    {
        this.mask = mask;
    }

    // Factory method to get or create ThreadGuard instance for a specific affinity mask
    // cacheThreads: If true, the thread will be cached and reused for subsequent calls with the same affinity mask
    public static ThreadGuard GetInstance(IntPtr affinityMask)
    {
        return GuardPool.GetOrAdd(affinityMask, _ => new ThreadGuard(affinityMask));
    }

    // Method to guard a thread with a specific affinity mask
    public void Guard()
    {
        // Get the handle of the current thread
        IntPtr currentThreadHandle = GetCurrentThread();

        // Set the thread affinity mask
        IntPtr result = SetThreadAffinityMask(currentThreadHandle, mask);
        if (result == IntPtr.Zero) { throw new InvalidOperationException("Failed to set thread affinity mask."); }
    }
}
