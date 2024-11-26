using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using RPBDIS_5.Models;

namespace RPBDIS_5.Data;

public partial class MonitoringContext : DbContext
{
    public MonitoringContext()
    {
    }

    public MonitoringContext(DbContextOptions<MonitoringContext> options)
        : base(options)
    {
    }

    public virtual DbSet<CompletedWork> CompletedWorks { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<Equipment> Equipments { get; set; }

    public virtual DbSet<MaintenanceSchedule> MaintenanceSchedules { get; set; }

    public virtual DbSet<MaintenanceType> MaintenanceTypes { get; set; }

    public virtual DbSet<ViewMaintenanceCostAnalysis> ViewMaintenanceCostAnalyses { get; set; }

    public virtual DbSet<ViewUnscheduledMaintenanceFrequency> ViewUnscheduledMaintenanceFrequencies { get; set; }
 
}
