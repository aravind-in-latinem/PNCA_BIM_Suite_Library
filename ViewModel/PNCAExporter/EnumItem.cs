using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PNCA_BIM_Suite_Library.ViewModel.PNCAExporter
{
    public class EnumItem<T>
    {
        public T Value { get; set; }
        public string Name { get; set; }

        public EnumItem(T value)
        {
            Value = value;
            Name = value.ToString();
        }

        public override string ToString() => Name;
    }
}
