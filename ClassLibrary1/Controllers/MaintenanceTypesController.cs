using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using RPBDIS_5.Data;
using RPBDIS_5.Models;

namespace RPBDIS_5.Controllers
{
    public class MaintenanceTypesController : Controller
    {
        private readonly MonitoringContext _context;

        public MaintenanceTypesController(MonitoringContext context)
        {
            _context = context;
        }

        // GET: MaintenanceTypes
        public async Task<IActionResult> Index()
        {
            var types = await _context.MaintenanceTypes.ToListAsync();
            return View(types);
        }

        // GET: MaintenanceTypes/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var type = await _context.MaintenanceTypes
                .FirstOrDefaultAsync(mt => mt.MaintenanceTypeId == id);

            if (type == null)
            {
                return NotFound();
            }

            return View(type);
        }

        // GET: MaintenanceTypes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: MaintenanceTypes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Description")] MaintenanceType type)
        {
            if (ModelState.IsValid)
            {
                _context.Add(type);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(type);
        }

        // GET: MaintenanceTypes/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var type = await _context.MaintenanceTypes.FindAsync(id);
            if (type == null)
            {
                return NotFound();
            }
            return View(type);
        }

        // POST: MaintenanceTypes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaintenanceTypeId,Description")] MaintenanceType type)
        {
            if (id != type.MaintenanceTypeId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(type);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.MaintenanceTypes.Any(mt => mt.MaintenanceTypeId == id))
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
            return View(type);
        }

        // GET: MaintenanceTypes/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var type = await _context.MaintenanceTypes
                .FirstOrDefaultAsync(mt => mt.MaintenanceTypeId == id);

            if (type == null)
            {
                return NotFound();
            }

            return View(type);
        }

        // POST: MaintenanceTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var type = await _context.MaintenanceTypes.FindAsync(id);
            if (type != null)
            {
                _context.MaintenanceTypes.Remove(type);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
