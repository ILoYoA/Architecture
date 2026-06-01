/// <summary>
/// Проект (справочная таблица, сторона «один»)
/// </summary>
class Project
{
    /// <summary>Идентификатор проекта</summary>
    public int Id { get; set; }

    /// <summary>Название проекта</summary>
    public string Name { get; set; }

    /// <summary>Конструктор с параметрами</summary>
    public Project(int id, string name)
    {
        Id = id;
        Name = name;
    }

    /// <summary>Конструктор по умолчанию</summary>
    public Project() : this(0, "") { }

    public override string ToString() => $"[{Id}] {Name}";
}
