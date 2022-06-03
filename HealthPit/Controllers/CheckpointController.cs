using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace HealthPit.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CheckpointController : ControllerBase
    {
        private readonly ILogger<CheckpointController> _logger;

        private readonly SurveillanceContext _db;
        public CheckpointController(ILogger<CheckpointController> logger, SurveillanceContext db)
        {
            _logger = logger;
            _db = db;
        }

        [HttpPost("StartShift")]
        public ActionResult StartShift(int id, DateTime startTime)
        {
            DateTime _startTime = SurveillanceInitializer.TheTimeIsNow(startTime);

            if (id == 0)
            {
                return BadRequest("Enter id.");
            }

            if (!_db.Employees.AsNoTracking().Any(u => u.Id == id))
            {
                return BadRequest("There is no employee with a given id.");
            }

            if (_db.Shifts.AsNoTracking()
                .Where(u => u.EmployeeId == id)
                .OrderBy(u => u.Id)
                .LastOrDefault(u => u.EmployeeId == id)
                .EndTime == default)
            {
                return BadRequest("You haven't ended your last shift.");
            }
                        
            if (_startTime > _startTime.Date.AddHours(9))
            {
                _db.Shifts.Add(new Shift { EmployeeId = id , StartTime = _startTime, Violation = true});
                _db.SaveChanges();
                return Ok("Your shift has started, yet you are LATE. I'll remember this.");
            }
            else
            {
                 _db.Shifts.Add(new Shift { EmployeeId = id, StartTime = _startTime });
                 _db.SaveChanges();
                 return Ok("Your shift has started, and you are on time. Good human.");
            }
        }

        [HttpPost("EndShift")]
        public ActionResult EndShift(int id, DateTime endTime)
        {
            DateTime _endTime = SurveillanceInitializer.TheTimeIsNow(endTime);
            
            Shift currentShift = _db.Shifts
                .Where(u => u.EmployeeId == id)
                .OrderBy(u => u.Id)
                .Include(u => u.Employee)
                .ThenInclude(u => u.Position)
                .LastOrDefault(u => u.EmployeeId == id);            

            if (id == 0)
            {
                return BadRequest("Enter id.");
            }

            if (!_db.Employees.AsNoTracking().Any(u => u.Id == id))
            {
                return BadRequest("There is no employee with a given id.");
            }

            if (!(currentShift.EndTime == default))
            {
                return BadRequest("You haven't started your shift yet.");
            }

            if (currentShift.StartTime.Date.AddHours(9 + currentShift.Employee.Position.RequiredWorkingHours) > _endTime)
            {
                currentShift.EndTime = _endTime;
                currentShift.Violation = true;
                currentShift.HoursWorked = (currentShift.EndTime - currentShift.StartTime).Hours;
                SurveillanceInitializer.Violation(currentShift);
                _db.SaveChanges();
                return Ok("Your shift has ended, but you are leaving early. You are gonna regret this.");
            }
            else
            {
                currentShift.EndTime = _endTime;
                currentShift.HoursWorked = (currentShift.EndTime - currentShift.StartTime).Hours;
                SurveillanceInitializer.Violation(currentShift);
                _db.SaveChanges();
                return Ok("Your shift has ended, and everything seems in order. Good human.");
            }
        }
    }
}
