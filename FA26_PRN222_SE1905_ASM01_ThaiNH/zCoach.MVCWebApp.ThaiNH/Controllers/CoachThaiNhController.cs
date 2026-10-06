using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using zCoach.Entities.ThaiNH.Models;
using zCoach.Repositories.ThaiNH.DbContext;
using zCoach.Services.ThaiNH;
using Microsoft.AspNetCore.SignalR;
using zCoach.MVCWebApp.ThaiNH.Hubs;
using Microsoft.AspNetCore.Authorization;

namespace zCoach.MVCWebApp.ThaiNH.Controllers
{
    [Authorize]
    public class CoachThaiNhController : Controller
    {
        private readonly PRN222Context _context;
        private readonly ICoachThaiNhService _transactionCoachService;
        private readonly ISpecializationThaiNhService _specializationThaiNhService;
        private readonly IHubContext<zCoachHub> _hubContext;

        public CoachThaiNhController(ICoachThaiNhService transactionCoachService, ISpecializationThaiNhService specializationThaiNhService, IHubContext<zCoachHub> hubContext)
        {
            _context = new PRN222Context();
            _transactionCoachService = transactionCoachService;
            _specializationThaiNhService = specializationThaiNhService;
            _hubContext = hubContext;
        }

        public async Task<IActionResult> Index(string? fullName, string? email, string? phone)
        {
            ViewBag.SearchName = fullName;
            ViewBag.SearchEmail = email;
            ViewBag.SearchPhone = phone;

            var items = string.IsNullOrEmpty(fullName) && string.IsNullOrEmpty(email) && string.IsNullOrEmpty(phone)
                ? await _transactionCoachService.GetAllAsync()
                : await _transactionCoachService.SearchAsync(fullName, email, phone);
            
            return View(items);
        }
        
        // GET: CoachThaiNh/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var coachThaiNh = await _transactionCoachService.GetByIdAsync(id.Value);
            if (coachThaiNh == null) return NotFound();

            return View(coachThaiNh);
        }
        
        // GET: CoachThaiNh/Create
        public async Task<IActionResult> Create()
        {
            var specializations = await _specializationThaiNhService.GetAllAsync();
            ViewData["SpecializationThaiNhid"] = new SelectList(specializations, "SpecializationThaiNhid", "SpecializationName");

            var item = new CoachThaiNh()
            { 
                FullName = "", Email = "", Phone = "", Gender = "", Address = "", 
                ImageUrl = "", Experience = "", Certification = "", IsActive = true
            };
            return View(item);
        }

        // POST: CoachThaiNh/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CoachThaiNhid,FullName,Email,Phone,DateOfBirth,Gender,Address,ImageUrl,Experience,Certification,Salary,HireDate,Status,PublishDate,UpdateAt,SpecializationThaiNhid,IsActive")] CoachThaiNh coachThaiNh)
        {
            if (ModelState.IsValid)
            {
                await _transactionCoachService.CreateAsync(coachThaiNh);
                await _hubContext.Clients.All.SendAsync("ReceiveCoachUpdate", "Thêm mới HLV");
                return RedirectToAction(nameof(Index));
            }
            var specializations = await _specializationThaiNhService.GetAllAsync();
            ViewData["SpecializationThaiNhid"] = new SelectList(specializations, "SpecializationThaiNhid", "SpecializationName", coachThaiNh.SpecializationThaiNhid);
            return View(coachThaiNh);
        }

        // GET: CoachThaiNh/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var coachThaiNh = await _transactionCoachService.GetByIdAsync(id.Value);
            if (coachThaiNh == null) return NotFound();

            var specializations = await _specializationThaiNhService.GetAllAsync();
            ViewData["SpecializationThaiNhid"] = new SelectList(specializations, "SpecializationThaiNhid", "SpecializationName", coachThaiNh.SpecializationThaiNhid);
            return View(coachThaiNh);
        }

        // POST: CoachThaiNh/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CoachThaiNhid,FullName,Email,Phone,DateOfBirth,Gender,Address,ImageUrl,Experience,Certification,Salary,HireDate,Status,PublishDate,UpdateAt,SpecializationThaiNhid,IsActive")] CoachThaiNh coachThaiNh)
        {
            if (id != coachThaiNh.CoachThaiNhid) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    await _transactionCoachService.UpdateAsync(coachThaiNh);
                    await _hubContext.Clients.All.SendAsync("ReceiveCoachUpdate", "Cập nhật HLV");
                }
                catch (Exception ex)
                {
                    throw new ApplicationException("Update error", ex);
                }
                return RedirectToAction(nameof(Index));
            }
            var specializations = await _specializationThaiNhService.GetAllAsync();
            ViewData["SpecializationThaiNhid"] = new SelectList(specializations, "SpecializationThaiNhid", "SpecializationName", coachThaiNh.SpecializationThaiNhid);
            return View(coachThaiNh);
        }

        // GET: CoachThaiNh/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var coachThaiNh = await _transactionCoachService.GetByIdAsync(id.Value);
            if (coachThaiNh == null) return NotFound();

            return View(coachThaiNh);
        }

        // POST: CoachThaiNh/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _transactionCoachService.DeleteAsync(id);
            await _hubContext.Clients.All.SendAsync("ReceiveCoachUpdate", "Xóa HLV");
            return RedirectToAction(nameof(Index));
        }
    }
}
