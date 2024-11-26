using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using RPBDIS_5.Data; // Замените на ваше пространство имен для DbContext
using RPBDIS_5.Models; // Замените на ваше пространство имен для моделей
using System;
using System.Linq;

namespace RPBDIS_5.Middlewares // Замените на ваше пространство имен
{
    public class DatabaseInitializerMiddleware
    {
        private readonly RequestDelegate _next;

        public DatabaseInitializerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Получаем ApplicationDbContext через DI
            var dbContext = context.RequestServices.GetRequiredService<MonitoringContext>();

            // Проверяем, пусты ли таблицы. Если да, то добавляем тестовые данные
            if (!dbContext.Equipments.Any() && !dbContext.Employees.Any() && !dbContext.MaintenanceTypes.Any())
            {
                // Добавляем данные для Equipment
                for (int i = 1; i <= 500; i++)
                {
                    dbContext.Equipments.Add(new Equipment
                    {
                        InventoryNumber = $"INV{i:D4}",
                        Name = $"Equipment {i}",
                        StartDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-i * 10)),
                        Location = $"Location {i % 10 + 1}"
                    });
                }

                // Добавляем данные для MaintenanceTypes
                for (int j = 1; j <= 10; j++)
                {
                    dbContext.MaintenanceTypes.Add(new MaintenanceType
                    {
                        Description = $"Maintenance Type {j}"
                    });
                }

                // Добавляем данные для Employees
                for (int k = 1; k <= 50; k++)
                {
                    dbContext.Employees.Add(new Employee
                    {
                        FullName = $"Employee {k}",
                        Position = k % 2 == 0 ? "Technician" : "Engineer"
                    });
                }

                // Сохраняем изменения перед добавлением записей для CompletedWorks и MaintenanceSchedules
                await dbContext.SaveChangesAsync();

                // Получаем созданные ID для Equipment, MaintenanceTypes и Employees
                var equipmentIds = dbContext.Equipments.Select(e => e.EquipmentId).ToList();
                var maintenanceTypeIds = dbContext.MaintenanceTypes.Select(mt => mt.MaintenanceTypeId).ToList();
                var employeeIds = dbContext.Employees.Select(emp => emp.EmployeeId).ToList();

                // Добавляем данные для MaintenanceSchedules
                for (int l = 1; l <= 10000; l++)
                {
                    dbContext.MaintenanceSchedules.Add(new MaintenanceSchedule
                    {
                        EquipmentId = equipmentIds[l % equipmentIds.Count],
                        MaintenanceTypeId = maintenanceTypeIds[l % maintenanceTypeIds.Count],
                        ScheduledDate = DateOnly.FromDateTime(DateTime.Now.AddDays(l)),
                        ResponsibleEmployeeId = employeeIds[l % employeeIds.Count],
                        EstimatedCost = 100 + (l % 500)
                    });
                }

                // Добавляем данные для CompletedWorks
                for (int m = 1; m <= 10000; m++)
                {
                    dbContext.CompletedWorks.Add(new CompletedWork
                    {
                        EquipmentId = equipmentIds[m % equipmentIds.Count],
                        MaintenanceTypeId = maintenanceTypeIds[m % maintenanceTypeIds.Count],
                        CompletionDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-m)),
                        ResponsibleEmployeeId = employeeIds[m % employeeIds.Count],
                        ActualCost = 100 + (m % 500)
                    });
                }

                // Сохраняем изменения в базе данных
                await dbContext.SaveChangesAsync();
            }

            // Передаем запрос дальше по конвейеру
            await _next(context);
        }
    }

    // Класс-расширение для упрощения добавления middleware
    public static class DatabaseInitializerExtensions
    {
        public static IApplicationBuilder UseDatabaseInitializer(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<DatabaseInitializerMiddleware>();
        }
    }
}
