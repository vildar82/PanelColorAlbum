using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlbumPanelColorTiles.RenamePanels;
using Autodesk.AutoCAD.DatabaseServices;

namespace AlbumPanelColorTiles.Lib
{
   public static class DictNOD
   {
      private const string _dicName = "AlbumPanelColorTiles";
      private const string _recNameRenameMarkAR = "MarkArRename";
      private const string _recNameAbbr = "Abbr";

      public static void SaveToDict(List<MarkArRename> renameMarkARList)
      {
         ObjectId idDict = getDict();
         if (idDict.IsNull)         
            return;         
         ObjectId idRec = getRec(idDict, _recNameRenameMarkAR+MarkArRename.Abbr.ToUpper());
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

      public static void LoadFromDict(ref Dictionary<string,MarkArRename> beforeRenameMarkARList)
      {
         ObjectId idDict = getDict();
         if (idDict.IsNull)         
            return;         
         ObjectId idRec = getRec(idDict, _recNameRenameMarkAR + MarkArRename.Abbr.ToUpper());
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
                  var names = value.Split(';');
                  string markArName = names[0];
                  MarkArRename markArRename;
                  if (beforeRenameMarkARList.TryGetValue(markArName, out markArRename))
                  {
                     markArRename.RenameMark(names[1], beforeRenameMarkARList);
                  }
               }
            }
         }        
      }

      private static string getValueRenameMark(MarkArRename renameMark)
      {
         return string.Format("{0};{1}", renameMark.MarkAR.MarkARPanelFullNameCalculated, renameMark.MarkPainting);
      }

      private static string getRenameMarkFromValue(string value)
      {
         return value;
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
      

      private static ObjectId getRec(ObjectId idDict, string recName)
      {
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

      public static void SaveAbbr(string abbr)
      {
         ObjectId idDict = getDict();
         if (idDict.IsNull)         
            return;         
         ObjectId idRec = getRec(idDict, _recNameAbbr);
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

      public static string LoadAbbr()
      {
         string res = string.Empty; 
         ObjectId idDict = getDict();
         if (idDict.IsNull)
            return res;
         ObjectId idRec = getRec(idDict, _recNameAbbr);
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
   }
}
