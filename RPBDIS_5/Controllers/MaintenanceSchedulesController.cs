using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using RPBDIS_5.Data;
using RPBDIS_5.Models;

namespace RPBDIS_5.Controllers
{
    public class MaintenanceSchedulesController : Controller
    {
        private readonly MonitoringContext _context;

        public MaintenanceSchedulesController(MonitoringContext context)
        {
            _context = context;
        }

        // GET: Equipments
        public async Task<IActionResult> Index(
     string equipmentName,
     string maintenanceTypeName,
     string responsibleEmployeeName,
     DateOnly? scheduledDate,
     string sortOrder)
        {
            // Восстанавливаем параметры из куки, если они не были переданы
            if (string.IsNullOrEmpty(equipmentName))
            {
                equipmentName = Request.Cookies["EquipmentFilter"];
            }
            if (string.IsNullOrEmpty(maintenanceTypeName))
            {
                maintenanceTypeName = Request.Cookies["MaintenanceTypeFilter"];
            }
            if (string.IsNullOrEmpty(responsibleEmployeeName))
            {
                responsibleEmployeeName = Request.Cookies["ResponsibleEmployeeFilter"];
            }
            if (!scheduledDate.HasValue && Request.Cookies.ContainsKey("ScheduledDateFilter"))
            {
                if (DateOnly.TryParse(Request.Cookies["ScheduledDateFilter"], out var parsedDate))
                {
                    scheduledDate = parsedDate;
                }
            }
            if (string.IsNullOrEmpty(sortOrder))
            {
                sortOrder = Request.Cookies["SortOrder"];
            }

            // Сохраняем текущие параметры в куки
            Response.Cookies.Append("EquipmentFilter", equipmentName ?? string.Empty, new CookieOptions { HttpOnly = true });
            Response.Cookies.Append("MaintenanceTypeFilter", maintenanceTypeName ?? string.Empty, new CookieOptions { HttpOnly = true });
            Response.Cookies.Append("ResponsibleEmployeeFilter", responsibleEmployeeName ?? string.Empty, new CookieOptions { HttpOnly = true });
            Response.Cookies.Append("ScheduledDateFilter", scheduledDate?.ToString("yyyy-MM-dd") ?? string.Empty, new CookieOptions { HttpOnly = true });
            Response.Cookies.Append("SortOrder", sortOrder ?? string.Empty, new CookieOptions { HttpOnly = true });

            // Добавляем параметры сортировки
            ViewData["CurrentSort"] = sortOrder;
            ViewData["EquipmentSortParm"] = sortOrder == "EquipmentAsc" ? "EquipmentDesc" : "EquipmentAsc";
            ViewData["MaintenanceTypeSortParm"] = sortOrder == "MaintenanceTypeAsc" ? "MaintenanceTypeDesc" : "MaintenanceTypeAsc";
            ViewData["ResponsibleEmployeeSortParm"] = sortOrder == "ResponsibleEmployeeAsc" ? "ResponsibleEmployeeDesc" : "ResponsibleEmployeeAsc";
            ViewData["ScheduledDateSortParm"] = sortOrder == "ScheduledDateAsc" ? "ScheduledDateDesc" : "ScheduledDateAsc";

            // Запрос данных с учётом фильтрации
            var schedules = _context.MaintenanceSchedules
                .Include(s => s.Equipment)
                .Include(s => s.MaintenanceType)
                .Include(s => s.ResponsibleEmployee)
                .AsQueryable();

            if (!string.IsNullOrEmpty(equipmentName))
            {
                schedules = schedules.Where(s => s.Equipment.Name.ToLower().Contains(equipmentName.ToLower()));
            }

            if (!string.IsNullOrEmpty(maintenanceTypeName))
            {
                schedules = schedules.Where(s => s.MaintenanceType.Description.ToLower().Contains(maintenanceTypeName.ToLower()));
            }

            if (!string.IsNullOrEmpty(responsibleEmployeeName))
            {
                schedules = schedules.Where(s => s.ResponsibleEmployee.FullName.ToLower().Contains(responsibleEmployeeName.ToLower()));
            }

            if (scheduledDate.HasValue)
            {
                schedules = schedules.Where(s => s.ScheduledDate == scheduledDate.Value);
            }

            // Сортировка
            schedules = sortOrder switch
            {
                "EquipmentDesc" => schedules.OrderByDescending(s => s.Equipment.Name),
                "MaintenanceTypeDesc" => schedules.OrderByDescending(s => s.MaintenanceType.Description),
                "ResponsibleEmployeeDesc" => schedules.OrderByDescending(s => s.ResponsibleEmployee.FullName),
                "ScheduledDateDesc" => schedules.OrderByDescending(s => s.ScheduledDate),
                "EquipmentAsc" => schedules.OrderBy(s => s.Equipment.Name),
                "MaintenanceTypeAsc" => schedules.OrderBy(s => s.MaintenanceType.Description),
                "ResponsibleEmployeeAsc" => schedules.OrderBy(s => s.ResponsibleEmployee.FullName),
                "ScheduledDateAsc" => schedules.OrderBy(s => s.ScheduledDate),
                _ => schedules.OrderBy(s => s.ScheduledDate) // Сортировка по умолчанию
            };

            // Преобразуем в список
            var schedulesList = await schedules.ToListAsync();

            // Передаём текущие параметры фильтрации в представление через ViewData для сохранения значений в полях поиска
            ViewData["CurrentEquipmentFilter"] = equipmentName;
            ViewData["CurrentMaintenanceTypeFilter"] = maintenanceTypeName;
            ViewData["CurrentResponsibleEmployeeFilter"] = responsibleEmployeeName;
            ViewData["CurrentScheduledDateFilter"] = scheduledDate?.ToString("yyyy-MM-dd");

            // Возвращаем данные в представление
            return View(schedulesList);
        }


