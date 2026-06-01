using System.Text;

Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

string dbPath = "tasks.db";
string projectsCsv = Path.Combine(AppContext.BaseDirectory, "projects.csv");
string tasksCsv = Path.Combine(AppContext.BaseDirectory, "tasks.csv");

var db = new DatabaseManager(dbPath);
db.InitializeDatabase(projectsCsv, tasksCsv);
Console.WriteLine();

string choice;
do
{
    Console.WriteLine("╔═══════════════════════════════════════╗");
    Console.WriteLine("║       УПРАВЛЕНИЕ ЗАДАЧАМИ              ║");
    Console.WriteLine("╠═══════════════════════════════════════╣");
    Console.WriteLine("║ 1 — Показать все проекты              ║");
    Console.WriteLine("║ 2 — Показать все задачи               ║");
    Console.WriteLine("║ 3 — Добавить задачу                   ║");
    Console.WriteLine("║ 4 — Редактировать задачу              ║");
    Console.WriteLine("║ 5 — Удалить задачу                    ║");
    Console.WriteLine("║ 6 — Отчёты                            ║");
    Console.WriteLine("║ 7 — Фильтр по проекту (группа Г)      ║");
    Console.WriteLine("║ 8 — Экспорт в CSV (группа Б)          ║");
    Console.WriteLine("║ 0 — Выход                             ║");
    Console.WriteLine("╚═══════════════════════════════════════╝");
    Console.Write("Ваш выбор: ");
    choice = Console.ReadLine()?.Trim() ?? "";
    Console.WriteLine();

    switch (choice)
    {
        case "1": ShowProjects(db); break;
        case "2": ShowTasks(db); break;
        case "3": AddTask(db); break;
        case "4": EditTask(db); break;
        case "5": DeleteTask(db); break;
        case "6": ReportsMenu(db); break;
        case "7": FilterByProject(db); break;
        case "8": ExportCsv(db); break;
        case "0": Console.WriteLine("До свидания!"); break;
        default: Console.WriteLine("Неверный пункт меню."); break;
    }
    Console.WriteLine();
} while (choice != "0");

// ---------- Вспомогательные методы ----------
static void ShowProjects(DatabaseManager db)
{
    Console.WriteLine("---- Все проекты ----");
    foreach (var p in db.GetAllProjects())
        Console.WriteLine($"  {p}");
}

static void ShowTasks(DatabaseManager db)
{
    Console.WriteLine("---- Все задачи ----");
    foreach (var t in db.GetAllTasks())
        Console.WriteLine($"  {t}");
}

static void AddTask(DatabaseManager db)
{
    Console.WriteLine("---- Добавление задачи ----");
    Console.WriteLine("Доступные проекты:");
    foreach (var p in db.GetAllProjects())
        Console.WriteLine($"  {p}");

    Console.Write("ID проекта: ");
    if (!int.TryParse(Console.ReadLine(), out int projId))
    { Console.WriteLine("Ошибка: введите целое число."); return; }

    Console.Write("Название задачи: ");
    string name = Console.ReadLine()?.Trim() ?? "";
    if (name.Length == 0) { Console.WriteLine("Ошибка: название не может быть пустым."); return; }

    Console.Write("Трудоёмкость (часы): ");
    if (!int.TryParse(Console.ReadLine(), out int hours))
    { Console.WriteLine("Ошибка: введите целое число."); return; }

    try
    {
        db.AddTask(new TaskItem(0, projId, name, hours));
        Console.WriteLine("Задача добавлена.");
    }
    catch (ArgumentException ex) { Console.WriteLine($"Ошибка: {ex.Message}"); }
}

static void EditTask(DatabaseManager db)
{
    Console.WriteLine("---- Редактирование задачи ----");
    Console.Write("Введите ID задачи: ");
    if (!int.TryParse(Console.ReadLine(), out int id))
    { Console.WriteLine("Ошибка: введите целое число."); return; }

    var task = db.GetTaskById(id);
    if (task == null) { Console.WriteLine($"Задача с ID={id} не найдена."); return; }

    Console.WriteLine($"Текущие данные: {task}");
    Console.WriteLine("(Нажмите Enter, чтобы оставить значение без изменений)");

    Console.Write($"Название [{task.Name}]: ");
    string input = Console.ReadLine()?.Trim() ?? "";
    if (input.Length > 0) task.Name = input;

    Console.Write($"ID проекта [{task.ProjectId}]: ");
    input = Console.ReadLine()?.Trim() ?? "";
    if (input.Length > 0 && int.TryParse(input, out int newProjId)) task.ProjectId = newProjId;

    Console.Write($"Часы [{task.Hours}]: ");
    input = Console.ReadLine()?.Trim() ?? "";
    if (input.Length > 0 && int.TryParse(input, out int newHours))
    {
        try { task.Hours = newHours; }
        catch (ArgumentException ex) { Console.WriteLine($"Ошибка: {ex.Message}"); return; }
    }

    db.UpdateTask(task);
    Console.WriteLine("Данные обновлены.");
}

