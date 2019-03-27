/////////////////////////////////////////////////////////////////////////////                                     //
//  Language:     C#                                                       //
//  Author:       YiLing Jiang                                             //
/////////////////////////////////////////////////////////////////////////////
/*
 *   This package implements the User Control for the UML Diagram 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DragAndDrop
{
    /// <summary>
    /// Interaction logic for ConnectorDiaLeft.xaml
    /// </summary>
    public partial class ConnectorDiaLeft : UserControl
    {
        public double left { get; set; }
        public double top { get; set; }

        public ConnectorDiaLeft()
        {
            InitializeComponent();
        }
        public ConnectorDiaLeft(ConnectorDiaLeft u)
        {
            InitializeComponent();
            this.classCanvas.Height = u.classCanvas.Height;
            this.classCanvas.Width = u.classCanvas.Width;
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                // Package the data.
                DataObject data = new DataObject();
                //data.SetData(DataFormats.StringFormat, classCanvas.Width.ToString());
                //data.SetData("Double", classCanvas.Height);
                data.SetData("Object", this);

                // Inititate the drag-and-drop operation.
                DragDrop.DoDragDrop(this, data, DragDropEffects.Copy | DragDropEffects.Move);
            }
        }
    }
}
