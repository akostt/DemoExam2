using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DemoExam2.Models;

[Table("City")]
public partial class City
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [StringLength(100)]
    public string Name { get; set; } = null!;

    [InverseProperty("IdCityNavigation")]
    public virtual ICollection<PickupPoint> PickupPoints { get; set; } = new List<PickupPoint>();
}
