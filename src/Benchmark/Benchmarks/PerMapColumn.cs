using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

namespace Mapster.Benchmark.Benchmarks
{
    public abstract class PerMapColumn : IColumn
    {
        public static readonly IColumn Nanoseconds = new NanosecondsPerMapColumn();
        public static readonly IColumn Bytes = new BytesPerMapColumn();

        public abstract string Id { get; }
        public abstract string ColumnName { get; }
        public abstract string Legend { get; }
        public abstract ColumnCategory Category { get; }

        public UnitType UnitType => UnitType.Dimensionless;
        public int PriorityInCategory => 100;
        public bool IsNumeric => true;
        public bool AlwaysShow => true;
        public bool IsAvailable(Summary summary) => true;
        public bool IsDefault(Summary summary, BenchmarkCase benchmarkCase) => false;

        public abstract string GetValue(Summary summary, BenchmarkCase benchmarkCase, SummaryStyle style);

        public string GetValue(Summary summary, BenchmarkCase benchmarkCase)
            => GetValue(summary, benchmarkCase, summary.Style);

        protected static long GetLogicalMapCount(BenchmarkCase benchmarkCase)
        {
            var mapOperations = 1;
            var parameter = benchmarkCase.Parameters.Items
                .FirstOrDefault(p => p.Name == nameof(MappingBenchmarkBase.MapOperations));

            if (parameter?.Value is int value && value > 0)
                mapOperations = value;

            //TestTotalAllTypes includes 3 separate mapping calls per benchmark call
            var mappingsPerBenchmarkCall = benchmarkCase.Descriptor.Type == typeof(TestTotalAllTypes) ? 3 : 1;
            return (long)mapOperations * mappingsPerBenchmarkCall;
        }

        protected static string Format(double value, SummaryStyle style)
            => value.ToString("0.###", style.CultureInfo);

        public override string ToString() => ColumnName;

        private sealed class NanosecondsPerMapColumn : PerMapColumn
        {
            public override string Id => nameof(NanosecondsPerMapColumn);
            public override string ColumnName => "Ns/Map";
            public override string Legend => "Mean nanoseconds per single mapping call";
            public override ColumnCategory Category => ColumnCategory.Statistics;

            public override string GetValue(Summary summary, BenchmarkCase benchmarkCase, SummaryStyle style)
            {
                if (!summary.HasReport(benchmarkCase))
                    return "?";

                var meanNanoseconds = summary[benchmarkCase].ResultStatistics?.Mean;
                return meanNanoseconds.HasValue
                    ? Format(meanNanoseconds.Value / GetLogicalMapCount(benchmarkCase), style)
                    : "?";
            }
        }

        private sealed class BytesPerMapColumn : PerMapColumn
        {
            public override string Id => nameof(BytesPerMapColumn);
            public override string ColumnName => "Bytes/Map";
            public override string Legend => "Allocated bytes per single mapping call";
            public override ColumnCategory Category => ColumnCategory.Metric;

            public override string GetValue(Summary summary, BenchmarkCase benchmarkCase, SummaryStyle style)
            {
                if (!summary.HasReport(benchmarkCase))
                    return "?";

                var allocatedBytesPerBenchmarkCall = summary[benchmarkCase].GcStats.GetBytesAllocatedPerOperation(benchmarkCase);
                return allocatedBytesPerBenchmarkCall.HasValue
                    ? Format((double)allocatedBytesPerBenchmarkCall.Value / GetLogicalMapCount(benchmarkCase), style)
                    : "?";
            }
        }
    }
}