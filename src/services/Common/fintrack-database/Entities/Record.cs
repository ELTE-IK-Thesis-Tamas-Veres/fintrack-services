using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fintrack_database.Entities
{
    [Table("record")]
    public class Record
    {
        [Key]
        [Column("id")]
        public uint Id { get; set; }

        [Column("amount")]
        public int Amount { get; set; }

        [Column("date")]
        public DateOnly Date { get; set; }

        [Column("description")]
        public string Description { get; set; } = "";

        [Column("categoryId")]
        public uint? CategoryId { get; set; }

        [Column("userId")]
        public uint UserId { get; set; }

        [ForeignKey("CategoryId")]
        public Category? Category { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }
    }
}
