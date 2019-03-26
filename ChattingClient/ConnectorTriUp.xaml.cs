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
    /// Interaction logic for ConnectorTriUp.xaml
    /// </summary>
    public partial class ConnectorTriUp : UserControl
    {
        public double left { get; set; }
        public double top { get; set; }

        public ConnectorTriUp()
        {
            InitializeComponent();
        }
        public ConnectorTriUp(ConnectorTriUp cd)
        {
            InitializeComponent();
            this.classCanvas.Height = cd.classCanvas.Height;
            this.classCanvas.Width = cd.classCanvas.Width;
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            //MessageBox.Show("Triggered");
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
