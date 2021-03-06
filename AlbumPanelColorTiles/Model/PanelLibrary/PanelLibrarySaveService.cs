﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Windows.Forms;
using AlbumPanelColorTiles.Panels;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using AcadLib;

namespace AlbumPanelColorTiles.PanelLibrary
{
   // Библиотека панелей покраски.
   // DWG файл
   public class PanelLibrarySaveService
   {
      public static readonly string LibPanelsExcelFilePath = Path.Combine(AutoCAD_PIK_Manager.Settings.PikSettings.ServerShareSettingsFolder, @"АР\AlbumPanelColorTiles\AKR_Panels.xlsx");
      public static readonly string LibPanelsFilePath = Path.Combine(AutoCAD_PIK_Manager.Settings.PikSettings.ServerShareSettingsFolder, @"АР\AlbumPanelColorTiles\AKR_Panels.dwg");
      private Database _dbCur;
      private Document _doc;

      public PanelLibrarySaveService()
      {
         _doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
         _dbCur = _doc.Database;
      }

        ///// <summary>
        ///// Проверка есть ли в текущем чертеже фасада новые панели, которых нет в библиотеке
        ///// </summary>
        //public static void CheckNewPanels()
        //{
        //   var doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
        //   var dbCur = doc.Database;
        //   var ed = doc.Editor;

        //   // список панелей (АКР-Панели марки СБ - без марки покраски) в текущем чертеже
        //   List<PanelAkrFacade> panelsAkrFacade = GetPanelsAkrInDb(dbCur);
        //   // исключение панелей с индексом электрики
        //   removeElectricPanels(panelsAkrFacade);
        //   // список панелей в бибилиотеке
        //   List<PanelAkrLib> panelsAkrLib = GetPanelsInLib();
        //   // сравнение списков и поиск новых панелей, которых нет в бибилиотеке
        //   List<string> panelsNotInLib = new List<string>();
        //   foreach (var panelInFacade in panelsAkrFacade)
        //   {
        //      if (!panelsAkrLib.Exists(p => string.Equals(p.BlName, panelInFacade.BlName, StringComparison.CurrentCultureIgnoreCase)))
        //      {
        //         panelsNotInLib.Add(panelInFacade.BlName);
        //      }
        //   }
        //   if (panelsNotInLib.Count > 0)
        //   {
        //      ed.WriteMessage("\n!!!Важно!!! В чертеже есть новые блоки АКР-Панелей которых нет в библиотеке:");
        //      foreach (var panel in panelsNotInLib)
        //      {
        //         ed.WriteMessage("\n{0}", panel);
        //      }
        //      ed.WriteMessage("\nРекомендуется сохранить их в библиотеку - на палитре есть кнопка для сохранения панелей в библиотеку.");
        //      Logger.Log.Error("Есть новые панели, которых нет в библиотеке: {0}", string.Join("; ", panelsNotInLib));
        //   }
        //}

        public static List<PanelAkrFacade> GetPanelsAkrInDb(Database db)
        {
            List<PanelAkrFacade> panels = new List<PanelAkrFacade>();
            using (var t = db.TransactionManager.StartTransaction())
            {
                var bt = t.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                foreach (ObjectId idBtr in bt)
                {
                    if (!idBtr.IsValidEx()) continue;
                    var btr = t.GetObject(idBtr, OpenMode.ForRead) as BlockTableRecord;
                    if (!btr.IsLayout)
                    {
                        if (MarkSb.IsBlockNamePanel(btr.Name) && !MarkSb.IsBlockNamePanelMarkAr(btr.Name))
                        {
                            panels.Add(new PanelAkrFacade(idBtr, btr.Name));
                        }
                    }
                }
                t.Commit();
            }
            return panels;
        }

      public static List<PanelAKR> GetPanelsInLib(string panelsFile, bool defineFullPanelData)
      {
         List<PanelAKR> panelsInLib = new List<PanelAKR>();
         // Получение списка панелей в библиотеке
         // файл библиотеки
         if (!File.Exists(PanelLibrarySaveService.LibPanelsFilePath))
         {
            throw new Exception("Не найден файл библиотеки АКР-Панелей - " + PanelLibrarySaveService.LibPanelsFilePath);
         }
         // копирование в temp
         //string fileLibPanelsTemp = Path.GetTempFileName();
         //File.Copy(PanelLibrarySaveService.LibPanelsFilePath, fileLibPanelsTemp, true);

         using (Database dbLib = new Database(false, true))
         {      
            dbLib.ReadDwgFile(panelsFile, FileShare.ReadWrite, true, "");
            dbLib.CloseInput(true);
            // список блоков АКР-Панелей в библиотеке (полные имена блоков).
            panelsInLib = PanelAKR.GetAkrPanelLib(dbLib, defineFullPanelData);
         }
         return panelsInLib;
      }

