# Serializing and Deserializing

## JsonConvert
``` c#
Test test = new Test();
test.Id = 1;
test.Name = "test";
test.Strings = new[] { "a", "b", "c" };

Console.WriteLine(JsonConvert.Serialize(test));
// {
//     "Strings" : ["a", "b", "c" ],
//     "Name" : "test",
//     "Id" : 1
// }
Test deserializedObject = JsonConvert.Deserialize<Test>(output);
```

## XmlConvert
``` c#
Test test = new Test();
test.Id = 1;
test.Name = "test";
test.Strings = new[] { "a", "b", "c" };

Console.WriteLine(XmlConvert.Serialize(test));
// <Test>
//     <Name>test</Name>
//     <Id>1</Id>
//     <Strings>a</String>
//     <Strings>b</String>
//     <Strings>c</String>
// </Test>
Test deserializedObject = XmlConvert.Deserialize<Test>(output);
```
# Attributes
## DisplayOrder
序列化時按照標籤順序生成 Xml / Json 字串。
```c#
class Test
{
    [DisplayOrder(0)]
    public int Id { get; set; }
    [DisplayOrder(1)]
    public string Name { get; set; }
}
```
```c#
Test test = new Test();
test.Id = 1;
test.Name = "test";

Console.WriteLine(JsonConvert.Serialize(test));
// {
//     "Id" : 1,
//     "Name" : "test"
// }
Console.WriteLine(XmlConvert.Serialize(test));
// <Test>
//     <Id>1</Id>
//     <Name>test</Name>
// </Test>
```
## TagRename
重新命名 Xml / Json 標籤名稱。
```c#
[TagRename("Content")]
class Test
{
    [TagRename("A")]
    public int t1 { get; set; }
}
```
```c#
Test test = new Test();
test.t1 = 1;

Console.WriteLine(JsonConvert.Serialize(test));
// {
//     "A" : 1
// }
Console.WriteLine(XmlConvert.Serialize(test));
// <Content>
//     <A>1</A>
// </Content>
```
## IgnoreItem
特定標籤不進行序列化或反序列化處理。
```c#
class Test
{
    [IgnoreItem(IgnoreFlags.Serialize)]
    public string t1 { get; set; }
    [IgnoreItem(IgnoreFlags.Deserialize)]
    public string t2 { get; set; }
}
```
```c#
Test test = new Test();
test.t1 = "1";
test.t2 = "2";

Console.WriteLine(JsonConvert.Serialize(test));
// {
//     "t2" : "2"
// }
Console.WriteLine(XmlConvert.Serialize(test));
// <Test>
//     <t2>2</t2>
// </Test>

Test obj = JsonConvert.Deserialize<Test>("{\"t1\":\"1\", \"t2\":\"2\"}");
// obj.t1 => "1"
// obj.t2 => null
```
## DisplayEmptyItem
標籤為<code>null</code>時序列化後仍顯示該標籤。
```c#
class Test
{
    [DisplayEmptyItem]
    public int? t1 { get; set; }
    [DisplayEmptyItem]
    public string t2 { get; set; }
}
```
```c#
Test test = new Test();

Console.WriteLine(JsonConvert.Serialize(test));
// {
//     "t1" : 0,
//     "t2" : ""
// }
Console.WriteLine(XmlConvert.Serialize(test));
// <Test>
//     <t1>0</t1>
//     <t2></t2>
// </Test>
```
## BooleanToInt
序列化時boolean以 0/1 呈現。
```c#
class Test
{
    public bool t1 { get; set; }
    [BooleanToInt]
    public bool t2 { get; set; }
}
```
```c#
Test test = new Test();

Console.WriteLine(JsonConvert.Serialize(test));
// {
//     "t1" : false,
//     "t2" : 0
// }
Console.WriteLine(XmlConvert.Serialize(test));
// <Test>
//     <t1>False</t1>
//     <t2>0</t2>
// </Test>
```
## EnumToInt
序列化時列舉以數值呈現。
```c#
enum TEnum
{
    e1 = 0,
    e2 = 1
}
class Test
{
    public TEnum t1 { get; set; }
    [EnumToInt]
    public TEnum t2 { get; set; }
}
```
```c#
Test test = new Test();

Console.WriteLine(JsonConvert.Serialize(test));
// {
//     "t1" : "e1",
//     "t2" : 0
// }
Console.WriteLine(XmlConvert.Serialize(test));
// <Test>
//     <t1>e1</t1>
//     <t2>0</t2>
// </Test>
```
## DateTimeFormat
DateTime與TimeSpan序列化時根據特定格式產生字串。
```c#
class Test
{
    public DateTime dt1 { get; set; }
    [DateTimeFormat("yyyy/MM/dd")]
    public DateTime dt2 { get; set; }
    public TimeSpan ts1 { get; set; }
    [DateTimeFormat("hh\\:mm")]
    public DateTime ts2 { get; set; }
}
```
```c#
Test test = new Test();
test.dt1 = new DateTime("1900/1/1");
test.dt2 = new DateTime("1900/1/1");
test.ts1 = new TimeSpan();
test.ts2 = new TimeSpan();

Console.WriteLine(JsonConvert.Serialize(test));
// {
//     "dt1" : "1900/1/1 00:00:00",
//     "dt2" : "1900/1/1",
//     "ts1" : "00:00:00",
//     "ts2" : "00:00"
// }
Console.WriteLine(XmlConvert.Serialize(test));
// <Test>
//     <dt1>1900/1/1 00:00:00</dt1>
//     <dt2>1900/1/1</dt2>
//     <ts1>00:00:00</ts1>
//     <ts2>00:00</ts2>
// </Test>
```
## XmlAttribute
特定Property在Xml序列化時生成在Xml屬性。
```c#
class Test
{
    [XmlAttribute]
    public int a { get; set; }
}
```
```c#
Test test = new Test();

Console.WriteLine(XmlConvert.Serialize(test));
// <Test a="0"/>
```
## XmlContent
Xml反序列化時將內容寫入到特定Property。
```c#
class Test
{
    [XmlContent]
    public string value { get; set; }
}
```
```c#
Test obj = XmlConvert.Deserialize<Test>("<Test>content</Test>");
// obj.value => content
```
## XmlArray
Array進行Xml序列化時，將PropertyName作為外層標籤；XmlArray中的ChildTagName作為內層標籤來生成字串。
```c#
class Test
{
    public string[] data { get; set; }
    [XmlArray(ChildTagName = "d")]
    public string[] array { get; set; }
}
```
```c#
Test test = new Test();
test.data = new [] { "1", "2", "3" };
test.array = new [] { "a", "b", "c" };

Console.WriteLine(XmlConvert.Serialize(test));
// <Test>
//     <array>
//         <d>a</d>
//         <d>b</d>
//         <d>c</d>
//     </array>
//     <data>1</data>
//     <data>2</data>
//     <data>3</data>
// </Test>
```
