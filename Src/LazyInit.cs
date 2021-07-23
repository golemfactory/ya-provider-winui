#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GolemUI.Src
{
    public class LazyInit<T>
    {
        

        public LazyInit(Func<T> initFunc)
        {
            _initFunc = initFunc;
        }

        public T Value
        {
            get
            {
                if (_value == null)
                {
                    _value = _initFunc!();
                    _initFunc = null;
                }
                return _value;
            }
        }

        private T? _value;

        private Func<T>? _initFunc;
        
    }
}
