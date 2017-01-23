using System;
using AcadLib.Errors;
using Autodesk.AutoCAD.DatabaseServices;
using System.Collections.Generic;
using Autodesk.AutoCAD.Geometry;

namespace AlbumPanelColorTiles.MountingsPlans
{
    public class Workspace
    {
        public const string WorkspaceAttrSection = "СЕКЦИЯ";
        public const string WorkspaceAttrFloor = "ЭТАЖ";
        public Extents3d Extents { get; private set; }
        public string Section { get; private set; }
        public string Floor { get; private set; }
        public bool IsOk { get { return string.IsNullOrEmpty(Error); } }
        public List<ObjectId> IdsElementInWS { get; set; }
        public string Error { get; private set; }
        public Point3d AxisPosition { get; set; }

        public Workspace(ObjectId idBlRef)
        {
            var blRef = idBlRef.GetObject(OpenMode.ForRead, false, true) as BlockReference;
            DefineWs(blRef);
        }

        public Workspace(BlockReference blRef)
        {
            DefineWs(blRef);
        }

        private void DefineWs (BlockReference blRef)
        {
            try
            {
                Extents = blRef.GeometricExtents;
            }
            catch
            {
                Error = "Ошибка определения границ блока. Необходимо выполнить проверку чертежа командой _audit с исправлением ошибок.";
            }
            defineAttrs(blRef);
            checks();
        }

        private void defineAttrs(BlockReference blRef)
        {
            if (blRef.AttributeCollection == null)
            {
                Error = $"Не определены атрибуты: '{WorkspaceAttrSection}', '{WorkspaceAttrFloor}'.";
            }
            else
            {
                foreach (ObjectId idAtr in blRef.AttributeCollection)
                {
                    var atrRef = idAtr.GetObject(OpenMode.ForRead, false, true) as AttributeReference;
                    if (atrRef.Tag.Equals(WorkspaceAttrSection, StringComparison.OrdinalIgnoreCase))
                    {
                        Section = atrRef.TextString;
                    }
                    else if (atrRef.Tag.Equals(WorkspaceAttrFloor, StringComparison.OrdinalIgnoreCase))
                    {
                        Floor = atrRef.TextString;
                    }
                }
            }
        }

        public static Workspace Define(BlockReference blRef)
        {
            string err = null;
            try
            {
                var ws = new Workspace(blRef);
                if (string.IsNullOrEmpty(ws.Error))
                    return ws;
                else
                    err = ws.Error;
            }
            catch (Exception ex)
            {
                err = ex.Message;
            }
            if (err != null)
            {
                Inspector.AddError($"Ошибка определения блока рабочей области - {err}", blRef, System.Drawing.SystemIcons.Error);
            }
            return null;
        }

        public static bool IsWorkSpace(string blName)
        {
            return blName.Equals("rab_obl", StringComparison.OrdinalIgnoreCase);
        }

        private void checks()
        {
            // Пока никаких проверок         
        }

        public override string ToString()
        {
            return $"Секция {Section}, Этаж {Floor}";
        }
    }
}