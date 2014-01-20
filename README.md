JValue
======
Lightweight C# Json Parser.  
JValue is simple and pure.  


Features
--------
- Pros
  - Small footprint. (no heap allocation)
  - Fastest parse.
  - Single file. (`DJValue.cs` is optional)
- Cons
  - When you get to repeat the same key, it is inefficient.
  - Slow serialization.
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


Benchmark
---------
- This is self test.
- See `test/Comparison/Program.cs`.
- Environment
  - Intel(R) Core(TM) i7-2600K CPU @ 3.40GHz 3.70 GHz
  - Windows 7 x86

| Library | Small Int Array (32bytes) × 100,000 | Big Int Array (209,670bytes) × 100 | Small Object (1,084bytes) × 10,000 | Big Object (5,792,000bytes) × 10 |
|:----------------:|:---------:|:---------:|:---------:|:---------:|
|          LitJson |     238ms |     621ms |     129ms |   1,845ms |
|           JsonFx |     400ms |     949ms |   1,412ms |   2,615ms |
|         Json.NET |     141ms |     237ms |      51ms |     661ms |
|         MiniJSON |     141ms |     616ms |      95ms |   1,525ms |
|           JValue |  **25ms** |  **88ms** |      68ms |     454ms |
|     JValue other |      38ms |     159ms |  **33ms** | **358ms** |


Installation
------------
1. Download `JValue.cs`.
2. Add `JValue.cs` to your project.
3. The End. use it. :)


Installation (Optional)
-----------------------
1. Open `JValue.cs`
2. Rename `namespace Halak` to your flavor.


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
Console.WriteLine("Unknown author: {0}", book["tags"][100].AsString());

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
