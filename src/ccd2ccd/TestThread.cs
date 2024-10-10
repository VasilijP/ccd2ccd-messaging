using System.Diagnostics;
using ccd2ccd.Windows;

namespace ccd2ccd;

public static class TestThread
{
    private const int MessagesToProcess = 2500000;
    private const int QueueSize = 1000000;

    public static double RunTestForCorePair(int core1, int core2)
    {
        // Create affinity masks for the two cores
        IntPtr mask1 = new(1 << core1);
        IntPtr mask2 = new(1 << core2);

        // Synchronization primitives
        SemaphoreSlim semaphoreProducer = new(QueueSize);
        SemaphoreSlim semaphoreConsumer = new(0);

        // Shared message variable
        string[] message = new string[QueueSize];
        int reader = -1;
        int writer = -1;

        Stopwatch sw = Stopwatch.StartNew();

        // Producer thread
        Thread producerThread = new(() =>
        {
            ThreadGuard.GetInstance(mask1).Guard(); // Guard to core1

            for (int i = 1; i <= MessagesToProcess; i++)
            {
                semaphoreProducer.Wait();

                // Produce a message
                int index = Interlocked.Increment(ref writer);
                message[index % QueueSize] = i.ToString();

                semaphoreConsumer.Release();
            }
        });

        // Consumer thread
        Thread consumerThread = new(() =>
        {
            ThreadGuard.GetInstance(mask2).Guard(); // Guard to core2

            int i = 0;   
            do 
            {
                semaphoreConsumer.Wait();

                // Consume the message
                int index = Interlocked.Increment(ref reader);
                i = int.Parse(message[index % QueueSize]);

                semaphoreProducer.Release();
            } while (i < MessagesToProcess);
        });

        // Start threads
        producerThread.Start();
        consumerThread.Start();

        // Wait for both threads to finish
        producerThread.Join();
        consumerThread.Join();

        sw.Stop();

        double elapsedSeconds = sw.Elapsed.TotalSeconds;
        double messagesPerSecond = MessagesToProcess / elapsedSeconds;
        //Console.WriteLine($"Test finished, Cores #{core1} and #{core2} in {elapsedSeconds:F2}s = {messagesPerSecond:F0} messages/s");
        return messagesPerSecond;
    }
}
