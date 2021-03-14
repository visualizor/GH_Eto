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
            else if (goo is GH_ObjectWrapper gwrapper)
                return gwrapper.Value;
            else if (goo is GH_Line gl)
                return gl.Value.Length;
            else if (goo is GH_Interval gitvl)
                return gitvl.Value.Length;
            else
                return goo;
        }
    }

    internal class ComboSlider : StackLayout
    {
        public Slider slider;
        public Label label;
        public double coef=1.0;
        public double val;

        public ComboSlider()
        {
            slider = new Slider()
            {
                MinValue = 1,
                MaxValue = 10,
                Value = 5,
            };
            label = new Label();
            val = slider.Value * coef;
            label.Text = val.ToString();
            slider.ValueChanged += OnSlide;

            Orientation = Orientation.Horizontal;
            Items.Add(slider);
            Items.Add(label);
            Spacing = 2;
        }

        protected void OnSlide(object s, EventArgs e)
        {
            val = slider.Value * coef;
            label.Text = val.ToString();
        }

        public void SetMin(double n)
        {
            double m = n / coef;
            slider.MinValue = (int)m;
            slider.Value = (slider.MinValue + slider.MaxValue) / 2;
        }

        public void SetMax(double n)
        {
            double M = n / coef;
            slider.MaxValue = (int)M;
            slider.Value = (slider.MinValue + slider.MaxValue) / 2;
        }
    }

    internal class PieData
    {
        protected double[] pcts;
        protected Color[] clrs;

        public string[] Keys { get; set; }
        public double[] Percentages
        {
            get { return pcts; }
            set
            {
                if (value.Length == Keys.Length)
                {
                    foreach (double p in value)
                        if (p <= 0 || p >= 1)
                            throw new ArrayTypeMismatchException(" items in pie data must be percentage numbers (0 < n < 1)");
                    pcts = value;
                } 
            }
        }
        public Color[] Colors
        {
            get { return clrs; }
            set
            {
                if (value.Length == Keys.Length)
                    clrs = value;
            }
        }

        /// <summary>
        /// constructor, don't forget to set properties of values of colors
        /// </summary>
        /// <param name="k">keys</param>
        public PieData(IEnumerable<string> k)
        {
            Keys = k.ToArray();
        }

        public override string ToString()
        {
            return string.Format("Pie legend data ({0} items)", Keys.Length);
        }
    }

    internal class ChartLegend
    {
        protected Dictionary<string, string> Markers = new Dictionary<string, string>()
        {
            {"bullet", @"●" },
            {"box",  @"■"},
            {"pointer", @"◄" },
        };
        protected string marker = "bullet";

        public string[] Keys { get; private set; }
        public string[] Appd { get; private set; }
        public Color[] Colors { get; private set; }
        public Font TxtFont { get; private set; } = null;

        /// <summary>
        /// direction in which notation items are stacked
        /// </summary>
        public bool Horizontal { get; set; } = false;
        /// <summary>
        /// show additional information
        /// usually the percentage of the data point
        /// </summary>
        public bool ShowAppd { get; set; } = true;
        /// <summary>
        /// spacing between sub-objects of the legend
        /// </summary>
        public int Gap { get; set; } = 5;
        /// <summary>
        /// padding
        /// </summary>
        public int Frame { get; set; } = 2;
        /// <summary>
        /// marker type
        /// </summary>
        public string Marker
        {
            get { return marker; }
            set
            {
                if (Markers.Keys.Contains(value))
                    marker = value;
            }
        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="k">item keys</param>
        /// <param name="c">color used to represent data item</param>
        /// <param name="a">additional information</param>
        public ChartLegend(IEnumerable<string> k, IEnumerable<Color> c, IEnumerable<string> a)
        {
            Keys = k.ToArray();
            Colors = c.ToArray();
            Appd = a.ToArray();
        }

        /// <summary>
        /// set font
        /// </summary>
        /// <param name="f">target font to use</param>
        public void SetFont(Font f)
        {
            TxtFont = f;
        }
        /// <summary>
        /// set font with parameters
        /// </summary>
        /// <param name="name">font family name</param>
        /// <param name="size">font size</param>
        /// <param name="style">font style</param>
        /// <param name="deco">font decoration</param>
        public void SetFont(string name, double size, FontStyle style = FontStyle.None, FontDecoration deco = FontDecoration.None)
        {
            Font f = new Font(name, (float)size, style, deco);
            TxtFont = f;
        }
        /// <summary>
        /// reset font to default
        /// </summary>
        public void SetFont()
        {
            TxtFont = null;
        }

        /// <summary>
        /// render the legend object
        /// </summary>
        /// <returns>legend object as a stack</returns>
        public StackLayout RenderLegend()
        {
            StackLayout legends = new StackLayout()
            {
                Orientation = Horizontal ? Orientation.Horizontal : Orientation.Vertical,
                Spacing = Gap,
                Padding = Frame,
            };

            StackLayout[] lineitems = new StackLayout[Keys.Length];
            for (int ki = 0; ki<Keys.Length; ki++)
            {
                StackLayout lineitem = new StackLayout() { Orientation = Orientation.Horizontal, };
                Label mark = new Label()
                {
                    Text = Markers[Marker],
                    TextColor = Colors[ki],
                };
                Label txt = new Label()
                {
                    Text = Keys[ki] + (ShowAppd ? string.Format(" ({0})",Appd[ki]) : ""),
                };
                if (TxtFont != null)
                {
                    mark.Font = TxtFont;
                    txt.Font = TxtFont;
                }
                lineitem.Items.Add(mark);
                lineitem.Items.Add(txt);

                lineitems.SetValue(lineitem, ki);
            }

            foreach (StackLayout i in lineitems)
                legends.Items.Add(i);

            return legends;
        }
    }
}
