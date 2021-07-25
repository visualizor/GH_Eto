using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Eto.Forms;
using Eto.Drawing;
using Grasshopper.Kernel.Types;
using System.ComponentModel;

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


    /* ImageButton and CustomButton are copied from internet with minor mod
     * url https://gist.github.com/cwensley/95000998e37acd93e830
     */
    internal class ImageButton : CustomButton
    {
        bool sizeSet;
        public Image BtnImage { get; set; }
        public Image DisabledImage { get; set; }

        public override Size Size
        {
            get
            {
                return base.Size;
            }
            set
            {
                base.Size = value;
                sizeSet = true;
            }
        }

        public ImageButton(Image enabled, Image disabled)
        {
            BtnImage = enabled;
            DisabledImage = disabled;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (BtnImage != null)
            {
                if (!sizeSet)
                    Size = BtnImage.Size + 4;
            }
        }

        protected override void OnLoadComplete(EventArgs e)
        {
            base.OnLoadComplete(e);
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);

            var image = this.Enabled ? BtnImage : DisabledImage;
            var size = image.Size.FitTo(this.Size - 2);
            var xoffset = (this.Size.Width - size.Width) / 2;
            var yoffset = (this.Size.Height - size.Height) / 2;
            pe.Graphics.DrawImage(image, xoffset, yoffset, size.Width, size.Height);
        }
    }
    internal class CustomButton : Drawable
    {
        bool pressed;
        bool hover;
        bool mouseDown;
        public static Color DisabledColor = Color.FromGrayscale(0.4f, 0.3f);
        public static Color EnabledColor = Colors.Black;

        public override bool Enabled
        {
            get
            {
                return base.Enabled;
            }
            set
            {
                if (base.Enabled != value)
                {
                    base.Enabled = value;
                    if (Loaded)
                        Invalidate();
                }
            }
        }

        public bool Pressed
        {
            get { return pressed; }
            set
            {
                if (pressed != value)
                {
                    pressed = value;
                    mouseDown = false;
                    if (Loaded)
                        Invalidate();
                }
            }
        }

        public Color DrawColor
        {
            get { return Enabled ? EnabledColor : DisabledColor; }
        }

        public bool Toggle { get; set; }

        public bool Persistent { get; set; }

        public event EventHandler<EventArgs> Click;

        protected virtual void OnClick(EventArgs e)
        {
            Click?.Invoke(this, e);
        }

        public CustomButton()
        {
            Enabled = true;
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            if (Loaded)
                Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (Enabled)
            {
                mouseDown = true;
                Invalidate();
            }
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            hover = true;
            Invalidate();
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            hover = false;
            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            var rect = new Rectangle(this.Size);
            if (mouseDown && rect.Contains((Point)e.Location))
            {
                if (Toggle)
                    pressed = !pressed;
                else if (Persistent)
                    pressed = true;
                else
                    pressed = false;
                mouseDown = false;

                Invalidate();
                if (Enabled)
                    OnClick(EventArgs.Empty);
            }
            else
            {
                mouseDown = false;
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            var rect = new Rectangle(this.Size);
            var col = Color.FromGrayscale(hover && Enabled ? 0.95f : 0.8f);
            if (Enabled && (pressed || mouseDown))
            {
                pe.Graphics.FillRectangle(col, rect);
                pe.Graphics.DrawInsetRectangle(Colors.Gray, Colors.White, rect);
            }
            else if (hover && Enabled)
            {
                pe.Graphics.FillRectangle(col, rect);
                pe.Graphics.DrawInsetRectangle(Colors.White, Colors.Gray, rect);
            }

            base.OnPaint(pe);
        }
    }

    internal class LatentTB: TextBox
    {
        public bool Live { get; set; } = true;
        public LatentTB() : base()
        {

        }
    }

    internal class SliderTB: Form
    {
        public TextBox InputBox { get; private set; }
        public ComboSlider Slider { get; private set; }

        public SliderTB(ComboSlider parent)
        {
            Slider = parent;
            Width = Slider.slider.Width;
            Shown += OnShown;
            
            InputBox = new TextBox()
            {
                Text = Slider.val.ToString(),
                Width = Width - 4,
            };
            InputBox.KeyUp += OnCommit;

            DynamicLayout dlo = new DynamicLayout()
            {
                Padding = 2,
                Spacing = new Size(2, 2),
            };
            dlo.AddSeparateRow(InputBox);
            Content = dlo;
        }

        protected void OnCommit(object s, KeyEventArgs e)
        {
            if (e.Key == Keys.Escape)
            {
                Close();
                return;
            }
            else if (e.Key == Keys.Enter)
            {
                if (double.TryParse(InputBox.Text, out double userval))
                    Slider.SetVal(userval); // out-of-bounds safeguard already built in :)
                Close();
                return;
            }
        }
        protected void OnUnfocus(object s, EventArgs e)
        {
            Close();
        }
        protected void OnShown(object s, EventArgs e)
        {
            Focus();
            LostFocus += OnUnfocus;
        }
    }

    internal class ComboSlider : StackLayout
    {
        public Slider slider;
        public Label label;
        public double coef=1.0;
        public double val;
        protected double min;
        protected double max;
        protected int tickers = 10;

        public ComboSlider()
        {
            slider = new Slider()
            {
                MinValue = 0,
                MaxValue = 10,
                Value = 5,
            };
            min = slider.MinValue * coef;
            max = slider.MaxValue * coef;
            slider.MouseDoubleClick += OnUserVal;
            slider.MouseUp += OnRightMouse;
            slider.ValueChanged += OnSlide;
            UpdateTickers();

            label = new Label();
            val = slider.Value * coef;
            label.Text = val.ToString();

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
        protected void OnUserVal(object s, MouseEventArgs e)
        {
            SliderTB tb = new SliderTB(this)
            {
                Location = new Point((int)Mouse.Position.X, (int)Mouse.Position.Y),
                Topmost = true,
                WindowStyle = WindowStyle.None,
                Title = "awaiting input...",
                Resizable = false,
            };
            
            try
            {
                tb.Show();
            }
            catch
            {
                // TODO: maybe handle this actually
            }
        }
        protected void OnRightMouse(object s, MouseEventArgs e)
        {
            if (e.Buttons == MouseButtons.Alternate)
                OnUserVal(s, e);
        }

        public void SetVal(double v)
        {
            if (v > max)
                val = max;
            else if (v < min)
                val = min;
            else
                val = v;
            slider.Value = (int)Math.Round(val / coef, 0);
        }

        public void SetMin(double n)
        {
            slider.MinValue = (int)Math.Round(n / coef, 0);
            slider.Value = (slider.MinValue + slider.MaxValue) / 2;
            min = slider.MinValue * coef;
            UpdateTickers();
        }

        public void SetMax(double n)
        {
            slider.MaxValue = (int)Math.Round(n / coef, 0);
            slider.Value = (slider.MinValue + slider.MaxValue) / 2;
            max = slider.MaxValue * coef;
            UpdateTickers();
        }

        public void UpdateTickers()
        {
            slider.TickFrequency = (slider.MaxValue - slider.MinValue) / tickers;
        }
        public void UpdateTickers(int count)
        {
            tickers = count > 0 ? count : tickers;
            UpdateTickers();
        }
    }

    internal class TrackPad: ImageView
    {
        protected Bitmap img;
        protected Graphics author;
        protected Pen stroke = new Pen(Color.FromArgb(45, 45, 45, 255));
        protected int x = 5; //number of grid divisions
        protected int y = 5;

        public bool Tracking { get; private set; } = false;
        public PointF Position { get; private set; } = new PointF(0, 0);

        /// <summary>
        /// base constructor
        /// </summary>
        /// <param name="bmp">canvas bitmap</param>
        public TrackPad(Bitmap bmp):base()
        {
            img = bmp;
            author = new Graphics(img);
            MouseUp += OnMouseUp;
            MouseDown += OnMouseDn;
        }
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="bmp">canvas bitmap</param>
        /// <param name="pen">line stroke</param>
        public TrackPad(Bitmap bmp, Pen pen) : this(bmp)
        {
            stroke = pen;
        }

        protected void OnMouseDn(object s, MouseEventArgs e)
        {
            Tracking = true;
            MouseMove += OnMouseMv;
        }
        protected void OnMouseMv(object s, MouseEventArgs e)
        {
            if (!Tracking) return;
            DrawCursor(e, true);
        }
        protected void OnMouseUp(object s, MouseEventArgs e)
        {
            Tracking = false;
            MouseMove -= OnMouseMv;
            DrawCursor(e);
        }
        /// <summary>
        /// mouse event triggered drawing of the cursor circle and xy label
        /// </summary>
        /// <param name="e">mouse event passed from handler</param>
        /// <param name="mark">set to true while mouse is moving</param>
        protected void DrawCursor(MouseEventArgs e, bool mark = false)
        {
            DrawGrid();

            PointF upperleft = new PointF(
                e.Location.X - stroke.Thickness,
                e.Location.Y - stroke.Thickness
                );
            Size dotsize = new Size((int)stroke.Thickness * 2, (int)stroke.Thickness * 2);
            author.DrawEllipse(stroke, new RectangleF(upperleft, dotsize));
            Position = e.Location;

            if (mark)
            {
                float txtx = e.Location.X;
                float txty = e.Location.Y - stroke.Thickness - 20;
                Font arial = new Font("Arial", 8f);
                author.DrawText(arial, Color.FromArgb(10, 10, 10, 255), txtx, txty, e.Location.ToString());
            }

            author.Flush();
            Image = img;
        }

        /// <summary>
        /// paint a grid on the trackpad
        /// </summary>
        /// <param name="flush">true to flush graphics, false if other drawings ensue</param>
        public void DrawGrid(bool flush = false)
        {
            RectangleF frame = new RectangleF(
                new PointF(stroke.Thickness / 2f, stroke.Thickness / 2f),
                new Size((int)(img.Size.Width - stroke.Thickness), (int)(img.Size.Width - stroke.Thickness))
                );
            author.DrawRectangle(stroke, frame);

            SolidBrush b = stroke.Brush as SolidBrush;
            Pen reduced = new Pen(Color.FromArgb(b.Color.Rb, b.Color.Gb, b.Color.Bb, 100), stroke.Thickness/2f);
            float xincr = img.Width / (float)x;
            float yincr = img.Height / (float)y;
            for (int i=1; i<x; i++)
            {
                PointF s = new PointF(i * xincr, 0);
                PointF e = new PointF(i * xincr, img.Height);
                author.DrawLine(reduced, s, e);
            }
            for (int j = 1; j < y; j++)
            {
                PointF s = new PointF(0, j * yincr);
                PointF e = new PointF(img.Width, j * yincr);
                author.DrawLine(reduced, s, e);
            }

            if (flush)
            {
                author.Flush();
                Image = img;
            }
        }
    }

    internal class ComboDomSl: StackLayout
    {
        public DomainSlider DomSl;
        protected Label min;
        protected Label max;

        public ComboDomSl(int width)
        {
            DomSl = new DomainSlider(width);
            DomSl.PaintCtrl();
            min = new Label() { Text = DomSl.Lower.ToString("F"),};
            max = new Label() { Text = DomSl.Upper.ToString("F"), };

            Spacing = 2;
            Orientation = Orientation.Horizontal;
            Items.Add(min);
            Items.Add(DomSl);
            Items.Add(max);

            DomSl.PropertyChanged += OnDomChanged;
        }

        protected void OnDomChanged(object s, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Upper" || e.PropertyName == "Lower")
            {
                min.Text = DomSl.Lower.ToString("F");
                max.Text = DomSl.Upper.ToString("F");
            }
        }
    }
    internal class DomainSlider: ImageView, INotifyPropertyChanged
    {
        protected Bitmap img;
        protected Graphics author;
        protected Pen stroke = new Pen(Color.FromArgb(125, 125, 125, 255));
        protected int knobD = 8;
        protected double t0 = 0;
        protected double t1 = 10;
        protected double lo = 1.3;
        protected double up = 7.2;
        protected double x0;
        protected double x1;
        protected bool tracking = false;
        protected bool mvup;
        protected float mousenow;

        public event PropertyChangedEventHandler PropertyChanged;
        public double Lower
        {
            get { return lo; }
            set
            {
                if (value >= t0 && value < up)
                {
                    lo = value;
                    int barw = img.Width - knobD;
                    x0 = barw * (lo - t0) / (t1 - t0);
                    PaintCtrl();
                    OnPropertyChanged("Lower");
                }
            }
        }
        public double Upper
        {
            get { return up; }
            set
            {
                if (value <= t1 && value > lo)
                {
                    up = value;
                    int barw = img.Width - knobD;
                    x1 = barw * (up - t0) / (t1 - t0);
                    PaintCtrl();
                    OnPropertyChanged("Upper");
                }
            }
        }

        public DomainSlider(int width)
        {
            img = new Bitmap(new Size(width, 10), PixelFormat.Format32bppRgba);
            author = new Graphics(img);
            int barw = img.Width - knobD;
            x0 = barw * (lo - t0) / (t1 - t0);
            x1 = barw * (up - t0) / (t1 - t0);
            MouseUp += OnMouseUp;
            MouseDown += OnMouseDn;
        }

        protected void OnMouseDn(object s, MouseEventArgs e)
        {
            tracking = true;
            double dx0 = Math.Abs(e.Location.X - Location.X - x0); // e.Location is actually relative to parent
            double dx1 = Math.Abs(e.Location.X - Location.X - x1);
            mvup = dx0 >= dx1;
            mousenow = e.Location.X - Location.X;
            MouseMove += OnMouseMv;
        }
        protected void OnMouseMv(object s, MouseEventArgs e)
        {
            if (!tracking) return;
            float d = e.Location.X - Location.X - mousenow;
            if (mvup)
            {
                Upper += d/10; // division for dampening of movement
                PaintCtrl();
            }
            else
            {
                Lower += d/10;
                PaintCtrl();
            }
            mousenow += d;
        }
        protected void OnMouseUp(object s, MouseEventArgs e)
        {
            tracking = false;
            MouseMove -= OnMouseMv;
            mousenow = e.Location.X - Location.X;
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void PaintCtrl()
        {
            DrawBar();
            DrawKnob();
            author.Flush();
            Image = img;
        }
        protected void DrawBar()
        {
            int barw = img.Width - knobD;
            int barh = 2;
            Point loc = new Point(knobD/2, knobD/2-barh/2);
            Rectangle rec = new Rectangle(loc, new Size(barw, barh));
            Rectangle fillrec = new Rectangle(
                new Point((int)Math.Round(x0,0), knobD / 2 - barh / 2),
                new Point((int)Math.Round(x1,0), knobD/2 + barh/2)
                );
            author.DrawRectangle(stroke, rec);
            author.FillRectangle(stroke.Brush, fillrec);
        }
        protected void DrawKnob()
        {
            int barw = img.Width - knobD;
            Point loc = new Point((int)Math.Round(x0, 0), 0);
            author.FillEllipse(stroke.Brush, new Rectangle(loc, new Size(knobD, knobD)));
            loc = new Point((int)Math.Round(x1, 0), 0);
            author.FillEllipse(stroke.Brush, new Rectangle(loc, new Size(knobD, knobD)));
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
        /// for trend graph it's average value; for pie/hierarchy it's percentage; for bar it's actual number
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
            {"pyramid", @"▲" },
            {"heart", @"♥" },
            { "diamond", @"♦"},
            {"circle", @"○"},
            {"arrow", @"►" }
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