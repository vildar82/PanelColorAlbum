﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан программой.
//     Исполняемая версия:4.0.30319.42000
//
//     Изменения в этом файле могут привести к неправильной работе и будут потеряны в случае
//     повторной генерации кода.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Xml.Serialization;

// 
// Этот исходный код был создан с помощью xsd, версия=4.6.1055.0.
// 

namespace AlbumPanelColorTiles.Model.Base
{
   /// <remarks/>
   [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
   [System.SerializableAttribute()]
   [System.Diagnostics.DebuggerStepThroughAttribute()]
   [System.ComponentModel.DesignerCategoryAttribute("code")]
   [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
   [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
   public partial class Panels
   {

      private Panel[] panelField;

      /// <remarks/>
      [System.Xml.Serialization.XmlElementAttribute("Panel", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
      public Panel[] Panel
      {
         get
         {
            return this.panelField;
         }
         set
         {
            this.panelField = value;
         }
      }
   }

   /// <remarks/>
   [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
   [System.SerializableAttribute()]
   [System.Diagnostics.DebuggerStepThroughAttribute()]
   [System.ComponentModel.DesignerCategoryAttribute("code")]
   [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
   public partial class Panel
   {

      private Gab gabField;

      private Windows windowsField;

      private string markField;

      /// <remarks/>
      [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
      public Gab gab
      {
         get
         {
            return this.gabField;
         }
         set
         {
            this.gabField = value;
         }
      }

      /// <remarks/>
      [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
      public Windows windows
      {
         get
         {
            return this.windowsField;
         }
         set
         {
            this.windowsField = value;
         }
      }

      /// <remarks/>
      [System.Xml.Serialization.XmlAttributeAttribute()]
      public string mark
      {
         get
         {
            return this.markField;
         }
         set
         {
            this.markField = value;
         }
      }      
   }

   /// <remarks/>
   [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
   [System.SerializableAttribute()]
   [System.Diagnostics.DebuggerStepThroughAttribute()]
   [System.ComponentModel.DesignerCategoryAttribute("code")]
   [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
   public partial class Gab
   {

      private double lengthField;

      private double heightField;

      /// <remarks/>
      [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
      public double length
      {
         get
         {
            return this.lengthField;
         }
         set
         {
            this.lengthField = value;
         }
      }

      /// <remarks/>
      [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
      public double height
      {
         get
         {
            return this.heightField;
         }
         set
         {
            this.heightField = value;
         }
      }
   }

   /// <remarks/>
   [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
   [System.SerializableAttribute()]
   [System.Diagnostics.DebuggerStepThroughAttribute()]
   [System.ComponentModel.DesignerCategoryAttribute("code")]
   [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
   public partial class Windows
   {

      private Window[] windowField;

      /// <remarks/>
      [System.Xml.Serialization.XmlElementAttribute("window", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
      public Window[] window
      {
         get
         {
            return this.windowField;
         }
         set
         {
            this.windowField = value;
         }
      }
   }

   /// <remarks/>
   [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
   [System.SerializableAttribute()]
   [System.Diagnostics.DebuggerStepThroughAttribute()]
   [System.ComponentModel.DesignerCategoryAttribute("code")]
   [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
   public partial class Window
   {

      private double widthField;

      private double heightField;

      private Posi posiField;

      /// <remarks/>
      [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
      public double width
      {
         get
         {
            return this.widthField;
         }
         set
         {
            this.widthField = value;
         }
      }

      /// <remarks/>
      [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
      public double height
      {
         get
         {
            return this.heightField;
         }
         set
         {
            this.heightField = value;
         }
      }

      /// <remarks/>
      [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
      public Posi posi
      {
         get
         {
            return this.posiField;
         }
         set
         {
            this.posiField = value;
         }
      }
   }

   /// <remarks/>
   [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
   [System.SerializableAttribute()]
   [System.Diagnostics.DebuggerStepThroughAttribute()]
   [System.ComponentModel.DesignerCategoryAttribute("code")]
   [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
   public partial class Posi
   {

      private double xField;

      private double yField;

      private double zField;

      /// <remarks/>
      [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
      public double X
      {
         get
         {
            return this.xField;
         }
         set
         {
            this.xField = value;
         }
      }

      /// <remarks/>
      [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
      public double Y
      {
         get
         {
            return this.yField;
         }
         set
         {
            this.yField = value;
         }
      }

      /// <remarks/>
      [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
      public double Z
      {
         get
         {
            return this.zField;
         }
         set
         {
            this.zField = value;
         }
      }
   }
}
