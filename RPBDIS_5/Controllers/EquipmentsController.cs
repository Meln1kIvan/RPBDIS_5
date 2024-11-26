using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using RPBDIS_5.Data;
using RPBDIS_5.Models;

namespace RPBDIS_5.Controllers
{
    public class EquipmentsController : Controller
    {
        private readonly MonitoringContext _context;

        public EquipmentsController(MonitoringContext context)
        {
            _context = context;
        }

        // GET: Equipments
        public async Task<IActionResult> Index(
     string inventoryNumber,
     string name,
     DateOnly? startDate,
     string location,
     string sortOrder)
        {
            // Извлечение параметров из Cookie, если они не указаны
            inventoryNumber ??= Request.Cookies["FilterInventoryNumber"];
            name ??= Request.Cookies["FilterName"];
            location ??= Request.Cookies["FilterLocation"];
            sortOrder ??= Request.Cookies["SortOrder"];

            // Обработка даты из Cookie
            if (!startDate.HasValue && Request.Cookies["FilterStartDate"] != null)
            {
                if (DateOnly.TryParse(Request.Cookies["FilterStartDate"], out var parsedDate))
                {
                    startDate = parsedDate;
                }
            }

            // Сохранение параметров в Cookie
            Response.Cookies.Append("FilterInventoryNumber", inventoryNumber ?? string.Empty, new CookieOptions { Expires = DateTimeOffset.UtcNow.AddMinutes(30) });
            Response.Cookies.Append("FilterName", name ?? string.Empty, new CookieOptions { Expires = DateTimeOffset.UtcNow.AddMinutes(30) });
            Response.Cookies.Append("FilterStartDate", startDate?.ToString("yyyy-MM-dd") ?? string.Empty, new CookieOptions { Expires = DateTimeOffset.UtcNow.AddMinutes(30) });
            Response.Cookies.Append("FilterLocation", location ?? string.Empty, new CookieOptions { Expires = DateTimeOffset.UtcNow.AddMinutes(30) });
            Response.Cookies.Append("SortOrder", sortOrder ?? string.Empty, new CookieOptions { Expires = DateTimeOffset.UtcNow.AddMinutes(30) });

            // Добавляем параметры сортировки
            ViewData["CurrentSort"] = sortOrder;
            ViewData["InventorySortParm"] = sortOrder == "InventoryAsc" ? "InventoryDesc" : "InventoryAsc";
            ViewData["NameSortParm"] = sortOrder == "NameAsc" ? "NameDesc" : "NameAsc";
            ViewData["StartDateSortParm"] = sortOrder == "StartDateAsc" ? "StartDateDesc" : "StartDateAsc";
            ViewData["LocationSortParm"] = sortOrder == "LocationAsc" ? "LocationDesc" : "LocationAsc";

            // Передаем значения фильтров в ViewData
            ViewData["FilterInventoryNumber"] = inventoryNumber;
            ViewData["FilterName"] = name;
            ViewData["FilterStartDate"] = startDate?.ToString("yyyy-MM-dd"); // Формат для HTML input type="date"
            ViewData["FilterLocation"] = location;

            // Фильтрация данных
            var equipments = _context.Equipments.AsQueryable();

            if (!string.IsNullOrEmpty(inventoryNumber))
            {
                equipments = equipments.Where(e => e.InventoryNumber.ToLower().Contains(inventoryNumber.ToLower()));
            }

            if (!string.IsNullOrEmpty(name))
            {
                equipments = equipments.Where(e => e.Name.ToLower().Contains(name.ToLower()));
            }

            if (startDate.HasValue)
            {
                equipments = equipments.Where(e => e.StartDate == startDate.Value);
            }

            if (!string.IsNullOrEmpty(location))
            {
                equipments = equipments.Where(e => e.Location.ToLower().Contains(location.ToLower()));
            }

            // Сортировка данных
            equipments = sortOrder switch
            {
                "InventoryDesc" => equipments.OrderByDescending(e => e.InventoryNumber),
                "NameDesc" => equipments.OrderByDescending(e => e.Name),
                "StartDateDesc" => equipments.OrderByDescending(e => e.StartDate),
                "LocationDesc" => equipments.OrderByDescending(e => e.Location),
                "InventoryAsc" => equipments.OrderBy(e => e.InventoryNumber),
                "NameAsc" => equipments.OrderBy(e => e.Name),
                "StartDateAsc" => equipments.OrderBy(e => e.StartDate),
                "LocationAsc" => equipments.OrderBy(e => e.Location),
                _ => equipments.OrderBy(e => e.Name) // Сортировка по умолчанию: по Name
            };

            var equipmentsList = await equipments.ToListAsync();
            return View(equipmentsList);
        }


        // GET: Equipments/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Equipments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Equipment equipment)
        {
            if (ModelState.IsValid)
            {
                // Проверка уникальности инвентарного номера
                var exists = await _context.Equipments.AnyAsync(e => e.InventoryNumber == equipment.InventoryNumber);
                if (exists)
                {
                    ModelState.AddModelError("InventoryNumber", "Этот инвентарный номер уже существует. Пожалуйста, введите уникальный номер.");
                    return View(equipment);
                }

                _context.Add(equipment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(equipment);
        }

        // GET: Equipments/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var equipment = await _context.Equipments.FindAsync(id);
            if (equipment == null)
            {
                return NotFound();
            }
            return View(equipment);
        }

        // POST: Equipments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("EquipmentId,InventoryNumber,Name,StartDate,Location")] Equipment equipment)
        {
            if (id != equipment.EquipmentId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Проверка уникальности инвентарного номера
                var exists = await _context.Equipments
                    .Where(e => e.InventoryNumber == equipment.InventoryNumber)
                    .Where(e => e.EquipmentId != id) // исключаем текущий объект
                    .AnyAsync();

                if (exists)
                {
                    ModelState.AddModelError("InventoryNumber", "Этот инвентарный номер уже существует. Пожалуйста, введите уникальный номер.");
                    return View(equipment);
                }

                try
                {
                    _context.Update(equipment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Equipments.Any(e => e.EquipmentId == id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(equipment);
        }

        // GET: Equipments/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var equipment = await _context.Equipments
                .FirstOrDefaultAsync(e => e.EquipmentId == id);

            if (equipment == null)
            {
                return NotFound();
            }

            return View(equipment);
        }

        // POST: Equipments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var equipment = await _context.Equipments.FindAsync(id);
            if (equipment != null)
            {
                _context.Equipments.Remove(equipment);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
