using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace HostKT200
{
    /// <summary>
    /// KT200II Local Host Server
    /// Offline activation server implementing the complete KT200II protocol
    /// </summary>
    public class KT200LocalHost
    {
        private int _port = 8080;
        private string _rootPath;
        private TcpListener _listener;
        private bool _isRunning = false;
        private Dictionary<string, SessionData> _sessions = new();
        private ActivationConfig _config;

        public KT200LocalHost(int port = 8080)
        {
            _port = port;
            _rootPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".kt200");
            Directory.CreateDirectory(_rootPath);
            _config = LoadConfig();
        }

        public async Task StartAsync()
        {
            _listener = new TcpListener(IPAddress.Loopback, _port);
            _isRunning = true;
            
            Console.WriteLine($"╔════════════════════════════════════════╗");
            Console.WriteLine($"║  KT200II LOCAL HOST SERVER - v1.0      ║");
            Console.WriteLine($"║  Offline Activation & Tool Management   ║");
            Console.WriteLine($"╚════════════════════════════════════════╝");
            Console.WriteLine($"\n✓ Server started on http://localhost:{_port}");
            Console.WriteLine($"✓ Mode: OFFLINE (No internet required)");
            Console.WriteLine($"✓ Root: {_rootPath}");
            Console.WriteLine("\n[Endpoints]");
            Console.WriteLine("  /weblm/activation.php  - License activation (stage 1 & 2)");
            Console.WriteLine("  /handshake.php         - Session initialization");
            Console.WriteLine("  /init2024.php          - INIT blob delivery");
            Console.WriteLine("  /chatgpt.php           - GPT endpoint test");
            Console.WriteLine("  /comm2024.php          - Session ping/keepalive");
            Console.WriteLine("  /tools                 - List active tools");
            Console.WriteLine("  /activate              - Activate tool");
            Console.WriteLine("\nPress Ctrl+C to stop...\n");
            
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

        private async Task HandleClientAsync(TcpClient client)
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

                    string response = RouteRequest(path);
                    string httpResponse = GenerateHttpResponse(response);
                    
                    await writer.WriteAsync(httpResponse);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"✗ Error: {ex.Message}");
                }
            }
        }

        private string RouteRequest(string path)
        {
            path = path.Split('?')[0].TrimStart('/');

            return path switch
            {
                "weblm/activation.php" => HandleActivation(),
                "handshake.php" => HandleHandshake(),
                "init2024.php" => HandleInit2024(),
                "chatgpt.php" => HandleChatGPT(),
                "comm2024.php" => HandleComm2024(),
                "tools" => ListTools(),
                "activate" => HandleToolActivation(),
                "status" => GetStatus(),
                _ => JsonError("Endpoint not found")
            };
        }

        // ============ KT200II PROTOCOL ENDPOINTS ============

        private string HandleActivation()
        {
            // ACTIVATION STAGE 1 & 2
            var response = new
            {
                status = "OK",
                license_valid = true,
                activation_stage = 1,
                guid = Guid.NewGuid().ToString(),
                timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                signature = GenerateSignature("b1k2d9s4")
            };
            return JsonSerialize(response);
        }

        private string HandleHandshake()
        {
            // SESSION INITIALIZATION
            string sessionId = Guid.NewGuid().ToString();
            _sessions[sessionId] = new SessionData 
            { 
                SessionId = sessionId,
                CreatedAt = DateTime.Now,
                IsActive = true
            };

            var response = new
            {
                status = "handshake_ok",
                session = sessionId,
                timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };
            return JsonSerialize(response);
        }

        private string HandleInit2024()
        {
            // INIT BLOB DELIVERY
            var initData = new
            {
                init_code = "INIT-FAKE-2024",
                serial = GenerateSerial(),
                timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                signature = GenerateSignature("init_key"),
                status = "OK"
            };

            string json = JsonSerialize(initData);
            return EncodeBlob(json); // XOR + Base64
        }

        private string HandleChatGPT()
        {
            // GPT ENDPOINT (SHA1 hash response)
            using (var sha1 = SHA1.Create())
            {
                byte[] hash = sha1.ComputeHash(Encoding.UTF8.GetBytes("gpt_test_data"));
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }

        private string HandleComm2024()
        {
            // SESSION PING/KEEPALIVE
            var response = new
            {
                status = "ok",
                tick = new Random().Next(1, 100),
                next_ping = DateTime.Now.AddSeconds(30).ToString("yyyy-MM-dd HH:mm:ss"),
                session_active = true
            };
            return JsonSerialize(response);
        }

        // ============ TOOL MANAGEMENT ENDPOINTS ============

        private string ListTools()
        {
            var tools = new
            {
                active = _config.ActiveTools,
                available = _config.AvailableTools,
                count = _config.ActiveTools.Count
            };
            return JsonSerialize(tools);
        }

        private string HandleToolActivation()
        {
            var activation = new
            {
                status = "activated",
                tool = "default_tool",
                version = "1.0.0",
                timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };
            return JsonSerialize(activation);
        }

        private string GetStatus()
        {
            var status = new
            {
                server = "KT200II HOST",
                port = _port,
                mode = "OFFLINE",
                status = "RUNNING",
                uptime_seconds = (int)(DateTime.Now - Process.GetCurrentProcess().StartTime).TotalSeconds,
                active_sessions = _sessions.Count(s => s.Value.IsActive),
                timestamp = DateTime.Now.ToString("O"),
                version = "1.0.0"
            };
            return JsonSerialize(status);
        }

        // ============ ENCODING & CRYPTOGRAPHY ============

        private string EncodeBlob(string json)
        {
            // XOR encoding with 0x55 + Base64
            byte[] raw = Encoding.UTF8.GetBytes(json);
            byte[] xored = raw.Select(b => (byte)(b ^ 0x55)).ToArray();
            return Convert.ToBase64String(xored);
        }

        private string DecodeBlob(string base64)
        {
            // Base64 decode + XOR 0x55
            byte[] raw = Convert.FromBase64String(base64);
            byte[] decoded = raw.Select(b => (byte)(b ^ 0x55)).ToArray();
            return Encoding.UTF8.GetString(decoded);
        }

        private string GenerateSignature(string key)
        {
            using (var hmac = new HMACSHA1(Encoding.UTF8.GetBytes(key)))
            {
                byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()));
                return Convert.ToBase64String(hash);
            }
        }

        private string GenerateSerial()
        {
            return $"KT-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
        }

        // ============ HELPERS ============

        private ActivationConfig LoadConfig()
        {
            string configPath = Path.Combine(_rootPath, "config.json");
            if (File.Exists(configPath))
            {
                string json = File.ReadAllText(configPath);
                return JsonSerializer.Deserialize<ActivationConfig>(json) ?? new ActivationConfig();
            }

            var config = new ActivationConfig();
            SaveConfig(config);
            return config;
        }

        private void SaveConfig(ActivationConfig config)
        {
            string configPath = Path.Combine(_rootPath, "config.json");
            File.WriteAllText(configPath, JsonSerialize(config));
        }

        private string JsonSerialize<T>(T obj)
        {
            return JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true });
        }

        private string JsonError(string message)
        {
            var error = new { error = true, message };
            return JsonSerializer.Serialize(error);
        }

        private string GenerateHttpResponse(string body)
        {
            return $"HTTP/1.1 200 OK\r\n" +
                   $"Content-Type: application/json; charset=utf-8\r\n" +
                   $"Content-Length: {body.Length}\r\n" +
                   $"Connection: close\r\n" +
                   $"\r\n" +
                   body;
        }
    }

    // ============ DATA MODELS ============

    public class SessionData
    {
        public string SessionId { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }

    public class ActivationConfig
    {
        public Dictionary<string, string> ActiveTools { get; set; } = new()
        {
            { "python", "3.12" },
            { "nodejs", "20.0.0" }
        };
        
        public List<string> AvailableTools { get; set; } = new()
        {
            "python", "nodejs", "java", "go", "rust", "dotnet"
        };
    }

    // ============ MAIN ENTRY ============

    public class Program
    {
        public static async Task Main(string[] args)
        {
            int port = 8080;
            
            if (args.Length > 0 && int.TryParse(args[0], out int p))
                port = p;

            var server = new KT200LocalHost(port);
            await server.StartAsync();
        }
    }
}
