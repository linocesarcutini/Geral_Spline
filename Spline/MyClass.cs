using System;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.ApplicationServices;

namespace MySpline
{
    public static class MyClass
    {
        public static Point2d pt1, pt2;
        public static Point3d pt1x, pt2x;

        [CommandMethod("ss")]
        public static void AddSpline()
        {
            pt1x = GetPoint();
            pt2x = GetPoint();

            pt1 = new Point2d(pt1x.X, pt1x.Y);
            pt2 = new Point2d(pt2x.X, pt2x.Y);

            if (pt1.X > pt2.X || pt1.Y > pt2.Y)
            {
                Point2d ptx = pt1;

                pt1 = pt2;
                pt2 = ptx;
            }

            // Get the current document and database
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;
            Editor ed = acDoc.Editor;

            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Block table for read
                BlockTable acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                double anglePt1Pt2 = pt1.GetVectorTo(pt2).Angle;

                if (anglePt1Pt2 > Math.PI)
                    anglePt1Pt2 -= Math.PI;


                //PolarPoints(pt1.Add((pt2.Subtract(pt1.GetAsVector()) / 4).GetAsVector()), pt1.GetVectorTo(pt2).GetAngleTo(pt2.GetAsVector()) + Math.PI / 2, 10);
                Point2d pt3 = PolarPoints(pt1.Add(pt1.GetVectorTo(pt2) / 4), anglePt1Pt2 + Math.PI / 2, pt1.GetDistanceTo(pt2) * 0.1);
                Point2d pt4 = PolarPoints(pt2.Subtract((pt2.Subtract(pt1.GetAsVector()) / 4).GetAsVector()), anglePt1Pt2 - Math.PI / 2, pt1.GetDistanceTo(pt2) * 0.1);

                Point3d pt5 = new Point3d(pt1.X, pt1.Y, 0); // pt1 em 3d
                Point3d pt6 = new Point3d(pt3.X, pt3.Y, 0); // pt3 em 3d
                Point3d pt7 = new Point3d(pt4.X, pt4.Y, 0); // pt4 em 3d
                Point3d pt8 = new Point3d(pt2.X, pt2.Y, 0); // pt2 em 3d

                // Define the fit points for the spline
                Point3dCollection ptColl = new Point3dCollection
                {
                    pt5,
                    pt6,
                    pt7,
                    pt8
                };

                // Create a spline through (0, 0, 0), (5, 5, 0), and (10, 0, 0) with a
                // start and end tangency of (0.5, 0.5, 0.0)
                using (Spline acSpline = new Spline(ptColl, new Point3d(0.0000, 0.0000, 0.0000).GetAsVector(), new Point3d(0.0000, 0.0000, 0.0000).GetAsVector(), 0, 0.0))
                {
                    // Add the new object to the block table record and the transaction
                    acBlkTblRec.AppendEntity(acSpline);
                    acTrans.AddNewlyCreatedDBObject(acSpline, true);
                }
                
                acDoc.SendStringToExecute("Trim  ", true, false, false);

                // Save the new line to the database
                acTrans.Commit();
            }
        }

        // Método destinado para pegar o ponto no desenho
        public static Point3d GetPoint()
        {
            Point3d pt;

            Document document = Application.DocumentManager.MdiActiveDocument;
            Database database = document.Database;

            PromptPointOptions pPopt = new PromptPointOptions("\nSelecione o ponto");
            var pPoRs = document.Editor.GetPoint(pPopt);
            pt = pPoRs.Value;

            return pt;
        }

        public static Point2d PolarPoints(Point2d pPt, double dAng, double dDist)
        {
            return new Point2d(pPt.X + dDist * Math.Cos(dAng),
                                pPt.Y + dDist * Math.Sin(dAng));
        }
    }
}