// Unity WebGL专用Go语言服务器
package main

import (
	"fmt"
	"io"
	"log"
	"net/http"
	"os"
	"path"
	"path/filepath"
	"strings"
)

const (
	PORT = "8000"
	HOST = "0.0.0.0"
)

// MIME类型映射
var mimeTypes = map[string]string{
	".html":    "text/html",
	".js":      "application/javascript",
	".css":     "text/css",
	".json":    "application/json",
	".png":     "image/png",
	".jpg":     "image/jpeg",
	".gif":     "image/gif",
	".ico":     "image/x-icon",
	".wasm":    "application/wasm",
	".data":    "application/octet-stream",
	".mem":     "application/octet-stream",
	".symbols": "application/octet-stream",
}

func main() {
	// 设置请求处理函数
	http.HandleFunc("/", handleRequest)
	
	// 启动服务器
	addr := fmt.Sprintf("%s:%s", HOST, PORT)
	fmt.Println("=" + strings.Repeat("=", 59))
	fmt.Println("Unity WebGL Go语言服务器")
	fmt.Println("=" + strings.Repeat("=", 59))
	fmt.Printf("服务器运行在: http://%s\n", addr)
	fmt.Printf("本机访问: http://localhost:%s\n", PORT)
	fmt.Printf("局域网访问: http://192.168.1.123:%s\n", PORT)
	fmt.Println("=" + strings.Repeat("=", 59))
	fmt.Println("按 Ctrl+C 停止服务器")
	
	log.Fatal(http.ListenAndServe(addr, nil))
}

func handleRequest(w http.ResponseWriter, r *http.Request) {
	log.Printf("%s %s", r.Method, r.URL.Path)
	
	// 处理CORS预检请求
	if r.Method == "OPTIONS" {
		setHeaders(w, "")
		w.WriteHeader(http.StatusOK)
		return
	}
	
	// 构建文件路径
	filePath := r.URL.Path
	if filePath == "/" {
		filePath = "/index.html"
	}
	
	// 移除开头的斜杠
	filePath = strings.TrimPrefix(filePath, "/")
	
	// 读取文件
	content, err := os.ReadFile(filePath)
	if err != nil {
		if os.IsNotExist(err) {
			http.NotFound(w, r)
		} else {
			http.Error(w, "Server Error", http.StatusInternalServerError)
		}
		return
	}
	
	// 获取MIME类型
	ext := strings.ToLower(path.Ext(filePath))
	mimeType := mimeTypes[ext]
	if mimeType == "" {
		mimeType = "application/octet-stream"
	}
	
	// 设置响应头
	setHeaders(w, mimeType)
	
	// 写入响应内容
	w.Write(content)
}

func setHeaders(w http.ResponseWriter, contentType string) {
	// 设置Unity WebGL必需的响应头
	headers := map[string]string{
		"Cross-Origin-Opener-Policy":    "same-origin",
		"Cross-Origin-Embedder-Policy":  "require-corp",
		"Access-Control-Allow-Origin":   "*",
		"Access-Control-Allow-Headers":  "*",
		"Access-Control-Allow-Methods":  "GET, POST, OPTIONS",
		"Cache-Control":                 "no-cache, no-store, must-revalidate",
		"Pragma":                        "no-cache",
		"Expires":                       "0",
	}
	
	for key, value := range headers {
		w.Header().Set(key, value)
	}
	
	if contentType != "" {
		w.Header().Set("Content-Type", contentType)
	}
}