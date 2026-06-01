using System.Text;

/// <summary>
/// Построитель отчётов с Fluent Interface.
/// </summary>
class ReportBuilder
{
    private DatabaseManager _db;
    private string _sql = "";
    private string _title = "";
    private string[] _headers = Array.Empty<string>();
    private int[] _widths = Array.Empty<int>();
    private bool _numbered = false;   // Группа А
    private string _footer = "";       // Группа В

    public ReportBuilder(DatabaseManager db) { _db = db; }

    // Промежуточные методы
    public ReportBuilder Query(string sql) { _sql = sql; return this; }
    public ReportBuilder Title(string title) { _title = title; return this; }
    public ReportBuilder Header(params string[] columns) { _headers = columns; return this; }
    public ReportBuilder ColumnWidths(params int[] widths) { _widths = widths; return this; }
    public ReportBuilder Numbered() { _numbered = true; return this; }
    public ReportBuilder Footer(string label) { _footer = label; return this; }

    // Терминальные методы
    public string Build()
    {
        var (columns, rows) = _db.ExecuteQuery(_sql);
        var sb = new StringBuilder();

        if (_title.Length > 0)
            sb.AppendLine($"\n=== {_title} ===\n");

        string[] displayHeaders = _headers.Length > 0 ? _headers : columns;
        int colCount = displayHeaders.Length;
        int[] widths = _widths.Length >= colCount ? _widths : Enumerable.Repeat(20, colCount).ToArray();
        int numWidth = _numbered ? 5 : 0;

        // Шапка
        if (_numbered) sb.Append("№".PadRight(numWidth));
        for (int i = 0; i < colCount; i++)
            sb.Append(displayHeaders[i].PadRight(widths[i]));
        sb.AppendLine();

        int totalWidth = numWidth + widths.Sum();
        sb.AppendLine(new string('-', totalWidth));

        // Данные
        for (int r = 0; r < rows.Count; r++)
        {
            if (_numbered) sb.Append((r + 1).ToString().PadRight(numWidth));
            for (int c = 0; c < rows[r].Length && c < colCount; c++)
                sb.Append(rows[r][c].PadRight(widths[c]));
            sb.AppendLine();
        }

        // Нижний колонтитул
        if (_footer.Length > 0)
        {
            sb.AppendLine(new string('-', totalWidth));
            sb.AppendLine($"{_footer}: {rows.Count}");
        }
        return sb.ToString();
    }

    public void Print() => Console.WriteLine(Build());
    public void SaveToFile(string path)   // Группа Б
    {
        File.WriteAllText(path, Build());
        Console.WriteLine($"Отчёт сохранён в файл: {path}");
    }
}