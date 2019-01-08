![SimplyTyped Logo](big_logo.png)

# SimplyTyped #

SimplyTyped is a .NET library that allows simple, strongly typed access to AWS SimpleDB domains. It wraps the `AWSSDK.SimpleDB` library, and exposes the functionality it offers with a simple, strongly-typed, asynchronous API.

SimplyTyped handles all of the SimpleDB dirty work for you - 
 * Serializing your data into SimpleDB attributes.
 * Converting enum values to their number representation to allow you to query them with comparison operators.
 * [Zero Padding all number types correctly so that it's possible to query them correctly.](https://docs.aws.amazon.com/AmazonSimpleDB/latest/DeveloperGuide/ZeroPadding.html)
 * [Offsetting negative numeric values](https://docs.aws.amazon.com/AmazonSimpleDB/latest/DeveloperGuide/NegativeNumbersOffsets.html). 
 * [Formatting DateTime data ISO 8601 format](https://docs.aws.amazon.com/AmazonSimpleDB/latest/DeveloperGuide/Dates.html)
 * [Correctly quoting all inputs for Select queries.](https://docs.aws.amazon.com/AmazonSimpleDB/latest/DeveloperGuide/QuotingRulesSelect.html)
 * Reading paginated query results.

If you want to know more about what SimplyTyped does and why you should use it, keep reading.

## Why do I need SimplyTyped? ##
AWS SimpleDB is a NoSQL database that represents all objects as sets of key-value pairs (called attributes), where all values have to be stored as strings. When used correctly, this still allows storing any data type, with SQL-like filtering being supported - but this requires a lot of work, converting data into SimpleDB compatible representation and back all the time.

Given the very simple class `Person` - 
```csharp
class Person
{
    public string Id { get; set; }
    public string Name { get; set; }
    public int Age { get; set; }
    public bool CanCode { get; set; }
}
```

Storing an item using the AWS SDK would look something like this -
```csharp
public async Task StorePersonAsync(Person p)
{
    var client = new AmazonSimpleDBClient();
    var request = new PutAttributesRequest
    {
        DomainName = "people",
        ItemName = p.Id, 
        Attributes = new List<Attribute>()
        {
            new Attribute
            {
                Name = "Age",
                Value = p.Age.ToString("D10") // padding because SimpleDB compares lexicographically 
            },
            new Attribute
            {
                Name = "Name",
                Value = p.Name
            },
            new Attribute
            {
                Name = "CanCode",
                Value = p.CanCode.ToString() 
            }
        }
    };
    await client.PutAttributesAsync(request);
}
```

As you can see, this is rather tedious. Of course, it's possible to write code that generalizes this, but that's quite a lot of work. Also - notice that because SimpleDB stores all values as strings, we need to pay close attention and correctly pad numbers so that comparison operations behave as expected. This is just one example of the little pitfalls and nuances that you need to keep an eye for when working with SimpleDB.

With SimplyTyped, you can simply do -

```csharp
public async Task StorePersonAsync(Person p)
{
    IConnection con = new Connection(new AmazonSimpleDBClient());
    IDomain peopleDomain = con.GetDomain<Person>("people");
    await peopleDomain.PutAsync(p)
}
```

All of the item's serialization is handled internally, including things like padding. As an added benefit, SimplyTyped can also (depending on your configuration) make sure that the AWS SimpleDB domain you want to work against actually exists, and create it if needed.

## Usage Examples ##

Here are a few examples of common operations. We'll be using the above `Person` class

### Getting an IDomain instance ###
```csharp
IConnection con = new Connection(new AmazonSimpleDBClient());
IDomain peopleDomain = con.GetDomain<Person>("people", new DomainConfiguration
{
    // If the requested domain does not exit, it will be created on the fly - otherwise an exception is thrown.
    CreateDomainIfNotExists = true,
    // if, when reading data from SimpleDB, we encounter an attribute that can't be mapped to a model member - we ignore it instead of throwing an exception
    IgnoreUnknownAttributes = true
}
```

### Inserting a batch of items ###
```csharp
IConnection con = new Connection(new AmazonSimpleDBClient());
IDomain peopleDomain = con.GetDomain<Person>("people");
await peopleDomain.BatchPutAsync(new Person[]
{
    new Person { Id = "p1", Age = 55, Name = "Donna H Jones", CanCode = true },
    new Person { Id = "p2", Age = 27, Name = "Jean Yun", CanCode = true  }
});
```

### Deleting a batch of items ###
```csharp
IConnection con = new Connection(new AmazonSimpleDBClient());
IDomain peopleDomain = con.GetDomain<Person>("people");
await peopleDomain.BatchDeleteAsync(new string[] { "p1", "p2"});
```

### Performing a select query ###
```csharp
IConnection con = new Connection(new AmazonSimpleDBClient());
IDomain peopleDomain = con.GetDomain<Person>("people");

// Get all people who's name starts with an 'a' and are 18 or older, sorted from oldest to youngest
builder = new QueryBuilder<Person>();
IQuery<Person> startsWithA = builder.StartsWith(p => p.Name, "a");
IQuery<Person> aged18OrOlder = builder.GreaterThanOrEqualTo(p => p.Age, 18);
IQuery<Person> query = builder.And(aged18OrOlder, startsWithA);
var all = await domain.SelectAsync(query, false);
while (await all.MoveNextAsync())
{
    Person cur = await all.Current;
    Console.WriteLine(cur.ToString());
}
```

## Queriable vs Non-Queriable Members ##

When storing information on AWS SimpleDB, every object must be represented as a collection of string-string key-value pairs. This means that you're using SimplyTyped, the library needs to decided how to serialize each of your models members, and whether or not the serialized data representation can allows performing queries. 

Because of the above, SimplyTyped treats every member of your model class as either queriable, or non-queriable. Queriable members are members who's type can be serialized to in a way that allows performing queries over their value. A good example for this is `DateTime` members - when converted to a string in the ISO 8601 format, the result representation can be lexicographically compared for querying or sorting.

The list of types that are supported as queriable contains - 
* Byte
* SByte
* Int16
* UInt16
* Int32
* UInt32
* Int64
* UInt64
* Char
* DateTime
* String
* Enum

> At the moment, floating point numbers are not supported as queriable members. This is due to the issue of representing these numbers in decimal notation. Using exponential notation yields a more accurate representation of the value, but is not comparable using lexicographic comparison, meaning it can't be used for querying.

A member of any type that is not on this list will be serialized in a non-queriable manner. To avoid any unexpected behavior, trying to perform a query over a non-queriable member results in an exception.