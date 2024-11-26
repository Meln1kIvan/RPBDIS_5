using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RPBDIS_5.Models;

[Table("View_UnscheduledMaintenanceFrequency")] // Указываем, что это представление
public partial class ViewUnscheduledMaintenanceFrequency
{
    [Key] // Указываем, что это ключ (можно использовать `HasNoKey` в конфигурации)
    [MaxLength(100)]
    public string? EquipmentName { get; set; }

    public int? UnscheduledMaintenanceCount { get; set; }
}
