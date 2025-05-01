using System.ComponentModel.DataAnnotations;
 

namespace Domain
{
 
    public enum StatusType
    {
        [Display(Name = "هیچ‌کدام")]
        None = 0,

        [Display(Name = "جدید")]
        New = 1,

        [Display(Name = "در حال انجام")]
        InProgress = 2,

        [Display(Name = "تکمیل شده")]
        Completed = 3
    }

}
