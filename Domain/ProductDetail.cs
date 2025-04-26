using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Domain
{
    public class ProductDetail
    {
        public int Id { get; set; }
        public string Feature { get; set; }
        public string Value { get; set; }
       
        public int productid { get; set; }
        [ForeignKey("productid")]
        [JsonIgnore]
        public Product? product { get; set; }

    }
}
