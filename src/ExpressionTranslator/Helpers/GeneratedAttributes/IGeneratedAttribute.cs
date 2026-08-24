namespace ExpressionDebugger.Helpers.GeneratedAttributes
{
    public interface IGeneratedAttribute : IGeneratedImplimentationOnly
    {
        public string? Declaration { get;}
        public string? FileName { get;}
    }

    public interface IGeneratedImplimentationOnly
    {
        public string NameSpace { get; }
        public string Implimentation { get; }
        public bool IsImplimentationOnly { get; }

    }
}
