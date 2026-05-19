using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace LocalHost
{
    class Program
    {
        private static int _port = 8080;
        private static string _rootPath = Environment.ExpandEnvironmentVariables("%USERPROFILE%\\.localhost");
        private static Dictionary<string, string> _activeTools = new Dictionary<string, string>();
        private static TcpListener _listener;
        private static bool _isRunning = false;

        static async Task Main(string[] args)
        {
            Console.WriteLine("╔════════════════════════════════════════╗");
            Console.WriteLine("║   LOCAL HOST SERVER - v1.0             ║");
            Console.WriteLine("║   Offline Tool Activation & Hosting     ║");
            Console.WriteLine("╚════════════════════════════════════════╝");
            Console.WriteLine();

            ParseArguments(args);
            InitializeHost();
            
            _listener = new TcpListener(IPAddress.Loopback, _port);
            _isRunning = true;
            
            Console.WriteLine($"\n✓ Server started on http://localhost:{_port}");
            Console.WriteLine($"✓ Root directory: {_rootPath}");
            Console.WriteLine($"✓ Status: OFFLINE MODE (No internet required)");
            Console.WriteLine("\n[Commands]");
            Console.WriteLine("  /status       - Server status");
            Console.WriteLine("  /tools        - List active tools");
            Console.WriteLine("  /activate?tool=NAME&version=VER - Activate tool");
            Console.WriteLine("  /config       - Show configuration");
            Console.WriteLine("  /files        - Browse files");
            Console.WriteLine("\nPress Ctrl+C to stop...");
            Console.WriteLine("═" * 40);
            Console.WriteLine();

            _listener.Start();
            
            try
            {
                while (_isRunning)
                {
                    TcpClient client = await _listener.AcceptTcpClientAsync();
                    _ = HandleClientAsync(client);
                }
            }
            catch (ObjectDisposedException)
            {
                Console.WriteLine("\n✓ Server stopped gracefully.");
            }
        }

        static void ParseArguments(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--port" && i + 1 < args.Length)
                    _port = int.Parse(args[i + 1]);
                else if (args[i] == "--root" && i + 1 < args.Length)
                    _rootPath = args[i + 1];
            }
        }

        static void InitializeHost()
        {
            Directory.CreateDirectory(_rootPath);
            Directory.CreateDirectory(Path.Combine(_rootPath, "tools"));
            Directory.CreateDirectory(Path.Combine(_rootPath, "data"));
            Directory.CreateDirectory(Path.Combine(_rootPath, "logs"));

            string configPath = Path.Combine(_rootPath, "config.json");
            if (!File.Exists(configPath))
            {
                var config = new
                {
                    port = _port,
                    mode = "offline",
                    created = DateTime.Now,
                    tools = new[] { "python", "nodejs", "java", "go", "rust" }
                };
                File.WriteAllText(configPath, JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true }));
            }
        }

        static async Task HandleClientAsync(TcpClient client)
        {
            using (client)
            using (StreamReader reader = new StreamReader(client.GetStream()))
            using (StreamWriter writer = new StreamWriter(client.GetStream()) { AutoFlush = true })
            {
                try
                {
                    string request = await reader.ReadLineAsync();
                    if (string.IsNullOrEmpty(request))
                        return;

                    string[] parts = request.Split(' ');
                    string method = parts[0];
                    string path = parts[1];

                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {method} {path}");

                    string response = RouteRequest(path, method);
                    string httpResponse = GenerateHttpResponse(response, GetContentType(path));
                    
                    await writer.WriteAsync(httpResponse);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"✗ Error: {ex.Message}");
                }
            }
        }

        static string RouteRequest(string path, string method)
        {
            path = path.TrimStart('/');

            if (path == "status")
                return GetStatus();
            else if (path == "tools")
                return GetActiveTools();
            else if (path.StartsWith("activate"))
                return HandleActivation(path);
            else if (path == "config")
                return GetConfig();
            else if (path == "files" || path == "")
                return BrowseFiles(path == "" ? _rootPath : Path.Combine(_rootPath, path));
            else if (path == "logs")
                return GetLogs();
            else
                return GetFileContent(Path.Combine(_rootPath, path));
        }

        static string GetStatus()
        {
            var status = new
            {
                server = "LOCAL HOST",
                port = _port,
                mode = "OFFLINE",
                status = "RUNNING",
                uptime = "check /logs",
                timestamp = DateTime.Now.ToString("O"),
                activatedTools = _activeTools.Count,
                version = "1.0.0"
            };
            return JsonSerializer.Serialize(status, new JsonSerializerOptions { WriteIndented = true });
        }

        static string GetActiveTools()
        {
            var tools = new
            {
                active = _activeTools,
                available = new[] { "python", "nodejs", "java", "go", "rust" },
                count = _activeTools.Count
            };
            return JsonSerializer.Serialize(tools, new JsonSerializerOptions { WriteIndented = true });
        }

        static string HandleActivation(string path)
        {
            var uri = new Uri("http://localhost" + path);
            var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
            string tool = query["tool"];
            string version = query["version"] ?? "latest";

            if (string.IsNullOrEmpty(tool))
                return JsonError("Tool name required");

            _activeTools[tool] = version;
            return JsonSuccess($"Tool '{tool}' v{version} activated");
        }

        static string GetConfig()
        {
            string configPath = Path.Combine(_rootPath, "config.json");
            if (File.Exists(configPath))
                return File.ReadAllText(configPath);
            return JsonError("Config not found");
        }

        static string BrowseFiles(string directory)
        {
            try
            {
                var files = Directory.GetFiles(directory).Select(f => Path.GetFileName(f)).ToList();
                var dirs = Directory.GetDirectories(directory).Select(d => Path.GetFileName(d) + "/").ToList();
                
                var result = new
                {
                    path = directory,
                    directories = dirs,
                    files = files,
                    totalItems = files.Count + dirs.Count
                };
                return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
            }
            catch
            {
                return JsonError("Directory not found");
            }
        }

        static string GetFileContent(string filePath)
        {
            if (!filePath.StartsWith(_rootPath))
                return JsonError("Access denied");
            
            if (File.Exists(filePath))
            {
                try
                {
                    return File.ReadAllText(filePath);
                }
                catch
                {
                    return JsonError("Cannot read file");
                }
            }
            return JsonError("File not found");
        }

        static string GetLogs()
        {
            string logsPath = Path.Combine(_rootPath, "logs");
            var logs = Directory.GetFiles(logsPath).Select(f => new
            {
                name = Path.GetFileName(f),
                size = new FileInfo(f).Length,
                modified = File.GetLastWriteTime(f).ToString("O")
            });
            return JsonSerializer.Serialize(logs, new JsonSerializerOptions { WriteIndented = true });
        }

        static string JsonSuccess(string message)
        {
            var result = new { success = true, message };
            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }

        static string JsonError(string message)
        {
            var result = new { error = true, message };
            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }

        static string GetContentType(string path)
        {
            if (path.EndsWith(".json")) return "application/json";
            if (path.EndsWith(".html")) return "text/html";
            if (path.EndsWith(".txt")) return "text/plain";
            return "application/json";
        }

        static string GenerateHttpResponse(string body, string contentType)
        {
            return $"HTTP/1.1 200 OK\r\n" +
                   $"Content-Type: {contentType}; charset=utf-8\r\n" +
                   $"Content-Length: {body.Length}\r\n" +
                   $"Connection: close\r\n" +
                   $"\r\n" +
                   body;
        }
    }
}
