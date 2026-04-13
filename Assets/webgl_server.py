#!/usr/bin/env python3
"""
Unity WebGL专用服务器
解决文件加载错误：Error loading the file 'LIB/70bde295c53eeb55366600a3151503e8'
"""

import http.server
import socketserver
import os
import sys

class UnityWebGLHTTPRequestHandler(http.server.SimpleHTTPRequestHandler):
    """专门处理Unity WebGL文件的HTTP请求处理器"""
    
    def end_headers(self):
        """添加Unity WebGL所需的HTTP响应头"""
        # 必需的响应头，确保WebAssembly正确加载
        self.send_header('Cross-Origin-Opener-Policy', 'same-origin')
        self.send_header('Cross-Origin-Embedder-Policy', 'require-corp')
        self.send_header('Access-Control-Allow-Origin', '*')
        self.send_header('Access-Control-Allow-Headers', '*')
        self.send_header('Access-Control-Allow-Methods', 'GET, POST, OPTIONS')
        
        # 缓存控制
        self.send_header('Cache-Control', 'no-cache, no-store, must-revalidate')
        self.send_header('Pragma', 'no-cache')
        self.send_header('Expires', '0')
        
        super().end_headers()
    
    def guess_type(self, path):
        """为Unity WebGL文件设置正确的MIME类型"""
        # Unity WebGL特殊文件类型
        if path.endswith('.wasm'):
            return 'application/wasm'
        elif path.endswith('.data'):
            return 'application/octet-stream'
        elif path.endswith('.js'):
            return 'application/javascript'
        elif path.endswith('.mem'):
            return 'application/octet-stream'
        elif path.endswith('.symbols'):
            return 'application/octet-stream'
        
        # 处理LIB目录下的二进制文件
        if '/LIB/' in path:
            return 'application/octet-stream'
            
        return super().guess_type(path)
    
    def do_OPTIONS(self):
        """处理OPTIONS请求（CORS预检）"""
        self.send_response(200)
        self.end_headers()

def start_server(port=8000, webgl_dir=None):
    """启动Unity WebGL服务器"""
    if webgl_dir is None:
        # 默认使用当前目录的WebGL文件夹
        webgl_dir = os.path.join(os.getcwd(), 'WebGL')
    
    if not os.path.exists(webgl_dir):
        print(f"❌ WebGL目录不存在: {webgl_dir}")
        print("请先构建WebGL版本或指定正确的目录")
        return False
    
    print("=" * 60)
    print("Unity WebGL专用服务器")
    print("=" * 60)
    print(f"服务目录: {webgl_dir}")
    print(f"访问地址: http://localhost:{port}")
    print("=" * 60)
    
    # 切换到WebGL构建目录
    os.chdir(webgl_dir)
    
    try:
        with socketserver.TCPServer(("", port), UnityWebGLHTTPRequestHandler) as httpd:
            print(f"✅ 服务器已启动，正在监听端口 {port}")
            print("⚠️  请确保使用现代浏览器访问（Chrome/Firefox/Edge）")
            print("⏹️  按 Ctrl+C 停止服务器")
            print("-" * 60)
            
            httpd.serve_forever()
            return True
    except KeyboardInterrupt:
        print("\n\n🛑 服务器已停止")
        return True
    except Exception as e:
        print(f"\n❌ 服务器启动失败: {e}")
        print("可能的原因：")
        print("1. 端口 {port} 已被占用，请尝试其他端口")
        print("2. 权限不足，请以管理员身份运行")
        return False

if __name__ == '__main__':
    # 解析命令行参数
    port = 8000
    webgl_dir = None
    
    if len(sys.argv) > 1:
        try:
            port = int(sys.argv[1])
        except ValueError:
            webgl_dir = sys.argv[1]
    
    if len(sys.argv) > 2:
        try:
            port = int(sys.argv[2])
        except ValueError:
            pass
    
    success = start_server(port, webgl_dir)
    sys.exit(0 if success else 1)