using System;
using System.Collections.Generic;
using System.Text;

namespace Jamesnet.Foundation
{
    public interface IDockable
    {
        string Header { get; set; }
        DockPosition DockPosition { get; set; }
        bool IsFloating { get; set; }
    }
}
