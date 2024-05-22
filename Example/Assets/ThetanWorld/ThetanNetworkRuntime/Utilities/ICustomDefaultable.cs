using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wolffun.RestAPI
{
    public interface ICustomDefaultable<out T>
    {
        public T SetDefault();

        public bool IsEmpty();
    }
}
