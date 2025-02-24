using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fintrack_database.Entities
{
    [Table("user")]
    [Index("Sub", Name = "IX_User_Sub", IsUnique = true)]
    public class User
    {
        [Key]
        [Column("id")]
        public uint Id { get; set; }

        [Column("sub")]
        public string Sub { get; set; } = "";

        [InverseProperty("User")]
        public List<Category> Categories { get; set; } = new List<Category>();

        [InverseProperty("User")]
        public List<Record> Records { get; set; } = new List<Record>();
    }
}
