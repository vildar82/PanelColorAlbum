using System;
using System.Collections.Generic;
using System.Linq;
using AcadLib.Errors;
using AlbumPanelColorTiles.RenamePanels;
using Autodesk.AutoCAD.DatabaseServices;

namespace AlbumPanelColorTiles.Lib
{
    public static class DictNOD
    {
        private const string _dicName = "AlbumPanelColorTiles";
        private const string _recNameAbbr = "Abbr";

        //private const string _recNameNumberFirstFloor = "NumberFirstFloor";
        private const string _recNameRenameMarkAR = "MarkArRename";

        public static void CopyDict(Database dbDest)
        {
            // Копирование словаря АКР в базу
            var idDict = getDict();
            if (idDict.IsNull)
            {
                throw new Exception("Словарь АКР в текущем чертеже не найден.");
            }
            var ids = new ObjectIdCollection();
            ids.Add(idDict);
            var map = new IdMapping();
            dbDest.WblockCloneObjects(ids, dbDest.NamedObjectsDictionaryId, map, DuplicateRecordCloning.Replace, false);
        }

        public static string LoadAbbr()
        {
            string res = string.Empty;
            ObjectId idRec = getRec(_recNameAbbr);
            if (idRec.IsNull)
                return res;

            using (var xRec = idRec.Open(OpenMode.ForRead) as Xrecord)
            {
                using (var data = xRec.Data)
                {
                    if (data == null)
                        return res;
                    foreach (var typedValue in data)
                    {
                        return typedValue.Value.ToString();
                    }
                }
            }
            return res;
        }

        public static bool LoadBool(string key, bool defValue)
        {
            bool res = defValue; // default
            ObjectId idRec = getRec(key);
            if (idRec.IsNull)
                return res;

            using (var xRec = idRec.Open(OpenMode.ForRead) as Xrecord)
            {
                using (var data = xRec.Data)
                {
                    if (data == null)
                        return res;
                    var values = data.AsArray();
                    foreach (var typedValue in values)
                    {
                        return Convert.ToBoolean(typedValue.Value);
                    }
                }
            }
            return res;
        }

        public static int LoadNumber(string keyName)
        {
            int res = 0;
            ObjectId idRec = getRec(keyName);
            if (idRec.IsNull)
                return res;

            using (var xRec = idRec.Open(OpenMode.ForRead) as Xrecord)
            {
                using (var data = xRec.Data)
                {
                    if (data == null)
                        return res;
                    foreach (var typedValue in data)
                    {
                        return (int)typedValue.Value;
                    }
                }
            }
            return res;
        }

        public static void LoadRenameMarkArFromDict(ref Dictionary<string, MarkArRename> beforeRenameMarkARList)
        {
            ObjectId idRec = getRec(_recNameRenameMarkAR + MarkArRename.Abbr.ToUpper());
            if (idRec.IsNull)
                return;

            using (var xRec = idRec.Open(OpenMode.ForRead) as Xrecord)
            {
                using (var data = xRec.Data)
                {
                    if (data == null)
                    {
                        return;
                    }
                    foreach (var typedValue in data)
                    {
                        string value = typedValue.Value.ToString();
                        var names = value.Split(';'); //renameMark.MarkAR.MarkARPanelFullNameCalculated;renameMark.MarkPainting
                        string markArDict = names[0];
                        MarkArRename markArRename;
                        if (beforeRenameMarkARList.TryGetValue(markArDict, out markArRename))
                        {
                            markArRename.RenameMark(names[1], beforeRenameMarkARList);
                        }
                        else
                        {
                            // Замена запятых на точки
                            markArDict = markArDict.Replace(',', '.');
                            List<MarkArRename> marksArRename = beforeRenameMarkARList.Values.ToList();
                            foreach (var item in marksArRename)
                            {
                                var markArDot = item.MarkArCurFull.Replace(',', '.');
                                if (string.Equals(markArDict, markArDot, System.StringComparison.OrdinalIgnoreCase))
                                {
                                    item.RenameMark(names[1], beforeRenameMarkARList);
                                }
                            }
                        }
                    }
                }
            }
        }

