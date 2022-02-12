using System;
using System.Collections.Generic;

namespace Rowlan.QuickNav
{
    [Serializable]
    public class QuickNavData
    {
        public List<QuickNavItem> history = new List<QuickNavItem>();

        public List<QuickNavItem> favorites = new List<QuickNavItem>();

    }
}
