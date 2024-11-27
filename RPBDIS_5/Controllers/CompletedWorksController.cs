using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using RPBDIS_5.Data;
using RPBDIS_5.Models;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;

namespace RPBDIS_5.Controllers
{
    public class CompletedWorksController : Controller
    {
        private readonly MonitoringContext _context;

        public CompletedWorksController(MonitoringContext context)
        {
            _context = context;
        }

        // GET: CompletedWorks
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Index(
                    string equipmentName,
                    string maintenanceType,
                    string responsibleEmployee,
                    DateTime? completionDate,
                    decimal? actualCost,
                    string sortOrder)
        {
            // Восстанавливаем параметры из куки, если они не были переданы
            if (string.IsNullOrEmpty(equipmentName))
            {
                equipmentName = Request.Cookies["EquipmentFilter"];
            }
            if (string.IsNullOrEmpty(maintenanceType))
            {
                maintenanceType = Request.Cookies["MaintenanceTypeFilter"];
            }
            if (string.IsNullOrEmpty(responsibleEmployee))
            {
                responsibleEmployee = Request.Cookies["ResponsibleEmployeeFilter"];
            }
            if (!completionDate.HasValue && Request.Cookies.ContainsKey("CompletionDateFilter"))
            {
                if (DateTime.TryParse(Request.Cookies["CompletionDateFilter"], out var parsedDate))
                {
                    completionDate = parsedDate;
                }
            }
            if (!actualCost.HasValue && Request.Cookies.ContainsKey("ActualCostFilter"))
            {
                if (decimal.TryParse(Request.Cookies["ActualCostFilter"], out var parsedCost))
                {
                    actualCost = parsedCost;
                }
            }
            if (string.IsNullOrEmpty(sortOrder))
            {
                sortOrder = Request.Cookies["SortOrder"];
            }

            // Сохраняем текущие параметры в куки
            Response.Cookies.Append("EquipmentFilter", equipmentName ?? string.Empty, new CookieOptions { HttpOnly = true });
            Response.Cookies.Append("MaintenanceTypeFilter", maintenanceType ?? string.Empty, new CookieOptions { HttpOnly = true });
            Response.Cookies.Append("ResponsibleEmployeeFilter", responsibleEmployee ?? string.Empty, new CookieOptions { HttpOnly = true });
            Response.Cookies.Append("CompletionDateFilter", completionDate?.ToString("yyyy-MM-dd") ?? string.Empty, new CookieOptions { HttpOnly = true });
            Response.Cookies.Append("ActualCostFilter", actualCost?.ToString() ?? string.Empty, new CookieOptions { HttpOnly = true });
            Response.Cookies.Append("SortOrder", sortOrder ?? string.Empty, new CookieOptions { HttpOnly = true });

            // Добавляем параметры сортировки
            ViewData["CurrentSort"] = sortOrder;
            ViewData["EquipmentSortParm"] = sortOrder == "EquipmentAsc" ? "EquipmentDesc" : "EquipmentAsc";
            ViewData["MaintenanceTypeSortParm"] = sortOrder == "MaintenanceTypeAsc" ? "MaintenanceTypeDesc" : "MaintenanceTypeAsc";
            ViewData["CompletionDateSortParm"] = sortOrder == "CompletionDateAsc" ? "CompletionDateDesc" : "CompletionDateAsc";
            ViewData["ResponsibleEmployeeSortParm"] = sortOrder == "ResponsibleEmployeeAsc" ? "ResponsibleEmployeeDesc" : "ResponsibleEmployeeAsc";
            ViewData["ActualCostSortParm"] = sortOrder == "ActualCostAsc" ? "ActualCostDesc" : "ActualCostAsc";

            // Фильтрация данных
            var completedWorks = _context.CompletedWorks
                .Include(cw => cw.Equipment)
                .Include(cw => cw.MaintenanceType)
                .Include(cw => cw.ResponsibleEmployee)
                .AsQueryable();

            if (!string.IsNullOrEmpty(equipmentName))
            {
                completedWorks = completedWorks.Where(cw => cw.Equipment.Name.ToLower().Contains(equipmentName.ToLower()));
            }

            if (!string.IsNullOrEmpty(maintenanceType))
            {
                completedWorks = completedWorks.Where(cw => cw.MaintenanceType.Description.ToLower().Contains(maintenanceType.ToLower()));
            }

            if (!string.IsNullOrEmpty(responsibleEmployee))
            {
                completedWorks = completedWorks.Where(cw => cw.ResponsibleEmployee.FullName.ToLower().Contains(responsibleEmployee.ToLower()));
            }

            if (completionDate.HasValue)
            {
                completedWorks = completedWorks.Where(cw => cw.CompletionDate.HasValue &&
                                                            cw.CompletionDate.Value.ToDateTime(TimeOnly.MinValue) == completionDate.Value);
            }

            if (actualCost.HasValue)
            {
                completedWorks = completedWorks.Where(cw => cw.ActualCost == actualCost.Value);
            }

            // Преобразуем запрос в AsEnumerable(), чтобы выполнить сортировку на клиенте
            var completedWorksList = completedWorks.AsEnumerable();

            // Универсальная сортировка по всем параметрам с использованием ExtractNumericPart
            switch (sortOrder)
            {
                case "EquipmentDesc":
                    completedWorksList = completedWorksList.OrderByDescending(cw => ExtractNumericPart(cw.Equipment.Name))
                                                           .ThenByDescending(cw => cw.Equipment.Name);
                    break;
                case "MaintenanceTypeAsc":
                    completedWorksList = completedWorksList.OrderBy(cw => cw.MaintenanceType.Description);
                    break;
                case "MaintenanceTypeDesc":
                    completedWorksList = completedWorksList.OrderByDescending(cw => cw.MaintenanceType.Description);
                    break;
                case "CompletionDateAsc":
                    completedWorksList = completedWorksList.OrderBy(cw => cw.CompletionDate);
                    break;
                case "CompletionDateDesc":
                    completedWorksList = completedWorksList.OrderByDescending(cw => cw.CompletionDate);
                    break;
                case "ResponsibleEmployeeAsc":
                    completedWorksList = completedWorksList.OrderBy(cw => cw.ResponsibleEmployee.FullName);
                    break;
                case "ResponsibleEmployeeDesc":
                    completedWorksList = completedWorksList.OrderByDescending(cw => cw.ResponsibleEmployee.FullName);
                    break;
                case "ActualCostAsc":
                    completedWorksList = completedWorksList.OrderBy(cw => cw.ActualCost);
                    break;
                case "ActualCostDesc":
                    completedWorksList = completedWorksList.OrderByDescending(cw => cw.ActualCost);
                    break;
                case "EquipmentAsc":
                default:
                    completedWorksList = completedWorksList.OrderBy(cw => ExtractNumericPart(cw.Equipment.Name))
                                                           .ThenBy(cw => cw.Equipment.Name);
                    break;
            }

            // Передаём текущие параметры фильтрации в представление через ViewData для сохранения значений в полях поиска
            ViewData["CurrentEquipmentFilter"] = equipmentName;
            ViewData["CurrentMaintenanceTypeFilter"] = maintenanceType;
            ViewData["CurrentResponsibleEmployeeFilter"] = responsibleEmployee;
            ViewData["CurrentCompletionDateFilter"] = completionDate?.ToString("yyyy-MM-dd");
            ViewData["CurrentActualCostFilter"] = actualCost;

            return View(await Task.FromResult(completedWorksList.ToList()));
        }