        public static string LoadString(string key, string defValue)
        {
            string res = defValue;
            ObjectId idRec = getRec(key);
            if (idRec.IsNull)
                return res;

            using (var xRec = idRec.Open(OpenMode.ForRead) as Xrecord)
            {
                using (var data = xRec.Data)
                {
                    if (data == null)
                        return res;
                    var values = data.AsArray();
                    foreach (var typedValue in values)
                    {
                        return typedValue.Value.ToString();
                    }
                }
            }
            return res;
        }

        public static void SaveAbbr(string abbr)
        {
            ObjectId idRec = getRec(_recNameAbbr);
            if (idRec.IsNull)
                return;

            using (var xRec = idRec.Open(OpenMode.ForWrite) as Xrecord)
            {
                using (ResultBuffer rb = new ResultBuffer())
                {
                    rb.Add(new TypedValue((int)DxfCode.Text, abbr));
                    xRec.Data = rb;
                }
            }
        }

        public static void SaveBool(bool value, string key)
        {
            ObjectId idRec = getRec(key);
            if (idRec.IsNull)
                return;

            using (var xRec = idRec.Open(OpenMode.ForWrite) as Xrecord)
            {
                using (ResultBuffer rb = new ResultBuffer())
                {
                    rb.Add(new TypedValue((int)DxfCode.Bool, value));
                    xRec.Data = rb;
                }
            }
        }

        public static void SaveNumber(int number, string keyName)
        {
            ObjectId idRec = getRec(keyName);
            if (idRec.IsNull)
                return;

            using (var xRec = idRec.Open(OpenMode.ForWrite) as Xrecord)
            {
                using (ResultBuffer rb = new ResultBuffer())
                {
                    rb.Add(new TypedValue((int)DxfCode.Int32, number));
                    xRec.Data = rb;
                }
            }
        }

        public static void SaveRenamedMarkArToDict(List<MarkArRename> renameMarkARList)
        {
            ObjectId idRec = getRec(_recNameRenameMarkAR + MarkArRename.Abbr.ToUpper());
            if (idRec.IsNull)
                return;

            using (var xRec = idRec.Open(OpenMode.ForWrite) as Xrecord)
            {
                using (ResultBuffer rb = new ResultBuffer())
                {
                    foreach (var renameMark in renameMarkARList)
                    {
                        if (renameMark.IsRenamed)
                        {
                            string value = getValueRenameMark(renameMark);
                            rb.Add(new TypedValue((int)DxfCode.Text, value));
                        }
                    }
                    xRec.Data = rb;
                }
            }
        }

        public static void SaveString(string text, string key)
        {
            ObjectId idRec = getRec(key);
            if (idRec.IsNull)
                return;

            using (var xRec = idRec.Open(OpenMode.ForWrite) as Xrecord)
            {
                using (ResultBuffer rb = new ResultBuffer())
                {
                    rb.Add(new TypedValue((int)DxfCode.Text, text));
                    xRec.Data = rb;
                }
            }
        }

        private static ObjectId getDict()
        {
            ObjectId idDic = ObjectId.Null;
            Database db = HostApplicationServices.WorkingDatabase;

            using (DBDictionary nod = (DBDictionary)db.NamedObjectsDictionaryId.Open(OpenMode.ForRead))
            {
                if (!nod.Contains(_dicName))
                {
                    nod.UpgradeOpen();
                    using (var dic = new DBDictionary())
                    {
                        idDic = nod.SetAt(_dicName, dic);
                        dic.TreatElementsAsHard = true;
                    }
                }
                else idDic = nod.GetAt(_dicName);
            }
            return idDic;
        }

        private static ObjectId getRec(string recName)
        {
            ObjectId idDict = getDict();
            if (idDict.IsNull)
            {
                return ObjectId.Null;
            }
            ObjectId idRec = ObjectId.Null;
            using (var dic = idDict.Open(OpenMode.ForRead) as DBDictionary)
            {
                if (!dic.Contains(recName))
                {
                    using (var xRec = new Xrecord())
                    {
                        dic.UpgradeOpen();
                        idRec = dic.SetAt(recName, xRec);
                    }
                }
                else idRec = dic.GetAt(recName);
            }
            return idRec;
        }

        private static string getValueRenameMark(MarkArRename renameMark)
        {
            return string.Format("{0};{1}", renameMark.MarkAR.MarkARPanelFullNameCalculated, renameMark.MarkPainting);
        }
    }
}