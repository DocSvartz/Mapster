namespace Mapster.Benchmark.Classes
{
    // FlatType POCO: no nested types, no collections. Used to exercise the "best case"
    // mapping path - a simple property-to-property copy with primitive/string values.
    public class Person
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public int Age { get; set; }
        public DateTime BirthDate { get; set; }
        public decimal Salary { get; set; }
        public bool IsActive { get; set; }
    }

    public class PersonDTO
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public int Age { get; set; }
        public DateTime BirthDate { get; set; }
        public decimal Salary { get; set; }
        public bool IsActive { get; set; }
    }
}
