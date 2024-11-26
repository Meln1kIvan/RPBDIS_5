using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using RPBDIS_5.Controllers;
using RPBDIS_5.Data;
using RPBDIS_5.Models;
using Xunit;

namespace TestControllers
{
    public class CompletedWorksControllerTests
    {
        private readonly MonitoringContext _context;
        private readonly CompletedWorksController _controller;

        public CompletedWorksControllerTests()
        {
            // Настройка контекста базы данных с использованием InMemory
            var options = new DbContextOptionsBuilder<MonitoringContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Уникальное имя базы данных для каждого теста
                .Options;

            _context = new MonitoringContext(options);
            _controller = new CompletedWorksController(_context);

            // Заполнение базы данных тестовыми данными
            SeedTestData();
        }

        private void SeedTestData()
        {
            // Очищаем базу данных перед каждым тестом
            _context.Equipments.RemoveRange(_context.Equipments);
            _context.MaintenanceTypes.RemoveRange(_context.MaintenanceTypes);
            _context.Employees.RemoveRange(_context.Employees);
            _context.CompletedWorks.RemoveRange(_context.CompletedWorks);
            _context.SaveChanges();

            // Добавляем тестовые данные в контекст
            var equipments = new List<Equipment>
            {
                new Equipment { EquipmentId = 1, Name = "Equipment 1" },
                new Equipment { EquipmentId = 2, Name = "Equipment 2" }
            };

            var maintenanceTypes = new List<MaintenanceType>
            {
                new MaintenanceType { MaintenanceTypeId = 1, Description = "Type A" },
                new MaintenanceType { MaintenanceTypeId = 2, Description = "Type B" }
            };

            var employees = new List<Employee>
            {
                new Employee { EmployeeId = 1, FullName = "John Doe" },
                new Employee { EmployeeId = 2, FullName = "Jane Smith" }
            };

            var completedWorks = new List<CompletedWork>
            {
                new CompletedWork
                {
                    CompletedMaintenanceId = 1,
                    EquipmentId = 1,
                    MaintenanceTypeId = 1,
                    ResponsibleEmployeeId = 1,
                    CompletionDate = DateOnly.FromDateTime(new DateTime(2023, 1, 1)),
                    ActualCost = 100.0m
                },
                new CompletedWork
                {
                    CompletedMaintenanceId = 2,
                    EquipmentId = 2,
                    MaintenanceTypeId = 2,
                    ResponsibleEmployeeId = 2,
                    CompletionDate = DateOnly.FromDateTime(new DateTime(2023, 2, 1)),
                    ActualCost = 200.0m
                }
            };

            _context.Equipments.AddRange(equipments);
            _context.MaintenanceTypes.AddRange(maintenanceTypes);
            _context.Employees.AddRange(employees);
            _context.CompletedWorks.AddRange(completedWorks);
            _context.SaveChanges();
        }

        [Fact]
        public async Task Details_ReturnsNotFound_WhenIdIsInvalid()
        {
            // Act
            var result = await _controller.Details(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Details_ReturnsViewResult_WhenIdIsValid()
        {
            // Act
            var result = await _controller.Details(1) as ViewResult;

            // Assert
            var model = Assert.IsAssignableFrom<CompletedWork>(result.ViewData.Model);
            Assert.Equal(1, model.CompletedMaintenanceId);
        }

        [Fact]
        public async Task Create_PostValidData_RedirectsToIndex()
        {
            // Arrange
            var newWork = new CompletedWork
            {
                EquipmentId = 1,
                MaintenanceTypeId = 1,
                ResponsibleEmployeeId = 1,
                CompletionDate = DateOnly.FromDateTime(new DateTime(2023, 3, 1)),
                ActualCost = 150.0m
            };

            // Act
            var result = await _controller.Create(newWork);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }

        [Fact]
        public async Task Edit_ReturnsNotFound_WhenIdIsInvalid()
        {
            // Act
            var result = await _controller.Edit(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_PostValidData_RedirectsToIndex()
        {
            // Arrange
            var originalWork = await _context.CompletedWorks.FirstOrDefaultAsync(cw => cw.CompletedMaintenanceId == 1);

            // Detach оригинальную сущность
            _context.Entry(originalWork).State = EntityState.Detached;

            var updatedWork = new CompletedWork
            {
                CompletedMaintenanceId = originalWork.CompletedMaintenanceId,
                EquipmentId = 2,
                MaintenanceTypeId = 2,
                ResponsibleEmployeeId = 2,
                CompletionDate = DateOnly.FromDateTime(new DateTime(2023, 4, 1)),
                ActualCost = 250.0m
            };

            // Act
            var result = await _controller.Edit(updatedWork.CompletedMaintenanceId, updatedWork);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }



        [Fact]
        public async Task Delete_ReturnsNotFound_WhenIdIsInvalid()
        {
            // Act
            var result = await _controller.Delete(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteConfirmed_RemovesEntity_AndRedirectsToIndex()
        {
            // Act
            var result = await _controller.DeleteConfirmed(1);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);

            // Проверяем, что объект удален
            var deletedWork = await _context.CompletedWorks.FindAsync(1);
            Assert.Null(deletedWork);
        }
    }
}
