// Models/ServiceProvider.cs
using EventMangementSystem.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class ServiceProvider
{
    [Required]
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
   
    public string Name { get; set; }

    public string Specialization { get; set; }  // e.g., Catering, Lighting, Audio
    public string email { get; set; }  // e.g., Catering, Lighting, Audio

    public string ContactInfo { get; set; }

    // Relationship: A service provider can submit multiple bids
    public virtual ICollection<Quotation> Quotations { get; set; }

    // Relationship: A service provider may have an inventory of equipment
    public virtual ICollection<Inventory2> InventoryItems { get; set; }
    public virtual ICollection<Employee> Employees { get; set; }
}
