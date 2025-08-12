using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public enum Status
    {
        active = 0,
        diactive = 1
    }
    [Display(Name ="جدول محصول")]
    public class Product
    {
        [Key]
        public int Id { get; set; }
        [Display(Name = "نام محصول")]
        public string Name { get; set; }
        [Display(Name = "وضعیت کاربر")]
        public bool IsActive { get; set; }
        [Display(Name = "جزئیات محصول")]
        public List<ProductDetail> Details { get; set; } = new List<ProductDetail>();

        public Status status { get; set; }

    }

}
