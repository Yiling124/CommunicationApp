using DragAndDrop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DragAndDrop
{
    public class CanvasContainer
    {
        [XmlElement(typeof(Rectangle))]
        [XmlElement(typeof(UsingConnector))]
        public List<UMLShape> ShapeList { get; set; }

        public CanvasContainer()
        {
            this.ShapeList = new List<UMLShape>();
        }

        public UMLShape AddShape(UMLShape newShape)
        {
            this.ShapeList.Add(newShape);
            return newShape;
        }

        //public UMLShape UpdateShape(UMLShape updatedShape)
        //{
            
        //    int index = this.ShapeList.FindIndex(elem => elem.ID == updatedShape.ID);
        //    this.ShapeList[index] = updatedShape;
        //    return updatedShape;
        //}
    }
}