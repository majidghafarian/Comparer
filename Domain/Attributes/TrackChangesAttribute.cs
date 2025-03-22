using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Attributes
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public class TrackChangesAttribute : Attribute
    {
        public string Description { get; set; }

        public TrackChangesAttribute(string description)
        {
            Description = description;
        }
    }
}
