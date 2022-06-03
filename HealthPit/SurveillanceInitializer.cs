using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HealthPit
{
    public class SurveillanceInitializer
    {
        public static void SeedStartup(IApplicationBuilder applicationBuilder, int employeesCount, int shiftsMaxCount)
        {
            using IServiceScope serviceScope = applicationBuilder.ApplicationServices.CreateScope();

            SurveillanceContext db = serviceScope.ServiceProvider.GetService<SurveillanceContext>();
                        
            InitialPositions(db);

            if (!db.Employees.Any())
            {
                SeedEmployees(db, employeesCount);
            }

            if (!db.Shifts.Any())
            {
                SeedShifts(db, shiftsMaxCount);
            }
        }

        public static string FirstCharToUpper(string s)
        {
            // Check for empty string.  
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            s = s.ToLower();
            // Return char and concat substring.  
            return char.ToUpper(s[0]) + s[1..];
        }

        public static void InitialPositions(SurveillanceContext db)
        {
            db.Database.EnsureCreated();

            if (!db.Positions.Any())
            {
                db.Positions.Add(new Position { Name = "Менеджер" , RequiredWorkingHours = 9});
                db.Positions.Add(new Position { Name = "Инженер" , RequiredWorkingHours = 9});
                db.Positions.Add(new Position { Name = "Тестировщик свечей" , RequiredWorkingHours = 12});
                db.SaveChanges();
            }
        }

        public static void SeedEmployees(SurveillanceContext db, int employeesCount)
        {
            db.Database.EnsureCreated();

            IEnumerable<Employee> employees = SurveillanceRandoGenerator.GenerateEmployees(employeesCount);

            foreach (Employee employee in employees)
            {
                db.Employees.Add(employee);
                db.SaveChanges();
            }

            db.SaveChanges();
        }

        public static void SeedShifts(SurveillanceContext db, int shiftsMaxCount)
        {
            int startHour = 9;
            int endHour = 18;
            int endHourAss = 21;
            DateOnly startDate = DateOnly.FromDateTime(DateTime.Today);

            int employeesCount = db.Employees.AsNoTracking().Count();

            IEnumerable<Shift> shifts = SurveillanceRandoGenerator.GenerateShifts(shiftsMaxCount, startDate, employeesCount);

            foreach (Shift shift in shifts)
            {
                Shift comparedShift = db.Shifts.AsNoTracking()
                    .Where(f => f.EmployeeId == shift.EmployeeId)
                    .FirstOrDefault(f => f.StartTime.Date == shift.StartTime.Date);

                if (!(comparedShift is null || comparedShift.EndTime < shift.StartTime))
                {
                    continue;
                }

                int positionId = db.Employees.AsNoTracking()
                    .Single(u => u.Id == shift.EmployeeId)
                    .PositionId;

                shift.Violation = shift.StartTime.Hour > startHour
                    || (positionId == 3 && shift.EndTime.Hour < endHourAss)
                    || shift.HoursWorked < 9 || shift.EndTime.Hour < endHour;

                db.Shifts.Add(shift);

                if (shift.Violation is true)
                {
                    db.Employees.FirstOrDefault(u => u.Id == shift.EmployeeId).ViolationCount++;
                }

                db.SaveChanges();
            }
        }
        public static DateTime TheTimeIsNow(DateTime dateTime)
        {
            DateTime _dateTime = dateTime == default ? DateTime.Now : dateTime;
            return _dateTime;
        }

        public static void Violation(Shift shift)
        {
            if (shift.Violation == true)
            {
                shift.Employee.ViolationCount++;
            }
        }
    }
}
