/// <summary>
/// Задача (основная таблица, сторона «много»)
/// </summary>
class TaskItem
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public string Name { get; set; }

    private int _hours;
    /// <summary>
    /// Трудоёмкость в человеко-часах (не может быть отрицательной)
    /// </summary>
    public int Hours
    {
        get => _hours;
        set
        {
            if (value < 0)
                throw new ArgumentException("Трудоёмкость не может быть отрицательной");
            _hours = value;
        }
    }

    /// <summary>Конструктор с параметрами</summary>
    public TaskItem(int id, int projectId, string name, int hours)
    {
        Id = id;
        ProjectId = projectId;
        Name = name;
        Hours = hours;
    }

    /// <summary>Конструктор по умолчанию</summary>
    public TaskItem() : this(0, 0, "", 0) { }

    public override string ToString() => $"[{Id}] {Name}, проект #{ProjectId}, часов: {Hours}";
}