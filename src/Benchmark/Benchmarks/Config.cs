using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Exporters.Csv;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;

namespace Mapster.Benchmark.Benchmarks
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
            AddColumn(ScenarioColumn.Default);
            AddColumn(TargetMethodColumn.Method);
            AddColumnProvider(DefaultColumnProviders.Params);

            AddColumn(StatisticColumn.Mean);
            AddColumn(PerMapColumn.Nanoseconds);
            AddColumn(StatisticColumn.StdDev);
            AddColumn(StatisticColumn.Error);

            AddColumn(BaselineRatioColumn.RatioMean);
            AddColumn(BaselineAllocationRatioColumn.RatioMean);
            AddColumnProvider(DefaultColumnProviders.Metrics);
            AddColumn(PerMapColumn.Bytes);

            AddJob(Job.ShortRun
                .WithLaunchCount(1)
                .WithWarmupCount(2)
                .WithIterationCount(10)
            );

            Options |= ConfigOptions.JoinSummary;
        }
    }
}