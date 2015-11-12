﻿using System;
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
      public static readonly string LibPanelsFilePath = Path.Combine(AutoCAD_PIK_Manager.Settings.PikSettings.ServerShareSettingsFolder, @"АР\AlbumPanelColorTiles\AKR_Panels.dwg");      

      /// <summary>
      /// Проверка есть ли в текущем чертеже фасада новые панели, которых нет в библиотеке
      /// </summary>
      public static void CheckNewPanels()
      {
         var doc = Application.DocumentManager.MdiActiveDocument;
         var ed = doc.Editor;

         // список панелей (АКР-Панели марки СБ - без марки покраски) в текущем чертеже
         var panelsAkrInFacade = GetPanelsAkrCurrentDb();
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
            ed.WriteMessage("\nБлоки панелей которых нет в библиотеке:");
            foreach (var panel in panelsNotInLib)
            {
               ed.WriteMessage("\n{0}", panel);
            }
            ed.WriteMessage("\nРекомендуется сохранить их в библиотеку - на палитре есть кнопка для сохранения панелей в библиотеку. Спасибо!");
            Log.Error("Есть новые панели, которых нет в библиотеке: {0}", string.Join("; ", panelsNotInLib));
         }
      }

      public static List<PanelAKR> GetPanelsAkrCurrentDb()
      {
         List<PanelAKR> panels = new List<PanelAKR>();
         Database db = HostApplicationServices.WorkingDatabase;
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
                     panels.Add(new PanelAKR (idBtr, btr.Name));
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
         List<PanelAKR> panelsAkrInFacade = GetPanelsAkrCurrentDb();

         // Открываем и блокируем от изменений файл библиотеки блоков
         using (var libDwg = new Database(false, true))
         {
            libDwg.ReadDwgFile(LibPanelsFilePath, FileShare.ReadWrite, true, "");
            // список панелей в библиотеке
            List<PanelAKR> panelsAkrInLib = GetPanelsInLib();
            // Список изменившихся панелей и новых для записи в базу.
            List<PanelAKR> panelsAkrToSave = PanelAKR.GetChangedAndNewPanels(panelsAkrInFacade, panelsAkrInLib);
            // копия текущего файла библиотеки панелей с приставкой сегодняшней даты
            copyLibPanelFile(LibPanelsFilePath);
            // Копирование новых панелей
            copyNewPanels(libDwg, panelsAkrToSave);
            // Запись изменений в файл библиотеки
            //TextChangesToLibDwg();
            // Сохранение файла библиотеки панелей
            libDwg.SaveAs(LibPanelsFilePath, DwgVersion.Current);
            // отправка отчета
            sendReport(panelsAkrToSave);
            // лог
            Log.Info("Обновлена библиотека панелей.");
         }
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
         var ids = new ObjectIdCollection(panelsAkrToCopy.Select(p=>p.IdBtrAkrPanelInLib).ToArray());
         using (var t = dbLib.TransactionManager.StartTransaction())
         {
            IdMapping iMap = new IdMapping();
            dbLib.WblockCloneObjects(ids, dbLib.BlockTableId, iMap, DuplicateRecordCloning.Replace, false);
            t.Commit();
         }
      }

      private void sendReport(List<PanelAKR> panels)
      {
         StringBuilder msg = new StringBuilder();
         msg.AppendLine(string.Format("Обновлены/добавлены следующие панели, от пользователя {0}:", Environment.UserName));
         foreach (var panel in panels)
         {
            msg.AppendLine(string.Format("{0} - {1}", panel.BlNameInLib, panel.ReportStatus));
         }
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
   }
}