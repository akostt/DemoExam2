using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DemoExam2.Models;

public partial class Order
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    public DateOnly CreatedAt { get; set; }

    public DateOnly DeliveredAt { get; set; }

    [Column("ID_PickupPoint")]
    public int IdPickupPoint { get; set; }

    [Column("ID_User")]
    public int IdUser { get; set; }

    [Column("Pickup_Code")]
    public int PickupCode { get; set; }

    [Column("ID_Status")]
    public int IdStatus { get; set; }

    [ForeignKey("IdPickupPoint")]
    [InverseProperty("Orders")]
    public virtual PickupPoint IdPickupPointNavigation { get; set; } = null!;

    [ForeignKey("IdStatus")]
    [InverseProperty("Orders")]
    public virtual Status IdStatusNavigation { get; set; } = null!;

    [ForeignKey("IdUser")]
    [InverseProperty("Orders")]
    public virtual User IdUserNavigation { get; set; } = null!;

    [InverseProperty("IdOrderNavigation")]
    public virtual ICollection<OrderProduct> OrderProducts { get; set; } = new List<OrderProduct>();
}
