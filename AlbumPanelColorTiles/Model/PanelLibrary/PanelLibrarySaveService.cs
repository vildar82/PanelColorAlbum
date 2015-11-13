using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using AlbumPanelColorTiles.Panels;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;

namespace AlbumPanelColorTiles.PanelLibrary
{
   // Библиотека панелей покраски.
   // DWG файл
   public class PanelLibrarySaveService
   {
      public static readonly string LibPanelsFilePath = Path.Combine(AutoCAD_PIK_Manager.Settings.PikSettings.ServerShareSettingsFolder, @"АР\AlbumPanelColorTiles\AKR_Panels_Test.dwg");
      public static readonly string LibPanelsExcelFilePath = Path.Combine(AutoCAD_PIK_Manager.Settings.PikSettings.ServerShareSettingsFolder, @"АР\AlbumPanelColorTiles\AKR_Panels.xlsx");
      private Database _dbCur;
      private Document _doc;

      public PanelLibrarySaveService()
      {
         _doc = Application.DocumentManager.MdiActiveDocument;
         _dbCur = _doc.Database;
      }

      /// <summary>
      /// Проверка есть ли в текущем чертеже фасада новые панели, которых нет в библиотеке
      /// </summary>
      public static void CheckNewPanels()
      {
         var doc = Application.DocumentManager.MdiActiveDocument;
         var dbCur = doc.Database;
         var ed = doc.Editor;

         // список панелей (АКР-Панели марки СБ - без марки покраски) в текущем чертеже
         var panelsAkrInFacade = GetPanelsAkrInDb(dbCur);
         // список панелей в бибилиотеке
         List<PanelAKR> panelsAkrInLib = GetPanelsInLib();
         // сравнение списков и поиск новых панелей, которых нет в бибилиотеке
         List<string> panelsNotInLib = new List<string>();
         foreach (var panelInFacade in panelsAkrInFacade)
         {
            if (!panelsAkrInLib.Exists(p => string.Equals(p.BlNameInLib, panelInFacade.BlNameInLib, StringComparison.CurrentCultureIgnoreCase)))
            {
               panelsNotInLib.Add(panelInFacade.BlNameInLib);
            }
         }
         if (panelsNotInLib.Count > 0)
         {
            ed.WriteMessage("\n!!!Важно!!! В чертеже есть новые блоки АКР-Панелей которых нет в библиотеке:");
            foreach (var panel in panelsNotInLib)
            {
               ed.WriteMessage("\n{0}", panel);
            }
            ed.WriteMessage("\nРекомендуется сохранить их в библиотеку - на палитре есть кнопка для сохранения панелей в библиотеку.");
            Log.Error("Есть новые панели, которых нет в библиотеке: {0}", string.Join("; ", panelsNotInLib));
         }
      }

      public static List<PanelAKR> GetPanelsAkrInDb(Database db)
      {
         List<PanelAKR> panels = new List<PanelAKR>();
         using (var t = db.TransactionManager.StartTransaction())
         {
            var bt = t.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
            foreach (ObjectId idBtr in bt)
            {
               var btr = t.GetObject(idBtr, OpenMode.ForRead) as BlockTableRecord;
               if (!btr.IsLayout)
               {
                  if (MarkSbPanelAR.IsBlockNamePanel(btr.Name) && !MarkSbPanelAR.IsBlockNamePanelMarkAr(btr.Name))
                  {
                     panels.Add(new PanelAKR(idBtr, btr.Name));
                  }
               }
            }
            t.Commit();
         }
         return panels;
      }

      public static List<PanelAKR> GetPanelsInLib()
      {
         List<PanelAKR> panelsInLib = new List<PanelAKR>();
         // Получение списка панелей в библиотеке
         // файл библиотеки
         if (!File.Exists(PanelLibrarySaveService.LibPanelsFilePath))
         {
            throw new Exception("Не найден файл библиотеки АКР-Панелей - " + PanelLibrarySaveService.LibPanelsFilePath);
         }
         // копирование в temp
         string fileLibPanelsTemp = Path.GetTempFileName();
         File.Copy(PanelLibrarySaveService.LibPanelsFilePath, fileLibPanelsTemp, true);

         using (Database dbLib = new Database(false, true))
         {
            dbLib.ReadDwgFile(fileLibPanelsTemp, FileShare.ReadWrite, true, "");
            dbLib.CloseInput(true);
            using (var t = dbLib.TransactionManager.StartTransaction())
            {
               // список блоков АКР-Панелей в библиотеке (полные имена блоков).
               panelsInLib = PanelSB.GetAkrPanelNames(dbLib);
            }
         }
         return panelsInLib;
      }

