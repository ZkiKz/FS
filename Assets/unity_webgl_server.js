// Unity WebGL专用优化服务器
// 解决所有可能的文件加载问题

const http = require('http');
const fs = require('fs');
const path = require('path');

const PORT = 3456;
const HOST = '0.0.0.0';

// 完整的MIME类型映射（包含Unity特殊文件）
const mimeTypes = {
    // 标准Web文件
    '.html': 'text/html; charset=utf-8',
    '.js': 'application/javascript; charset=utf-8',
    '.css': 'text/css; charset=utf-8',
    '.json': 'application/json; charset=utf-8',
    
    // 图片文件
    '.png': 'image/png',
    '.jpg': 'image/jpeg',
    '.jpeg': 'image/jpeg',
    '.gif': 'image/gif',
    '.ico': 'image/x-icon',
    '.svg': 'image/svg+xml',
    
    // Unity WebGL特殊文件
    '.wasm': 'application/wasm',
    '.data': 'application/octet-stream',
    '.mem': 'application/octet-stream',
    '.symbols': 'application/octet-stream',
    '.unityweb': 'application/octet-stream'
};

// 特殊处理的文件路径模式
const specialPaths = [
    '/LIB/',
    '\\LIB\\',
    '/Build/',
    '\\Build\\',
    '/TemplateData/',
    '\\TemplateData\\'
];

const server = http.createServer((req, res) => {
    const requestUrl = req.url;
    console.log(`[${new Date().toISOString()}] ${req.method} ${requestUrl}`);
    
    // 处理根路径
    let filePath = '.' + requestUrl;
    if (filePath === './') {
        filePath = './index.html';
    }
    
    // 规范化路径（解决Windows/Unix路径差异）
    filePath = filePath.replace(/\\/g, '/');
    
    // 获取文件扩展名
    const extname = path.extname(filePath).toLowerCase();
    
    // 确定Content-Type
    let contentType = mimeTypes[extname];
    
    // 特殊处理：Unity WebGL文件
    if (!contentType) {
        const isSpecialPath = specialPaths.some(pattern => filePath.includes(pattern));
        if (isSpecialPath) {
            contentType = 'application/octet-stream';
            console.log(`🔧 特殊路径文件: ${filePath} -> ${contentType}`);
        } else {
            // 尝试根据文件内容猜测类型
            contentType = 'application/octet-stream';
        }
    }
    
    // 读取文件
    fs.readFile(filePath, (error, content) => {
        if (error) {
            if (error.code === 'ENOENT') {
                console.log(`❌ 文件不存在: ${filePath}`);
                
                // 尝试大小写不敏感的查找（Windows环境）
                findCaseInsensitiveFile(filePath, (caseError, caseContent, caseFilePath) => {
                    if (caseError) {
                        res.writeHead(404, { 'Content-Type': 'text/html; charset=utf-8' });
                        res.end(`<h1>404 - File Not Found</h1><p>${filePath}</p>`);
                    } else {
                        console.log(`✅ 大小写修正: ${filePath} -> ${caseFilePath}`);
                        sendFile(res, caseContent, contentType, caseFilePath);
                    }
                });
            } else {
                console.log(`❌ 服务器错误: ${error.code} - ${filePath}`);
                res.writeHead(500);
                res.end(`Server Error: ${error.code}`);
            }
        } else {
            sendFile(res, content, contentType, filePath);
        }
    });
});

// 发送文件响应
function sendFile(res, content, contentType, filePath) {
    console.log(`✅ 文件加载成功: ${filePath} (${contentType})`);
    
    // Unity WebGL必需的响应头
    const headers = {
        'Content-Type': contentType,
        'Cross-Origin-Opener-Policy': 'same-origin',
        'Cross-Origin-Embedder-Policy': 'require-corp',
        'Access-Control-Allow-Origin': '*',
        'Access-Control-Allow-Headers': '*',
        'Access-Control-Allow-Methods': 'GET, POST, OPTIONS, HEAD',
        'Cache-Control': 'no-cache, no-store, must-revalidate',
        'Pragma': 'no-cache',
        'Expires': '0',
        'X-Content-Type-Options': 'nosniff'
    };
    
    res.writeHead(200, headers);
    res.end(content);
}

// 大小写不敏感的文件查找（Windows环境）
function findCaseInsensitiveFile(originalPath, callback) {
    const dir = path.dirname(originalPath);
    const filename = path.basename(originalPath);
    
    fs.readdir(dir, (err, files) => {
        if (err) {
            callback(err);
            return;
        }
        
        const foundFile = files.find(file => 
            file.toLowerCase() === filename.toLowerCase()
        );
        
        if (foundFile) {
            const correctedPath = path.join(dir, foundFile);
            fs.readFile(correctedPath, (readErr, content) => {
                callback(readErr, content, correctedPath);
            });
        } else {
            callback(new Error('File not found'));
        }
    });
}

server.listen(PORT, HOST, () => {
    console.log('='.repeat(70));
    console.log('🚀 Unity WebGL优化服务器启动成功');
    console.log('='.repeat(70));
    console.log(`📍 服务器地址: http://${HOST}:${PORT}`);
    console.log(`💻 本机访问: http://localhost:${PORT}`);
    console.log(`🌐 局域网访问: http://192.168.1.123:${PORT}`);
    console.log('='.repeat(70));
    console.log('📋 功能特性:');
    console.log('  ✅ 自动处理LIB目录无后缀文件');
    console.log('  ✅ 大小写不敏感文件查找');
    console.log('  ✅ 完整的Unity WebGL安全头');
    console.log('  ✅ 详细的文件加载日志');
    console.log('='.repeat(70));
    console.log('⏹️  按 Ctrl+C 停止服务器');
});

// 优雅关闭
process.on('SIGINT', () => {
    console.log('\n🛑 服务器正在关闭...');
    server.close(() => {
        console.log('✅ 服务器已关闭');
        process.exit(0);
    });
});