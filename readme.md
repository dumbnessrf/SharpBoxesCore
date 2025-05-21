# SharpBoxesCore
[![](https://img.shields.io/github/issues/dumbnessrf/SharpBoxesCore.svg)](https://github.com/zhouie/markdown-emoji/issues)
[![](https://img.shields.io/github/forks/dumbnessrf/SharpBoxesCore.svg)](https://github.com/zhouie/markdown-emoji/network)



集成了一些常用的方法；
如通用的缓存静态操作类、通用的反射加载dll类`LibLoadHelper`、`HTTPHelper`、`IniHelper`、`XMLHelper`、`ZipHelper`、`CSVHelper`、`Preview Features Import`、`ClassHelper`、`EventHelper`、`ValidationHelper`其他是一些通用的扩展方法类

:bowtie:

It integrates some commonly used methods.
Such as the general cache static operation class, the general reflection loading dll class'LibLoadHelper ',' HTTPHelper ',' IniHelper ',' XMLHelper ',' ZipHelper ',' CSVHelper ',' Preview Features Import ',' ClassHelper ',' EventHelper ',' ValidationHelper ', and others are some general extension method classes

Orignal Source:
[SharpBoxesCore](https://github.com/dumbnessrf/SharpBoxesCore)

其他相关工具、扩展
Other Toolkit:
[SharpBoxesCore.Cuts](https://marketplace.visualstudio.com/items?itemName=dumbnessrf.SharpBoxesCore)

其中提供了许多cSharp、xaml有用的代码片段，如`OnPropertyChanged`的完整属性语句，`Task.Run（）=>{}`）的自动环绕；…

which provided lots of csharp、xaml useful code snippets Like full property statement with `OnPropertyChanged`, auto surround with `Task.Run(()=>{ }`);...
# Install
```shell
Install-Package SharpBoxesCore
```


## 功能代码示例

### 1. 反射工具(ClassHelper)
```csharp
// 设置属性的显示名称
ClassHelper.SetDisplayName<Person>("Name", "姓名");
// 设置属性是否可见
ClassHelper.SetBrowsable<Person>("Age", false);
// 设置属性分类
ClassHelper.SetCategory<Person>("Email", "联系信息");
```

### 2. INI文件操作(IniHelper)
```csharp
// 读取INI值
string value = IniHelper.ReadItemValue("Section1", "Key1", "default.ini", "默
认值");
// 写入INI值
IniHelper.WriteItems("Section1", "Key1", "NewValue", "config.ini");
```
### 3. ZIP压缩解压(ZipHelper)
```csharp
// 压缩文件夹
ZipHelper.PackFiles("output.zip", "sourceFolder");
// 解压文件
ZipHelper.UnpackFiles("archive.zip", "outputFolder");
```
### 4. CSV操作(CsvOprHelper)
```csharp
var students = Student.FakeMany(10);
// 创建CSV文件
CsvOprHelper.ToCSV(new List<CsvDataBase> {
    new CsvDataNormal<Student>(students)
}).SaveToFile("students.csv");
```
### 5. DLL动态加载(LibLoadHelper)
```csharp
string message;
var instance = LibLoadHelper.LoadDll<IMyInterface>(
    "MyPlugin.dll", 
    "MyNamespace", 
    "MyClass", 
    out message
);
```
### 6. 配置管理(ConfigFileHelper)
```csharp
// 读取配置
var config = ConfigFileHelper.ReadConfig<AppConfig>();
// 保存配置
ConfigFileHelper.SaveConfig(config);
```

# 主要API一览
