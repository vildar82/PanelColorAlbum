using Autodesk.AutoCAD.DatabaseServices;

namespace AlbumPanelColorTiles.Checks
{
   // Объект с ошибками
   public class ErrorObject
   {
      #region Private Fields

      private string _errorMsg;
      private ObjectId _idEnt;

      #endregion Private Fields

      #region Public Constructors

      /// <summary>
      ///
      /// </summary>
      /// <param name="errMsg">Сообщение об ошибке. Для показа пользователю</param>
      /// <param name="idEnt">Если не null, то должен быть примитивом чертежа (для показа пользователю)</param>
      public ErrorObject(string errMsg, ObjectId idEnt)
      {
         _idEnt = idEnt;
         _errorMsg = errMsg;
      }

      #endregion Public Constructors
   }
}