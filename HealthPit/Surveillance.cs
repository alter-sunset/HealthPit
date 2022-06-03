using System;
using System.Collections.Generic;


namespace HealthPit
{
    public class Employee
    {
        public int Id { get; set; } 
        public Gender Gender { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public int ViolationCount { get; set; }
        public int PositionId { get; set; } 
        public Position Position { get; set; }
        public ICollection<Shift> Shifts { get; set; }
    }

    public class Position
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int RequiredWorkingHours { get; set; }
        public ICollection<Employee> Employees { get; set; }
    }

    public class Shift
    {
        public int Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int HoursWorked { get; set; }
        public bool Violation { get; set; }
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; }
    }
    public enum Gender
    {
        Male,
        Female
    }

    public enum PostFix
    {
        ович,
        овна
    }
   
}
