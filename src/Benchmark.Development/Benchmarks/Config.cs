using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Exporters.Csv;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using Perfolizer.Models;

namespace Benchmark.Benchmarks
{
    public class Config : ManualConfig
    {
        public Config()
        {
            AddLogger(ConsoleLogger.Default);

            AddExporter(CsvExporter.Default);
            AddExporter(MarkdownExporter.GitHub);
            AddExporter(HtmlExporter.Default);

            AddDiagnoser(MemoryDiagnoser.Default);
            AddColumn(TargetMethodColumn.Method);

            AddColumn(JobCharacteristicColumn.AllColumns);
            AddColumnProvider(DefaultColumnProviders.Params);
            AddColumn(StatisticColumn.Mean);

            AddColumn(StatisticColumn.StdDev);
            AddColumn(StatisticColumn.Error);

            AddColumn(BaselineRatioColumn.RatioMean);
            AddColumnProvider(DefaultColumnProviders.Metrics);

            string[] targetVersions = [
                    "7.4.0",
                    "9.0.0-pre01",
                ];

            foreach (var version in targetVersions)
            {
                AddJob(Job.ShortRun
                    .WithLaunchCount(1)
                    .WithWarmupCount(2)
                    .WithIterationCount(10)
                    .WithMsBuildArguments($"/p:SciVersion={version}")
                    .WithId($"v{version}")
                );
            }

            Options |= ConfigOptions.JoinSummary;
        }
    }
}