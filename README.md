# Unity Hub Clone

这是一个使用WPF UI框架开发的Unity Hub克隆版本，提供了类似Unity Hub的功能。

## 功能特性

- 🎮 **项目管理**: 添加、打开和管理Unity项目
- 🔧 **引擎管理**: 管理Unity引擎安装
- ⚙️ **设置页面**: 配置应用程序设置
- 🎨 **现代UI**: 使用WPF UI框架，支持深色主题
- 📁 **文件集成**: 与Windows资源管理器集成

## 系统要求

- Windows 10/11
- .NET 8.0 或更高版本
- Visual Studio 2022 或更高版本

## 安装和运行

1. 克隆项目到本地
2. 使用Visual Studio打开 `UnityHub.sln`
3. 还原NuGet包
4. 按F5运行项目

## 项目结构

```
UnityHub/
├── UnityHub.sln                 # 解决方案文件
├── UnityHub/
│   ├── UnityHub.csproj         # 项目文件
│   ├── App.xaml                # 应用程序XAML
│   ├── App.xaml.cs             # 应用程序代码
│   ├── MainWindow.xaml         # 主窗口XAML
│   ├── MainWindow.xaml.cs      # 主窗口代码
│   ├── EnginesPage.xaml        # 引擎页面XAML
│   ├── EnginesPage.xaml.cs     # 引擎页面代码
│   ├── SettingsPage.xaml       # 设置页面XAML
│   ├── SettingsPage.xaml.cs    # 设置页面代码
│   └── Assets/
│       └── icon.ico            # 应用程序图标
└── README.md                   # 项目说明
```

## 主要功能

### 项目管理
- 自动扫描Unity项目文件夹
- 手动添加项目
- 打开项目（使用对应的Unity编辑器）
- 在资源管理器中显示项目

### 引擎管理
- 自动检测已安装的Unity引擎
- 手动添加引擎路径
- 管理引擎版本

### 设置
- 配置默认项目路径
- 配置默认引擎路径
- 主题设置

## 技术栈

- **WPF**: Windows Presentation Foundation
- **WPF UI**: 现代UI框架
- **C#**: 编程语言
- **.NET 8**: 运行时框架

## 开发说明

这个项目使用了WPF UI框架来提供现代化的用户界面。主要特点：

1. **响应式设计**: 界面会根据窗口大小自动调整
2. **主题支持**: 支持深色和浅色主题
3. **现代化控件**: 使用WPF UI提供的现代化控件
4. **MVVM模式**: 使用数据绑定和命令模式

## 注意事项

- 确保Unity项目文件夹包含 `ProjectSettings/ProjectVersion.txt` 文件
- Unity引擎路径需要包含 `Editor/Unity.exe` 文件
- 某些功能可能需要管理员权限

## 许可证

MIT License



