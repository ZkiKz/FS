# Unity WebGL服务器防火墙配置脚本
# 解决本地IP无法访问问题

Write-Host "=== Unity WebGL服务器防火墙配置 ===" -ForegroundColor Green
Write-Host ""

# 检查现有规则
Write-Host "检查现有防火墙规则..." -ForegroundColor Yellow
$existingRule = Get-NetFirewallRule -DisplayName "Python HTTP Server Port 8000" -ErrorAction SilentlyContinue

if ($existingRule) {
    Write-Host "✅ 防火墙规则已存在" -ForegroundColor Green
    Write-Host "规则状态: $($existingRule.Enabled)" -ForegroundColor Cyan
} else {
    Write-Host "❌ 未找到防火墙规则，正在创建..." -ForegroundColor Red
    
    # 创建新的防火墙规则
    try {
        New-NetFirewallRule -DisplayName "Python HTTP Server Port 8000" `
                           -Direction Inbound `
                           -Protocol TCP `
                           -LocalPort 8000 `
                           -Action Allow `
                           -Profile Any `
                           -Description "允许Unity WebGL服务器通过8000端口访问"
        
        Write-Host "✅ 防火墙规则创建成功" -ForegroundColor Green
    }
    catch {
        Write-Host "❌ 防火墙规则创建失败: $($_.Exception.Message)" -ForegroundColor Red
        Write-Host ""
        Write-Host "可能需要以管理员身份运行此脚本" -ForegroundColor Yellow
        exit 1
    }
}

Write-Host ""
Write-Host "=== 网络连接测试 ===" -ForegroundColor Green

# 测试本地连接
Write-Host "测试本地连接 (localhost:8000)..." -ForegroundColor Yellow
test-netconnection -ComputerName localhost -Port 8000

Write-Host ""
Write-Host "测试本地IP连接 ($(Get-NetIPAddress -AddressFamily IPv4 -InterfaceAlias Ethernet | Where-Object {$_.IPAddress -like '192.168.*'} | Select-Object -First 1).IPAddress):8000)..." -ForegroundColor Yellow
test-netconnection -ComputerName (Get-NetIPAddress -AddressFamily IPv4 -InterfaceAlias Ethernet | Where-Object {$_.IPAddress -like '192.168.*'} | Select-Object -First 1).IPAddress -Port 8000

Write-Host ""
Write-Host "=== 使用说明 ===" -ForegroundColor Green
Write-Host "1. 启动Unity WebGL服务器:" -ForegroundColor White
Write-Host "   cd 'E:\UnityWork\FuSheng\WebGL'" -ForegroundColor Gray
Write-Host "   python -m http.server 8000 --bind 0.0.0.0" -ForegroundColor Gray
Write-Host ""
Write-Host "2. 访问地址:" -ForegroundColor White
Write-Host "   - 本机: http://localhost:8000" -ForegroundColor Gray
Write-Host "   - 局域网: http://192.168.1.123:8000" -ForegroundColor Gray
Write-Host ""
Write-Host "3. 如果仍有问题，请检查:" -ForegroundColor White
Write-Host "   - 防火墙是否已正确配置" -ForegroundColor Gray
Write-Host "   - Python进程是否绑定到正确端口" -ForegroundColor Gray
Write-Host "   - 网络连接是否正常" -ForegroundColor Gray

Write-Host ""
Write-Host "配置完成!" -ForegroundColor Green