using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HealthPit.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestingController : ControllerBase
    {
        private readonly ILogger<TestingController> _logger;

        private readonly SurveillanceContext _db;
        public TestingController(ILogger<TestingController> logger, SurveillanceContext db)
        {
            _logger = logger;
            _db = db;
        }

        [HttpPost("SeedDatabase")]
        public ActionResult SeedData(int employeesCount, int shiftsMaxCount)
        {
            _db.Database.EnsureCreated();
            
            SurveillanceInitializer.InitialPositions(_db);

            SurveillanceInitializer.SeedEmployees(_db, employeesCount);

            int shiftsCountBefore = _db.Shifts.AsNoTracking().Count();

            SurveillanceInitializer.SeedShifts(_db, shiftsMaxCount);

            int shiftsCountAdded = _db.Shifts.AsNoTracking().Count() - shiftsCountBefore;

            return Ok($"Seeded {employeesCount} employees and {shiftsCountAdded} shifts ({shiftsMaxCount - shiftsCountAdded} self crossed shifts excluded). Total: {_db.Employees.Count()} employees and {_db.Shifts.Count()} shifts.");
        }

        [HttpPost("SeedEmployeesOnly")] 
        public ActionResult SeedEmployees(int employeesCount)
        {
            _db.Database.EnsureCreated();

            SurveillanceInitializer.InitialPositions(_db);

            SurveillanceInitializer.SeedEmployees(_db, employeesCount);

            return Ok($"Seeded {employeesCount} employees. Total: {_db.Employees.Count()} employees.");              
        }

        [HttpPost("SeedShiftsOnly")]
        public ActionResult SeedShifts(int shiftsMaxCount)
        {
            _db.Database.EnsureCreated();

            if (!_db.Employees.AsNoTracking().Any())
            {
                return BadRequest("No employees in the database. Seed some first.");
            }

            else
            {
                int shiftsCountBefore = _db.Shifts.AsNoTracking().Count();

                SurveillanceInitializer.SeedShifts(_db, shiftsMaxCount);

                int shiftsCountAdded = _db.Shifts.AsNoTracking().Count() - shiftsCountBefore;

                return Ok($"Seeded {shiftsCountAdded} shifts ({shiftsMaxCount - shiftsCountAdded} self crossed shifts excluded). Total: {_db.Shifts.AsNoTracking().Count()} shifts.");
            }
        }

        [HttpDelete("PurgeDatabase")]
        public ActionResult PurgeData()
        {
            _db.Database.EnsureDeleted();

            SurveillanceInitializer.InitialPositions(_db);

            return Ok("Database purged.");
        }
    }
}
