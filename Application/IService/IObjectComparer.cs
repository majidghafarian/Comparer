﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.IService
{
    public interface IObjectComparer
    {
        string CompareAndLogChanges<T>(T oldObject, T newObject);
    }
}
