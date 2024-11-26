using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RPBDIS_5.Models;

[Table("View_MaintenanceCostAnalysis")] // Указываем, что это представление
public partial class ViewMaintenanceCostAnalysis
{
    [Key] // Указываем, что это ключ (можно использовать `HasNoKey` в конфигурации)
    [MaxLength(100)]
    public string? EquipmentName { get; set; }

    [Column(TypeName = "money")] // Указываем тип данных для столбца
    public decimal? TotalMaintenanceCost { get; set; }
}
