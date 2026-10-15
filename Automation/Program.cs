using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;

namespace HostKt200.Automation;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new DashboardForm());
    }
}

internal sealed class DashboardForm : Form
{
    private readonly HttpClient _http = new() { Timeout = TimeSpan.FromSeconds(3) };
    private readonly TextBox _log = new() { Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical, Dock = DockStyle.Fill };
    private readonly Label _status = new() { AutoSize = true, Text = "Serwer: zatrzymany" };
    private readonly NumericUpDown _port = new() { Minimum = 1024, Maximum = 65535, Value = 8080, Width = 100 };
    private readonly ComboBox _tool = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 130 };
    private readonly TextBox _version = new() { Text = "latest", Width = 100 };
    private string? _kt200Executable;
    private Process? _server;
    private readonly string _dataRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".localhost");

    public DashboardForm()
    {
        Text = "HOST-KT200 — Centrum automatyzacji";
        Width = 900;
        Height = 650;
        MinimumSize = new Size(720, 500);
        StartPosition = FormStartPosition.CenterScreen;
        _tool.Items.AddRange(["python", "nodejs", "java", "go", "rust"]);
        _tool.SelectedIndex = 0;

        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(16), ColumnCount = 1, RowCount = 5 };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.Controls.Add(new Label { Text = "HOST-KT200  |  Panel zarządzania lokalnym serwerem", AutoSize = true, Font = new Font(Font.FontFamily, 15, FontStyle.Bold), Padding = new Padding(0, 0, 0, 12) }, 0, 0);

        var serverRow = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = true, Padding = new Padding(0, 4, 0, 8) };
        serverRow.Controls.Add(new Label { Text = "Port:", AutoSize = true, Padding = new Padding(0, 8, 0, 0) });
        serverRow.Controls.Add(_port);
        serverRow.Controls.Add(MakeButton("Uruchom serwer", async (_, _) => await StartServerAsync()));
        serverRow.Controls.Add(MakeButton("Zatrzymaj", (_, _) => StopServer()));
        serverRow.Controls.Add(MakeButton("Sprawdź status", async (_, _) => await RefreshStatusAsync()));
        serverRow.Controls.Add(MakeButton("Otwórz panel HTTP", (_, _) => OpenServer()));
        serverRow.Controls.Add(MakeButton("Wybierz aplikację KT200", (_, _) => SelectKt200Executable()));
        serverRow.Controls.Add(MakeButton("Uruchom aplikację KT200", async (_, _) => await LaunchKt200Async()));
        layout.Controls.Add(serverRow, 0, 1);

        var toolsRow = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = true, Padding = new Padding(0, 4, 0, 8) };
        toolsRow.Controls.Add(new Label { Text = "Narzędzie:", AutoSize = true, Padding = new Padding(0, 8, 0, 0) });
        toolsRow.Controls.Add(_tool);
        toolsRow.Controls.Add(new Label { Text = "Wersja:", AutoSize = true, Padding = new Padding(0, 8, 0, 0) });
        toolsRow.Controls.Add(_version);
        toolsRow.Controls.Add(MakeButton("Aktywuj", async (_, _) => await ActivateToolAsync()));
        toolsRow.Controls.Add(MakeButton("Dezaktywuj", async (_, _) => await DeactivateToolAsync()));
        toolsRow.Controls.Add(MakeButton("Lista narzędzi", async (_, _) => await ReadEndpointAsync("/tools", "Narzędzia")));
        toolsRow.Controls.Add(MakeButton("Sprawdź narzędzie", async (_, _) => await CheckSelectedToolAsync()));
        toolsRow.Controls.Add(MakeButton("Zainstaluj przez winget", async (_, _) => await InstallSelectedToolAsync()));
        toolsRow.Controls.Add(MakeButton("Konfiguracja", async (_, _) => await ReadEndpointAsync("/config", "Konfiguracja")));
        toolsRow.Controls.Add(MakeButton("Logi", async (_, _) => await ReadEndpointAsync("/logs", "Indeks logów")));
        toolsRow.Controls.Add(MakeButton("Diagnostyka", async (_, _) => await RunDiagnosticsAsync()));
        layout.Controls.Add(toolsRow, 0, 2);

        _log.Font = new Font("Consolas", 9);
        layout.Controls.Add(_log, 0, 3);
        var footer = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, Padding = new Padding(0, 8, 0, 0) };
        footer.Controls.Add(_status);
        footer.Controls.Add(new Label { Text = "   Dane: " + _dataRoot, AutoSize = true, Padding = new Padding(0, 2, 0, 0) });
        layout.Controls.Add(footer, 0, 4);
        Controls.Add(layout);
        FormClosing += (_, _) => { StopServer(); _http.Dispose(); };
        LoadLauncherSettings();
        Log("Gotowe. Uruchom diagnostykę, aby sprawdzić środowisko.");
    }

    private static Button MakeButton(string text, EventHandler handler)
    {
        var button = new Button { Text = text, AutoSize = true, Padding = new Padding(8, 4, 8, 4), Margin = new Padding(4) };
        button.Click += handler;
        return button;
    }

    private string ServerExecutable => Path.Combine(AppContext.BaseDirectory, "server", "LocalHost.exe");
    private string ServerUrl => $"http://127.0.0.1:{_port.Value}";

    private async Task StartServerAsync()
    {
        if (_server is { HasExited: false })
        {
            Log("Serwer uruchomiony przez tę aplikację już działa.");
            await RefreshStatusAsync();
            return;
        }
        if (await IsServerRespondingAsync())
        {
            Log($"Port {_port.Value} odpowiada. Nie uruchamiam drugiej instancji.");
            await RefreshStatusAsync();
            return;
        }
        if (!File.Exists(ServerExecutable))
        {
            Log($"BŁĄD: brak pliku serwera: {ServerExecutable}");
            Log("Ponownie zainstaluj aplikację lub sprawdź zawartość katalogu instalacyjnego.");
            return;
        }

        try
        {
            Directory.CreateDirectory(_dataRoot);
            var start = new ProcessStartInfo(ServerExecutable) { UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true, WorkingDirectory = Path.GetDirectoryName(ServerExecutable)! };
            start.ArgumentList.Add("--port");
            start.ArgumentList.Add(((int)_port.Value).ToString());
            start.ArgumentList.Add("--root");
            start.ArgumentList.Add(_dataRoot);
            _server = new Process { StartInfo = start, EnableRaisingEvents = true };
            _server.OutputDataReceived += (_, e) => { if (!string.IsNullOrWhiteSpace(e.Data)) SafeLog(e.Data); };
            _server.ErrorDataReceived += (_, e) => { if (!string.IsNullOrWhiteSpace(e.Data)) SafeLog("BŁĄD: " + e.Data); };
            _server.Exited += (_, _) => SafeLog("Proces serwera zakończył działanie.");
            if (!_server.Start()) { Log("Nie udało się uruchomić procesu serwera."); return; }
            _server.BeginOutputReadLine();
            _server.BeginErrorReadLine();
            Log($"Uruchomiono proces serwera na porcie {_port.Value}.");
            for (var i = 0; i < 15; i++)
            {
                await Task.Delay(300);
                if (await IsServerRespondingAsync())
                {
                    _status.Text = $"Serwer: działa — {ServerUrl}";
                    Log("Kontrola HTTP zakończona powodzeniem.");
                    return;
                }
                if (_server.HasExited) break;
            }
            Log("Serwer nie odpowiedział w oczekiwanym czasie. Uruchom diagnostykę i sprawdź logi.");
        }
        catch (Exception ex) { Log("Nie można uruchomić serwera: " + ex.Message); }
    }

    private void StopServer()
    {
        try
        {
            if (_server is { HasExited: false })
            {
                _server.Kill(entireProcessTree: true);
                _server.WaitForExit(3000);
                Log("Zatrzymano proces serwera.");
            }
        }
        catch (Exception ex) { Log("Błąd zatrzymywania serwera: " + ex.Message); }
        finally { _server?.Dispose(); _server = null; _status.Text = "Serwer: zatrzymany"; }
    }

    private async Task RefreshStatusAsync()
    {
        try
        {
            using var response = await _http.GetAsync(ServerUrl + "/status");
            var body = await response.Content.ReadAsStringAsync();
            response.EnsureSuccessStatusCode();
            using var json = JsonDocument.Parse(body);
            var state = json.RootElement.TryGetProperty("status", out var s) ? s.GetString() : "odpowiada";
            _status.Text = $"Serwer: {state} — {ServerUrl}";
            Log("Status serwera: " + body.Replace(Environment.NewLine, " "));
        }
        catch (Exception ex) { _status.Text = "Serwer: brak odpowiedzi"; Log("Kontrola statusu nie powiodła się: " + ex.Message); }
    }

    private async Task ActivateToolAsync()
    {
        if (_tool.SelectedItem is not string tool) return;
        if (!await IsServerRespondingAsync()) { Log("Najpierw uruchom serwer."); return; }
        try
        {
            var url = $"{ServerUrl}/activate?tool={Uri.EscapeDataString(tool)}&version={Uri.EscapeDataString(_version.Text.Trim())}";
            using var response = await _http.GetAsync(url);
            var body = await response.Content.ReadAsStringAsync();
            response.EnsureSuccessStatusCode();
            Log($"Aktywacja {tool}: {body.Replace(Environment.NewLine, " ")}");
        }
        catch (Exception ex) { Log("Aktywacja nie powiodła się: " + ex.Message); }
    }

    private async Task RunDiagnosticsAsync()
    {
        Log("=== Diagnostyka HOST-KT200 ===");
        Log($"System: {Environment.OSVersion}; architektura 64-bit: {Environment.Is64BitOperatingSystem}");
        Log($".NET runtime: {Environment.Version}");
        Log($"Katalog aplikacji: {AppContext.BaseDirectory}");
        Log($"Plik serwera istnieje: {File.Exists(ServerExecutable)}");
        try
        {
            Directory.CreateDirectory(_dataRoot);
            var probe = Path.Combine(_dataRoot, ".write-test");
            await File.WriteAllTextAsync(probe, "ok");
            File.Delete(probe);
            Log("Katalog danych: zapis i usuwanie plików OK.");
        }
        catch (Exception ex) { Log("Katalog danych: BŁĄD — " + ex.Message); }
        try
        {
            using var probe = new TcpListener(IPAddress.Loopback, (int)_port.Value);
            probe.Start();
            Log($"Port {_port.Value}: wolny.");
        }
        catch (SocketException) { Log($"Port {_port.Value}: zajęty (możliwe, że serwer już działa)."); }
        if (await IsServerRespondingAsync()) { Log("HTTP /status: odpowiedź OK."); await RefreshStatusAsync(); }
        else Log("HTTP /status: brak odpowiedzi. Sprawdź port, proces serwera i zaporę.");
        Log("=== Koniec diagnostyki ===");
    }

    private static readonly IReadOnlyDictionary<string, (string Executable, string PackageId, string[] Arguments)> ToolCatalog =
        new Dictionary<string, (string Executable, string PackageId, string[] Arguments)>(StringComparer.OrdinalIgnoreCase)
        {
            ["python"] = ("python", "Python.Python.3.12", ["--version"]),
            ["nodejs"] = ("node", "OpenJS.NodeJS.LTS", ["--version"]),
            ["java"] = ("java", "EclipseAdoptium.Temurin.21.JDK", ["-version"]),
            ["go"] = ("go", "GoLang.Go", ["version"]),
            ["rust"] = ("rustc", "Rustlang.Rustup", ["--version"])
        };

    private async Task DeactivateToolAsync()
    {
        if (_tool.SelectedItem is not string tool)
            return;
        if (!await IsServerRespondingAsync())
        {
            Log("Najpierw uruchom serwer.");
            return;
        }
        try
        {
            var url = $"{ServerUrl}/deactivate?tool={Uri.EscapeDataString(tool)}";
            using var response = await _http.GetAsync(url);
            var body = await response.Content.ReadAsStringAsync();
            response.EnsureSuccessStatusCode();
            Log($"Dezaktywacja {tool}: {body.Replace(Environment.NewLine, " ")}");
        }
        catch (Exception ex) { Log("Dezaktywacja nie powiodła się: " + ex.Message); }
    }

    private static string ResolveToolExecutable(string tool, string fallback)
    {
        var candidates = new List<string>();
        var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        switch (tool)
        {
            case "python":
                if (Directory.Exists(programFiles))
                    candidates.AddRange(Directory.GetDirectories(programFiles, "Python*")
                        .OrderByDescending(path => path, StringComparer.OrdinalIgnoreCase)
                        .Select(path => Path.Combine(path, "python.exe")));
                var localPython = Path.Combine(localAppData, "Programs", "Python");
                if (Directory.Exists(localPython))
                    candidates.AddRange(Directory.GetDirectories(localPython, "Python*")
                        .OrderByDescending(path => path, StringComparer.OrdinalIgnoreCase)
                        .Select(path => Path.Combine(path, "python.exe")));
                break;
            case "nodejs":
                candidates.Add(Path.Combine(programFiles, "nodejs", "node.exe"));
                break;
            case "java":
                var javaRoot = Path.Combine(programFiles, "Eclipse Adoptium");
                if (Directory.Exists(javaRoot))
                    candidates.AddRange(Directory.GetDirectories(javaRoot, "jdk-*")
                        .OrderByDescending(path => path, StringComparer.OrdinalIgnoreCase)
                        .Select(path => Path.Combine(path, "bin", "java.exe")));
                break;
            case "go":
                candidates.Add(Path.Combine(programFiles, "Go", "bin", "go.exe"));
                break;
            case "rust":
                candidates.Add(Path.Combine(userProfile, ".cargo", "bin", "rustc.exe"));
                break;
        }

        return candidates.FirstOrDefault(File.Exists) ?? fallback;
    }

    private async Task CheckSelectedToolAsync()
    {
        if (_tool.SelectedItem is not string tool || !ToolCatalog.TryGetValue(tool, out var definition))
            return;

        try
        {
            var start = new ProcessStartInfo(ResolveToolExecutable(tool, definition.Executable))
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            foreach (var argument in definition.Arguments)
                start.ArgumentList.Add(argument);
            using var process = Process.Start(start);
            if (process is null)
            {
                Log($"Nie udało się sprawdzić {tool}.");
                return;
            }
            var stdoutTask = process.StandardOutput.ReadToEndAsync();
            var stderrTask = process.StandardError.ReadToEndAsync();
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            try { await process.WaitForExitAsync(timeout.Token); }
            catch (OperationCanceledException)
            {
                try { process.Kill(entireProcessTree: true); } catch { }
                Log($"Kontrola {tool} przekroczyła limit 10 sekund.");
                return;
            }
            var output = (await stdoutTask + " " + await stderrTask).Trim();
            if (process.ExitCode == 0)
                Log($"{tool} jest zainstalowany: {output}");
            else
                Log($"{tool} zwrócił kod {process.ExitCode}: {output}");
        }
        catch (System.ComponentModel.Win32Exception)
        {
            Log($"{tool} nie został znaleziony w PATH. Możesz spróbować instalacji przez winget.");
        }
        catch (Exception ex) { Log($"Kontrola {tool} nie powiodła się: {ex.Message}"); }
    }

    private async Task InstallSelectedToolAsync()
    {
        if (_tool.SelectedItem is not string tool || !ToolCatalog.TryGetValue(tool, out var definition))
            return;

        var answer = MessageBox.Show(
            $"Zainstalować {tool} ({definition.PackageId}) za pomocą Windows Package Manager (winget)?\n\nWymagane jest połączenie z repozytorium pakietów. Instalator może zmienić środowisko systemowe.",
            "Potwierdź instalację narzędzia",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);
        if (answer != DialogResult.Yes)
        {
            Log($"Anulowano instalację {tool}.");
            return;
        }

        try
        {
            Log($"Rozpoczynam instalację {tool} przez winget...");
            var start = new ProcessStartInfo("winget.exe")
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            foreach (var argument in new[]
            {
                "install", "--id", definition.PackageId, "--exact", "--silent",
                "--disable-interactivity", "--accept-package-agreements", "--accept-source-agreements"
            })
                start.ArgumentList.Add(argument);

            using var process = Process.Start(start);
            if (process is null)
            {
                Log("Nie udało się uruchomić winget.");
                return;
            }
            var stdoutTask = process.StandardOutput.ReadToEndAsync();
            var stderrTask = process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();
            var output = (await stdoutTask + Environment.NewLine + await stderrTask).Trim();
            if (!string.IsNullOrWhiteSpace(output))
                Log(output);
            if (process.ExitCode == 0)
            {
                Log($"winget zakończył instalację {tool} powodzeniem. Sprawdzam wersję...");
                await CheckSelectedToolAsync();
            }
            else
                Log($"Instalacja {tool} nie powiodła się (kod {process.ExitCode}). Sprawdź dostępność winget, sieć i logi instalatora.");
        }
        catch (System.ComponentModel.Win32Exception ex)
        {
            Log($"Nie znaleziono winget.exe lub nie można go uruchomić: {ex.Message}. Zainstaluj/napraw App Installer w Windows.");
        }
        catch (Exception ex) { Log($"Instalacja {tool} nie powiodła się: {ex.Message}"); }
    }

    private async Task ReadEndpointAsync(string endpoint, string label)
    {
        try
        {
            using var response = await _http.GetAsync(ServerUrl + endpoint);
            var body = await response.Content.ReadAsStringAsync();
            response.EnsureSuccessStatusCode();
            Log(label + ": " + body.Replace(Environment.NewLine, " "));
        }
        catch (Exception ex) { Log(label + " — błąd odczytu: " + ex.Message); }
    }

    private async Task<bool> IsServerRespondingAsync()
    {
        try { using var response = await _http.GetAsync(ServerUrl + "/status"); return response.IsSuccessStatusCode; }
        catch { return false; }
    }

    private string LauncherSettingsPath => Path.Combine(_dataRoot, "launcher.json");

    private void LoadLauncherSettings()
    {
        try
        {
            if (!File.Exists(LauncherSettingsPath)) return;
            using var document = JsonDocument.Parse(File.ReadAllText(LauncherSettingsPath));
            if (document.RootElement.TryGetProperty("kt200Executable", out var path))
            {
                var candidate = path.GetString();
                if (!string.IsNullOrWhiteSpace(candidate) && File.Exists(candidate))
                {
                    _kt200Executable = candidate;
                    Log("Zapisana ścieżka aplikacji KT200: " + candidate);
                }
            }
        }
        catch (Exception ex) { Log("Nie udało się odczytać ustawień launchera: " + ex.Message); }
    }

    private void SelectKt200Executable()
    {
        using var dialog = new OpenFileDialog
        {
            Title = "Wskaż legalnie zainstalowaną aplikację KT200",
            Filter = "Pliki aplikacji (*.exe)|*.exe|Wszystkie pliki (*.*)|*.*",
            CheckFileExists = true,
            Multiselect = false
        };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        _kt200Executable = dialog.FileName;
        try
        {
            Directory.CreateDirectory(_dataRoot);
            File.WriteAllText(LauncherSettingsPath, JsonSerializer.Serialize(
                new { kt200Executable = _kt200Executable },
                new JsonSerializerOptions { WriteIndented = true }));
            Log("Zapisano ścieżkę aplikacji KT200: " + _kt200Executable);
        }
        catch (Exception ex) { Log("Nie udało się zapisać ustawień launchera: " + ex.Message); }
    }

    private async Task LaunchKt200Async()
    {
        if (string.IsNullOrWhiteSpace(_kt200Executable) || !File.Exists(_kt200Executable))
        {
            Log("Najpierw wybierz plik EXE aplikacji KT200.");
            SelectKt200Executable();
            if (string.IsNullOrWhiteSpace(_kt200Executable) || !File.Exists(_kt200Executable)) return;
        }

        // Start the local management server as a convenience only. Do not rewrite DNS,
        // hosts, proxy settings, TLS, or the target application's licensing endpoints.
        if (!await IsServerRespondingAsync())
        {
            await StartServerAsync();
            if (!await IsServerRespondingAsync())
            {
                Log("Aplikacja KT200 nie została uruchomiona, ponieważ lokalny serwer nie jest gotowy.");
                return;
            }
        }

        try
        {
            Process.Start(new ProcessStartInfo(_kt200Executable)
            {
                UseShellExecute = true,
                WorkingDirectory = Path.GetDirectoryName(_kt200Executable)!
            });
            Log("Uruchomiono aplikację KT200. Jej ruch sieciowy i weryfikacja licencji pozostają bez zmian.");
        }
        catch (Exception ex) { Log("Nie udało się uruchomić aplikacji KT200: " + ex.Message); }
    }

    private void OpenServer()
    {
        try { Process.Start(new ProcessStartInfo(ServerUrl + "/status") { UseShellExecute = true }); }
        catch (Exception ex) { Log("Nie można otworzyć przeglądarki: " + ex.Message); }
    }

    private void Log(string message)
    {
        if (InvokeRequired) { BeginInvoke(new Action(() => Log(message))); return; }
        _log.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
    }

    private void SafeLog(string message)
    {
        if (IsDisposed) return;
        try { if (InvokeRequired) BeginInvoke(new Action(() => Log(message))); else Log(message); }
        catch (InvalidOperationException) { }
    }
}