static void DeleteTask(DatabaseManager db)
{
    Console.WriteLine("---- Удаление задачи ----");
    Console.Write("Введите ID задачи: ");
    if (!int.TryParse(Console.ReadLine(), out int id))
    { Console.WriteLine("Ошибка: введите целое число."); return; }

    var task = db.GetTaskById(id);
    if (task == null) { Console.WriteLine($"Задача с ID={id} не найдена."); return; }

    Console.Write($"Удалить «{task.Name}»? (да/нет): ");
    if (Console.ReadLine()?.Trim().ToLower() == "да")
    {
        db.DeleteTask(id);
        Console.WriteLine("Задача удалена.");
    }
    else Console.WriteLine("Удаление отменено.");
}

static void FilterByProject(DatabaseManager db)   // Группа Г
{
    Console.WriteLine("---- Фильтр по проекту ----");
    Console.WriteLine("Доступные проекты:");
    foreach (var p in db.GetAllProjects())
        Console.WriteLine($"  {p}");
    Console.Write("Введите ID проекта: ");
    if (!int.TryParse(Console.ReadLine(), out int projId))
    { Console.WriteLine("Ошибка: введите целое число."); return; }

    var tasks = db.GetTasksByProject(projId);
    if (tasks.Count == 0) { Console.WriteLine("В этом проекте нет задач."); return; }

    Console.WriteLine($"\nЗадачи проекта #{projId}:");
    foreach (var t in tasks) Console.WriteLine($"  {t}");
    Console.WriteLine($"Итого: {tasks.Count}");
}

static void ExportCsv(DatabaseManager db)   // Группа Б
{
    string projPath = Path.Combine(AppContext.BaseDirectory, "projects_export.csv");
    string tasksPath = Path.Combine(AppContext.BaseDirectory, "tasks_export.csv");
    db.ExportToCsv(projPath, tasksPath);
    Console.WriteLine($"Проекты экспортированы в: {projPath}");
    Console.WriteLine($"Задачи экспортированы в: {tasksPath}");
}

static void ReportsMenu(DatabaseManager db)
{
    string choice;
    do
    {
        Console.WriteLine("--- Отчёты ---");
        Console.WriteLine(" 1 - Список задач с названиями проектов");
        Console.WriteLine(" 2 - Количество задач по проектам");
        Console.WriteLine(" 3 - Средняя трудоёмкость по проектам");
        Console.WriteLine(" 0 - Назад");
        Console.Write("Ваш выбор: ");
        choice = Console.ReadLine()?.Trim() ?? "";
        switch (choice)
        {
            case "1": Report1_TasksWithProjects(db); break;
            case "2": Report2_CountByProject(db); break;
            case "3": Report3_AvgHoursByProject(db); break;
            case "0": break;
            default: Console.WriteLine("Неверный пункт."); break;
        }
        Console.WriteLine();
    } while (choice != "0");
}

// Отчёт 1: задачи с названиями проектов (JOIN)
static void Report1_TasksWithProjects(DatabaseManager db)
{
    new ReportBuilder(db)
        .Query(@"SELECT t.task_name, p.project_name, t.hours
                 FROM tasks t
                 JOIN projects p ON t.project_id = p.project_id
                 ORDER BY t.task_name")
        .Title("Задачи по проектам")
        .Header("Задача", "Проект", "Часы")
        .ColumnWidths(25, 20, 10)
        .Numbered()   // группа А
        .Footer("Всего задач")  // группа В
        .Print();
}

// Отчёт 2: количество задач по проектам (GROUP BY COUNT)
static void Report2_CountByProject(DatabaseManager db)
{
    new ReportBuilder(db)
        .Query(@"SELECT p.project_name, COUNT(*) AS cnt
                 FROM tasks t
                 JOIN projects p ON t.project_id = p.project_id
                 GROUP BY p.project_name
                 ORDER BY p.project_name")
        .Title("Количество задач по проектам")
        .Header("Проект", "Кол-во задач")
        .ColumnWidths(25, 12)
        .Print();
}

// Отчёт 3: средняя трудоёмкость по проектам (GROUP BY AVG)
static void Report3_AvgHoursByProject(DatabaseManager db)
{
    new ReportBuilder(db)
        .Query(@"SELECT p.project_name, ROUND(AVG(t.hours), 1) AS avg_hours
                 FROM tasks t
                 JOIN projects p ON t.project_id = p.project_id
                 GROUP BY p.project_name
                 ORDER BY avg_hours DESC")
        .Title("Средняя трудоёмкость по проектам (часы)")
        .Header("Проект", "Среднее часов")
        .ColumnWidths(25, 15)
        .Print();
}