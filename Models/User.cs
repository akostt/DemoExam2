using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DemoExam2.Models;

public partial class User
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("ID_Role")]
    public int IdRole { get; set; }

    [Column("First_Name")]
    [StringLength(100)]
    public string FirstName { get; set; } = null!;

    [Column("Last_Name")]
    [StringLength(100)]
    public string LastName { get; set; } = null!;

    [Column("Middle_Name")]
    [StringLength(100)]
    public string MiddleName { get; set; } = null!;

    [StringLength(254)]
    public string Login { get; set; } = null!;

    [StringLength(100)]
    public string Password { get; set; } = null!;

    [ForeignKey("IdRole")]
    [InverseProperty("Users")]
    public virtual Role IdRoleNavigation { get; set; } = null!;

    [InverseProperty("IdUserNavigation")]
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
