---
uid: Mapster.Mapping.DataTypes.Collections
title: "Mapping - Collections"
---

## Collections

This includes mapping among lists, arrays, collections, dictionary including various interfaces: `IList<T>`, `ICollection<T>`, `IEnumerable<T>`, `ISet<T>`, `IDictionary<TKey, TValue>` etc...

```csharp
var list = db.Pocos.ToList();
var target = list.Adapt<IEnumerable<Dto>>();
```
