using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ConsoleApp1
{
    /// <summary>
    /// 
    /// </summary>
    [Table("COMMON_SPOTTING")]
    public class Spotting
    {
        [Key,Column("SpottingId")]
        public string SpottingId { get; set; }

        [Column("SpottingName")]
        public string SpottingName { get; set; }

        [Column("CREATEDON")]
        public DateTime? CreatedOn { get; set; }
    }
}
