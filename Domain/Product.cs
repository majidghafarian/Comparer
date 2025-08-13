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
    [AttributeUsage(AttributeTargets.Property)]
    public  class GetValueordinal : Attribute
    {
        public readonly string Value;

        public GetValueordinal(string value)
        {
            Value = value;
        }
    }
    [Display(Name ="جدول محصول")]
    public class Product
    {
        [Key]
        public int Id { get; set; }
        [Display(Name="نام کالا")]
        public string Name { get; set; }
        [Display(Name = "وضعیت کاربر")]
        public bool IsActive { get; set; }
        [Display(Name = "جزئیات محصول")]
        public List<ProductDetail> Details { get; set; } = new List<ProductDetail>();

        public Status status { get; set; }

    }

}
