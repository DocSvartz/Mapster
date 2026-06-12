using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Exporters.Csv;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;

namespace Mapster.Benchmark.Development.Benchmarks
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



            foreach (var version in MapsterVersion.Get())
            {
                AddJob(Job.ShortRun
                    .WithLaunchCount(1)
                    .WithWarmupCount(2)
                    .WithIterationCount(10)
                    .WithCustomBuildConfiguration("nuget-bench")
                    .WithMsBuildArguments($"/p:SciVersion={version}")
                    .WithId($"v{version}")
                );
            }

            AddJob(Job.ShortRun
                .WithLaunchCount(1)
                .WithWarmupCount(2)
                .WithIterationCount(10)
                .WithCustomBuildConfiguration("developer-bench")
                .WithId("developer"));

            Options |= ConfigOptions.JoinSummary;
        }
    }
}