        // GET: MaintenanceSchedules/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var schedule = await _context.MaintenanceSchedules
                .Include(s => s.Equipment)
                .Include(s => s.MaintenanceType)
                .Include(s => s.ResponsibleEmployee)
                .FirstOrDefaultAsync(s => s.ScheduleId == id);

            if (schedule == null)
            {
                return NotFound();
            }

            return View(schedule);
        }

        // GET: MaintenanceSchedules/Create
        public IActionResult Create()
        {
            PopulateDropDowns(); // Заполняем данные для списков выбора
            return View();
        }

        // Метод для заполнения ViewData данными для списков выбора
        private void PopulateDropDowns()
        {
            ViewData["Equipments"] = _context.Equipments?.ToList() ?? new List<Equipment>();
            ViewData["MaintenanceTypes"] = _context.MaintenanceTypes?.ToList() ?? new List<MaintenanceType>();
            ViewData["Employees"] = _context.Employees?.ToList() ?? new List<Employee>();
        }

        // POST: MaintenanceSchedules/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EquipmentId,MaintenanceTypeId,ScheduledDate,ResponsibleEmployeeId,EstimatedCost")] MaintenanceSchedule schedule)
        {
            if (ModelState.IsValid)
            {
                _context.Add(schedule);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Повторно заполняем данные, если модель не валидна
            PopulateDropDowns();
            return View(schedule);
        }

        // GET: MaintenanceSchedules/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var schedule = await _context.MaintenanceSchedules.FindAsync(id);
            if (schedule == null)
            {
                return NotFound();
            }

            PopulateDropDowns(); // Заполняем данные для списков выбора
            return View(schedule);
        }

        // POST: MaintenanceSchedules/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ScheduleId,EquipmentId,MaintenanceTypeId,ScheduledDate,ResponsibleEmployeeId,EstimatedCost")] MaintenanceSchedule schedule)
        {
            if (id != schedule.ScheduleId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(schedule);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.MaintenanceSchedules.Any(ms => ms.ScheduleId == id))
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

            PopulateDropDowns(); // Повторно заполняем данные, если модель не валидна
            return View(schedule);
        }

        // GET: MaintenanceSchedules/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var schedule = await _context.MaintenanceSchedules
                .Include(s => s.Equipment)
                .Include(s => s.MaintenanceType)
                .Include(s => s.ResponsibleEmployee)
                .FirstOrDefaultAsync(s => s.ScheduleId == id);

            if (schedule == null)
            {
                return NotFound();
            }

            return View(schedule);
        }

        // POST: MaintenanceSchedules/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var schedule = await _context.MaintenanceSchedules.FindAsync(id);
            if (schedule != null)
            {
                _context.MaintenanceSchedules.Remove(schedule);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
