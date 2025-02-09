using System.ComponentModel.DataAnnotations;

namespace GroupAllocator.DAL.Entities
{
    public class UserRole
    {
        public Guid Id { get; set; }

        [Required]
        public string? Name { get; set; }

        public string? Description { get; set; }
    }
}