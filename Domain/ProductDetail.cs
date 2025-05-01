using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Domain
{

    public class ProductDetail
    {
        [Key]
        public int Id { get; set; }
        [Display(Name = "2مقدار")]
        public string Feature { get; set; }
        [Display(Name = "کش")]
        public string Value { get; set; }
        [JsonIgnore]
        public int? productid { get; set; }
        [ForeignKey("productid")]
        [JsonIgnore]
        public Product? product { get; set; }
        [Display(Name = "جزئیات بیشتر")]
        public List<ProductSubDetail> SubDetails { get; set; } = new List<ProductSubDetail>();

    }
}
