using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConsoleApp1
{
    ///<summary>
    ///
    ///</summary>
    [Table("mytest")]
    public class Mytest
    {
        ///<summary>
        ///
        ///</summary> 
        [Key, Column("id")]
        public string Id { get; set; }

        ///<summary>
        ///
        ///</summary> 
        [Column("aa",TypeName = "VARCHAR2")]
        public string AA { get; set; }     
    }
}