      public void SavePanelsToLibrary()
      {
         // Сохранить блоки панелей в файл библиотеки блоков панелей.
         // Куда сохранять? - [CAD_Settings\AutoCAD_server\ShareSettings\]АР\AlbumPanelColorTiles\AKR_Panels.dwg
         // Если такой блок уже есть в бибилиотеке? - старому блоку изменить имя с приставкой сегодняшней даты - [АКР_Панель_МаркаСБ]_25.10.2015-14:15
         // Если файл занят другим процессом? - подождать 3 секунды и повторить.

         // Файл библиотеки блоков панелей.
         //string libPanelsFilePath = Path.Combine(AutoCAD_PIK_Manager.Settings.PikSettings.ServerShareSettingsFolder, @"АР\AlbumPanelColorTiles\AKR_Panels.dwg");
         if (!File.Exists(LibPanelsFilePath))
         {
            Log.Error("Нет файла библиотеки панелей {0}", LibPanelsFilePath);
            return;
         }
         // сбор блоков для сохранения
         List<PanelAKR> panelsAkrInFacade = GetPanelsAkrInDb(_dbCur);

         string msgReport = string.Empty;
         List<PanelAKR> panelsAkrToSave;

         // Открываем и блокируем от изменений файл библиотеки блоков
         using (var dbLib = new Database(false, true))
         {
            dbLib.ReadDwgFile(LibPanelsFilePath, FileShare.Read, false, "");
            dbLib.CloseInput(true);
            // список панелей в библиотеке
            List<PanelAKR> panelsAkrInLib = GetPanelsAkrInDb(dbLib); //GetPanelsInLib();
            // Список изменившихся панелей и новых для записи в базу.
            panelsAkrToSave = PanelAKR.GetChangedAndNewPanels(panelsAkrInFacade, panelsAkrInLib);
            if (panelsAkrToSave.Count > 0)
            {
               // копия текущего файла библиотеки панелей с приставкой сегодняшней даты
               copyLibPanelFile(LibPanelsFilePath);
               // Копирование новых панелей
               copyNewPanels(dbLib, panelsAkrToSave);
               // Текст изменений.
               //textChangesToLibDwg(panelsAkrToCopy, dbLib, t);
               // Сохранение файла библиотеки панелей
               dbLib.SaveAs(dbLib.Filename, DwgVersion.Current);
               // строка отчета
               msgReport = getReport(panelsAkrToSave);
               Log.Info("Обновлена библиотека панелей.");
            }
            else
            {
               _doc.Editor.WriteMessage("\nНет панелей для сохранения в библиотеку (в текущем чертеже нет новых и изменившихся панелей).");
            }
         }
         SaveChangesToExel.Save(panelsAkrToSave);
         _doc.Editor.WriteMessage(string.Format("\n{0}", msgReport));
         sendReport(msgReport);
      }

      private void copyLibPanelFile(string libPanelsFilePath)
      {
         string suffix = string.Format("{0}", DateTime.Now.ToString("dd.MM.yyyy-HH.mm"));
         string newFile = Path.Combine(
            Path.GetDirectoryName(libPanelsFilePath), string.Format("{0}_{1}.{2}",
            Path.GetFileNameWithoutExtension(libPanelsFilePath), suffix, "dwg"));
         File.Copy(libPanelsFilePath, newFile, true);
      }

      // Копирование новых панелей
      private void copyNewPanels(Database dbLib, List<PanelAKR> panelsAkrToCopy)
      {
         var ids = new ObjectIdCollection(panelsAkrToCopy.Select(p => p.IdBtrAkrPanelInLib).ToArray());
         IdMapping iMap = new IdMapping();
         dbLib.WblockCloneObjects(ids, dbLib.BlockTableId, iMap, DuplicateRecordCloning.Replace, false);         
      }

      private string getReport(List<PanelAKR> panels)
      {
         StringBuilder msg = new StringBuilder();
         msg.AppendLine(string.Format("Обновлены/добавлены следующие панели, от пользователя {0}:", Environment.UserName));
         foreach (var panel in panels)
         {
            msg.AppendLine(string.Format("{0} - {1}", panel.BlNameInLib, panel.ReportStatus));
         }
         return msg.ToString();
      }

      private void sendReport(string msg)
      {
         try
         {
            using (var mail = new MailMessage())
            {
               mail.To.Add("vildar82@gmail.com");
               mail.To.Add("KhisyametdinovVT@pik.ru");
               mail.From = new MailAddress("KhisyametdinovVT@pik.ru");
               mail.Subject = string.Format("AKR - Изменение библиотеки панелей {0}", Environment.UserName);
               mail.Body = msg.ToString();

               SmtpClient client = new SmtpClient();
               client.Host = "ex20pik.picompany.ru";
               client.Send(mail);
            }
         }
         catch (Exception ex)
         {
            Log.Error(ex, "sendReport MailMessage");
         }
      }

      private void textChangesToLibDwg(List<PanelAKR> panelsAkrToCopy, Database db, Transaction t)
      {
         // Запись изменений в текстовый объект
         var bt = t.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
         var ms = t.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
         foreach (ObjectId idEnt in ms)
         {
            if (idEnt.ObjectClass.Name == "AcDbMText")
            {
               var textOld = idEnt.GetObject(OpenMode.ForWrite) as MText;
               textOld.Erase();
               break;
            }
         }
         MText text = new MText();
         text.SetDatabaseDefaults(db);

         StringBuilder sbChanges = new StringBuilder();
         // Заголовок Изменений
         sbChanges.AppendLine(string.Format("Последнее изменение от {0}. Дата {1}. Чертеж {2}", Environment.UserName, DateTime.Now, _dbCur.Filename));
         // Список новых панелей
         var newPanels = panelsAkrToCopy.Where(p => p.ReportStatus == "Новая");
         if (newPanels.Count() > 0)
         {
            sbChanges.AppendLine("Список новых панелей:");
            foreach (var item in newPanels)
            {
               sbChanges.AppendLine(item.BlNameInLib);
            }
         }
         var changedPanels = panelsAkrToCopy.Where(p => p.ReportStatus == "Изменившаяся");
         if (changedPanels.Count() > 0)
         {
            sbChanges.AppendLine("Список изменившихся панелей:");
            foreach (var item in changedPanels)
            {
               sbChanges.AppendLine(item.BlNameInLib);
            }
         }

         text.Contents = sbChanges.ToString();
         ms.AppendEntity(text);
         t.AddNewlyCreatedDBObject(text, true);
      }
   }
}