using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Domain.DTOs.Requests
{
    public class DTOIndexMapRequest
    {
        public int? Id { get; set; }

        [Required]
        [Display(Name = "FilterName")]
        [StringLength(50)]
        [RegularExpression(@"^(?!\d+$)(?!\.+$)(?!_+$)[A-Za-z0-9._\s]+$", ErrorMessage = "Letters, numbers, spaces, . and _ are allowed, but the value cannot be only numbers, only dots, or only underscores.")]
        public required string FilterName { get; set; }

        [Required]
        public required List<DTOIndexMapUrlWithIndex> Indexs{ get; set; }

    }
    public class DTOIndexMapUrlWithIndex
    {
        public required string IndexNames { get; set; }
        public string? Url { get; set; }
    }
}
