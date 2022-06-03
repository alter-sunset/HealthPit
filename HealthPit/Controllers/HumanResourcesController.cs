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
    public class HumanResourcesController : ControllerBase
    {
        private readonly ILogger<HumanResourcesController> _logger;

        private readonly SurveillanceContext _db;
        public HumanResourcesController(ILogger<HumanResourcesController> logger, SurveillanceContext db)
        {
            _logger = logger;
            _db = db;
        }

        [HttpPost("NewEmployee")]
        public ActionResult<Employee> PostEmployee(string lastName, string firstName, string middleName, string position)
        {
            _db.Database.EnsureCreated();

            string _position = SurveillanceInitializer.FirstCharToUpper(position);

            if (lastName == null || firstName == null || position == null)
            {
                return BadRequest("Fill required fields: lastName, firstName, position");
            }

            if (!(_db.Positions.Any(u => u.Name == _position)))
            {
                return BadRequest("No such position in the database.");
            }

            else
            {
                string _lastName = SurveillanceInitializer.FirstCharToUpper(lastName);
                string _firstName = SurveillanceInitializer.FirstCharToUpper(firstName);
                string _middleName = middleName == null ? null : SurveillanceInitializer.FirstCharToUpper(middleName);

                _db.Employees.Add(new Employee { LastName = _lastName, FirstName = _firstName, MiddleName = _middleName, PositionId = _db.Positions.Single(u => u.Name == _position).Id });
                _db.SaveChanges();

                return Ok(_db.Employees.OrderBy(u => u.Id).LastOrDefault());
            }
        }

        [HttpPut("EditEmployee")]
        public ActionResult<Employee> EditEmployee(int id, string lastName, string firstName, string middleName, string position)
        {
            if (id == 0)
            {
                return BadRequest("Enter id.");
            }

            if (!_db.Employees.Any(u => u.Id == id))
            {
                return BadRequest("There is no employee with a given id.");
            }

            if (!(position is null || _db.Positions.Any(u => u.Name == SurveillanceInitializer.FirstCharToUpper(position))))
            {
                return BadRequest("No such position in the database.");
            }

            else
            {
                Employee _employee = _db.Employees.Include(u => u.Position).FirstOrDefault(u => u.Id == id);

                string _lastName = lastName == null ? _employee.LastName : SurveillanceInitializer.FirstCharToUpper(lastName);
                string _firstName = firstName == null ? _employee.FirstName : SurveillanceInitializer.FirstCharToUpper(firstName);
                string _middleName = middleName == null ? _employee.MiddleName : SurveillanceInitializer.FirstCharToUpper(middleName);
                string _position = position == null ? _employee.Position.Name : SurveillanceInitializer.FirstCharToUpper(position);

                _employee.LastName = _lastName;
                _employee.FirstName = _firstName;
                _employee.MiddleName = _middleName;
                _employee.Position = _db.Positions.FirstOrDefault(u => u.Name == _position);

                _db.SaveChanges();

                return Ok(_employee);
            }
        }

        [HttpDelete("DeleteEmployee")]
        public ActionResult DeleteEmployee(int id)
        {
            if (id == 0)
            {
                return BadRequest("Enter id.");
            }

            if (!_db.Employees.Any(u => u.Id == id))
            {
                return BadRequest("There is no employee with a given id.");
            }

            else
            {
                Employee _employee = new() { Id = id };
                _db.Remove(_employee);
                _db.SaveChanges();

                return Ok($"Employee with id {id} successfully deleted.");
            }
        }        

        [HttpGet("Employees")]
        public  ActionResult<IEnumerable<Employee>> GetEmployees(string position)
        {
            _db.Database.EnsureCreated();
            IQueryable<Employee> employees = _db.Employees.AsNoTracking();

            if (!employees.Any())
            {
                return BadRequest("No employees in the database.");
            }

            if (position == null)
            {
                return Ok(employees
                    .Include(u => u.Position)
                    .Include(u => u.Shifts
                        .Where(u => u.StartTime.Month == DateTime.Now.Month)
                        .Where(u => u.Violation == true)));
            }

            string _position = SurveillanceInitializer.FirstCharToUpper(position);

            if (_db.Positions.AsNoTracking().Any(u => u.Name == _position))
            {
                return Ok(employees
                    .Where(x => x.Position.Name == _position)
                    .Include(u => u.Position)
                    .Include(u => u.Shifts
                        .Where(u => u.StartTime.Month == DateTime.Now.Month)
                        .Where(u => u.Violation == true)));
            }

            else
            {
                return BadRequest("No such position in the database.");
            }
        }

        [HttpGet("Positions")]
        public ActionResult<IEnumerable<Position>> GetPositions()
        {
            _db.Database.EnsureCreated();
            IQueryable<Position> positions = _db.Positions.AsNoTracking();

            if (positions.Any())
            {
                return Ok(positions);
            }

            else
            {
                return BadRequest("No idea how, but there are NO positions in database.");
            }
        }
    }
}