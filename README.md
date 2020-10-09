JValue
======
Lightweight C# Json Parser.  
JValue is simple and pure.  
Version: 1.6.0


Features
--------
- Pros
  - Small memory footprint. (no heap allocation)
  - Fastest parse.
  - Easy to install.
  - Easy to use.
  - Unity3D friendly.
  - Multithread safe. (JValue is immutable type)
- Cons
  - When you get to repeat the same key, it is inefficient.
  - Object mapping NOT supported. (`System.Reflection`)


Simple usage
------------
```cs
JValue location = JValue.Parse(@"{""x"":10, ""y"":20}");
Console.WriteLine((int)location["x"]); // 10
Console.WriteLine((int)location["y"]); // 20
Console.WriteLine(location["z"].ToString()); // null

var person = new Dictionary<string, JValue>()
{
    {"name", "John"},
    {"age", 27},
};
Console.WriteLine(new JValue(person).ToString()); // {"name":"John","age":27}
```


Mechanism
---------
1. `JValue` is `struct`.
2. `JValue` has three members. `{string source, int startIndex, int length}`

```cs
// raw source
// {"name":"John", "age":27, "friend":{"name":"Tom"}}
JValue person = JValue.Parse(...);
// person.source = ...
// person.startIndex = 0
// person.length = 50

JValue name = person["name"];
// name.source = person.source
// name.startIndex = 8
// name.length = 6

JValue age = person["age"];
// age.source = person.source
// age.startIndex = 22
// age.length = 2

JValue friend = person["friend"];
// friend.source = person.source
// friend.startIndex = 35
// friend.length = 14

JValue friendName = friend["name"];
// friendName.source = friend.source = person.source
// friendName.startIndex = 43
// friendName.length = 5

string nameValue = (string)name; // At this time, encode string.
int ageValue = (int)age; // At this time, parse int.
```

Usage
-----
```cs
JValue book = JValue.Parse(@"{
    ""name"": ""Json guide"",
    ""pages"": 400,
    ""tags"": [""computer"", ""data-interchange format"", ""easy""],
    ""price"": {""usd"":0.99, ""krw"":1000}
}");
string name = book["name"];
Console.WriteLine("Name: {0}", name);

int pages = book["pages"];
Console.WriteLine("Pages: {0}", pages);

Console.WriteLine("Primary tag: {0}", book["tags"][0].AsString());
Console.WriteLine("Tags:");
foreach (var item in book["tags"].Array())
    Console.WriteLine("\t{0}", item);
Console.WriteLine("Unknown tag: {0}", book["tags"][100].AsString());

JValue price = book["price"];
Console.WriteLine("Price: ${0}", (double)price["usd"]);
Console.WriteLine("");
Console.WriteLine("Reserialization");
Console.WriteLine(book.Serialize(false));

/*
Name: Json guide
Pages: 400
Primary tag: computer
Tags:
        "computer"
        "data-interchange format"
        "easy"
Unknown author:
Price: $0.99

Reserialization
{"name":"Json guide","pages":400,"tags":["computer","data-interchange format","easy"],"price":{"usd":0.99,"krw":1000}}
*/
```
