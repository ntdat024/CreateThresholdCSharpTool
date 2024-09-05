using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Windows;

namespace NDT_RevitAPI
{
    public class CreateThresholdUtils
    {
        public static bool CheckWallKind(Document doc, Wall wall)
        {
            WallType wall_type = doc.GetElement(wall.GetTypeId()) as WallType;
            string wall_kind = wall_type.Kind.ToString();
            if (wall_kind == "Basic" || wall_kind == "Stacked")
            {
                return true;
            }
            return false;
        }

        private static Floor CreateSeparateFloors(Document doc, FamilyInstance door, string uniqueId, double distanceOffset)
        {
            try
            {//get door information
                FamilySymbol doorType = doc.GetElement(door.GetTypeId()) as FamilySymbol;
                LocationPoint doorLocation = door.Location as LocationPoint;
                XYZ door_point = doorLocation.Point;
                double doorAngle = doorLocation.Rotation;
                double doorWidth = doorType.get_Parameter(BuiltInParameter.DOOR_WIDTH).AsDouble();
                var levelId = door.get_Parameter(BuiltInParameter.FAMILY_LEVEL_PARAM).AsElementId();
                var level = doc.GetElement(levelId) as Level;

                //get host wall and properties
                Wall wall = door.Host as Wall;
                WallType wallType = doc.GetElement(wall.GetTypeId()) as WallType;
                double wallWidth = wallType.get_Parameter(BuiltInParameter.WALL_ATTR_WIDTH_PARAM).AsDouble();


                //create curveloops
                XYZ p1 = door_point;
                XYZ p2 = new XYZ(p1.X + doorWidth, p1.Y, p1.Z);
                XYZ p3 = new XYZ(p2.X, p2.Y + wallWidth, p1.Z);
                XYZ p4 = new XYZ(p1.X, p1.Y + wallWidth, p1.Z);
                XYZ center = (p1 + p2 + p3 + p4) / 4;

                Line line1 = Line.CreateBound(p1, p2);
                Line line2 = Line.CreateBound(p2, p3);
                Line line3 = Line.CreateBound(p3, p4);
                Line line4 = Line.CreateBound(p4, p1);
                IList<Curve> listLines = new List<Curve> { line1, line2, line3, line4 };

                CurveLoop curveLoop = CurveLoop.Create(listLines);
                IList<CurveLoop> profiles = new List<CurveLoop> { curveLoop };
                CurveArray curveArray = new CurveArray();
                foreach (var line in listLines)
                {
                    curveArray.Append(line);
                }

                FloorType floorType = doc.GetElement(uniqueId) as FloorType;
#if R2021
                Floor floor = doc.Create.NewFloor(curveArray, floorType, level, false);
#else
                Floor floor = Floor.Create(doc, profiles, floorType.Id, level.Id);
#endif

                ElementTransformUtils.MoveElement(doc, floor.Id, p1 - center);
                Line axis = Line.CreateBound(p1, p1 + XYZ.BasisZ);
                ElementTransformUtils.RotateElement(doc, floor.Id, axis, doorAngle);
                floor.get_Parameter(BuiltInParameter.FLOOR_HEIGHTABOVELEVEL_PARAM).Set(distanceOffset);

                return floor;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Message");
                return null;
            }



        }

        private static void CombineFloors(Document doc, List<Floor> list_floor, ElementId floorTypeId, double offset)
        {
#if R2021
#else
            try
            {

                ElementId levelId = list_floor[0].LevelId;
                IList<CurveLoop> profiles = new List<CurveLoop>();
                IList<ElementId> idToDel = new List<ElementId>();

                foreach (Floor floor in list_floor)
                {
                    IList<Curve> curves = new List<Curve>();
                    Sketch sketch = doc.GetElement(floor.SketchId) as Sketch;
                    CurveArray curveArray = sketch.Profile.get_Item(0);
                    foreach (Curve curve in curveArray)
                    {
                        curves.Add(curve);
                    }
                    CurveLoop curveLoop = CurveLoop.Create(curves);
                    profiles.Add(curveLoop);
                    idToDel.Add(floor.Id);
                }

                doc.Delete(idToDel);

                Floor newFloor = Floor.Create(doc, profiles, floorTypeId, levelId);
                newFloor.get_Parameter(BuiltInParameter.FLOOR_HEIGHTABOVELEVEL_PARAM).Set(offset);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Message");
            }
#endif

        }


        public static void Main(Document doc, CreateThresholdVM vm, IList<Reference> selectedDoors)
        {

            try
            {
                double offset = vm.HeightOffset / 304.8;
                string uniqueId = vm.SelectedFloorType.UniqueId;

                var listFloors = new List<Floor>();
                using (Transaction t = new Transaction(doc, " "))
                {
                    t.Start();

                    //create floors
                    foreach (Reference r in selectedDoors)
                    {
                        FamilyInstance door = doc.GetElement(r) as FamilyInstance;
                        Element host = door.Host;
                        if (host is Wall wall)
                        {
                            bool checkWallKind = CheckWallKind(doc, wall);
                            if (checkWallKind)
                            {
                                var floor = CreateSeparateFloors(doc, door, uniqueId, offset);
                                if (floor != null)
                                {
                                    listFloors.Add(floor);
                                }
                            }
                        }    
                        
                    }

                    doc.Regenerate();
#if R2021
                //
#else
                    //combine floors
                    var floorType = doc.GetElement(uniqueId) as FloorType;
                    if (!vm.IsCreateSeparateFloors)
                    {
                        CombineFloors(doc, listFloors, floorType.Id, offset);
                    }
#endif
                    t.Commit();
                }

            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Message");
            }
          
        }
       
    }
}
