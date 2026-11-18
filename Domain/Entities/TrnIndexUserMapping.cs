using Domain.CommonModel;
using Domain.IdentityEntities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Domain.Entities
{
    public class TrnIndexUserMapping
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IndexUserMapId { get; set; }
        [Required]
        [StringLength(200)]
        public required int IndexId { get; set; }
        [ForeignKey(nameof(IndexId))]
        public MIndexes? Index { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public ApplicationUser? AspUserId { get; set; }

    }
}
