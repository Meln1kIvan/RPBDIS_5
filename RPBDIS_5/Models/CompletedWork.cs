using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RPBDIS_5.Models;
 
public class CompletedWork
{
    [Key]
    [Column("CompletedMaintenanceID")] // Указание имени столбца
    public int CompletedMaintenanceId { get; set; }

    [ForeignKey("MaintenanceType")]
    [Column("MaintenanceTypeID")]
    public int? MaintenanceTypeId { get; set; }

    [ForeignKey("Equipment")]
    [Column("EquipmentID")]
    public int? EquipmentId { get; set; }

    [Column("CompletionDate")] // Тип данных для столбца
    public DateOnly? CompletionDate { get; set; }

    [Column(TypeName = "money")] // Тип данных для столбца
    public decimal? ActualCost { get; set; }

    [ForeignKey("ResponsibleEmployee")]
    [Column("ResponsibleEmployeeID")]
    public int? ResponsibleEmployeeId { get; set; }
    public virtual Employee? ResponsibleEmployee { get; set; } // Навигационное свойство
    public virtual Equipment? Equipment { get; set; }
    public virtual MaintenanceType? MaintenanceType { get; set; }
}
