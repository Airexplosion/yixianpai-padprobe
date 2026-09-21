# 手柄探针

诊断用:每帧读 Unity Input 的手柄状态(手柄名/按钮/各轴),把非零的打到日志。用来确认 Unity 能不能读到右摇杆。插上手柄推摇杆看日志。

## 构建

本 mod 需在「弈仙牌 MOD SDK / 加载器」工作区内构建（`.csproj` 用相对路径引用 SDK 项目和本机游戏 DLL，单独 clone 无法直接编译）：

1. 取得 SDK / 加载器工作区（含 `sdk/`、`tools/yx-patch`），把本仓库放到工作区的 `mods/com.yx.padprobe/`。
2. 准备本机 `refs/`：`yx-patch refs --game <游戏目录> --out refs` 从游戏取热更 DLL（厂商版权材料，**不随仓库分发**）。
3. `dotnet build Padprobe.csproj -c Release` → 产物在 `plugins/*.dll`。
4. `yx-patch check plugins/*.dll` 应为 0 error。

产物 DLL 与 `bin/ obj/ plugins/` 不入库（见 `.gitignore`）。
