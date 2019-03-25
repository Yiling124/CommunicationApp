using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public enum ShapeType { Rectangle, UsingConnector, ConnectorDown };

namespace DragAndDrop
{
    public class UMLShape
    {
        public ShapeType ShpType;
        public Double Top;
        public Double Left;

        public UMLShape() {}

        public UMLShape(ShapeType shapeType,  Double top, Double left) {
            this.ShpType = shapeType;
            this.Top = top;
            this.Left = left;
        }
    }
}
