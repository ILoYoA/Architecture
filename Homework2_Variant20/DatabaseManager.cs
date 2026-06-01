using Microsoft.Data.Sqlite;
using System.Text;

/// <summary>
/// Управление базой данных SQLite. Инкапсулирует создание таблиц,
/// импорт из CSV, CRUD-операции и выполнение произвольных запросов.
/// </summary>
class DatabaseManager
{
    private string _connectionString;

    public DatabaseManager(string dbPath)
    {
        _connectionString = $"Data Source={dbPath}";
    }

    // ========== Инициализация ==========
    public void InitializeDatabase(string projectsCsvPath, string tasksCsvPath)
    {
        CreateTables();
        if (GetAllProjects().Count == 0 && File.Exists(projectsCsvPath))
        {
            ImportProjectsFromCsv(projectsCsvPath);
            Console.WriteLine($"[OK] Загружены проекты из {projectsCsvPath}");
        }
        if (GetAllTasks().Count == 0 && File.Exists(tasksCsvPath))
        {
            ImportTasksFromCsv(tasksCsvPath);
            Console.WriteLine($"[OK] Загружены задачи из {tasksCsvPath}");
        }
    }

    private void CreateTables()
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS projects (
                project_id INTEGER PRIMARY KEY AUTOINCREMENT,
                project_name TEXT NOT NULL
            );
            CREATE TABLE IF NOT EXISTS tasks (
                task_id INTEGER PRIMARY KEY AUTOINCREMENT,
                project_id INTEGER NOT NULL,
                task_name TEXT NOT NULL,
                hours INTEGER NOT NULL,
                FOREIGN KEY (project_id) REFERENCES projects(project_id)
            );
        ";
        cmd.ExecuteNonQuery();
    }

    private void ImportProjectsFromCsv(string path)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        string[] lines = File.ReadAllLines(path);
        for (int i = 1; i < lines.Length; i++) // пропускаем заголовок
        {
            string[] parts = lines[i].Split(';');
            if (parts.Length < 2) continue;
            var cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO projects (project_id, project_name) VALUES (@id, @name)";
            cmd.Parameters.AddWithValue("@id", int.Parse(parts[0]));
            cmd.Parameters.AddWithValue("@name", parts[1]);
            cmd.ExecuteNonQuery();
        }
    }

    private void ImportTasksFromCsv(string path)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        string[] lines = File.ReadAllLines(path);
        for (int i = 1; i < lines.Length; i++)
        {
            string[] parts = lines[i].Split(';');
            if (parts.Length < 4) continue;
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO tasks (task_id, project_id, task_name, hours)
                VALUES (@id, @projectId, @name, @hours)
            ";
            cmd.Parameters.AddWithValue("@id", int.Parse(parts[0]));
            cmd.Parameters.AddWithValue("@projectId", int.Parse(parts[1]));
            cmd.Parameters.AddWithValue("@name", parts[2]);
            cmd.Parameters.AddWithValue("@hours", int.Parse(parts[3]));
            cmd.ExecuteNonQuery();
        }
    }

    // ========== Чтение данных ==========
    public List<Project> GetAllProjects()
    {
        var result = new List<Project>();
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT project_id, project_name FROM projects ORDER BY project_id";
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            result.Add(new Project(reader.GetInt32(0), reader.GetString(1)));
        return result;
    }

    public List<TaskItem> GetAllTasks()
    {
        var result = new List<TaskItem>();
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT task_id, project_id, task_name, hours FROM tasks ORDER BY task_id";
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            result.Add(new TaskItem(reader.GetInt32(0), reader.GetInt32(1), reader.GetString(2), reader.GetInt32(3)));
        return result;
    }

    public TaskItem GetTaskById(int id)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT task_id, project_id, task_name, hours FROM tasks WHERE task_id = @id";
        cmd.Parameters.AddWithValue("@id", id);
        using var reader = cmd.ExecuteReader();
        if (reader.Read())
            return new TaskItem(reader.GetInt32(0), reader.GetInt32(1), reader.GetString(2), reader.GetInt32(3));
        return null;
    }

    // ========== Изменение данных ==========
    public void AddTask(TaskItem task)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "INSERT INTO tasks (project_id, task_name, hours) VALUES (@projectId, @name, @hours)";
        cmd.Parameters.AddWithValue("@projectId", task.ProjectId);
        cmd.Parameters.AddWithValue("@name", task.Name);
        cmd.Parameters.AddWithValue("@hours", task.Hours);
        cmd.ExecuteNonQuery();
    }

    public void UpdateTask(TaskItem task)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE tasks SET project_id = @projectId, task_name = @name, hours = @hours WHERE task_id = @id";
        cmd.Parameters.AddWithValue("@id", task.Id);
        cmd.Parameters.AddWithValue("@projectId", task.ProjectId);
        cmd.Parameters.AddWithValue("@name", task.Name);
        cmd.Parameters.AddWithValue("@hours", task.Hours);
        cmd.ExecuteNonQuery();
    }

    public void DeleteTask(int id)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM tasks WHERE task_id = @id";
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }

    // ========== Для отчётов (выполнение произвольного SQL) ==========
    public (string[] columns, List<string[]> rows) ExecuteQuery(string sql)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        using var reader = cmd.ExecuteReader();

        string[] columns = new string[reader.FieldCount];
        for (int i = 0; i < reader.FieldCount; i++)
            columns[i] = reader.GetName(i);

        var rows = new List<string[]>();
        while (reader.Read())
        {
            string[] row = new string[reader.FieldCount];
            for (int i = 0; i < reader.FieldCount; i++)
                row[i] = reader.GetValue(i)?.ToString() ?? "";
            rows.Add(row);
        }
        return (columns, rows);
    }

    // ========== Фильтр по проекту (группа Г) ==========
    public List<TaskItem> GetTasksByProject(int projectId)
    {
        var result = new List<TaskItem>();
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT task_id, project_id, task_name, hours FROM tasks WHERE project_id = @pid ORDER BY task_name";
        cmd.Parameters.AddWithValue("@pid", projectId);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            result.Add(new TaskItem(reader.GetInt32(0), reader.GetInt32(1), reader.GetString(2), reader.GetInt32(3)));
        return result;
    }

    // ========== Экспорт в CSV (группа Б) ==========
    public void ExportToCsv(string projectsPath, string tasksPath)
    {
        var projLines = new List<string> { "project_id;project_name" };
        foreach (var p in GetAllProjects())
            projLines.Add($"{p.Id};{p.Name}");
        File.WriteAllLines(projectsPath, projLines);

        var taskLines = new List<string> { "task_id;project_id;task_name;hours" };
        foreach (var t in GetAllTasks())
            taskLines.Add($"{t.Id};{t.ProjectId};{t.Name};{t.Hours}");
        File.WriteAllLines(tasksPath, taskLines);
    }
}