---
uid: Mapster.Mapping.DataTypes.Overview
title: "Mapping - Mappable Objects"
---

## Mappable Objects

Mapster can map two different objects using the following rules:

- Source and destination property names are the same. Ex: `dest.Name = src.Name`
- Source has get method. Ex: `dest.Name = src.GetName()`
- Source property has child object which can flatten to destination. Ex: `dest.ContactName = src.Contact.Name` or `dest.Contact_Name = src.Contact.Name`

Example:

```csharp
class Staff {
    public string Name { get; set; }
    public int GetAge() {
        return (DateTime.Now - this.BirthDate).TotalDays / 365.25;
    }
    public Staff Supervisor { get; set; }
    ...
}

struct StaffDto {
    public string Name { get; set; }
    public int Age { get; set; }
    public string SupervisorName { get; set; }
}

var dto = staff.Adapt<StaffDto>();
//dto.Name = staff.Name, dto.Age = staff.GetAge(), dto.SupervisorName = staff.Supervisor.Name
```

**Mappable Object types are included:**

- POCO classes
- POCO structs
- POCO interfaces
- Dictionary type implement `IDictionary<string, T>`
- Record types (either class, struct, and interface)

Example for object to dictionary:

```csharp
var point = new { X = 2, Y = 3 };
var dict = point.Adapt<Dictionary<string, int>>();
dict["Y"].ShouldBe(3);
```
