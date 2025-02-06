using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fintrack_database.Entities
{
    [Table("category")]
    public class Category
    {
        [Key]
        [Column("id")]
        public uint Id { get; set; }

        [Column("user_id")]
        public uint UserId { get; set; }

        [Column("name")]
        public string Name { get; set; } = "";

        [ForeignKey("UserId")]
        public User User { get; set; } = new User();
    }
}
