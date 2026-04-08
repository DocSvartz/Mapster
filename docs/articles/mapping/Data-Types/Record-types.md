---
uid: Mapster.Mapping.DataTypes.Records
title: "Mapping - Record Types"
---

## Record types

>[!IMPORTANT]
> Mapster treats Record type as an immutable type.
> Only a Nondestructive mutation - creating a new object with modified properties.
>
> ```csharp
> var result = source.adapt(data) 
>//equal var result = data with { X = source.X.Adapt(), ...}
>```

### Features and Limitations:

# [v10.0](#tab/Records-v10)

>[!NOTE]
> By default, all [C# Records](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/record) are defined as a record type.
> Limitations by count of constructors and constructor parameters used in Mapster version 7.4.0 do not apply.


#### Using default value in constuctor param

If the source type does not contain members that can be used as constructor parameters, then will be used the default values ​​for the parameter type.

Example: 

```csharp

class SourceData
{
   public string MyString {get; set;}
}

record RecordDestination(int myInt, string myString);

var result = source.Adapt<RecordDestination>()

// equal var result = new RecordDestination (default(int),source.myString)

```

#### MultiConstructor Record types

If there is more than one constructor, by default, mapping will be performed on the constructor with the largest number of parameters.

Example: 

```csharp
record MultiCtorRecord
{
    public MultiCtorRecord(int myInt)
    {
        MyInt = myInt;
    }

    public MultiCtorRecord(int myInt, string myString) // This constructor will be used
        : this(myInt) 
    {
        MyString = myString; 
    }

}
```

# [v7.4.0](#tab/Records-v7-4-0)

>[!NOTE]
>Record type must not have a setter and have only one non-empty constructor, and all parameter names must match with properties.

Otherwise you need to add [`MapToConstructor` configuration](xref:Mapster.Settings.ConstructorMapping#map-to-constructor).

Example for record types:

```csharp
class Person {
    public string Name { get; }
    public int Age { get; }

    public Person(string name, int age) {
        this.Name = name;
        this.Age = age;
    }
}

var src = new { Name = "Mapster", Age = 3 };
var target = src.Adapt<Person>();
```
---

### Support additional mapping features:

| Mapping features | v7.4.0 | v10.0 |
|:-----------------|:------:|:-----:|
|[Custom constructor mapping](xref:Mapster.Settings.ConstructorMapping)| - | ✅ |
|[Ignore](xref:Mapster.Settings.Custom.IgnoringMembers#ignore-extension-method)| - | ✅ |
|[IgnoreNullValues](xref:Mapster.Settings.Custom.IgnoringMembers#ignorenullvalues-extension-method)| - | ✅ |
