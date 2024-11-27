using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using RPBDIS_5.Data;
using RPBDIS_5.Models;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;

namespace RPBDIS_5.Controllers
{
    public class EmployeesController : Controller
    {
        private readonly MonitoringContext _context;

        public EmployeesController(MonitoringContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string fullName, string position, string sortOrder)
        {
            // Если параметры не переданы, пытаемся взять их из куки
            if (string.IsNullOrEmpty(fullName))
            {
                fullName = Request.Cookies["FullNameFilter"];
            }
            if (string.IsNullOrEmpty(position))
            {
                position = Request.Cookies["PositionFilter"];
            }
            if (string.IsNullOrEmpty(sortOrder))
            {
                sortOrder = Request.Cookies["SortOrder"];
            }

            // Сохраняем текущие параметры фильтрации и сортировки в куки
            Response.Cookies.Append("FullNameFilter", fullName ?? string.Empty, new CookieOptions { HttpOnly = true });
            Response.Cookies.Append("PositionFilter", position ?? string.Empty, new CookieOptions { HttpOnly = true });
            Response.Cookies.Append("SortOrder", sortOrder ?? string.Empty, new CookieOptions { HttpOnly = true });

            // Добавляем параметры сортировки
            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParm"] = sortOrder == "NameAsc" ? "NameDesc" : "NameAsc";
            ViewData["PositionSortParm"] = sortOrder == "PositionAsc" ? "PositionDesc" : "PositionAsc";

            // Фильтрация данных
            var employees = _context.Employees.AsQueryable();

            if (!string.IsNullOrEmpty(fullName))
            {
                employees = employees.Where(e => e.FullName.ToLower() == fullName.ToLower());
            }

            if (!string.IsNullOrEmpty(position))
            {
                employees = employees.Where(e => e.Position.ToLower() == position.ToLower());
            }

            // Преобразуем запрос в AsEnumerable(), чтобы выполнить сортировку на клиенте
            var employeeList = employees.AsEnumerable();

            // Универсальная сортировка по имени или должности
            switch (sortOrder)
            {
                case "NameDesc":
                    employeeList = employeeList.OrderByDescending(e => ExtractNumericPart(e.FullName)).ThenByDescending(e => e.FullName);
                    break;
                case "PositionAsc":
                    employeeList = employeeList.OrderBy(e => e.Position);
                    break;
                case "PositionDesc":
                    employeeList = employeeList.OrderByDescending(e => e.Position);
                    break;
                case "NameAsc":
                default:
                    employeeList = employeeList.OrderBy(e => ExtractNumericPart(e.FullName)).ThenBy(e => e.FullName);
                    break;
            }

            // Передаём текущие параметры фильтрации в представление через ViewData для сохранения значений в полях поиска
            ViewData["CurrentFullNameFilter"] = fullName;
            ViewData["CurrentPositionFilter"] = position;

            return View(await Task.FromResult(employeeList.ToList()));
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


        // GET: Employees/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var employee = await _context.Employees
                .Include(e => e.CompletedWorks) // Если есть связанные таблицы, можно использовать Include
                .Include(e => e.MaintenanceSchedules) // Если есть связанные таблицы
                .FirstOrDefaultAsync(e => e.EmployeeId == id);

            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }



        // GET: Employees/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
            {
                return NotFound();
            }
            return View(employee);
        }

        // POST: Employees/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("EmployeeId,FullName,Position")] Employee employee)
        {
            if (id != employee.EmployeeId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(employee);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Employees.Any(e => e.EmployeeId == id))
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
            return View(employee);
        }

        // GET: Employees/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.EmployeeId == id);

            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        // POST: Employees/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null) // Добавлено условие проверки
            {
                return NotFound();
            }

            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

    }
}
