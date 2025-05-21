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
---

### 1. 缓存操作类 (`CacheHelper`)
| 功能描述 | 方法名 | 参数 | 返回值 | 说明 |
|---------|--------|------|--------|------|
| 设置缓存 | SetCache | string key, object value | void | 将对象存储到缓存中 |
| 获取缓存 | GetCache | string key | object | 根据键获取缓存对象 |
| 删除缓存 | RemoveCache | string key | void | 根据键删除缓存 |

---

### 2. 反射加载DLL类 (`LibLoadHelper`)
| 功能描述 | 方法名 | 参数 | 返回值 | 说明 |
|---------|--------|------|--------|------|
| 加载DLL并创建实例 | LoadDll | string dllPath, string namespaceName, string className, out string message | T | 通过反射加载指定DLL并创建指定类的实例 |
| 获取类型 | GetTypeFromDll | string dllPath, string fullName | Type | 从指定 DLL 获取指定类型的 Type 对象 |

---

### 3. HTTP请求帮助类 (`HTTPHelper`)
| 功能描述 | 方法名 | 参数 | 返回值 | 说明 |
|---------|--------|------|--------|------|
| 发送GET请求 | GetRequest | string url | string | 发送 GET 请求并返回响应结果 |
| 发送POST请求 | PostRequest | string url, string jsonData | string | 发送 POST 请求并返回响应结果 |

---

### 4. INI文件操作类 (`IniHelper`)
| 功能描述 | 方法名 | 参数 | 返回值 | 说明 |
|---------|--------|------|--------|------|
| 读取INI值 | ReadItemValue | string section, string key, string filePath, string defaultValue | string | 从指定的INI文件中读取值 |
| 写入INI值 | WriteItems | string section, string key, string value, string filePath | void | 向指定的INI文件写入值 |

---

### 5. XML文件操作类 (`XMLHelper`)
| 功能描述 | 方法名 | 参数 | 返回值 | 说明 |
|---------|--------|------|--------|------|
| 序列化对象为XML | SerializeToXml | object obj, string filePath | void | 将对象序列化为XML文件 |
| 反序列化XML为对象 | DeserializeFromXml | string filePath, Type type | object | 将XML文件反序列化为指定类型的对象 |

---

### 6. 压缩与解压工具类 (`ZipHelper`)
| 功能描述 | 方法名 | 参数 | 返回值 | 说明 |
|---------|--------|------|--------|------|
| 压缩文件夹 | PackFiles | string zipFilePath, string sourceFolder | void | 将指定文件夹压缩为ZIP文件 |
| 解压文件 | UnpackFiles | string zipFilePath, string outputFolder | void | 将ZIP文件解压到指定目录 |

---

### 7. CSV文件操作类 (`CSVHelper`)
| 功能描述 | 方法名 | 参数 | 返回值 | 说明 |
|---------|--------|------|--------|------|
| 导出数据到CSV | ToCSV | List<CsvDataBase> dataList | string | 将数据列表导出为CSV格式字符串 |
| 保存CSV文件 | SaveToFile | string filePath | void | 将CSV格式字符串保存为文件 |

---

### 8. 类型辅助类 (`ClassHelper`)
| 功能描述 | 方法名 | 参数 | 返回值 | 说明 |
|---------|--------|------|--------|------|
| 设置属性显示名称 | SetDisplayName | Type type, string propertyName, string displayName | void | 给指定类的属性设置自定义显示名称 |
| 设置属性是否可见 | SetBrowsable | Type type, string propertyName, bool isBrowsable | void | 设置指定属性在设计时是否可见 |
| 设置属性分类 | SetCategory | Type type, string propertyName, string category | void | 给指定属性设置分类信息 |

---

### 9. 事件管理类 (`EventHelper`)
| 功能描述 | 方法名 | 参数 | 返回值 | 说明 |
|---------|--------|------|--------|------|
| 注册事件 | RegisterEvent | EventHandler handler | void | 注册一个事件处理程序 |
| 触发事件 | TriggerEvent | object sender, EventArgs e | void | 触发已注册的事件 |
| 移除事件 | UnregisterEvent | EventHandler handler | void | 移除一个事件处理程序 |

---

### 10. 数据验证类 (`ValidationHelper`)
| 功能描述 | 方法名 | 参数 | 返回值 | 说明 |
|---------|--------|------|--------|------|
| 验证对象有效性 | ValidateObject | object obj | bool | 验证传入的对象是否符合预定义的规则 |
| 获取验证错误信息 | GetValidationErrors | object obj | List<string> | 获取对象的验证错误信息列表 |

---

### 11. 配置文件操作类 (`ConfigFileHelper`)
| 功能描述 | 方法名 | 参数 | 返回值 | 说明 |
|---------|--------|------|--------|------|
| 读取配置 | ReadConfig | string filePath | T | 从指定路径读取配置文件并返回强类型对象 |
| 保存配置 | SaveConfig | T config | void | 将强类型对象保存为配置文件 |

---

### 12. 扩展方法类
扩展方法通常用于增强现有类的功能，以下是一些常用扩展方法：
| 功能描述 | 方法名 | 参数 | 返回值 | 说明 |
|---------|--------|------|--------|------|
| 对象克隆 | Clone | T source | T | 深度克隆对象 |
| 列表排序 | SortByProperty | List<T> list, Func<T, object> selector | List<T> | 根据指定属性对列表进行排序 |

---