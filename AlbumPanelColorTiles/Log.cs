using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlbumPanelColorTiles
{
   public static class Log
   {
      #region Private Fields

      private static string _appmsg = "Plugin AlbumPanelColorTiles ";

      #endregion Private Fields

      #region Public Methods

      /// <summary>
      /// Debug: сообщения отладки, профилирования. В production системе обычно сообщения этого уровня включаются при первоначальном запуске системы или для поиска узких мест (bottleneck-ов).
      /// </summary>
      /// <param name="message"></param>
      public static void Debug(string message)
      {
         AutoCAD_PIK_Manager.Log.Debug(_appmsg + message);
      }

      public static void Debug(string message, params object[] args)
      {
         AutoCAD_PIK_Manager.Log.Debug(_appmsg + message, args);
      }

      public static void Debug(Exception ex, string message, params object[] args)
      {
         AutoCAD_PIK_Manager.Log.Debug(ex, _appmsg + message, args);
      }

      /// <summary>
      /// Error: ошибка в работе системы, требующая вмешательства. Что-то не сохранилось, что-то отвалилось. Необходимо принимать меры довольно быстро! Ошибки этого уровня и выше требуют немедленной записи в лог, чтобы ускорить реакцию на них.Нужно понимать, что ошибка пользователя – это не ошибка системы. Если пользователь ввёл в поле -1, где это не предполагалось – не надо писать об этом в лог ошибок.
      /// </summary>
      /// <param name="message"></param>
      public static void Error(string message)
      {
         AutoCAD_PIK_Manager.Log.Error(_appmsg + message);
      }

      public static void Error(Exception ex, string message, params object[] args)
      {
         AutoCAD_PIK_Manager.Log.Error(ex, _appmsg + message, args);
      }

      public static void Error(string message, params object[] args)
      {
         AutoCAD_PIK_Manager.Log.Error(_appmsg + message, args);
      }

      /// <summary>
      /// Fatal: это особый класс ошибок. Такие ошибки приводят к неработоспособности системы в целом, или неработоспособности одной из подсистем.Чаще всего случаются фатальные ошибки из-за неверной конфигурации или отказов оборудования. Требуют срочной, немедленной реакции. Возможно, следует предусмотреть уведомление о таких ошибках по SMS.
      /// </summary>
      /// <param name="message"></param>
      public static void Fatal(string message)
      {
         AutoCAD_PIK_Manager.Log.Fatal(_appmsg + message);
      }

      public static void Fatal(Exception ex, string message, params object[] args)
      {
         AutoCAD_PIK_Manager.Log.Fatal(ex, _appmsg + message, args);
      }

      public static void Fatal(string message, params object[] args)
      {
         AutoCAD_PIK_Manager.Log.Fatal(_appmsg + message, args);
      }

      /// <summary>
      /// Info: обычные сообщения, информирующие о действиях системы.Реагировать на такие сообщения вообще не надо, но они могут помочь, например, при поиске багов, расследовании интересных ситуаций итд.
      /// </summary>
      /// <param name="message"></param>
      public static void Info(string message)
      {
         AutoCAD_PIK_Manager.Log.Info(_appmsg + message);
      }

      public static void Info(string message, params object[] args)
      {
         AutoCAD_PIK_Manager.Log.Info(_appmsg + message, args);
      }

      public static void Info(Exception ex, string message, params object[] args)
      {
         AutoCAD_PIK_Manager.Log.Info(ex, _appmsg + message, args);
      }

      /// <summary>
      /// Warn: записывая такое сообщение, система пытается привлечь внимание обслуживающего персонала.Произошло что-то странное. Возможно, это новый тип ситуации, ещё не известный системе. Следует разобраться в том, что произошло, что это означает, и отнести ситуацию либо к инфо-сообщению, либо к ошибке.Соответственно, придётся доработать код обработки таких ситуаций.
      /// </summary>
      /// <param name="message"></param>
      public static void Warn(string message)
      {
         AutoCAD_PIK_Manager.Log.Warn(_appmsg + message);
      }

      public static void Warn(string message, params object[] args)
      {
         AutoCAD_PIK_Manager.Log.Warn(_appmsg + message, args);
      }

      public static void Warn(Exception ex, string message, params object[] args)
      {
         AutoCAD_PIK_Manager.Log.Warn(ex, _appmsg + message, args);
      }

      #endregion Public Methods
   }
}