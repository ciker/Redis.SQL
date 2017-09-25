using System;
using System.ComponentModel.DataAnnotations;

namespace Redis.SQL.Client.Demo
{
    public class Employee
    {
        [Key]
        public long Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public DateTime Joined { get; set; }
        public TimeSpan ShiftStartTime { get; set; }
        public char Department { get; set; }
        public bool Insured { get; set; }
    }
}