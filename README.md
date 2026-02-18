# MES 查询工具 (MES_PRINT)

基于 .NET 8 的 Windows 窗体应用，用于 MES 相关查询与工艺路线启动。支持按条码查询打印记录、按工单查询未打包条码、按工单启动指定目录下的程序，并具备 SFC/EQ 登录与 Tab 可见性配置。

---

## 环境要求

- **系统**：Windows
- **运行时**：.NET 8（或 Visual Studio 2022 含 .NET 8 桌面开发工作负载）

---

## 如何运行

1. 克隆仓库后，用 **Visual Studio 2022** 打开 **`MES_PRINT.sln`**（或直接打开 `MES_PRINT.csproj`）。
2. 还原 NuGet 包（首次打开时 VS 会自动执行）。
3. 按 **F5** 运行。

程序配置与登录状态保存在本机 `%LocalAppData%\MES_PRINT\config.json`，无需随仓库分发。

---

## 功能概览

| 功能 | 说明 |
|------|------|
| **Print Log** | 按 Barcode 查询 `MES_PRINT_LOG`，结果在 DataGridView 中显示，支持单元格复制。 |
| **Barcodes Not Packed** | 按 WO_NO 查询未打包条码（`MES_WO_BARCODE` 且不在 `MES_Packing_Detail` 中），结果在 DataGridView 中显示。 |
| **Route Launch** | 选择文件夹并输入 Work_Order，根据工单从数据库取 `MODEL_NO`，在目录中匹配 `.lnk`/`.exe` 并启动；可保存上次选择的文件夹路径。 |
| **登录** | 顶部 Login：账号 SFC 或 EQ，密码 12345；同一时间仅允许一个账号登录；SFC 登录后可打开「Tab 设置」控制三个 Tab 的显示与隐藏；Route Launch 的「文件夹」行仅在 SFC 或 EQ 任一登录时显示。 |

---

## 项目文件说明

### 解决方案与项目

| 文件 | 作用 |
|------|------|
| **MES_PRINT.sln** | Visual Studio 解决方案文件，包含本项目的引用。用 VS 打开此文件即可加载整个项目。 |
| **MES_PRINT.csproj** | 项目文件。定义目标框架（.NET 8 Windows）、WinForms、NuGet 依赖（如 `Microsoft.Data.SqlClient`），以及发布选项（单文件、自包含、win-x64）等。 |

### 程序入口与主界面

| 文件 | 作用 |
|------|------|
| **Program.cs** | 程序入口。设置 STA 线程、初始化应用程序配置并启动主窗体 `MainForm`。 |
| **MainForm.cs** | 主窗体。包含：登录/登出与「Tab 设置」按钮、三个 Tab（Print Log / Barcodes Not Packed / Route Launch）、DataGridView 与查询逻辑、Route Launch 的文件夹选择与按工单启动、数据库连接与 SQL 执行。加载与保存 `AppConfig`，并根据登录状态与配置控制 Tab 与「文件夹」行的显示。 |

### 配置与持久化

| 文件 | 作用 |
|------|------|
| **AppConfig.cs** | 配置管理。将「Route Launch 文件夹路径」及三个 Tab 的启用状态读写到 `%LocalAppData%\MES_PRINT\config.json`，便于打包后仍可写、多机独立配置。提供 `Load()` / `Save()` 及对应静态属性。 |

### 弹窗与辅助窗体

| 文件 | 作用 |
|------|------|
| **LoginForm.cs** | 登录弹窗。提供账号下拉（SFC / EQ）与密码输入，校验通过后通过 `LoggedInAccount` 返回 "SFC" 或 "EQ"，取消或失败为 null。 |
| **TabSettingsForm.cs** | Tab 启用设置弹窗。仅 SFC 登录后可用。三个复选框对应 Print Log、Barcodes Not Packed、Route Launch 是否显示；至少保留一个 Tab 启用；确定后由主窗体写回 `AppConfig` 并刷新 Tab 显示。 |
| **InputBoxForm.cs** | 通用简单输入框。用于标题、提示文字和默认值可配置的单一文本框输入，供需要简单文本输入的地方复用。 |

### 版本控制与忽略

| 文件 | 作用 |
|------|------|
| **.gitignore** | Git 忽略规则。排除 `.vs/`、`bin/`、`obj/`、`*.user`、NuGet 相关等，避免 IDE 与构建产物、用户本机配置进入仓库。 |

---

## 数据库说明

- 程序使用 **Microsoft.Data.SqlClient** 连接 SQL Server，连接字符串写在 `MainForm.cs` 中（示例库：`SFCS_TH_Assembly`）。
- 涉及表包括：`MES_PRINT_LOG`、`MES_WO_BARCODE`、`MES_Packing_Detail`、`MES_WO` 等，具体以代码内 SQL 为准。
- 在其他环境运行或协作时，请根据实际服务器修改连接字符串。

---

## 许可证与维护

请按公司内部规定使用与修改。如有问题可联系项目维护者。