        // Метод для извлечения числовой части из строки
        private int ExtractNumericPart(string input)
        {
            if (string.IsNullOrEmpty(input)) return 0;

            // Ищем числовую часть в строке
            var match = Regex.Match(input, @"\d+");
            if (match.Success)
            {
                return int.Parse(match.Value);
            }
            return 0;
        }



            // GET: CompletedWorks/Details/5
            public async Task<IActionResult> Details(int id)
        {
            var completedWork = await _context.CompletedWorks
                .Include(cw => cw.Equipment)
                .Include(cw => cw.MaintenanceType)
                .Include(cw => cw.ResponsibleEmployee)
                .FirstOrDefaultAsync(cw => cw.CompletedMaintenanceId == id);

            if (completedWork == null)
            {
                return NotFound();
            }

            return View(completedWork);
        }

        // GET: CompletedWorks/Create
        public IActionResult Create()
        {
            ViewData["Equipments"] = _context.Equipments.ToList();
            ViewData["MaintenanceTypes"] = _context.MaintenanceTypes.ToList();
            ViewData["Employees"] = _context.Employees.ToList();
            return View();
        }

        // POST: CompletedWorks/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EquipmentId,MaintenanceTypeId,CompletionDate,ResponsibleEmployeeId,ActualCost")] CompletedWork completedWork)
        {
            if (ModelState.IsValid)
            {
                _context.Add(completedWork);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["Equipments"] = _context.Equipments.ToList();
            ViewData["MaintenanceTypes"] = _context.MaintenanceTypes.ToList();
            ViewData["Employees"] = _context.Employees.ToList();
            return View(completedWork);
        }

        // GET: CompletedWorks/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var completedWork = await _context.CompletedWorks.FindAsync(id);
            if (completedWork == null)
            {
                return NotFound();
            }

            ViewData["Equipments"] = _context.Equipments.ToList();
            ViewData["MaintenanceTypes"] = _context.MaintenanceTypes.ToList();
            ViewData["Employees"] = _context.Employees.ToList();
            return View(completedWork);
        }

        // POST: CompletedWorks/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CompletedMaintenanceId,EquipmentId,MaintenanceTypeId,CompletionDate,ResponsibleEmployeeId,ActualCost")] CompletedWork completedWork)
        {
            if (id != completedWork.CompletedMaintenanceId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(completedWork);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.CompletedWorks.Any(cw => cw.CompletedMaintenanceId == id))
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

            ViewData["Equipments"] = _context.Equipments.ToList();
            ViewData["MaintenanceTypes"] = _context.MaintenanceTypes.ToList();
            ViewData["Employees"] = _context.Employees.ToList();
            return View(completedWork);
        }

        // GET: CompletedWorks/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var completedWork = await _context.CompletedWorks
                .Include(cw => cw.Equipment)
                .Include(cw => cw.MaintenanceType)
                .Include(cw => cw.ResponsibleEmployee)
                .FirstOrDefaultAsync(cw => cw.CompletedMaintenanceId == id);

            if (completedWork == null)
            {
                return NotFound();
            }

            return View(completedWork);
        }

        // POST: CompletedWorks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var completedWork = await _context.CompletedWorks.FindAsync(id);
            if (completedWork != null)
            {
                _context.CompletedWorks.Remove(completedWork);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
