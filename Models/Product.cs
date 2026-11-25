using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DemoExam2.Models;

public partial class Product
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [StringLength(30)]
    public string Articul { get; set; } = null!;

    [StringLength(100)]
    public string Name { get; set; } = null!;

    [Column("ID_Unit")]
    public int IdUnit { get; set; }

    [Column(TypeName = "decimal(12, 2)")]
    public decimal Price { get; set; }

    [Column("ID_Factory")]
    public int IdFactory { get; set; }

    [Column("ID_Manufacturer")]
    public int IdManufacturer { get; set; }

    [Column("ID_Category")]
    public int IdCategory { get; set; }

    public int Discount { get; set; }

    public int Count { get; set; }

    [StringLength(1000)]
    public string Description { get; set; } = null!;

    [StringLength(255)]
    public string Image { get; set; } = null!;

    [ForeignKey("IdCategory")]
    [InverseProperty("Products")]
    public virtual Category IdCategoryNavigation { get; set; } = null!;

    [ForeignKey("IdFactory")]
    [InverseProperty("Products")]
    public virtual Factory IdFactoryNavigation { get; set; } = null!;

    [ForeignKey("IdManufacturer")]
    [InverseProperty("Products")]
    public virtual Manufacturer IdManufacturerNavigation { get; set; } = null!;

    [ForeignKey("IdUnit")]
    [InverseProperty("Products")]
    public virtual Unit IdUnitNavigation { get; set; } = null!;

    [InverseProperty("IdProductNavigation")]
    public virtual ICollection<OrderProduct> OrderProducts { get; set; } = new List<OrderProduct>();
}
