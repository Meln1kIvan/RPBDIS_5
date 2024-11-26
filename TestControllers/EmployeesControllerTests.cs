using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using RPBDIS_5.Controllers;
using RPBDIS_5.Data;
using RPBDIS_5.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace TestControllers
{
    public class EmployeesControllerTests
    {
        private readonly EmployeesController _controller;
        private readonly MonitoringContext _context;

        public EmployeesControllerTests()
        {
            // Используем InMemory базу данных для тестов
            var options = new DbContextOptionsBuilder<MonitoringContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _context = new MonitoringContext(options);

            // Добавляем тестовые данные
            if (!_context.Employees.Any())
            {
                _context.Employees.AddRange(new List<Employee>
                {
                    new Employee { EmployeeId = 1, FullName = "Employee 1", Position = "Manager" },
                    new Employee { EmployeeId = 2, FullName = "Employee 2", Position = "Developer" }
                });
                _context.SaveChanges();
            }

            _controller = new EmployeesController(_context);
        }


        [Fact]
        public async Task Details_ValidId_ReturnsViewResult_WithEmployee()
        {
            // Act
            var result = await _controller.Details(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<Employee>(viewResult.Model);
            Assert.Equal(1, model.EmployeeId);
        }

        [Fact]
        public async Task Details_InvalidId_ReturnsNotFound()
        {
            // Act
            var result = await _controller.Details(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Get_ValidId_ReturnsViewResult_WithEmployee()
        {
            // Act
            var result = await _controller.Edit(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<Employee>(viewResult.Model);
            Assert.Equal(1, model.EmployeeId);
        }

        [Fact]
        public async Task Edit_Get_InvalidId_ReturnsNotFound()
        {
            // Act
            var result = await _controller.Edit(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Post_ValidData_RedirectsToIndex()
        {
            // Arrange
            var employee = _context.Employees.AsNoTracking().First(); // Получаем сущность без отслеживания
            var updatedEmployee = new Employee
            {
                EmployeeId = employee.EmployeeId, // Существующий ID
                FullName = "Updated Name",
                Position = "Updated Position"
            };

            // Act
            var result = await _controller.Edit(employee.EmployeeId, updatedEmployee);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
        }

        [Fact]
        public async Task Edit_Post_InvalidId_ReturnsNotFound()
        {
            // Arrange
            var employee = new Employee { EmployeeId = 999, FullName = "Updated Name", Position = "Updated Position" };

            // Act
            var result = await _controller.Edit(999, employee);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_Get_ValidId_ReturnsViewResult_WithEmployee()
        {
            // Act
            var result = await _controller.Delete(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<Employee>(viewResult.Model);
            Assert.Equal(1, model.EmployeeId);
        }

        [Fact]
        public async Task Delete_Get_InvalidId_ReturnsNotFound()
        {
            // Act
            var result = await _controller.Delete(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteConfirmed_ValidId_RedirectsToIndex()
        {
            // Act
            var result = await _controller.DeleteConfirmed(1);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
        }

        [Fact]
        public async Task DeleteConfirmed_InvalidId_ReturnsNotFound()
        {
            // Act
            var result = await _controller.DeleteConfirmed(999); // Неверный ID

            // Assert
            Assert.IsType<NotFoundResult>(result); // Проверяем, что возвращается NotFound
        }

    }
}
