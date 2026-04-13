// Unity WebGL专用Node.js服务器
// 解决Python服务器绑定问题

const http = require('http');
const fs = require('fs');
const path = require('path');
const url = require('url');

const PORT = 8085;
const HOST = '0.0.0.0'; // 绑定到所有接口

// MIME类型映射
const mimeTypes = {
    '.html': 'text/html',
    '.js': 'application/javascript',
    '.css': 'text/css',
    '.json': 'application/json',
    '.png': 'image/png',
    '.jpg': 'image/jpg',
    '.gif': 'image/gif',
    '.ico': 'image/x-icon',
    '.wasm': 'application/wasm',
    '.data': 'application/octet-stream',
    '.mem': 'application/octet-stream',
    '.symbols': 'application/octet-stream'
};

const server = http.createServer((req, res) => {
    console.log(`${new Date().toISOString()} - ${req.method} ${req.url}`);
    
    // 解析请求路径
    let filePath = '.' + req.url;
    if (filePath === './') {
        filePath = './index.html';
    }
    
    // 获取文件扩展名
    const extname = path.extname(filePath).toLowerCase();
    
    // 特殊处理：LIB目录下的无后缀文件
    let contentType = mimeTypes[extname];
    if (!contentType) {
        // 如果是LIB目录下的文件，默认为二进制流
        if (filePath.includes('/LIB/') || filePath.includes('\\LIB\\')) {
            contentType = 'application/octet-stream';
        } else {
            // 其他无后缀文件也按二进制处理
            contentType = 'application/octet-stream';
        }
    }
    
    // 读取文件
    fs.readFile(filePath, (error, content) => {
        if (error) {
            if (error.code === 'ENOENT') {
                console.log(`❌ 文件不存在: ${filePath}`);
                res.writeHead(404, { 'Content-Type': 'text/html' });
                res.end('<h1>404 - File Not Found</h1>');
            } else {
                console.log(`❌ 服务器错误: ${error.code} - ${filePath}`);
                res.writeHead(500);
                res.end('Server Error: ' + error.code);
            }
        } else {
            console.log(`✅ 文件加载成功: ${filePath} (${contentType})`);
            
            // 设置响应头（Unity WebGL必需）
            const headers = {
                'Content-Type': contentType,
                'Cross-Origin-Opener-Policy': 'same-origin',
                'Cross-Origin-Embedder-Policy': 'require-corp',
                'Access-Control-Allow-Origin': '*',
                'Access-Control-Allow-Headers': '*',
                'Access-Control-Allow-Methods': 'GET, POST, OPTIONS',
                'Cache-Control': 'no-cache, no-store, must-revalidate',
                'Pragma': 'no-cache',
                'Expires': '0'
            };
            
            res.writeHead(200, headers);
            res.end(content, 'utf-8');
        }
    });
});

server.listen(PORT, HOST, () => {
    console.log('='.repeat(60));
    console.log('Unity WebGL Node.js服务器');
    console.log('='.repeat(60));
    console.log(`服务器运行在: http://${HOST}:${PORT}`);
    console.log(`本机访问: http://localhost:${PORT}`);
    console.log(`局域网访问: http://192.168.1.123:${PORT}`);
    console.log('='.repeat(60));
    console.log('按 Ctrl+C 停止服务器');
});

// 优雅关闭
process.on('SIGINT', () => {
    console.log('\n服务器正在关闭...');
    server.close(() => {
        console.log('服务器已关闭');
        process.exit(0);
    });
});