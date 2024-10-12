
using System.ComponentModel.DataAnnotations;
namespace EventMangementSystem.Models
{
    public class ServiceCategory
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
