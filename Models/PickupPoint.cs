using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DemoExam2.Models;

[Table("PickupPoint")]
public partial class PickupPoint
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    public int Index { get; set; }

    [Column("ID_City")]
    public int IdCity { get; set; }

    [StringLength(150)]
    public string Street { get; set; } = null!;

    public int? House { get; set; }

    [ForeignKey("IdCity")]
    [InverseProperty("PickupPoints")]
    public virtual City IdCityNavigation { get; set; } = null!;

    [InverseProperty("IdPickupPointNavigation")]
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
