using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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
using ChattingInterfaces;
using ChattingClient;

namespace DragAndDrop
{
    /// <summary>
    /// Interaction logic for Circle.xaml
    /// </summary>
    public partial class Rectangle : UserControl
    {
        public Brush _previousFill = null;
        public double left { get; set; }
        public double top { get; set; }

        public Rectangle()
        {
            InitializeComponent();
        }
        public Rectangle(Rectangle r)
        {
            InitializeComponent();
            this.rectangleUI.Height = r.rectangleUI.Height;
            this.rectangleUI.Width = r.rectangleUI.Width;
            this.rectangleUI.Fill = r.rectangleUI.Fill;
            this.className.Text = r.className.Text;
        }
        public Rectangle(Rectangle r, string text)
        {
            InitializeComponent();
            this.rectangleUI.Height = r.rectangleUI.Height;
            this.rectangleUI.Width = r.rectangleUI.Width;
            this.rectangleUI.Fill = r.rectangleUI.Fill;
            this.className.Text = text;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                // Package the data.
                DataObject data = new DataObject();
               // data.SetData(DataFormats.StringFormat, rectangleUI.Fill.ToString());
                data.SetData("Double", rectangleUI.Height);
                data.SetData("Object", this);
                data.SetData(DataFormats.StringFormat, Stopwatch.GetTimestamp().ToString());
                // Inititate the drag-and-drop operation.
                DragDrop.DoDragDrop(this, data, DragDropEffects.Copy | DragDropEffects.Move);
            }
        }
        protected override void OnGiveFeedback(GiveFeedbackEventArgs e)
        {
            base.OnGiveFeedback(e);
            // These Effects values are set in the drop target's
            // DragOver event handler.
            if (e.Effects.HasFlag(DragDropEffects.Copy))
            {
                Mouse.SetCursor(Cursors.Cross);
            }
            else if (e.Effects.HasFlag(DragDropEffects.Move))
            {
                Mouse.SetCursor(Cursors.Pen);
            }
            else
            {
                Mouse.SetCursor(Cursors.No);
            }
            e.Handled = true;
        }

        protected override void OnDragOver(DragEventArgs e)
        {
            base.OnDragOver(e);
            e.Effects = DragDropEffects.None;

            // If the DataObject contains string data, extract it.
            if (e.Data.GetDataPresent(DataFormats.StringFormat))
            {
                string dataString = (string)e.Data.GetData(DataFormats.StringFormat);

                // If the string can be converted into a Brush, allow copying or moving.
                BrushConverter converter = new BrushConverter();
                if (converter.IsValid(dataString))
                {
                    // Set Effects to notify the drag source what effect
                    // the drag-and-drop operation will have. These values are 
                    // used by the drag source's GiveFeedback event handler.
                    // (Copy if CTRL is pressed; otherwise, move.)
                    if (e.KeyStates.HasFlag(DragDropKeyStates.ControlKey))
                    {
                        e.Effects = DragDropEffects.Copy;
                    }
                    else
                    {
                        e.Effects = DragDropEffects.Move;
                    }
                }
            }
            e.Handled = true;
        }

        protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseRightButtonDown(e);
            // className.Text = "Class1";
        }
    }
}
