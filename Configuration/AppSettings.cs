using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace PC7866.Configuration;

/// <summary>
/// Configuración global de la aplicación.
/// Se persiste en 'appsettings.json' junto al ejecutable.
/// </summary>
public class AppSettings
{
    private static AppSettings? _instance;
    private static readonly object _lock = new();

    private static readonly string SettingsPath =
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public static AppSettings Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance ??= new AppSettings();
                }
            }
            return _instance;
        }
    }

    private AppSettings()
    {
        LoadDefaults();
    }

    // Configuración de comunicación serie
    public string DefaultPortName { get; set; } = "COM4";
    public int DefaultBaudRate { get; set; } = 115200;
    public int DefaultTimeout { get; set; } = 5000;

    // Configuración de base de datos
    public string DatabaseServer { get; set; } = "localhost";
    public string DatabaseName { get; set; } = "control_electrico_salud";
    public string DatabaseUser { get; set; } = "root";
    public string DatabasePassword { get; set; } = "";
    public int DatabasePort { get; set; } = 3306;

    // Configuración de test
    public int MaxRetries { get; set; } = 3;
    public int DelayBetweenCommandsMs { get; set; } = 100;
    public bool AutoSaveResults { get; set; } = true;

    // Configuración de interfaz
    public string ApplicationTitle { get; set; } = "PC7866 - Test Resistivo Embega";
    public bool ShowDetailedLogs { get; set; } = true;

    public string GetConnectionString()
    {
        return $"Server={DatabaseServer};Port={DatabasePort};Database={DatabaseName};" +
               $"User={DatabaseUser};Password={DatabasePassword};";
    }

    private void LoadDefaults()
    {
        try
        {
            if (!File.Exists(SettingsPath)) return;

            string json = File.ReadAllText(SettingsPath);
            var data = JsonSerializer.Deserialize<SettingsDto>(json, JsonOptions);
            if (data is null) return;

            DefaultPortName        = data.DefaultPortName;
            DefaultBaudRate        = data.DefaultBaudRate;
            DefaultTimeout         = data.DefaultTimeout;
            DatabaseServer         = data.DatabaseServer;
            DatabaseName           = data.DatabaseName;
            DatabaseUser           = data.DatabaseUser;
            DatabasePassword       = DecryptPassword(data.DatabasePassword);
            DatabasePort           = data.DatabasePort;
            MaxRetries             = data.MaxRetries;
            DelayBetweenCommandsMs = data.DelayBetweenCommandsMs;
            AutoSaveResults        = data.AutoSaveResults;
            ApplicationTitle       = data.ApplicationTitle;
            ShowDetailedLogs       = data.ShowDetailedLogs;
        }
        catch (Exception ex)
        {
            // Si el archivo está corrupto se usan los valores por defecto
            System.Diagnostics.Debug.WriteLine($"[AppSettings] Error al cargar configuración: {ex.Message}");
        }
    }

    public void Save()
    {
        try
        {
            var dto = new SettingsDto
            {
                DefaultPortName        = DefaultPortName,
                DefaultBaudRate        = DefaultBaudRate,
                DefaultTimeout         = DefaultTimeout,
                DatabaseServer         = DatabaseServer,
                DatabaseName           = DatabaseName,
                DatabaseUser           = DatabaseUser,
                DatabasePassword       = EncryptPassword(DatabasePassword),
                DatabasePort           = DatabasePort,
                MaxRetries             = MaxRetries,
                DelayBetweenCommandsMs = DelayBetweenCommandsMs,
                AutoSaveResults        = AutoSaveResults,
                ApplicationTitle       = ApplicationTitle,
                ShowDetailedLogs       = ShowDetailedLogs
            };
            string json = JsonSerializer.Serialize(dto, JsonOptions);
            File.WriteAllText(SettingsPath, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AppSettings] Error al guardar configuración: {ex.Message}");
            throw new InvalidOperationException(
                $"No se pudo guardar la configuración en '{SettingsPath}'.", ex);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Cifrado DPAPI (ámbito: usuario actual de Windows)
    // ─────────────────────────────────────────────────────────────────────────

    private static string EncryptPassword(string plainText)
    {
        if (string.IsNullOrEmpty(plainText)) return string.Empty;
        byte[] data      = Encoding.UTF8.GetBytes(plainText);
        byte[] encrypted = ProtectedData.Protect(data, null, DataProtectionScope.CurrentUser);
        return Convert.ToBase64String(encrypted);
    }

    private static string DecryptPassword(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText)) return string.Empty;
        try
        {
            byte[] data      = Convert.FromBase64String(cipherText);
            byte[] decrypted = ProtectedData.Unprotect(data, null, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(decrypted);
        }
        catch (FormatException)
        {
            // La contraseña estaba en texto plano (archivo de configuración antiguo).
            // Se devuelve tal cual; al próximo Save() quedará cifrada.
            return cipherText;
        }
        catch (CryptographicException)
        {
            // El cifrado pertenece a otro usuario o máquina.
            System.Diagnostics.Debug.WriteLine("[AppSettings] No se pudo descifrar la contraseña: fue cifrada por otro usuario o máquina.");
            return string.Empty;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // DTO para serialización JSON (permite constructor privado en AppSettings)
    // ─────────────────────────────────────────────────────────────────────────

    private sealed class SettingsDto
    {
        public string DefaultPortName        { get; set; } = "COM4";
        public int    DefaultBaudRate        { get; set; } = 115200;
        public int    DefaultTimeout         { get; set; } = 5000;
        public string DatabaseServer         { get; set; } = "localhost";
        public string DatabaseName           { get; set; } = "control_electrico_salud";
        public string DatabaseUser           { get; set; } = "root";
        public string DatabasePassword       { get; set; } = "";   // cifrado en Base64
        public int    DatabasePort           { get; set; } = 3306;
        public int    MaxRetries             { get; set; } = 3;
        public int    DelayBetweenCommandsMs { get; set; } = 100;
        public bool   AutoSaveResults        { get; set; } = true;
        public string ApplicationTitle       { get; set; } = "PC7866 - Test Resistivo Embega";
        public bool   ShowDetailedLogs       { get; set; } = true;
    }
}
