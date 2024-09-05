using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatNguyenTool
{
    public class ElementFilterByCategory : ISelectionFilter
    {
        private BuiltInCategory _category;

        public ElementFilterByCategory (BuiltInCategory category)
        {
            _category = category;
        }

        public bool AllowElement(Element elem)
        {
            try
            {
#if R2024
                return elem.Category.Id.Value == (long)_category;
#else
                return elem.Category.Id.IntegerValue == (int)_category;

#endif

            }
            catch
            {
                return false;
            }
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            throw new NotImplementedException();
        }
    }
}
