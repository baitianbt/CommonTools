# CommonTools.Core

CommonTools.Core 是一个通用工具类库，提供了常用的辅助方法和扩展功能，帮助开发者提高开发效率。

## 功能特性

### 文件操作
- 文件和目录基础操作
- ZIP压缩和解压
- 文件类型判断和转换

### Office文档处理
- Excel文件读写
- CSV文件处理
- PDF文档操作
- Word文档处理

### 多媒体处理
- 图片处理（调整大小、压缩、水印等）
- 音频处理（格式转换、剪辑等）
- 视频处理（压缩、转换、剪辑等）

### 网络功能
- HTTP请求封装
- FTP操作
- 邮件发送

### 安全功能
- 加密解密（AES、RSA）
- Hash计算（MD5、SHA256）
- 随机密钥生成

### 配置管理
- JSON配置文件处理
- INI文件读写
- 多环境配置支持

### 扩展方法
- 字符串扩展
- 日期时间扩展
- 类型转换扩展

## 开发环境
- .NET 8.0
- C# 12.0

## 依赖包
- System.Text.Json (9.0.1)
- NPOI (2.7.2)
- CsvHelper (33.0.1)
- SixLabors.ImageSharp (3.1.6)
- NAudio (2.2.1)
- FFMpegCore (5.1.0)
- DocumentFormat.OpenXml (3.2.0)
- itext7 (9.0.0)

## 使用说明

1. 添加包引用：
```xml
<PackageReference Include="CommonTools.Core" Version="1.0.0" />
```

2. 引入命名空间：
```csharp
using CommonTools.Core;
```

3. 使用示例：
```csharp
// 文件操作
FileHelper.CreateDirectoryIfNotExists("path/to/dir");
await FileHelper.WriteTextAsync("file.txt", "content");

// Excel操作
ExcelHelper.CreateExcel(data, "output.xlsx");
var content = ExcelHelper.ReadExcel("input.xlsx");

// 图片处理
await ImageHelper.ResizeImageAsync("input.jpg", "output.jpg", 800, 600);
await ImageHelper.CompressImageAsync("input.jpg", "output.jpg", 75);

// 加密解密
var encrypted = EncryptHelper.AesEncrypt("text", "key");
var decrypted = EncryptHelper.AesDecrypt(encrypted, "key");
```

## 许可证

MIT License
