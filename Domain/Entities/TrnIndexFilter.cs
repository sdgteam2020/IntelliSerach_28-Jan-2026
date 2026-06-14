using Domain.CommonModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Domain.Entities
{
    public class TrnIndexFilter: Common
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [Display(Name = "Index Name")]
        [StringLength(50)]
        [RegularExpression(@"^(?!\d+$)(?!\.+$)(?!_+$)[A-Za-z0-9._\s]+$", ErrorMessage = "Letters, numbers, spaces, . and _ are allowed, but the value cannot be only numbers, only dots, or only underscores.")]
        public required string Name { get; set; }
    }
}
