using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace RPBDIS_5.Models;

public partial class Employee
{
    [Key]
    [Column("EmployeeID")] // Указываем имя столбца
    public int EmployeeId { get; set; }

    [Column("FullName")] // Указываем имя столбца
    [MaxLength(100)] // Максимальная длина строки
    public string? FullName { get; set; }

    [Column("Position")] // Указываем имя столбца
    [MaxLength(50)]
    public string? Position { get; set; }

    public virtual ICollection<CompletedWork> CompletedWorks { get; set; } = new List<CompletedWork>();

    public virtual ICollection<MaintenanceSchedule> MaintenanceSchedules { get; set; } = new List<MaintenanceSchedule>();
}
