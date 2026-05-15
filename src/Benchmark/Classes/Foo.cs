namespace Mapster.Benchmark.Classes
{
    // Deep recursive graph with collections.
    public class Foo
    {
        public string Name { get; set; }

        public int Int32 { get; set; }

        public long Int64 { set; get; }

        public int? NullInt { get; set; }

        public float Floatn { get; set; }

        public double Doublen { get; set; }

        public DateTime DateTime { get; set; }

        public Foo Foo1 { get; set; }

        public IEnumerable<Foo> Foos { get; set; }

        public Foo[] FooArr { get; set; }

        public int[] IntArr { get; set; }

        public IEnumerable<int> Ints { get; set; }
    }

    // DTO copy of Foo.
    public class FooDTO
    {
        public string Name { get; set; }

        public int Int32 { get; set; }

        public long Int64 { get; set; }

        public int? NullInt { get; set; }

        public float Floatn { get; set; }

        public double Doublen { get; set; }

        public DateTime DateTime { get; set; }

        public FooDTO Foo1 { get; set; }

        public IEnumerable<FooDTO> Foos { get; set; }

        public FooDTO[] FooArr { get; set; }

        public int[] IntArr { get; set; }

        public IEnumerable<int> Ints { get; set; }
    }
}
