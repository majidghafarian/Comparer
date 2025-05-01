using Domain;
 
namespace Application.Models
{
    public class CompareEnumRequest
    {
        public StatusType OldValue { get; set; }
        public StatusType NewValue { get; set; }
    }

}
