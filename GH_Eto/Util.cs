using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Eto.Forms;
using Eto.Drawing;
using Grasshopper.Kernel.Types;

namespace Synapse
{
    internal class Util
    {
        /// <summary>
        /// the random seeder object all everything should call for arbitrary numbers within this namespace
        /// </summary>
        public static Random Rand = new Random();

        /// <summary>
        /// set eto control properties
        /// </summary>
        /// <param name="subject">eto control object</param>
        /// <param name="pname">property name</param>
        /// <param name="pval">property value</param>
        public static void SetProp(object subject, string pname, object pval)
        {
            PropertyInfo prop = subject.GetType().GetProperty(pname);
            prop.SetValue(subject, pval);
        }

        /// <summary>
        /// get the object value attached to grasshopper goo
        /// </summary>
        /// <param name="goo">the goo object</param>
        /// <returns>object value</returns>
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

    internal enum ChartType
    {
        Pie,
        Bar,
        Trend,
        Region,
    }

    internal class ChartData
    {
        protected double[] mappedvals;
        protected Color[] clrs;
        protected double[] vals;

        /// <summary>
        /// name tags/keys
        /// </summary>
        public string[] Keys { get; set; }
        /// <summary>
        /// appended extra values in parenthese
        /// </summary>
        public double[] AppdVals
        {
            get { return mappedvals; }
            set
            {
                if (value.Length == Keys.Length)
                    mappedvals = value;
            }
        }
        /// <summary>
        /// original data values
        /// </summary>
        public double[] OGVals
        {
            get { return vals; }
            set
            {
                if (value.Length == Keys.Length)
                    vals = value;
            }
        }
        /// <summary>
        /// graph colors
        /// </summary>
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
        /// type of chart
        /// </summary>
        public ChartType Type { get; set; }

        /// <summary>
        /// constructor, don't forget to set properties of values of colors
        /// </summary>
        /// <param name="k">keys</param>
        public ChartData(IEnumerable<string> k, ChartType t)
        {
            Keys = k.ToArray();
            Type = t;
        }

        public override string ToString()
        {
            return string.Format("chart data ({0} items)", Keys.Length);
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
        /// prefix to appended texts
        /// </summary>
        public string AppdPrefix { get; set; } = "";
        /// <summary>
        /// suffix to appended texts
        /// </summary>
        public string AppdSuffix { get; set; } = "";

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
                    Text = Keys[ki] + (ShowAppd ? string.Format(" ({2}{0}{1})",Appd[ki],AppdSuffix,AppdPrefix) : ""),
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

    internal enum ChartAxisType
    {
        X,
        Y,
        Both,
    }

    internal class ChartAxis
    {
        private int tickX = 4;
        private int tickY = 4;

        /// <summary>
        /// axis to draw
        /// </summary>
        public ChartAxisType AxisType { get; private set; }
        /// <summary>
        /// graphics object that actually draws
        /// </summary>
        public Graphics Author { get; private set; }
        /// <summary>
        /// draw area
        /// </summary>
        public RectangleF Canvas { get; private set; }
        public float LineWt { get; set; } = 1f;
        public Color LineClr { get; set; } = Color.FromArgb(0, 0, 0);
        public int TickCountX
        {
            get { return tickX; }
            set
            {
                if (value >= 2)
                    tickX = value;
            }
        }
        public int TickCountY
        {
            get { return tickY; }
            set
            {
                if (value >= 2)
                    tickY = value;
            }
        }
        /// <summary>
        /// room for axes
        /// </summary>
        public int Margin { get; set; } = 10;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="r">rectangular graphing area</param>
        /// <param name="g">eto graphics object that authors drawings</param>
        /// <param name="t">chart axis</param>
        public ChartAxis(RectangleF r, Graphics g, ChartAxisType t = ChartAxisType.Both)
        {
            AxisType = t;
            Author = g;
            Canvas = r;
        }

        /// <summary>
        /// draws contents to the image area
        /// </summary>
        public void Draw()
        {
            Pen pen = new Pen(LineClr, LineWt);
            PointF ytip = new PointF((float)(Margin-LineWt/2.0), (float)(LineWt/2.0));
            PointF origin = new PointF((float)(Margin - LineWt / 2.0), (float)(Canvas.Height-Margin+LineWt/2.0));
            PointF xtip = new PointF((float)(Canvas.Width-LineWt/2.0), (float)(Canvas.Height - Margin + LineWt / 2.0));
            
            switch (AxisType)
            {
                case ChartAxisType.Both:
                    Author.DrawLines(pen, ytip, origin, xtip);
                    break;
                case ChartAxisType.X:
                    Author.DrawLine(pen, origin, xtip);
                    break;
                case ChartAxisType.Y:
                    Author.DrawLine(pen, ytip, origin);
                    break;
                default:
                    break;
            }
            DrawTickMarks(pen, ytip, xtip, origin);
        }
        /// <summary>
        /// draw tick marks
        /// </summary>
        /// <param name="p">pen object</param>
        /// <param name="y">y tip point</param>
        /// <param name="x">x tip point</param>
        /// <param name="o">origin</param>
        protected void DrawTickMarks(Pen p, PointF y, PointF x, PointF o)
        {
            if (AxisType != ChartAxisType.X) {
                PointF[] yticks = new PointF[TickCountY];
                for (int yi = 0; yi < yticks.Length; yi++)
                {
                    float ty = (o.Y-y.Y) / (TickCountY-1) * yi + y.Y;
                    float tx = y.X;
                    yticks.SetValue(new PointF(tx, ty), yi);
                }
                foreach (PointF yp in yticks)
                    Author.DrawLine(LineClr, yp, yp + new PointF(-2, 0));
            }

            if (AxisType != ChartAxisType.Y)
            {
                PointF[] xticks = new PointF[TickCountY];
                for (int xi = 0; xi < xticks.Length; xi++)
                {
                    float ty = x.Y;
                    float tx = (x.X - o.X) / (TickCountX-1) * xi + o.X;
                    xticks.SetValue(new PointF(tx, ty), xi);
                }
                foreach (PointF xp in xticks)
                    Author.DrawLine(LineClr, xp, xp + new PointF(0, 2));
            }
        }
    }
}