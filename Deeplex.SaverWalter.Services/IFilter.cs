using Deeplex.Utils.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deeplex.SaverWalter.Services
{
    public interface IFilterViewModel
    {
        ObservableProperty<string> Filter { get; set; }
    }
}
