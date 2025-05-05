using Microsoft.AspNetCore.Identity;
using CreamyFusion.Enums;

namespace CreamyFusion.Models
{
    public class Customer : BaseEntity
    {
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public Gender Gender { get; set; }
        public short Point { get; set; }
        public DateTime? LastOrderAt { get; set; }
    }
}
