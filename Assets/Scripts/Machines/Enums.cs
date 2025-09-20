// Enums.cs
public enum MachineType
{
    Cutter,     // Резак
    Bender,     // Гибочный станок
    Welder,     // Сварочный аппарат
    QualityControl // Контроль качества
}

public enum ProductType
{
    RawPipe,        // Сырая труба
    BentSector,     // Гнутый сектор
    FinalProduct,   // Готовый отвод
    DefectiveProduct // Бракованная продукция
}

public enum WorkerType
{
    Logist,     // Логист
    Controller  // Контролер качества
}
