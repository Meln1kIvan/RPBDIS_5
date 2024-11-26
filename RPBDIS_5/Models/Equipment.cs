using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace RPBDIS_5.Models;

public partial class Equipment
{
    [Key]
    [Column("EquipmentID")] // Указываем имя столбца
    public int EquipmentId { get; set; }

    [MaxLength(50)]
    public string? InventoryNumber { get; set; }

    [MaxLength(100)]
    public string? Name { get; set; }

    public DateOnly? StartDate { get; set; }

    [MaxLength(100)]
    public string? Location { get; set; }

    public virtual ICollection<CompletedWork> CompletedWorks { get; set; } = new List<CompletedWork>();

    public virtual ICollection<MaintenanceSchedule> MaintenanceSchedules { get; set; } = new List<MaintenanceSchedule>();
}
