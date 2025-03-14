using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Eto.Forms;
using Eto.Drawing;
using Grasshopper.Kernel.Types;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
using System.ComponentModel;
using wdraw = System.Drawing;
using wf = System.Windows.Forms;
using Grasshopper.GUI.Canvas;
using rhg = Rhino.Geometry;

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

        /// <summary>
        /// helper method to list out all properties of an object
        /// </summary>
        /// <param name="DA">data access object</param>
        /// <param name="i">index of target output, must be list</param>
        /// <param name="o">object</param>
        public static void OutputProps (IGH_DataAccess DA, int i, object o)
        {
            PropertyInfo[] allprops = o.GetType().GetProperties();
            List<string> printouts = new List<string>();
            foreach (PropertyInfo prop in allprops)
                if (prop.CanWrite)
                    printouts.Add(prop.Name + ": " + prop.PropertyType.ToString());
            DA.SetDataList(i, printouts);
        }
        /// <summary>
        /// location to initialize value list for component properties
        /// </summary>
        public static wdraw.PointF ListPropLoc = wdraw.PointF.Empty;
        /// <summary>
        /// pass control type here upon context menu click
        /// </summary>
        public static Type ListPropType = null;
        /// <summary>
        /// shared event handler when user wants to list all properties of a component
        /// </summary>
        /// <param name="s">sender</param>
        /// <param name="e">event</param>
        public static void OnListProps(object s, EventArgs e)
        {
            var gcv = Instances.ActiveCanvas;
            var gsrv = Instances.ComponentServer;
            var gdoc = gcv.Document;

            Guid cid = gsrv.FindObjectByName("ValueList", true, false).Guid;
            gcv.InstantiateNewObject(cid, ListPropLoc, false);
            GH_ValueList gvl = gdoc.Objects.Last() as GH_ValueList;
            gvl.ListItems.Clear();
            gvl.NickName = "Properties";
            gvl.ListMode = GH_ValueListMode.CheckList;

            PropertyInfo[] btnprops = ListPropType.GetProperties();
            foreach (PropertyInfo prop in btnprops)
                if (prop.CanWrite)
                    gvl.ListItems.Add(new GH_ValueListItem(prop.Name, "\"" + prop.Name + "\""));

            ListPropLoc = wdraw.PointF.Empty;
            ListPropType = null;
            gdoc.ScheduleSolution(1);
        }

        /// <summary>
        /// instantiate a control on canvas
        /// </summary>
        /// <param name="loc">location</param>
        /// <param name="cid">component GUID</param>
        /// <returns>document object just added</returns>
        public static IGH_DocumentObject CtrlCanvasInit(wdraw.PointF loc, Guid cid)
        {
            var gcv = Instances.ActiveCanvas;
            var gsrv = Instances.ComponentServer;
            var gdoc = gcv.Document;
            gcv.InstantiateNewObject(cid, loc, false);
            return gdoc.Objects.Last();
        }

    }

    /// <summary>
    /// tagging whether eto form is a standalone window or in a rhino panel
    /// </summary>
    internal enum EWinTag
    {
        None,
        Indie,
        InPanel,
    }

    internal class WebForm : WebView
    {
        public string Html { get; }
        public Dictionary<string, string> CtrlVals { get; } = new Dictionary<string, string>();
        public List<string> OrderedKeys { get; protected set; } = new List<string>();

        public WebForm() : base()
        {
        }

        public WebForm(string html) : base()
        {
            Html = html;
        }

        public WebForm(string html, IEnumerable<string> vars) : base()
        {
            Html = html;
            foreach (string k in vars)
            {
                CtrlVals[k] = "";
                OrderedKeys.Add(k);
            }
        }

        public bool TryLoadHtml()
        {
            try
            {
                LoadHtml(Html);
                return true;
            }
            catch
            {
                return false;
            }
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

    /// <summary>
    /// TextBox with only one additional property
    /// </summary>
    internal class LatentTB: TextBox
    {
        public bool Live { get; set; } = true;
        public LatentTB() : base()
        {

        }
    }

    /// <summary>
    /// pop up textbox for slider double clicks
    /// </summary>
    internal class SliderTB: Form
    {
        public TextBox InputBox { get; private set; }
        public ComboSlider ParentSL { get; private set; }

        public SliderTB(ComboSlider parent)
        {
            ParentSL = parent;
            Width = ParentSL.slider.Width;
            Shown += OnShown;

            InputBox = new TextBox()
            {
                Text = ParentSL.val.ToString(),
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
                //ParentSL.OnUserInputClosed(e);
                Close(); //this would trigger unfocus. no need to call above line
                return;
            }
            else if (e.Key == Keys.Enter)
            {
                if (double.TryParse(InputBox.Text, out double userval))
                    ParentSL.SetVal(userval); // out-of-bounds safeguard already built in :)
                //ParentSL.OnUserInputClosed(e);
                Close();//this would trigger unfocus. no need to call above line
                return;
            }
        }
        protected void OnUnfocus(object s, EventArgs e)
        {
            ParentSL.OnUserInputClosed(e);
            Close();
        }
        protected void OnShown(object s, EventArgs e)
        {
            Focus();
            LostFocus += OnUnfocus;
        }
    }
    /// <summary>
    /// slider with a value label
    /// </summary>
    internal class ComboSlider : StackLayout
    {
        public Slider slider;
        public Label label;
        public int maxdeci = 3;
        public double coef=1.0;
        public double val;
        public bool Live { get; set; } = false;
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
                Tag = this,
            }; //using tag to track parent
            min = slider.MinValue * coef;
            max = slider.MaxValue * coef;
            //slider.MouseDoubleClick += OnUserVal;
            
            slider.ValueChanged += OnSlide;
            UpdateTickers();

            label = new Label();
            val = slider.Value * coef;
            label.Text = val.ToString("0."+new string('#',maxdeci));

            Orientation = Orientation.Horizontal;
            VerticalContentAlignment = VerticalAlignment.Center;
            Items.Add(slider);
            Items.Add(label);
            Spacing = 2;
        }

        public event EventHandler UserInputClosed;
        public virtual void OnUserInputClosed(EventArgs e)
        {
            UserInputClosed?.Invoke(this, e);
        }

        protected void OnSlide(object s, EventArgs e)
        {
            val = slider.Value * coef;
            label.Text = val.ToString("0." + new string('#', maxdeci));
        }
        public void OnUserVal(object s, MouseEventArgs e)
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
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("ComboSlider: OnUserVal\nContact developer for this error:\n{0}", ex.Message));
            }
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
    
    /// <summary>
    /// deprecated. use RangeSlider
    /// </summary>
    internal class ComboDomSl: StackLayout
    {
        public DomainSlider DomSl;
        protected Label min;
        protected Label max;
        protected int sw;

        public ComboDomSl(int width)
        {
            sw = width;
            InitDomSl();
        }

        protected void InitDomSl()
        {
            MouseDoubleClick -= OnUserVal;
            DomSl = new DomainSlider(sw);
            DomSl.PaintCtrl();
            min = new Label() { Text = DomSl.Lower.ToString("F"), };
            max = new Label() { Text = DomSl.Upper.ToString("F"), };

            Spacing = 2;
            Orientation = Orientation.Horizontal;
            Items.Add(min);
            Items.Add(DomSl);
            Items.Add(max);

            DomSl.PropertyChanged += OnDomChanged;
            DomSl.PropertyChanged += OnMetaChanged;
            MouseDoubleClick += OnUserVal;
        }

        protected void OnUserVal(object s, MouseEventArgs e)
        {
            DomSlTB tb = new DomSlTB(this)
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
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Contact developer for this error:\n{0}", ex.Message));
            }
        }
        protected void OnDomChanged(object s, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Upper" || e.PropertyName == "Lower")
            {
                min.Text = DomSl.Lower.ToString("F");
                max.Text = DomSl.Upper.ToString("F");
            }
        }
        protected void OnMetaChanged(object s, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "KnobDiameter":
                    SuspendLayout();
                    ResumeLayout();
                    break;
                case "SliderColor":
                    break;
                case "LeftEnd":
                    break;
                case "RightEnd":
                    break;
                default:
                    break;
            }
        }
    }    
    /// <summary>
    /// deprecated. use RangeSlider
    /// </summary>
    internal class DomainSlider: ImageView, INotifyPropertyChanged
    {
        protected Bitmap img;
        protected Graphics author;
        protected Color clr;
        protected Pen stroke;
        protected int knobD = 12;
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
        // setters of the following two will re-draw control
        public double Lower
        {
            get { return lo; }
            set
            {
                if (value >= t0 && value < up)
                    lo = value;
                else
                {
                    tracking = false;
                    lo = t0;
                }
                OnPropertyChanged("Lower");
            }
        }
        public double Upper
        {
            get { return up; }
            set
            {
                if (value <= t1 && value > lo)
                    up = value;
                else
                {
                    tracking = false;
                    up = t1;
                }
                OnPropertyChanged("Upper");
            }
        }
        public int KnobDiameter
        {
            get { return knobD; }
            set
            {
                if (value > 3)
                {
                    knobD = value;
                    OnPropertyChanged("KnobDiameter");
                }
            }
        }
        public Color SliderColor
        {
            get { return clr; }
            set
            {
                clr = value;
                stroke = new Pen(clr);
                OnPropertyChanged("SliderColor");
            }
        }
        public double LeftEnd
        {
            get { return t0; }
            set
            {
                if (value < lo)
                    t0 = value;
                else
                    t0 = lo;
                OnPropertyChanged("LeftEnd");
            }
        }
        public double RightEnd
        {
            get { return t1; }
            set
            {
                if (value > up)
                    t1 = value;
                else
                    t1 = up;
                OnPropertyChanged("RightEnd");
            }
        }

        public DomainSlider(int width)
        {
            clr = Color.FromArgb(125, 125, 125);
            stroke = new Pen(clr);
            img = new Bitmap(new Size(width, knobD), PixelFormat.Format32bppRgba);
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
                Upper += d/12; // division for dampening of movement
                PaintCtrl();
            }
            else
            {
                Lower += d/12;
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
            int barw = img.Width - knobD;
            x0 = barw * (lo - t0) / (t1 - t0);
            x1 = barw * (up - t0) / (t1 - t0);
            PaintCtrl();
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
    internal class DomSlTB:Form
    {
        public TextBox LowerInput { get; private set; }
        public TextBox UpperInput { get; private set; }
        public ComboDomSl Slider { get; private set; }

        public DomSlTB(ComboDomSl parent)
        {
            Slider = parent;
            Width = parent.DomSl.Width;
            Shown += OnShown;

            LowerInput = new TextBox()
            {
                Text = parent.DomSl.Lower.ToString("F"),
                Width = (Width - 6)/2,
            };
            LowerInput.KeyUp += OnCommit;

            UpperInput = new TextBox()
            {
                Text = parent.DomSl.Upper.ToString("F"),
                Width = (Width - 6) / 2,
            };
            UpperInput.KeyUp += OnCommit;

            DynamicLayout dlo = new DynamicLayout()
            {
                Padding = 2,
                Spacing = new Size(2, 2),
            };
            dlo.AddSeparateRow(LowerInput, null, UpperInput);
            Content = dlo;
        }

        protected void OnShown(object s, EventArgs e)
        {
            Focus();
            LostFocus += OnUnfocus;
        }
        protected void OnUnfocus(object s, EventArgs e)
        {
            Close();
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
                if (double.TryParse(LowerInput.Text, out double lower))
                    Slider.DomSl.Lower = lower;
                if (double.TryParse(UpperInput.Text, out double upper))
                    Slider.DomSl.Upper = upper;
                Close();
                return;
            }
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

    internal class ESwitch : Drawable
    {
        private bool _isOn;
        private Color _activeColor = Colors.DimGray;
        private Color _inactiveColor = Colors.LightGrey;
        private Color _knobColor = Colors.White;

        /// <summary>
        ///  Fires whenever the toggle changes state.
        /// </summary>
        public event EventHandler<EventArgs> Toggled;

        /// <summary>
        ///  Gets or sets the toggle state (true for On, false for Off).
        /// </summary>
        public bool IsOn
        {
            get => _isOn;
            private set
            {
                if (_isOn != value)
                {
                    _isOn = value;
                    Invalidate(); // queues repaint
                    OnToggled();
                }
            }
        }

        public Color ActiveColor
        {
            get => _activeColor;
            set
            {
                if (_activeColor != value)
                {
                    _activeColor = value;
                    Invalidate();
                }
            }
        }

        public Color InactiveColor
        {
            get => _inactiveColor;
            set
            {
                if (_inactiveColor != value)
                {
                    _inactiveColor = value;
                    Invalidate();
                }
            }
        }

        public Color KnobColor
        {
            get => _knobColor;
            set
            {
                if (_knobColor != value)
                {
                    _knobColor = value;
                    Invalidate();
                }
            }
        }

        public ESwitch()
        {
            // Set a preferred size (can be changed to suit your design).
            Size = new Size(50, 25);
            this.Cursor = Cursors.Pointer;

            // Handle mouse clicks.
            MouseDown += (sender, e) =>
            {
                if (e.Buttons == MouseButtons.Primary)
                {
                    // Toggle the current state.
                    IsOn = !IsOn;
                }
            };

            Paint += OnPaint;
        }

        private void OnPaint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.AntiAlias = true;

            // Basic geometry params
            float cornerR = Height / 2f;

            // Colors and styling (customize as desired)
            var backgroundColor = _isOn ? ActiveColor : InactiveColor;
            var togglecirclr = KnobColor;

            // Draw the background track as a rounded rectangle
            var bgRect = new RectangleF(0, 0, Width, Height);
            using (var path = new GraphicsPath())
            {
                AddRoundedRectangle(path, bgRect, cornerR);
                using (var brush = new SolidBrush(backgroundColor))
                {
                    g.FillPath(brush, path);
                }
            }

            // Position of the toggle circle
            float cirDia = Height - 4;
            float cirY = 2;
            float cirX = _isOn ? (Width - cirDia - 2) : 2;

            // Draw the circle
            g.FillEllipse(togglecirclr, cirX, cirY, cirDia, cirDia);
        }

        // Adds a helper method for the rounded rectangle
        private void AddRoundedRectangle(GraphicsPath path, RectangleF rect, float radius)
        {
            // Ensure radius isn't bigger than half the smallest dimension
            radius = Math.Min(radius, Math.Min(rect.Width, rect.Height) / 2);

            float x = rect.X;
            float y = rect.Y;
            float dia = radius * 2;

            // Top-left arc
            path.AddArc(x, y, dia, dia, 180, 90);
            // Top edge
            path.LineTo(x + rect.Width - radius, y);
            // Top-right arc
            path.AddArc(x + rect.Width - dia, y, dia, dia, 270, 90);
            // Right edge
            path.LineTo(x + rect.Width, y + rect.Height - radius);
            // Bottom-right arc
            path.AddArc(x + rect.Width - dia, y + rect.Height - dia, dia, dia, 0, 90);
            // Bottom edge
            path.LineTo(x + radius, y + rect.Height);
            // Bottom-left arc
            path.AddArc(x, y + rect.Height - dia, dia, dia, 90, 90);
            path.CloseFigure();
        }

        protected virtual void OnToggled()
        {
            Toggled?.Invoke(this, EventArgs.Empty);
        }
    }


    public class RangeSliderEventArgs : EventArgs
    {
        public double LowerValue { get; }
        public double UpperValue { get; }

        public RangeSliderEventArgs(double l, double u)
        {
            LowerValue = l;
            UpperValue = u;
        }
    }
    internal class RangeSlider : Drawable
    {
        private double min = 5.5;
        private double max = 13.5;
        private double step = 0.5; // increment
        private double lowerVal = 6.5;
        private double upperVal = 11.0;
        private int maxdeci = 3;
        private string labelformat = "0.###";

        private readonly int knobR = 8;
        private readonly int trackH = 4;
        private readonly int padding = 15;

        private bool draggingLower = false;
        private bool draggingUpper = false;
        public Color AccentColor { get; set; } = Colors.DimGray;
        public Color FontColor { get; set; } = Colors.Gray;
        public bool AltKnob { get; set; } = false;

        public rhg.Interval Range
        {
            get => new rhg.Interval(min, max);
            set
            {
                // Capture the old range length and knob fractions
                double oldRange = max - min;
                double fracLower = 0.0;
                double fracUpper = 1.0;

                if (Math.Abs(oldRange) > double.Epsilon)
                {
                    fracLower = (lowerVal - min) / oldRange;
                    fracUpper = (upperVal - min) / oldRange;
                }

                // Assign new min and max from the Interval
                min = value.T0;
                max = value.T1;

                // Handle cases where T1 < T0 by swapping
                double newRange = max - min;
                if (newRange < 0)
                {
                    double temp = min;
                    min = max;
                    max = temp;
                    newRange = -newRange;
                }

                // Recalculate knob positions based on the fractions
                lowerVal = SnapToStep(min + fracLower * newRange);
                upperVal = SnapToStep(min + fracUpper * newRange);

                // Clamp knobs to ensure valid positions
                if (lowerVal < min) lowerVal = min;
                if (upperVal > max) upperVal = max;
                if (upperVal < lowerVal) upperVal = lowerVal;

                Invalidate();
            }
        }
        /// <summary>
        /// util method to snap val to step increments
        /// </summary>
        /// <param name="v">value to modify</param>
        /// <returns>snapped value</returns>
        private double SnapToStep(double v)
        {
            // Snap value to multiples of 'step' starting from 'min'
            double snapped = min + Math.Round((v - min) / step) * step;
            // Clamp to ensure it doesn't go beyond min or max
            return Math.Max(min, Math.Min(snapped, max));
        }

        public int MaxDecimals
        {
            get => maxdeci;
            set
            {
                maxdeci = value;
                labelformat = "0." + new string('#', maxdeci);
            }
        }

        public double Step
        {
            get => step;
            set
            {
                if (value > 0)
                    step = value;
            }
        }

        public double LowerValue { get => lowerVal; }
        public double UpperValue { get => upperVal; }

        public event EventHandler<RangeSliderEventArgs> ValueChanged;


        /// <summary>
        /// main constructor
        /// </summary>
        public RangeSlider()
        {
            Size = new Size(200, 45); // Set control size
            MouseDown += OnMouseDown;
            MouseMove += OnMouseMove;
            MouseUp += OnMouseUp;
            Paint += OnPaint;
        }

        private float ValueToPosition(double val)
        {
            return padding + (float)((val - min) / (max - min) * (Width - 2 * padding));
        }

        private double PositionToValue(float position)
        {
            double rawValue = min + (position - padding) / (Width - 2 * padding) * (max - min);
            double snappedValue = Math.Round((rawValue - min) / step) * step + min;
            return Math.Max(min, Math.Min(max, snappedValue)); // Clamp to range
        }

        private void OnPaint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            var trackY = Height / 2;

            float lowerX = ValueToPosition(lowerVal);
            float upperX = ValueToPosition(upperVal);

            // Draw track
            g.FillRectangle(Brushes.LightGrey, padding, trackY - trackH / 2, Width - 2 * padding, trackH);

            // Draw selected range
            g.FillRectangle(AccentColor, lowerX, trackY - trackH / 2-1, upperX - lowerX, trackH+2);

            // Draw circle knobs
            if (AltKnob)
            {
                g.FillEllipse(Brushes.White, lowerX - knobR, trackY - knobR, knobR * 2, knobR * 2);
                g.FillEllipse(Brushes.White, upperX - knobR, trackY - knobR, knobR * 2, knobR * 2);
                g.DrawEllipse(Pens.DimGray, lowerX - knobR, trackY - knobR, knobR * 2, knobR * 2);
                g.DrawEllipse(Pens.DimGray, upperX - knobR, trackY - knobR, knobR * 2, knobR * 2);
            }
            //Draw pointy knob
            else
            {
                // Define pentagon for lower knob
                PointF[] lowerKnobPoints = new PointF[]
                {
                    new PointF(lowerX - knobR+2.75f, trackY - knobR), // top-left
                    new PointF(lowerX + knobR-2.75f, trackY - knobR), // top-right
                    new PointF(lowerX + knobR-2.75f, trackY+trackH/2+2),              // mid-right
                    new PointF(lowerX, trackY + knobR+2),     // bottom point
                    new PointF(lowerX - knobR+2.75f, trackY+trackH/2+2),              // mid-left
                };
                g.FillPolygon(Brushes.White, lowerKnobPoints);
                g.DrawPolygon(Pens.DimGray, lowerKnobPoints);

                // Define pentagon for upper knob
                PointF[] upperKnobPoints = new PointF[]
                {
                    new PointF(upperX - knobR+2.75f, trackY - knobR),
                    new PointF(upperX + knobR-2.75f, trackY - knobR),
                    new PointF(upperX + knobR-2.75f, trackY + trackH / 2 + 2),
                    new PointF(upperX, trackY + knobR+2),
                    new PointF(upperX - knobR+2.75f, trackY + trackH / 2 + 2),
                };
                g.FillPolygon(Brushes.White, upperKnobPoints);
                g.DrawPolygon(Pens.DimGray, upperKnobPoints);
            }

            // draw values above knobs
            float textOffsetY = trackY - knobR - 17; // Adjust height above knobs
            g.DrawText(SystemFonts.Default(), FontColor, lowerX - 10, textOffsetY, $"{lowerVal.ToString(labelformat)}");
            g.DrawText(SystemFonts.Default(), FontColor, upperX - 10, textOffsetY, $"{upperVal.ToString(labelformat)}");
        

            // Draw values at the ends
            /*g.DrawText(SystemFonts.Default(), Brushes.Black, padding, trackY + 10, $"{lowerVal}");
            g.DrawText(SystemFonts.Default(), Brushes.Black, Width - padding - 30, trackY + 10, $"{upperVal}");*/
        }

        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            float lowerX = ValueToPosition(lowerVal);
            float upperX = ValueToPosition(upperVal);

            if (Math.Abs(e.Location.X - upperX) <= knobR)
            {
                draggingUpper = true;
            }
            else if (Math.Abs(e.Location.X - lowerX) <= knobR)
            {
                draggingLower = true;
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            bool changed = false;

            if (draggingLower)
            {
                double newLowerVal = Math.Max(min, Math.Min(upperVal - step, PositionToValue(e.Location.X)));
                if (newLowerVal != lowerVal)
                {
                    lowerVal = newLowerVal;
                    changed = true;
                }
            }
            else if (draggingUpper)
            {
                double newUpperVal = Math.Min(max, Math.Max(lowerVal + step, PositionToValue(e.Location.X)));
                if (newUpperVal != upperVal)
                {
                    upperVal = newUpperVal;
                    changed = true;
                }
            }

            if (changed)
            {
                Invalidate();
                OnValueChanged(); //raise value changed event
            }
        }


        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            draggingLower = false;
            draggingUpper = false;
        }

        private void OnValueChanged()
        {
            ValueChanged?.Invoke(this, new RangeSliderEventArgs(lowerVal, upperVal));
        }

    }
}