# Unity WebGL 构建配置指南

## 🔍 问题诊断结果

通过分析您的WebGL构建，发现了以下问题：

### ⚠️ 当前构建的问题
1. **文件结构不标准**：文件分散在 `Build/` 和 `LIB/` 目录
2. **团结引擎特定配置**：使用了非标准的 `createTuanjieInstance`
3. **可能的压缩格式问题**：可能导致文件加载失败

## 🚀 重新构建步骤

### 步骤1：清理现有构建
```bash
# 删除现有的WebGL构建文件夹
rm -rf "E:\UnityWork\FuSheng\WebGL"
```

### 步骤2：Unity构建设置

#### 2.1 打开Build Settings
- **File → Build Settings**
- 选择 **WebGL** 平台
- 点击 **Switch Platform**

#### 2.2 Player Settings配置

**Resolution and Presentation:**
- Default Canvas Width: 1080
- Default Canvas Height: 1920
- Run in Background: ✓ Enabled

**Publishing Settings (关键配置):**
```
Compression Format: Brotli (推荐) 或 Gzip
Decompression Fallback: ✓ Enabled
Code Optimization: Size
Enable Exceptions: None
Data Caching: Disabled (避免缓存问题)
```

**Other Settings:**
- Scripting Backend: IL2CPP
- Api Compatibility Level: .NET Standard 2.1
- Strip Engine Code: ✓ Enabled (减小体积)

### 步骤3：构建选项

在Build Settings窗口中：
- **Development Build**: ❌ 取消勾选（发布版本）
- **Autoconnect Profiler**: ❌ 取消勾选
- **Deep Profiling**: ❌ 取消勾选
- **Script Debugging**: ❌ 取消勾选

### 步骤4：执行构建

1. 点击 **Build** 按钮
2. 选择构建目录：`E:\UnityWork\FuSheng\WebGL`
3. 等待构建完成

## 🔧 构建后验证

### 验证构建文件结构
构建完成后，WebGL目录应该包含：
```
WebGL/
├── Build/
│   ├── WebGL.data
│   ├── WebGL.framework.js
│   ├── WebGL.loader.js
│   └── WebGL.wasm
├── TemplateData/ (可选)
│   ├── UnityProgress.js
│   └── ...
└── index.html
```

### 验证index.html内容
确保index.html使用正确的文件路径：
```html
<script src="Build/WebGL.loader.js"></script>
<script>
  createTuanjieInstance(document.querySelector("#tuanjie-canvas"), {
    dataUrl: "Build/WebGL.data",
    frameworkUrl: "Build/WebGL.framework.js", 
    codeUrl: "Build/WebGL.wasm",
    // ... 其他配置
  });
</script>
```

## 🛠️ 服务器配置

### 使用优化后的Node.js服务器
```bash
cd "E:\UnityWork\FuSheng\WebGL"
node "E:\UnityWork\FuSheng\Assets\unity_webgl_server.js"
```

### 访问地址
- 本机：`http://localhost:8085`
- 局域网：`http://192.168.1.123:8085`

## 🔍 故障排除

### 如果仍然有问题

1. **检查浏览器控制台错误**
   - 打开F12开发者工具
   - 查看Console标签的具体错误信息

2. **验证文件完整性**
   ```bash
   # 检查文件大小
   dir "E:\UnityWork\FuSheng\WebGL\Build"
   ```

3. **测试文件访问**
   ```powershell
   # 测试文件是否可以访问
   Invoke-WebRequest -Uri "http://localhost:8085/Build/WebGL.data" -Method Head
   ```

### 常见错误解决方案

**错误：文件加载失败**
- 重新构建WebGL版本
- 检查压缩格式设置
- 确保服务器MIME类型正确

**错误：团结引擎初始化失败**
- 确认使用正确的团结引擎版本
- 检查WebGL模板设置

## 📋 构建检查清单

- [ ] 清理了旧的WebGL构建
- [ ] 配置了正确的Player Settings
- [ ] 使用了Brotli压缩格式
- [ ] 启用了Decompression Fallback
- [ ] 构建后验证了文件结构
- [ ] 测试了服务器访问

通过按照这个指南重新构建，应该能解决文件加载问题。