using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace RPBDIS_5.Models;

public class MaintenanceSchedule
{
    [Key]
    [Column("ScheduleID")] // Указываем имя столбца
    public int ScheduleId { get; set; }

    [ForeignKey("Equipment")]
    [Column("EquipmentID")] // Указываем внешний ключ
    public int? EquipmentId { get; set; }

    [ForeignKey("MaintenanceType")]
    [Column("MaintenanceTypeID")]
    public int? MaintenanceTypeId { get; set; }

    [Column("ScheduledDate")] // Указываем внешний ключ
    public DateOnly? ScheduledDate { get; set; }

    [Column(TypeName = "money")] // Указываем тип данных для столбца
    public decimal? EstimatedCost { get; set; }

    [ForeignKey("ResponsibleEmployee")]
    [Column("ResponsibleEmployeeID")]
    public int? ResponsibleEmployeeId { get; set; }
    public virtual Employee? ResponsibleEmployee { get; set; } // Навигационное свойство
    public virtual Equipment? Equipment { get; set; }
    public virtual MaintenanceType? MaintenanceType { get; set; }
}
