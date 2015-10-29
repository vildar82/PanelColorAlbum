using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;

namespace AlbumPanelColorTiles.PanelLibrary
{
   // Библиотека панелей покраски.
   // DWG файл
   public class PanelLibrarySaveService
   {
      private Album _album;

      public PanelLibrarySaveService(Album album)
      {
         _album = album;
      }

      public void SavePanelsToLibrary()
      {
         // Сохранить блоки панелей в файл библиотеки блоков панелей.
         // Куда сохранять? - [CAD_Settings\AutoCAD_server\ShareSettings\]АР\AlbumPanelColorTiles\AKR_Panels.dwg
         // Если такой блок уже есть в бибилиотеке? - старому блоку изменить имя с приставкой сегодняшней даты - [АКР_Панель_МаркаСБ]_25.10.2015-14:15
         // Если файл занят другим процессом? - подождать 3 секунды и повторить.

         // Файл библиотеки блоков панелей.
         string libPanelsFilePath = Path.Combine(AutoCAD_PIK_Manager.Settings.PikSettings.ServerShareSettingsFolder, @"АР\AlbumPanelColorTiles\AKR_Panels.dwg");
         if (!File.Exists(libPanelsFilePath))
         {
            Log.Error("Нет файла библиотеки панелей {0}", libPanelsFilePath);
            return;
         }
         // Открываем и блокируем от изменений файл библиотеки блоков
         using (var libDwg = new Database(false, true))
         {
            libDwg.ReadDwgFile(libPanelsFilePath, FileShare.ReadWrite, true, "");
            // копия текущего файла библиотеки панелей с приставкой сегодняшней даты
            copyLibPanelFile(libPanelsFilePath);
            // Копирование новых панелей
            copyNewPanels(libDwg);
            // Сохранение файла библиотеки панелей
            libDwg.SaveAs(libPanelsFilePath, DwgVersion.Current);
            // отправка отчета
            sendReport();
            // лог
            Log.Info("Обновлена библиотека панелей.");
         }
      }

      private void sendReport()
      {
         StringBuilder msg = new StringBuilder();
         msg.AppendLine(string.Format("Обновлены/добавлены следующие панели, от пользователя {0}:", Environment.UserName));
         foreach (var markSb in _album.MarksSB)
         {
            msg.AppendLine(markSb.MarkSbBlockName);
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

      private void copyLibPanelFile(string libPanelsFilePath)
      {
         string suffix = string.Format("{0}", DateTime.Now.ToString("dd.MM.yyyy-HH.mm"));
         string newFile = Path.Combine(
            Path.GetDirectoryName(libPanelsFilePath), string.Format("{0}_{1}.{2}", 
            Path.GetFileNameWithoutExtension(libPanelsFilePath), suffix, "dwg"));
         File.Copy(libPanelsFilePath, newFile, true);
      }

      // Копирование новых панелей
      private void copyNewPanels(Database dbLib)
      {
         var ids = new ObjectIdCollection();
         foreach (var markSb in _album.MarksSB)
         {
            ids.Add(markSb.IdBtr);
         }
         using (var t = dbLib.TransactionManager.StartTransaction())
         {            
            IdMapping iMap = new IdMapping();
            dbLib.WblockCloneObjects(ids, dbLib.BlockTableId, iMap, DuplicateRecordCloning.Replace, false);            
            t.Commit();
         }
      }      
   }
}