      public static void WarningBusyLibrary(Exception ex)
      {
         // Предупреждение, что библиотека занята
         var whoHas = Autodesk.AutoCAD.ApplicationServices.Application.GetWhoHasInfo(LibPanelsFilePath);
         MessageBox.Show(string.Format("Другим пользователем уже выполняется сохранение панелей в библиотеку. Повторите позже.\n" +
            "Кем занято: {0}, время {1}",
            whoHas.UserName, whoHas.OpenTime));
      }

      public void SavePanelsToLibrary()
      {
         // Сохранить блоки панелей в файл библиотеки блоков панелей.
         // Куда сохранять? - [CAD_Settings\AutoCAD_server\ShareSettings\]АР\AlbumPanelColorTiles\AKR_Panels.dwg
         // Если такой блок уже есть в бибилиотеке? - старому блоку изменить имя с приставкой сегодняшней даты - [АКР_Панель_МаркаСБ]_25.10.2015-14:15
         // Если файл занят другим процессом? - подождать 3 секунды и повторить.

         // Файл библиотеки блоков панелей.
         if (!File.Exists(LibPanelsFilePath))
         {
            Logger.Log.Error("Нет файла библиотеки панелей {0}", LibPanelsFilePath);
            return;
         }
         // сбор блоков для сохранения
         List<PanelAkrFacade> panelsAkrInFacade = GetPanelsAkrInDb(_dbCur);
         removeElectricPanels(panelsAkrInFacade);
         if (panelsAkrInFacade.Count == 0)
         {
            _doc.Editor.WriteMessage("\nНет блоков АКР-Панелей для сохранения в библиотеку.");
            return;
         }

         string msgReport;
         // Сохранение панелей в библиотеку
         savePanelsAkrToLibDb(panelsAkrInFacade, out msgReport);

         if (!string.IsNullOrEmpty(msgReport))
         {
            _doc.Editor.WriteMessage(string.Format("\n{0}", msgReport));
            sendReport(msgReport);
         }
      }

      private static string getBackupPanelsLibFile(string libPanelsFilePath)
      {
         string suffix = DateTime.Now.ToString("dd.MM.yyyy-HH.mm");
         string newFile = Path.Combine(Path.GetDirectoryName(libPanelsFilePath), 
             $"{Path.GetFileNameWithoutExtension(libPanelsFilePath)}_{suffix}.dwg");
         return newFile;
      }

      private void removeElectricPanels(List<PanelAkrFacade> panelsAkrFacade)
      {
         List<PanelAkrFacade> removes = new List<PanelAkrFacade>();
         foreach (var panel in panelsAkrFacade)
         {
            var markNoElec = AkrHelper.GetMarkWithoutElectric(panel.BlName);
            if (panel.BlName.Length != markNoElec.Length)
            {
               removes.Add(panel);
               _doc.Editor.WriteMessage("\n{0} панель с индексом электрики проигнорирована.".f(panel.BlName));
            }
         }
         removes.ForEach(r => panelsAkrFacade.Remove(r));
      }

      private void backupChangedPanels(List<PanelAkrFacade> panelsToSave, List<PanelAKR> panelsAkrInLib, Database dbLib)
      {
         // сохранение изменяемых панель в файл
         // создание новоq базы и копирование туда блоков изменяемемых панелей (до изменения)

         ObjectIdCollection idsBtrToCopy = new ObjectIdCollection();
         foreach (var panelFacadeTosave in panelsToSave)
         {
            PanelAKR panelLib = panelsAkrInLib.Find(
               p => string.Equals(p.BlName, panelFacadeTosave.BlName, StringComparison.OrdinalIgnoreCase));
            if (panelLib != null)
            {
               idsBtrToCopy.Add(panelLib.IdBtrAkrPanel);
            }
         }
         if (idsBtrToCopy.Count > 0)
         {
            string newFile = getBackupPanelsLibFile(LibPanelsFilePath);
            using (var dbBak = new Database(true, true))
            {
               dbBak.CloseInput(true);
               using (IdMapping map = new IdMapping())
               {
                  dbLib.WblockCloneObjects(idsBtrToCopy, dbBak.BlockTableId, map, DuplicateRecordCloning.Replace, false);
                  dbBak.SaveAs(newFile, DwgVersion.Current);
               }
            }
         }
      }

      public static void BackUpLibPanelsFile()
      {
         string newFile = getBackupPanelsLibFile(LibPanelsFilePath);
         File.Copy(LibPanelsFilePath, newFile, true);
      }

      // Копирование новых панелей
      private void copyNewPanels(Database dbLib, List<PanelAkrFacade> panelsAkrToCopy)
      {
         var ids = new ObjectIdCollection(panelsAkrToCopy.Select(p => p.IdBtrAkrPanel).ToArray());
         IdMapping iMap = new IdMapping();
         dbLib.WblockCloneObjects(ids, dbLib.BlockTableId, iMap, DuplicateRecordCloning.Replace, false);
      }

