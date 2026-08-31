using System.Globalization;
using System.Text;
using System.Text.Json;
using PC7866.Models;

namespace PC7866.Utils;

/// <summary>
/// Importación/exportación de <see cref="ParametroEnsayo"/> en formato CSV y JSON.
/// </summary>
public static class ParametroImportExport
{
    private static readonly JsonSerializerOptions JsonOpts = new() { WriteIndented = true };
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;
    private const char Sep = ';';

    private static readonly string[] Headers =
    {
        "NPasoEnsayo", "NombreContacto", "ResistenciaNominal", "Tolerancia", "Pendiente", "Offset",
        "ResistenciaMinima", "McpArribaChip", "McpArribaPin", "McpAbajoChip", "McpAbajoPin",
        "CanalMultiplexor", "PosX", "PosY"
    };

    public static void ExportJson(string path, IEnumerable<ParametroEnsayo> parametros)
        => File.WriteAllText(path, JsonSerializer.Serialize(parametros, JsonOpts), Encoding.UTF8);

    public static List<ParametroEnsayo> ImportJson(string path)
        => JsonSerializer.Deserialize<List<ParametroEnsayo>>(File.ReadAllText(path, Encoding.UTF8)) ?? new();

    public static void ExportCsv(string path, IEnumerable<ParametroEnsayo> parametros)
    {
        var sb = new StringBuilder();
        sb.AppendLine(string.Join(Sep, Headers));
        foreach (var p in parametros)
        {
            sb.AppendLine(string.Join(Sep, new[]
            {
                p.NPasoEnsayo.ToString(Inv),
                Escape(p.NombreContacto),
                p.ResistenciaNominal.ToString(Inv),
                p.Tolerancia.ToString(Inv),
                p.Pendiente.ToString(Inv),
                p.Offset.ToString(Inv),
                p.ResistenciaMinima.ToString(Inv),
                p.McpArribaChip.ToString(Inv),
                p.McpArribaPin.ToString(Inv),
                p.McpAbajoChip.ToString(Inv),
                p.McpAbajoPin.ToString(Inv),
                p.CanalMultiplexor.ToString(Inv),
                p.PosX.ToString(Inv),
                p.PosY.ToString(Inv)
            }));
        }
        File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
    }

    public static List<ParametroEnsayo> ImportCsv(string path)
    {
        var list = new List<ParametroEnsayo>();
        var lines = File.ReadAllLines(path, Encoding.UTF8);
        for (int i = 1; i < lines.Length; i++) // se salta la cabecera
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            var c = SplitCsv(lines[i]);
            if (c.Count < Headers.Length) continue;
            list.Add(new ParametroEnsayo
            {
                NPasoEnsayo        = ParseInt(c[0]),
                NombreContacto     = c[1],
                ResistenciaNominal = ParseFloat(c[2]),
                Tolerancia         = ParseFloat(c[3]),
                Pendiente          = ParseFloat(c[4]),
                Offset             = ParseFloat(c[5]),
                ResistenciaMinima  = ParseFloat(c[6]),
                McpArribaChip      = ParseInt(c[7]),
                McpArribaPin       = ParseInt(c[8]),
                McpAbajoChip       = ParseInt(c[9]),
                McpAbajoPin        = ParseInt(c[10]),
                CanalMultiplexor   = ParseInt(c[11]),
                PosX               = ParseInt(c[12]),
                PosY               = ParseInt(c[13])
            });
        }
        return list;
    }

    private static string Escape(string value)
    {
        if (value.IndexOf(Sep) < 0 && value.IndexOf('"') < 0 && value.IndexOf('\n') < 0)
            return value;
        return "\"" + value.Replace("\"", "\"\"") + "\"";
    }

    private static List<string> SplitCsv(string line)
    {
        var result = new List<string>();
        var sb = new StringBuilder();
        bool inQuotes = false;
        for (int i = 0; i < line.Length; i++)
        {
            char ch = line[i];
            if (inQuotes)
            {
                if (ch == '"')
                {
                    if (i + 1 < line.Length && line[i + 1] == '"') { sb.Append('"'); i++; }
                    else inQuotes = false;
                }
                else sb.Append(ch);
            }
            else if (ch == '"') inQuotes = true;
            else if (ch == Sep) { result.Add(sb.ToString()); sb.Clear(); }
            else sb.Append(ch);
        }
        result.Add(sb.ToString());
        return result;
    }

    private static int ParseInt(string s)
        => int.TryParse(s.Trim(), NumberStyles.Integer, Inv, out int v) ? v : 0;

    // Acepta punto o coma como separador decimal (por si el CSV se editó en Excel en español).
    private static float ParseFloat(string s)
        => float.TryParse(s.Trim().Replace(',', '.'), NumberStyles.Float, Inv, out float v) ? v : 0f;
}
