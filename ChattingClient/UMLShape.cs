using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public enum ShapeType { Rectangle, UsingConnector };

namespace DragAndDrop
{
    public class UMLShape
    {
        public ShapeType ShpType;
        public long ID; 
        public string Color;
        public Double Top;
        public Double Left;

        public UMLShape() {
        }

        public UMLShape(ShapeType shapeType, long id, string color, Double top, Double left) {
            this.ShpType = shapeType;
            this.ID = id;
            this.Color = color;
            this.Top = top;
            this.Left = left;
        }
    }
}
