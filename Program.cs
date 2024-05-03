using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ThreadingBenchmark
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var test = BenchmarkRunner.Run<BenchMarkService>();
        }
    }

    [ShortRunJob, Config(typeof(Config))]
    public class BenchMarkService
    {
        private const int STEPSIZE = 1_000_000;

        private class Config : ManualConfig
        {
            public Config()
            {
                SummaryStyle = BenchmarkDotNet.Reports.SummaryStyle.Default.WithRatioStyle(BenchmarkDotNet.Columns.RatioStyle.Trend); //yüzde olarak karşılaştırma
            }
        }

        [Benchmark(Baseline = true)]
        public void UseSingleThread()
        {
            for (int i = 0; i < STEPSIZE; i++)
            {
                CheapCalculation(i);
            }
        }

        [Benchmark]
        public void UseMultipleThreads()
        {
            object lockObject = new object();
            var workerThreads = new List<Thread>();
            int currentNumber = 1;

            for (int i = 0; i < 4; i++)
            {
                var thread = new Thread(delegate ()
                {
                    int number = 0;

                    while (true)
                    {
                        lock (lockObject)
                        {
                            if (currentNumber < STEPSIZE)
                            {
                                number = ++currentNumber;
                            }
                            else
                            {
                                break;
                            }
                        }

                        CheapCalculation(number);
                    }
                });

                workerThreads.Add(thread);
            }

            foreach (var thread in workerThreads)
            {
                thread.Start();
            }

            foreach (var thread in workerThreads)
            {
                thread.Join();
            }
        }

        [Benchmark]
        public void UseThreadPool()
        {
            for (int i = 0; i < STEPSIZE; i++)
            {
                ThreadPool.QueueUserWorkItem(delegate { CheapCalculation(i); });
            }
        }

        [Benchmark]
        public void UseParallelAPI()
        {
            Parallel.For(0, STEPSIZE, n => CheapCalculation(n));
        }

        public void CheapCalculation(int number)
        {
        }
    }
}