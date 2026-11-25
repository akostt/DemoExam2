using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DemoExam2.Models;

[Table("Manufacturer")]
public partial class Manufacturer
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [StringLength(100)]
    public string Name { get; set; } = null!;

    [InverseProperty("IdManufacturerNavigation")]
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
