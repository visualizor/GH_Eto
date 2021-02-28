using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Eto.Forms;
using Eto.Drawing;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel.Data;

namespace Synapse
{
    internal class Util
    {

        public static void SetProp(object subject, string pname, object pval)
        {
            PropertyInfo prop = subject.GetType().GetProperty(pname);
            prop.SetValue(subject, pval);
        }

        public static object GetGooVal(object goo)
        {
            if (goo is GH_Point p)
                return new Point((int)p.Value.X, (int)p.Value.Y);
            else if (goo is GH_Vector v)
                return new Point((int)v.Value.X, (int)v.Value.Y);
            else if (goo is GH_String s)
                return s.Value;
            else if (goo is GH_Number n)
                return n.Value;
            else if (goo is GH_Integer i)
                return i.Value;
            else if (goo is GH_Colour clr)
                return Color.FromArgb(clr.Value.ToArgb());
            else if (goo is GH_Boolean b)
                return b.Value;
            else if (goo is GH_Rectangle rec)
                return new Size((int)rec.Value.X.Length, (int)rec.Value.Y.Length);
            else
                return goo;
        }
    }

    internal class ComboSlider : StackLayout
    {
        public Slider slider;
        public Label label;

        public ComboSlider()
        {
            slider = new Slider()
            {
                MinValue = 1,
                MaxValue = 10,
                Value = 5,
            };
            label = new Label() { Text = slider.Value.ToString(), };
            slider.ValueChanged += OnValChange;
        }
        public ComboSlider(int min, int max)
        {
            slider = new Slider()
            {
                MinValue = min,
                MaxValue = max,
                Value = (min + max) / 2,
            };
            label = new Label() { Text = slider.Value.ToString(), };
            slider.ValueChanged += OnValChange;
        }

        /// <summary>
        /// must be called to initialize comboslider
        /// </summary>
        public void Init()
        {
            Orientation = Orientation.Horizontal;
            Items.Add(slider);
            Items.Add(label);
        }

        protected void OnValChange(object s, EventArgs e)
        {
            label.Text = slider.Value.ToString();
        }
    }
}
