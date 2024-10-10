using System.Text;
using ccd2ccd;

const int coreCount = 16;

double[,] results = new double[coreCount, coreCount]; // 2D array to store messages per second for each core pair

for (int i = 0; i < 10; ++i) { TestThread.RunTestForCorePair(0, 0); } // warmup

for (int core1 = 0; core1 < coreCount; core1++) // Loop through all pairs of cores
{
    for (int core2 = 0; core2 < coreCount; core2++) //for (int core2 = core1 + 1; core2 < coreCount; core2++)
    {
        double mmps = TestThread.RunTestForCorePair(core1, core2) * 0.000001d;
        results[core1, core2] = mmps;
    }
}

// Build markdown table
StringBuilder sb = new();

// Header row
sb.Append("| CoreA \\ CoreB -> |");
for (int core2 = 0; core2 < coreCount; core2++) { sb.Append($" {core2} |"); }
sb.AppendLine();

// Separator row
sb.Append("| --- |");
for (int core2 = 0; core2 < coreCount; core2++) { sb.Append(" --- |"); }
sb.AppendLine();

// Data rows
for (int core1 = 0; core1 < coreCount; core1++)
{
    sb.Append($"| {core1} |");
    for (int core2 = 0; core2 < coreCount; core2++) { sb.Append($" {results[core1, core2]:F2} |"); }
    sb.AppendLine();
}

Console.WriteLine(sb.ToString());