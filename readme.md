# SharpBoxesCore
[![](https://img.shields.io/github/issues/dumbnessrf/SharpBoxesCore.svg)](https://github.com/dumbnessrf/SharpBoxesCore/issues)
[![](https://img.shields.io/github/forks/dumbnessrf/SharpBoxesCore.svg)](https://github.com/dumbnessrf/SharpBoxesCore/network)

集成常用 .NET 工具方法：O2Html（对象→HTML 序列化）、反射与动态加载 DLL、HTTP/INI/XML/ZIP 文件操作、CSV/Excel 数据处理、汉字拼音与繁简转换、几何形状数据结构与计算扩展、数据验证、MVVM 基础（RelayCommand/VMBase）、后台任务与线程池管理、TCP 通信、高级计时器等。

A .NET utility library integrating common tools: O2Html (object-to-HTML serialization), reflection & dynamic DLL loading, HTTP/INI/XML/ZIP file operations, CSV/Excel data processing, Chinese Pinyin & character conversion, geometric shape structures with math extensions, data validation, MVVM basics (RelayCommand/VMBase), background task & thread pool management, TCP communication, advanced stopwatch, and more.

Original Source:
[SharpBoxesCore](https://github.com/dumbnessrf/SharpBoxesCore)

其他相关工具、扩展
Other Toolkit:
[SharpBoxesCore.Cuts](https://marketplace.visualstudio.com/items?itemName=dumbnessrf.SharpBoxesCore)

其中提供了许多 C#、XAML 有用的代码片段，如 `OnPropertyChanged` 的完整属性语句，`Task.Run(()=>{ })` 的自动环绕等。

Which provided lots of C#、XAML useful code snippets like full property statement with `OnPropertyChanged`, auto surround with `Task.Run(()=>{ }`);...

## Table of Contents
- [Installation](#install)
- [API Documentation](#api-documentation)
  - [Helpers](#helpers)
    - [HTTPHelper](#httphelper)
    - [IniHelper](#inihelper)
    - [ZipHelper](#ziphelper)
    - [ConfigFileHelper](#configfilehelper)
    - [PathHelper](#pathhelper)
    - [IOHelper](#iohelper)
    - [XMLHelper](#xmlhelper)
    - [AdvancedStopWatch](#advancedstopwatch)
    - [FastDateTime](#fastdatetime)
    - [FolderDeleteService](#folderdeleteservice)
    - [SmartFileWriter](#smartfilewriter)
    - [EventManager](#eventmanager)
    - [TCP Helpers](#tcp-helpers)
  - [ChineseSpell](#chinesespell)
    - [PinYin](#pinyin)
    - [EcanConvertToCh](#ecanconverttoch)
  - [O2Html](#o2html)
    - [HtmlSerializer](#htmlserializer)
    - [HtmlConverter](#htmlconverter)
  - [Office](#office)
    - [CSV](#csv)
    - [Excel](#excel)
  - [Validation](#validation)
    - [ValidationHelper](#validationhelper)
    - [FormatValidationHelper](#formatvalidationhelper)
  - [Reflection](#reflection)
    - [ClassHelper](#classhelper)
    - [EventHelper](#eventhelper)
  - [DynamicLoad](#dynamicload)
    - [DynamicLoadHelper](#dynamicloadhelper)
  - [DataStruct](#datastruct)
    - [Shape Structures](#shape-structures)
    - [Math & Shape Extensions](#math--shape-extensions)
  - [Mvvm](#mvvm)
    - [RelayCommand](#relaycommand)
    - [VMBase](#vmbase)
  - [TaskHelper](#taskhelper)
    - [BackgroundTaskManager](#backgroundtaskmanager)
    - [ThreadPoolManager](#threadpoolmanager)
    - [AsyncManualResetEvent](#asyncmanualresetevent)

# Install
```shell
Install-Package SharpBoxesCore
```

# API Documentation

## Helpers

### HTTPHelper
用于处理HTTP请求的帮助类。

**Methods:**
- `Get(string url)` - 发送GET请求
- `Get<T>(string url)` - 发送GET请求并反序列化为指定类型
- `Post(string url, string data, Encoding encoding = null)` - 发送POST请求
- `Post<T>(string url, string data, Encoding encoding = null)` - 发送POST请求并反序列化为指定类型
- `GetAsync(string url)` - 异步发送GET请求
- `GetAsync<T>(string url)` - 异步发送GET请求并反序列化为指定类型
- `PostAsync(string url, string data, Encoding encoding = null)` - 异步发送POST请求
- `PostAsync<T>(string url, string data, Encoding encoding = null)` - 异步发送POST请求并反序列化为指定类型

**Example:**
```csharp
// 发送GET请求
var response = HTTPHelper.Get("https://api.example.com/data");
var content = response.ReadAsStringAsync().Result;

// 发送GET请求并反序列化为指定类型
var user = HTTPHelper.Get<User>("https://api.example.com/user/123");

// 发送POST请求
var result = HTTPHelper.Post("https://api.example.com/users", "{\"name\":\"John\"}");
```

### IniHelper
用于处理INI文件的操作类。

**Methods:**
- `ReadSectionNames(string iniPath)` - 读取INI文件中所有节点名称
- `ReadAllItems(string iniPath, string section)` - 获取指定节点中的所有条目
- `ReadAllItemKeys(string iniPath, string section)` - 获取指定节点中的所有条目的Key列表
- `ReadItemValue(string iniPath, string section, string key, string defaultValue = "")` - 读取指定KEY的字符串型值
- `WriteItems(string iniPath, string section, string items)` - 写入多个键值对到指定节点
- `WriteValue(string iniPath, string section, string key, string value)` - 写入指定的键和值
- `DeleteKey(string iniPath, string section, string key)` - 删除指定节点中的指定键
- `DeleteSection(string iniPath, string section)` - 删除指定节点
- `EmptySection(string iniPath, string section)` - 清空指定节点中的所有内容
- `ToDictionary(string iniPath, string section, char split = '=')` - 获取指定节点下的key和value，返回字典

**Example:**
```csharp
// 读取INI值
string value = IniHelper.ReadItemValue("config.ini", "Section1", "Key1", "默认值");

// 写入INI值
IniHelper.WriteValue("config.ini", "Section1", "Key1", "NewValue");

// 获取指定节点的字典
var dict = IniHelper.ToDictionary("config.ini", "Section1");
```

### ZipHelper
压缩与解压工具类。

**Methods:**
- `PackFiles(string outputFileName, string dirBePacked)` - 压缩文件夹
- `UnpackFiles(string fileBeUnpacked, string outputDir)` - 解压缩

**Example:**
```csharp
// 压缩文件夹
ZipHelper.PackFiles("output.zip", "sourceFolder");

// 解压文件
ZipHelper.UnpackFiles("archive.zip", "outputFolder");
```

### ConfigFileHelper
配置文件操作类。

**Properties:**
- `NormalConfigFileName` - config.json
- `SystemConfigFileName` - system.json
- `SetupConfigFileName` - setup.json
- `UserConfigFileName` - user.json
- `ExeFolder` - 当前exe所在目录
- `ConfigFolder` - 配置文件目录
- `LogFolder` - 日志目录
- `DataFolder` - 数据目录
- `TempFolder` - 临时目录

**Methods:**
- `GetFolder(string folderName)` - 获取指定名称的文件夹
- `GetNormalConfigFilePathByExePath()` - 获取默认配置文件路径
- `GetSystemConfigFilePathByExePath()` - 获取系统配置文件路径
- `GetSetupConfigFilePathByExePath()` - 获取安装配置文件路径
- `GetUserConfigFilePathByExePath()` - 获取用户配置文件路径

**ConfigBase Methods:**
- `Save<T>(T t, string filepath)` - 保存配置到文件
- `Load<T>(string filepath, out T t)` - 从文件加载配置

**Example:**
```csharp
// 获取配置文件路径
string configPath = ConfigFileHelper.GetNormalConfigFilePathByExePath();

// 保存配置
var config = new MyConfig { Setting1 = "value" };
config.Save(configPath);

// 加载配置
ConfigFileHelper.Load(configPath, out MyConfig loadedConfig);
```

### PathHelper
路径操作帮助类（具体功能需查看源代码）。

### IOHelper
输入输出操作帮助类（具体功能需查看源代码）。

### XMLHelper
XML文件操作类（具体功能需查看源代码）。

### AdvancedStopWatch
高级计时器类，支持毫秒级精度计时、暂停/恢复、多圈计时等功能。

**Example:**
```csharp
var sw = new AdvancedStopWatch();
sw.Start();
// ... 执行操作 ...
sw.Stop();
Console.WriteLine($"Elapsed: {sw.ElapsedMilliseconds}ms");
```

### FastDateTime
快速日期时间操作类（具体功能需查看源代码）。

### FolderDeleteService
文件夹删除服务类（具体功能需查看源代码）。

### SmartFileWriter
智能文件写入器（具体功能需查看源代码）。

### EventManager
事件管理器（具体功能需查看源代码）。

### TCP Helpers
位于 `Helpers/TCP` 目录下的 Socket 客户端和服务器帮助类。

## ChineseSpell

### PinYin
汉字拼音转换类，提供获取汉字拼音首字母的功能。

**Methods:**
- `GetFirstLetter(string hz)` - 获取汉字字符串的首字母缩写

**Example:**
```csharp
string result = PinYin.GetFirstLetter("中国");
// result: "ZG"
```

### EcanConvertToCh
繁简体汉字转换类。

**Methods:**
- `ToTraditionalChinese(string str)` - 简体转繁体
- `ToSimplifiedChinese(string str)` - 繁体转简体

**Example:**
```csharp
string traditional = EcanConvertToCh.ToTraditionalChinese("中国");
// traditional: "中國"

string simplified = EcanConvertToCh.ToSimplifiedChinese("中國");
// simplified: "中国"
```

## O2Html
对象到 HTML 的序列化库，支持将 .NET 对象、集合、DataTable、JSON 等各种类型转换为友好的 HTML 展示。

### HtmlSerializer
核心序列化类，负责将 .NET 对象转换为 HTML 文档。

**Options:**
- `MaxDepth` - 最大序列化深度，防止循环引用导致栈溢出
- `ReferenceLoopHandling` - 循环引用处理策略（Ignore/Error/Serialize）
- `Formatting` - HTML 输出格式（压缩/缩进）
- `SerializePublicProperties` - 是否序列化公共属性（默认 true）
- `SerializeNonPublicProperties` - 是否序列化非公共属性（private/protected/internal，默认 false）
- `SerializePublicFields` - 是否序列化公共字段（默认 false）
- `SerializeNonPublicFields` - 是否序列化非公共字段（默认 false；自动属性后备字段等编译器生成字段始终排除）

**Supported Types:**
- 基本类型（string, int, DateTime 等）
- 集合类型（List, Array, Dictionary 等）
- DataTable / DataSet
- 匿名类型
- 元组 (Tuple / ValueTuple)
- JSON 文档（JsonDocument / JsonElement）
- XML 节点（XNode / XmlNode）
- 多维数组
- 内存流（Memory / ReadOnlyMemory）
- 文件系统信息（FileInfo / DirectoryInfo）

**Example:**
```csharp
var serializer = new HtmlSerializer();
string html = serializer.Serialize(new { Name = "Alice", Age = 30 });

// 同时序列化公共属性、私有属性与字段
var options = new HtmlSerializerOptions
{
    SerializeNonPublicProperties = true,
    SerializePublicFields = true,
    SerializeNonPublicFields = true,
};
Node node = HtmlSerializer.Serialize(myObject, options);
```

### O2HtmlHelper
WPF 辅助类，负责把序列化结果渲染到 WebBrowser，并支持浅色/深色主题。

**Methods:**
- `Render(WebBrowser, object)` - 用默认选项渲染
- `Render(WebBrowser, object, HtmlSerializerOptions)` - 用自定义选项渲染
- `WrapWithStyle(string bodyHtml)` - 把 HTML 片段套进完整文档
- `WrapWithStyle(string bodyHtml, O2HtmlTheme theme)` - 指定主题（`O2HtmlTheme.Light` / `O2HtmlTheme.Dark`）

### HtmlConverter
静态入口类，提供简洁的序列化方法。

**Methods:**
- `ToHtml(object obj)` - 使用默认设置将对象转换为 HTML
- `ToHtml(object obj, HtmlSerializerOptions options)` - 使用自定义设置转换

**Example:**
```csharp
string html = HtmlConverter.ToHtml(myObject);
```

## Office

### CSV
CSV文件操作相关类。

**Main Classes:**
- `CSVLite` - 轻量级CSV处理类
- `CsvDataBase` - CSV数据基类
- `CsvDataNormal<T>` - 普通CSV数据类
- `CsvDataBlank` - 空CSV数据类
- `CsvDataCustom` - 自定义CSV数据类
- `CsvOprHelper` - CSV操作帮助类
- `CsvServiceExtensions` - CSV服务扩展类

**CsvOprHelper Methods:**
- `ToDT<T>(List<T> datas, bool isUseDisplayName = false)` - 将列表转换为DataTable
- `ToDT<T>(T data, bool isUseDisplayName = false)` - 将单个对象转换为DataTable
- `ToCSV(DataTable dt, bool isWriteColumnName = true)` - 将DataTable转换为CSV格式
- `ToCSV(List<CsvDataBase> csvDatas)` - 将CSV数据列表转换为CSV格式
- `SaveToFile(StringBuilder sb, string filename)` - 保存到文件
- `AppendDataToFile(StringBuilder sb, string filename)` - 追加到文件
- `ReadCsvToList(string csvPath)` - 读取CSV到嵌套列表
- `ReadCsvToListEntity<T>(string csvPath, bool useDisplayName = false, ...)` - 读取CSV到实体列表

**Example:**
```csharp
var students = new List<Student> { /* ... */ };

// 创建CSV文件
var csv = CsvOprHelper.ToCSV(new List<CsvDataBase> {
    new CsvDataNormal<Student>(students)
});
csv.SaveToFile("students.csv");

// 从CSV文件读取实体列表
var users = CsvOprHelper.ReadCsvToListEntity<User>("data.csv", useDisplayName: true);
```

### Excel
Excel导出相关类。

**Classes:**
- `ExcelExporter` - Excel导出器
- `IExcelExporter` - Excel导出器接口

**Example:**
```csharp
// Excel导出示例（具体用法需查看源代码）
```

## Validation

### ValidationHelper
验证帮助类，提供参数验证方法。

**Methods:**
- `Assert(bool condition, string message)` - 断言条件为真
- `MustLessThan<T>(T argument, T limit)` - 验证参数小于限制
- `MustMoreThan<T>(T argument, T limit)` - 验证参数大于限制
- `InRange<T>(T argument, T low, T high)` - 验证参数在范围内
- `ThrowIfNull<T>(T argument)` - 验证参数不为null
- `ArrayLengthNotEqualZero<T>(T[] argument)` - 验证数组长度不为0
- `CollectionCountNotEqualZero<T>(ICollection<T> argument)` - 验证集合元素数量不为0

**Example:**
```csharp
// 验证参数范围
ValidationHelper.InRange(5, 1, 10, "Value must be between 1 and 10");

// 验证参数不为null
ValidationHelper.ThrowIfNull(myObject, "myObject cannot be null");

// 验证参数小于限制
ValidationHelper.MustLessThan(5, 10, "Value must be less than 10");
```

### FormatValidationHelper
格式验证帮助类（具体功能需查看源代码）。

## Reflection

### ClassHelper
反射类帮助类，提供类的辅助方法。

**Methods:**
- `SetDisplayName<T>(string propertyName, string newDisplayName)` - 设置属性显示名称
- `SetDescription<T>(string propertyName, string newDesc)` - 设置属性描述
- `SetBrowsable<T>(string propertyName, bool isBrowsable)` - 设置属性是否可见
- `SetCategory<T>(string propertyName, string newCate)` - 设置属性类别
- `GetFieldValue<TInstance, TResult>(TInstance t, string name)` - 获取字段值
- `GetPropertyValue<TInstance, TResult>(TInstance t, string name)` - 获取属性值
- `SetInstanceFieldValue<TInstance, TValue>(TInstance instance, string name, TValue value)` - 设置实例字段值
- `SetInstancePropertyValue<TInstance, TValue>(TInstance instance, string name, TValue value)` - 设置实例属性值
- `GetStaticFieldValue<TClass, TResult>(string name)` - 获取静态字段值
- `SetStaticFieldValue<TClass, TValue>(string name, TValue value)` - 设置静态字段值
- `InvokeInstanceMethod<TInstance, TResult>(TInstance instance, string name, params object[] args)` - 调用实例方法
- `InvokeStaticMethod<TClass, TResult>(string name, params object[] args)` - 调用静态方法
- `GetStaticMethods<TClass>()` - 获取静态方法列表
- `GetInstanceMethods<TInstance>()` - 获取实例方法列表
- `IsInstanceOfGenericType<T>(T obj, Type genericType)` - 判断对象是否是指定泛型类型

**Example:**
```csharp
// 设置属性的显示名称
ClassHelper.SetDisplayName<Person>("Name", "姓名");

// 设置属性是否可见
ClassHelper.SetBrowsable<Person>("Age", false);

// 设置属性分类
ClassHelper.SetCategory<Person>("Email", "联系信息");

// 获取属性值
var name = ClassHelper.GetPropertyValue<Person, string>(person, "Name");

// 调用方法
ClassHelper.InvokeInstanceMethod(person, "SetName", "John");
```

### EventHelper
事件帮助类（具体功能需查看源代码）。

## DynamicLoad

### DynamicLoadHelper
动态加载DLL的帮助类。

**Methods:**
- `LoadDll<T>(string dllName, string namespaceName, string typeName, out string message)` - 加载指定的DLL并获取指定类型的实例
- `LoadDll<T>(string dllName, string namespaceName, string typeName, object[] args, out string message)` - 加载指定的DLL并获取指定类型的实例（带参数）
- `GetDllModelsFromFolder(string folder, Type baseTypeFilter = null)` - 从指定文件夹获取DllModel列表
- `GetDllModelFromFile(string file, Type baseTypeFilter = null)` - 从指定文件获取DllModel
- `FindSpecifiedTypeInheritFromAssembly(Assembly assembly, Type baseType = null)` - 从程序集中查找指定类型的子类
- `FindSpecifiedTypeInheritFromAssembliesAndSpecifiedAttributes(string dllFile, Type[] baseTypes, Type[] attributes)` - 从DLL中查找指定特性的类型
- `FindSpecifiedTypeInheritFromFolderAndSpecifiedAttributes(string folder, Type[] baseTypes, Type[] attributes)` - 从文件夹中查找指定特性的类型
- `GetAllNamespacesFromDll(string dllFile)` - 获取DLL中所有命名空间
- `GetTypesFromDll(string dllFile)` - 获取DLL中所有类型
- `CreateObjectFromType<T>(out string message, object[] args = null)` - 创建指定类型的实例
- `FindSpecifiedTypeHasAttributeFromAssembly(Assembly assembly, Type attr)` - 从程序集中查找附加了指定Attribute的类型
- `FindSpecifiedPropertyHasAttributeFromType(Type classType, params Type[] attrs)` - 从类型中查找附加了指定Attribute的属性
- `FindSpecifiedMethodHasAttributeFromType(Type classType, Type attr)` - 从类型中查找附加了指定Attribute的方法

**Example:**
```csharp
// 加载DLL并创建实例
string message;
var instance = DynamicLoadHelper.LoadDll<IMyInterface>(
    "MyPlugin.dll", 
    "MyNamespace", 
    "MyClass", 
    out message
);

// 从文件夹中查找特定类型的类
var types = DynamicLoadHelper.FindSpecifiedTypeInheritFromFolderAndSpecifiedAttributes(
    "Plugins", 
    new Type[] { typeof(BasePlugin) }, 
    new Type[] { typeof(PluginAttribute) }
);
```

## DataStruct

### Shape Structures
数据结构相关类，位于 `DataStruct/Structure` 目录。

**Classes:**
- `Point` - 点结构
- `Line` - 线结构
- `Rectangle1D` - 一维矩形
- `Rectangle2D` - 二维矩形
- `Circle` - 圆形
- `Ellipse` - 椭圆
- `Polygon` - 多边形
- `Cross` - 十字形
- `Hexagon` - 六边形
- `Size` - 尺寸
- `IShapeStructure` - 形状结构接口
- `EmptyShape` - 空形状

### Math & Shape Extensions
数学与图形扩展方法，提供形状计算与几何变换功能。

**Extensions (MathExtensions):**
- `Round(double value, int digits)` - 四舍五入
- `ToListAsync<T>(IAsyncEnumerable<T> source)` - 异步转换为列表

**Extensions (ShapeExtensions):**
- `Angle(Point origin, Point p1)` - 计算三点间角度
- `DistanceToLine(Point p, Point lineStart, Point lineEnd)` - 计算点到线段的距离
- `Translate(Point p, double dx, double dy)` - 平移点
- `Rotate(Point p, double angleDeg, Point center)` - 绕中心旋转点
- `ExtendLine(Point start, Point end, double distance)` - 延长线段
- `ProjectionOfLine(Point p, Point lineStart, Point lineEnd)` - 计算点在线段上的投影
- `Centroid(params Point[] points)` - 计算点集的质心
- `IsIntersect(Line l1, Line l2)` - 判断两条线段是否相交

**Example:**
```csharp
// 计算两点间距离
var point1 = new Point(0, 0);
var point2 = new Point(3, 4);
var distance = point1.DistanceToLine(point2, new Point(1, 1));

// 旋转点
var rotatedPoint = point1.Rotate(45, new Point(0, 0));

// 计算质心
var centroid = Point.Centroid(new Point(0,0), new Point(10,0), new Point(5,10));
```

## Mvvm

### RelayCommand
命令实现类，用于MVVM模式。

**Example:**
```csharp
// 创建命令
var command = new RelayCommand(param => ExecuteMethod(), param => CanExecuteMethod());
```

### VMBase
ViewModel基类，提供基本的MVVM功能。

**Example:**
```csharp
// 继承VMBase
public class MyViewModel : VMBase
{
    private string _name;
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }
}
```

## TaskHelper

### BackgroundTaskManager
后台任务管理器，支持任务的注册、启动、暂停、取消和状态监控。

**Features:**
- 任务生命周期管理（启动、暂停、恢复、取消）
- 任务状态变更通知
- 支持异步任务执行
- 错误处理与日志记录

**Example:**
```csharp
// 创建后台任务
var task = new BackgroundTaskManager("MyTask", async (ct, reporter) =>
{
    while (!ct.IsCancellationRequested)
    {
        await Task.Delay(1000, ct);
        reporter.ReportProgress("Working...");
    }
});
task.Start();
```

### ThreadPoolManager
线程池管理器，提供线程池的创建、调度和监控功能。

**Example:**
```csharp
// 使用线程池管理器（具体用法需查看源代码）
```

### AsyncManualResetEvent
异步手动重置事件，用于异步协调。

**Methods:**
- `Set()` - 将事件设置为有信号状态
- `Reset()` - 将事件设置为无信号状态
- `WaitAsync()` - 异步等待事件变为有信号状态

**Example:**
```csharp
var resetEvent = new AsyncManualResetEvent(false);

// 等待信号
await resetEvent.WaitAsync();

// 发送信号
resetEvent.Set();
```
