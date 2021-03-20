using System;
using System.Collections.Generic;
using System.Linq;
using Eto.Forms;
using Eto.Drawing;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Grasshopper;
using Grasshopper.Kernel.Data;

namespace Synapse
{
    public class EtoTrends : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the EtoTrends class.
        /// </summary>
        public EtoTrends()
          : base("SynapseTrends", "STrends",
              "line graph",
              "Synapse", "Graphs")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Size", "S", "size", GH_ParamAccess.item);
            pManager.AddNumberParameter("Numbers", "N", "numbers to represent in a line chart\neach branch will generate a polyline", GH_ParamAccess.tree);
            pManager.AddTextParameter("Name", "K", "trend name\neach on an individual branch corresponding to numbers", GH_ParamAccess.tree);
            pManager.AddColourParameter("Color", "C", "regular Grasshopper color will do\nno need to use Synapse colors\neach on an individual branch corresponding to numbers", GH_ParamAccess.tree);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Chart Object", "G", "chart", GH_ParamAccess.item);
            pManager.AddGenericParameter("Chart Data", "D", "chart data if you need to generate a legend", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Size s;
            GH_ObjectWrapper gobj = null;
            DA.GetData(0, ref gobj);
            DA.GetDataTree(1, out GH_Structure<GH_Number> data);
            DA.GetDataTree(2, out GH_Structure<GH_String> keys);
            DA.GetDataTree(3, out GH_Structure<GH_Colour> clrs);

            #region get size
            if (gobj.Value is GH_Rectangle grec)
                s = new Size((int)grec.Value.X.Length, (int)grec.Value.Y.Length);
            else if (gobj.Value is GH_Vector gvec)
                s = new Size((int)gvec.Value.X, (int)gvec.Value.Y);
            else if (gobj.Value is GH_ComplexNumber gcomp)
                s = new Size((int)gcomp.Value.Real, (int)gcomp.Value.Imaginary);
            else if (gobj.Value is GH_Point gpt)
                s = new Size((int)gpt.Value.X, (int)gpt.Value.Y);
            else if (gobj.Value is GH_Integer gint)
                s = new Size(gint.Value, gint.Value);
            else if (gobj.Value is GH_Number gn)
                s = new Size((int)gn.Value, (int)gn.Value);
            else if (gobj.Value is GH_String gstr)
            {
                string str = gstr.Value;
                if (str.Contains(","))
                {
                    string[] split = str.Split(',');
                    if (split.Length != 2)
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, " cannot parse size input string");
                        return;
                    }
                    else
                    {
                        bool a = int.TryParse(split[0], out int xi);
                        bool b = int.TryParse(split[1], out int yi);
                        if (a && b)
                            s = new Size(xi, yi);
                        else
                        {
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, " cannot parse size input string");
                            return;
                        }
                    }
                }
                else if (int.TryParse(str, out int i))
                    s = new Size(i, i);
                else
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, " cannot parse size input string");
                    return;
                }
            }
            else if (gobj.Value is Size etosize)
                s = etosize;
            else
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, " size object not valid\n use a point, integer, vector, complex number or rectangle\n an actual Eto.Drawing.Size object would be better!");
                return;
            }
            #endregion

            Bitmap bitmap = new Bitmap(s, PixelFormat.Format32bppRgba);
            Graphics graphics = new Graphics(bitmap);
            ChartAxis axis = new ChartAxis(new RectangleF(s), graphics);
            if (s.Height <= 10 || s.Width <= 10)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, " size too small to draw");
                return;
            }

            IEnumerable<double> flatdata = data.FlattenData().Select(i => i.Value);
            double h_incr = (s.Height - 10) / flatdata.Max(); // -10 is leaving space for axes
            double[] avg = new double[data.Branches.Count];
            Color[] etoclrs = new Color[data.Branches.Count];
            string[] txtkeys = new string[data.Branches.Count];
            for (int bi = 0; bi<data.Branches.Count; bi++)
            {
                try
                {
                    etoclrs.SetValue(clrs.Branches[bi][0].Value.ToEto(), bi);
                    txtkeys.SetValue(keys.Branches[bi][0].Value, bi);
                }
                catch (ArgumentOutOfRangeException)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, " mismatch of data length\n if N has 3 branches each with 5 numbers, K and C must be 3 branches each has one item\n did you forget to graft or duplicate data as necessary?");
                    return;
                }

                double[] nums = data.Branches[bi].Select(i => i.Value).ToArray();
                avg.SetValue(nums.Average(), bi);
                double barwidth = (s.Width - 10) / (double)nums.Length; // -10 leaving left for axis

                PointF[] nodes = new PointF[nums.Length];
                Pen pen = new Pen(etoclrs[bi], 2f);
                for (int i = 0; i < nums.Length; i++)
                {
                    double x = (i + 0.5) * barwidth + 10; // +10 moving right, leaving left for axis
                    double h = nums[i] * h_incr;
                    double y = s.Height - 10 + 4 - h; // -10 moving up leaving space for x axis, additional +4 moving down to avoid top chop off
                    nodes.SetValue(new PointF((float)x, (float)y), i);
                }
                graphics.DrawLines(pen, nodes);
                foreach(PointF p in nodes)
                    graphics.DrawArc(pen, p.X - 4, p.Y - 4, 8f, 8f, 0f, 360f);
            }
            axis.Draw();
            graphics.Flush();

            ImageView graph = new ImageView() { Image = bitmap, };
            ChartData bardata = new ChartData(txtkeys, ChartType.Trend) { AppdVals = avg, Colors = etoclrs, };
            DA.SetData(0, new GH_ObjectWrapper(graph));
            DA.SetData(1, new GH_ObjectWrapper(bardata));
        }


#if !DEBUG
        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.hidden; }
        }
#endif

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.trends;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("e57895f7-6d5e-4b92-a8fd-e10c481408dd"); }
        }
    }
}