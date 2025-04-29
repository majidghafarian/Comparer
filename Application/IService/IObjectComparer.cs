using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.IService
{
    public interface IObjectComparer
    {
        //string CompareAndLogChanges<T>(T oldObject, T newObject);
        //public List<string> CompareByKey(IEnumerable<object> oldList, IEnumerable<object> newList, string keyName, string prefix = "");
        public List<string> CompareObjects<T>(T oldObj, T newObj );
    }
}
