using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

namespace Mapster.Benchmark.Benchmarks
{
    /// <summary>
    /// Adds a "Scenario" column to the joined summary so each row clearly indicates which benchmark class
    /// (TestFlatTypes / TestRecursiveTypes / TestComplexTypes / TestTotalAllTypes) produced it.
    /// </summary>
    public class ScenarioColumn : IColumn
    {
        public static readonly IColumn Default = new ScenarioColumn();

        public string Id => nameof(ScenarioColumn);
        public string ColumnName => "Scenario";
        public string Legend => "Benchmark class the row belongs to";
        public UnitType UnitType => UnitType.Dimensionless;
        public ColumnCategory Category => ColumnCategory.Job;
        public int PriorityInCategory => -10;
        public bool IsNumeric => false;
        public bool AlwaysShow => true;
        public bool IsAvailable(Summary summary) => true;
        public bool IsDefault(Summary summary, BenchmarkCase benchmarkCase) => false;

        public string GetValue(Summary summary, BenchmarkCase benchmarkCase)
        {
            var name = benchmarkCase.Descriptor.Type.Name;
            return name.StartsWith("Test") ? name[4..] : name;
        }

        public string GetValue(Summary summary, BenchmarkCase benchmarkCase, SummaryStyle style)
            => GetValue(summary, benchmarkCase);

        public override string ToString() => ColumnName;
    }
}
