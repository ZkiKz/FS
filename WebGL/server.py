import http.server
import socketserver
import socket

PORT = 3333

class CustomHandler(http.server.SimpleHTTPRequestHandler):
    # 扩展MIME类型映射
    extensions_map = http.server.SimpleHTTPRequestHandler.extensions_map.copy()
    extensions_map.update({
        '.wasm': 'application/wasm',
        '.br': 'application/octet-stream',
        '.gz': 'application/gzip',
        '.data': 'application/octet-stream',
        '.js': 'application/javascript',
        '.mem': 'application/octet-stream',
    })

    # 关键修改：重写 end_headers 方法，强制禁用缓存
    def end_headers(self):
        # 添加 Cache-Control 头，强制浏览器每次都重新验证
        self.send_header('Cache-Control', 'no-cache, no-store, must-revalidate')
        self.send_header('Pragma', 'no-cache')
        self.send_header('Expires', '0')
        
        # 针对 .br 和 .gz 文件添加正确的压缩头
        if self.path.endswith('.br'):
            self.send_header('Content-Encoding', 'br')
        elif self.path.endswith('.gz'):
            self.send_header('Content-Encoding', 'gzip')
            
        super().end_headers()

def get_local_ip():
    s = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    try:
        s.connect(('8.8.8.8', 80))
        ip = s.getsockname()[0]
    except Exception:
        ip = '127.0.0.1'
    finally:
        s.close()
    return ip

if __name__ == '__main__':
    with socketserver.TCPServer(("0.0.0.0", PORT), CustomHandler) as httpd:
        local_ip = get_local_ip()
        print(f"本地服务器已启动，端口 {PORT}")
        print(f"在电脑上访问: http://localhost:{PORT}")
        print(f"在同一局域网内的手机上，请访问: http://{local_ip}:{PORT}")
        print("按 Ctrl + C 停止服务器")
        try:
            httpd.serve_forever()
        except KeyboardInterrupt:
            print("\n服务器已停止")