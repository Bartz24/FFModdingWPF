using Bartz24.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bartz24.RandoWPF;
public interface DataStoreItemProvider<T>
{
    public T GetItemData(bool orig);
}
