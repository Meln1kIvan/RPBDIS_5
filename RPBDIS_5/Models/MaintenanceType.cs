using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace RPBDIS_5.Models;

public partial class MaintenanceType
{
    [Key]
    [Column("MaintenanceTypeID")] // Указываем имя столбца
    public int MaintenanceTypeId { get; set; }

    [MaxLength(100)] // Максимальная длина строки
    public string? Description { get; set; }

    public virtual ICollection<CompletedWork> CompletedWorks { get; set; } = new List<CompletedWork>();

    public virtual ICollection<MaintenanceSchedule> MaintenanceSchedules { get; set; } = new List<MaintenanceSchedule>();
}
