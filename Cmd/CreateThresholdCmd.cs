using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DatNguyenTool;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Autodesk.Revit.UI.Selection;
using System.Linq;

namespace NDT_RevitAPI
{
    [Transaction(TransactionMode.Manual)]
    public class CreateThresholdCmd : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            var filterDoors = new ElementFilterByCategory(BuiltInCategory.OST_Doors);
            IList<Reference> selectedDoors = uidoc.Selection.PickObjects(ObjectType.Element, filterDoors, "Select Doors");

            if (selectedDoors.Any())
            {
                var floorTypes = RoomToFloorUtils.GetFloorTypeModels(doc);
                var collector = new ObservableCollection<FloorTypeModel>(floorTypes);
                var vm = new CreateThresholdVM(collector, collector[0], 0, true);
                var window = new CreateThresholdsView();

                vm.mainWindow = window;
                window.DataContext = vm;

                window.ShowDialog();
                if (window.DialogResult == true)
                {
                    CreateThresholdUtils.Main(doc, vm, selectedDoors);
                }
            }

            return Result.Succeeded;
        }

    }
}
