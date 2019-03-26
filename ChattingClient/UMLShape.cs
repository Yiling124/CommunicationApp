using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public enum ShapeType { Rectangle, UsingConnector, ConnectorDown, ConnectorLeft, ConnectorUp, ConnectorTriDown, ConnectorTriUp, ConnectorTriLeft, ConnectorDiaRight, ConnectorDiaUp, ConnectorDiaDown };

namespace DragAndDrop
{
    public class UMLShape
    {
        public ShapeType ShpType;
        public Double Top;
        public Double Left;
        public string ClassName;

        public UMLShape() {}

        public UMLShape(ShapeType shapeType,  Double top, Double left) {
            this.ShpType = shapeType;
            this.Top = top;
            this.Left = left;
            this.ClassName = "";
        }

        public UMLShape(ShapeType shapeType, string className, Double top, Double left)
        {
            this.ShpType = shapeType;
            this.Top = top;
            this.Left = left;
            this.ClassName = className;
        }

        public void setClassName(string name) {
            this.ClassName = name;
        }
    }
}
