using Bogus;
using Bogus.DataSets;
using System;
using Bogus.Extensions;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace HealthPit
{
    public class SurveillanceRandoGenerator
    {
        public static IEnumerable<Employee> GenerateEmployees(int count)
        {          
            Faker<Employee> testEmployee = new Faker<Employee>("ru")
                .RuleFor(u => u.Gender, f => f.PickRandom<Gender>())
                .RuleFor(u => u.FirstName, (f, u) => f.Name.FirstName((Name.Gender?)u.Gender))
                .RuleFor(u => u.LastName, (f, u) => f.Name.LastName((Name.Gender?)u.Gender))
                .RuleFor(u => u.MiddleName, (f, u) => f.Name.FirstName(0) + Enum.GetName(typeof(PostFix), u.Gender))
                .RuleFor(u => u.PositionId, f => f.Random.Int(1, 3));

            IEnumerable <Employee> testEmployees = testEmployee.Generate(count);

            return testEmployees;
        }

        public static IEnumerable<Shift> GenerateShifts(int count, DateOnly startDate, int employeesCount)
        {
            TimeOnly minWorkHours = new(7, 55);
            TimeOnly maxWorkHours = new(14, 30);            

            Faker<Shift> testShift = new Faker<Shift>()
                .RuleFor(u => u.StartTime, f => startDate.AddDays(f.Random.Int(-45, 45)).ToDateTime(new(0, 0)) + f.Date.BetweenTimeOnly(new(8, 45), new(9, 30)).ToTimeSpan())
                .RuleFor(u => u.EndTime, (f, u) => u.StartTime + f.Date.BetweenTimeOnly(minWorkHours, maxWorkHours).ToTimeSpan())
                .RuleFor(u => u.HoursWorked, (f, u) => Convert.ToInt32((u.EndTime - u.StartTime).TotalHours))
                .RuleFor(u => u.EmployeeId, f => f.Random.Int(1, employeesCount));
                      
            IEnumerable<Shift> testShifts = testShift.Generate(count);

            return testShifts;
        }

    }
}
