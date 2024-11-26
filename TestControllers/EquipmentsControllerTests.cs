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
    public class EquipmentsControllerTests
    {
        private Mock<MonitoringContext> GetMockContext()
        {
            var options = new DbContextOptionsBuilder<MonitoringContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase").Options;
            return new Mock<MonitoringContext>(options);
        }

        #region Index Tests
    //    [Fact]
    //    public async Task Index_ReturnsViewResult_WithEquipmentList()
    //    {
    //        // Arrange
    //        var data = new List<Equipment>
    //{
    //    new Equipment { EquipmentId = 1, Name = "Equipment1", InventoryNumber = "INV1", Location = "Room1", StartDate = new DateOnly(2023, 11, 1) },
    //    new Equipment { EquipmentId = 2, Name = "Equipment2", InventoryNumber = "INV2", Location = "Room2", StartDate = new DateOnly(2023, 11, 2) }
    //}.AsQueryable();

    //        var mockDbSet = new Mock<DbSet<Equipment>>();
    //        mockDbSet.AsAsyncEnumerable(data); // Используем Moq.EntityFrameworkCore для поддержки IAsyncEnumerable

    //        var mockContext = GetMockContext();
    //        mockContext.Setup(m => m.Equipments).Returns(mockDbSet.Object);

    //        var controller = new EquipmentsController(mockContext.Object);

    //        // Act
    //        var result = await controller.Index(null, null, null, null, null);

    //        // Assert
    //        var viewResult = Assert.IsType<ViewResult>(result);
    //        var model = Assert.IsAssignableFrom<List<Equipment>>(viewResult.Model);
    //        Assert.Equal(2, model.Count);
    //    }
        #endregion

        #region Create Tests
        [Fact]
        public void Create_Get_ReturnsViewResult()
        {
            // Arrange
            var controller = new EquipmentsController(null);

            // Act
            var result = controller.Create();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        //[Fact]
        //public async Task Create_Post_RedirectsToIndex_WhenModelStateIsValid()
        //{
        //    // Arrange
        //    var mockContext = GetMockContext();
        //    var mockDbSet = new Mock<DbSet<Equipment>>();
        //    mockDbSet
        //        .Setup(m => m.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Equipment, bool>>>(), It.IsAny<CancellationToken>()))
        //        .ReturnsAsync(false); // Указываем, что записи с таким InventoryNumber нет

        //    mockContext.Setup(m => m.Equipments).Returns(mockDbSet.Object);

        //    var controller = new EquipmentsController(mockContext.Object);
        //    var equipment = new Equipment { EquipmentId = 1, Name = "Equipment1", InventoryNumber = "INV1", Location = "Room1", StartDate = new DateOnly(2023, 11, 1) };

        //    // Act
        //    var result = await controller.Create(equipment);

        //    // Assert
        //    mockContext.Verify(m => m.Add(equipment), Times.Once);
        //    mockContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        //    var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
        //    Assert.Equal("Index", redirectToActionResult.ActionName);
        //}

        [Fact]
        public async Task Create_Post_ReturnsViewResult_WhenModelStateIsInvalid()
        {
            // Arrange
            var mockContext = GetMockContext();
            var controller = new EquipmentsController(mockContext.Object);
            controller.ModelState.AddModelError("Name", "Required");
            var equipment = new Equipment { EquipmentId = 1, InventoryNumber = "INV1" };

            // Act
            var result = await controller.Create(equipment);

            // Assert
            Assert.IsType<ViewResult>(result);
        }
        #endregion

        #region Edit Tests
        [Fact]
        public async Task Edit_Get_ReturnsViewResult_WithEquipment()
        {
            // Arrange
            var mockContext = GetMockContext();
            var equipment = new Equipment { EquipmentId = 1, Name = "Equipment1", InventoryNumber = "INV1", Location = "Room1", StartDate = new DateOnly(2023, 11, 1) };

            var mockDbSet = new Mock<DbSet<Equipment>>();
            mockDbSet.Setup(m => m.FindAsync(1)).ReturnsAsync(equipment);

            mockContext.Setup(m => m.Equipments).Returns(mockDbSet.Object);

            var controller = new EquipmentsController(mockContext.Object);

            // Act
            var result = await controller.Edit(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<Equipment>(viewResult.Model);
            Assert.Equal("Equipment1", model.Name);
        }

        [Fact]
        public async Task Edit_Get_ReturnsNotFound_WhenEquipmentDoesNotExist()
        {
            // Arrange
            var mockContext = GetMockContext();
            var mockDbSet = new Mock<DbSet<Equipment>>();
            mockDbSet.Setup(m => m.FindAsync(1)).ReturnsAsync((Equipment)null);

            mockContext.Setup(m => m.Equipments).Returns(mockDbSet.Object);

            var controller = new EquipmentsController(mockContext.Object);

            // Act
            var result = await controller.Edit(1);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
        #endregion

        #region Delete Tests
        //[Fact]
        //public async Task Delete_Get_ReturnsViewResult_WithEquipment()
        //{
        //    // Arrange
        //    var equipment = new Equipment { EquipmentId = 1, Name = "Equipment1", InventoryNumber = "INV1", Location = "Room1", StartDate = new DateOnly(2023, 11, 1) };

        //    var mockDbSet = new Mock<DbSet<Equipment>>();
        //    mockDbSet.AsAsyncEnumerable(new List<Equipment> { equipment }.AsQueryable());

        //    var mockContext = GetMockContext();
        //    mockContext.Setup(m => m.Equipments).Returns(mockDbSet.Object);

        //    var controller = new EquipmentsController(mockContext.Object);

        //    // Act
        //    var result = await controller.Delete(1);

        //    // Assert
        //    var viewResult = Assert.IsType<ViewResult>(result);
        //    var model = Assert.IsType<Equipment>(viewResult.Model);
        //    Assert.Equal("Equipment1", model.Name);
        //}

        //[Fact]
        //public async Task Delete_Get_ReturnsNotFound_WhenEquipmentDoesNotExist()
        //{
        //    // Arrange
        //    var mockContext = GetMockContext();
        //    var mockDbSet = new Mock<DbSet<Equipment>>();
        //    mockDbSet
        //        .Setup(m => m.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Equipment, bool>>>(), It.IsAny<CancellationToken>()))
        //        .ReturnsAsync((Equipment)null);

        //    mockContext.Setup(m => m.Equipments).Returns(mockDbSet.Object);

        //    var controller = new EquipmentsController(mockContext.Object);

        //    // Act
        //    var result = await controller.Delete(1);

        //    // Assert
        //    Assert.IsType<NotFoundResult>(result);
        //}

        [Fact]
        public async Task DeleteConfirmed_Post_RedirectsToIndex_WhenEquipmentIsDeleted()
        {
            // Arrange
            var mockContext = GetMockContext();
            var equipment = new Equipment { EquipmentId = 1, Name = "Equipment1" };

            var mockDbSet = new Mock<DbSet<Equipment>>();
            mockDbSet.Setup(m => m.FindAsync(1)).ReturnsAsync(equipment);

            mockContext.Setup(m => m.Equipments).Returns(mockDbSet.Object);

            var controller = new EquipmentsController(mockContext.Object);

            // Act
            var result = await controller.DeleteConfirmed(1);

            // Assert
            mockDbSet.Verify(m => m.Remove(equipment), Times.Once);
            mockContext.Verify(m => m.SaveChangesAsync(default), Times.Once);
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
        }
        #endregion
    }
}