      private string getReport(List<PanelAkrFacade> panels)
      {
         StringBuilder msg = new StringBuilder();
         msg.AppendLine(string.Format("Обновлены/добавлены следующие панели, от пользователя {0}:", Environment.UserName));
         foreach (var panel in panels)
         {
            msg.AppendLine(string.Format("{0} - {1}", panel.BlName, panel.ReportStatusString()));
         }
         return msg.ToString();
      }

      private void savePanelsAkrToLibDb(List<PanelAkrFacade> panelsAkrInFacade, out string msgReport)
      {
         msgReport = string.Empty;
         using (var dbLib = new Database(false, true))
         {            
            try
            {
               dbLib.ReadDwgFile(LibPanelsFilePath, FileShare.Read, false, "");
               dbLib.CloseInput(true);
            }
            catch (Exception ex)
            {
               // Кто-то уже выполняет сохранение панелей в библиотеку. Сообщить кто занял библиотеку и попросить повторить позже.
               WarningBusyLibrary(ex);
               return;
            }
            dbLib.CloseInput(true);
            // список панелей в библиотеке
            List<PanelAKR> panelsAkrInLib = PanelAKR.GetAkrPanelLib(dbLib, false); //GetPanelsAkrInDb(dbLib); //GetPanelsInLib();
            // Список изменившихся панелей и новых для записи в базу.
            List<PanelAkrFacade> panelsAkrToSave = PanelAkrFacade.GetChangedAndNewPanels(panelsAkrInFacade, panelsAkrInLib);

            // Форма для просмотра и управления списков сохранения панелей
            FormSavePanelsToLib formSave = new FormSavePanelsToLib(
               panelsAkrToSave.Where(p => p.ReportStatus == EnumReportStatus.New).ToList(),
               panelsAkrToSave.Where(p => p.ReportStatus == EnumReportStatus.Changed).ToList(),
               panelsAkrInFacade.Where(p => p.ReportStatus == EnumReportStatus.Other).ToList());
            if (Autodesk.AutoCAD.ApplicationServices.Application.ShowModalDialog(formSave) != System.Windows.Forms.DialogResult.OK)
            {
               return;
            }

            if (formSave.PanelsToSave.Count > 0)
            {
               //// копия текущего файла библиотеки панелей с приставкой сегодняшней даты
               //copyLibPanelFile(LibPanelsFilePath);
               // файл с панелями до изменений - сохранить.
               backupChangedPanels(formSave.PanelsToSave, panelsAkrInLib, dbLib);
               // Копирование новых панелей
               copyNewPanels(dbLib, formSave.PanelsToSave);
               // Текст изменений.
               //textChangesToLibDwg(panelsAkrToCopy, dbLib, t);
               // Сохранение файла библиотеки панелей
               dbLib.SaveAs(dbLib.Filename, DwgVersion.Current);
               // строка отчета
               msgReport = getReport(formSave.PanelsToSave);
               Logger.Log.Info("Обновлена библиотека панелей.");
               SaveChangesToExel.Save(formSave.PanelsToSave);
            }
            else
            {
               throw new Exception("\nНет панелей для сохранения в библиотеку (в текущем чертеже нет новых и изменившихся панелей).");
            }
         }
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
            Logger.Log.Error(ex, "sendReport MailMessage");
         }
      }

      //private void textChangesToLibDwg(List<PanelAKR> panelsAkrToCopy, Database db, Transaction t)
      //{
      //   // Запись изменений в текстовый объект
      //   var bt = t.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
      //   var ms = t.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
      //   foreach (ObjectId idEnt in ms)
      //   {
      //      if (idEnt.ObjectClass.Name == "AcDbMText")
      //      {
      //         var textOld = idEnt.GetObject(OpenMode.ForWrite) as MText;
      //         textOld.Erase();
      //         break;
      //      }
      //   }
      //   MText text = new MText();
      //   text.SetDatabaseDefaults(db);

      //   StringBuilder sbChanges = new StringBuilder();
      //   // Заголовок Изменений
      //   sbChanges.AppendLine(string.Format("Последнее изменение от {0}. Дата {1}. Чертеж {2}", Environment.UserName, DateTime.Now, _dbCur.Filename));
      //   // Список новых панелей
      //   var newPanels = panelsAkrToCopy.Where(p => p.ReportStatus ==   EnumReportStatus.New);
      //   if (newPanels.Count() > 0)
      //   {
      //      sbChanges.AppendLine("Список новых панелей:");
      //      foreach (var item in newPanels)
      //      {
      //         sbChanges.AppendLine(item.BlName);
      //      }
      //   }
      //   var changedPanels = panelsAkrToCopy.Where(p => p.ReportStatus ==  EnumReportStatus.Changed);
      //   if (changedPanels.Count() > 0)
      //   {
      //      sbChanges.AppendLine("Список изменившихся панелей:");
      //      foreach (var item in changedPanels)
      //      {
      //         sbChanges.AppendLine(item.BlName);
      //      }
      //   }

      //   text.Contents = sbChanges.ToString();
      //   ms.AppendEntity(text);
      //   t.AddNewlyCreatedDBObject(text, true);
      //}
   }
}