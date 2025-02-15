using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Eto.Drawing;
using Eto.Forms;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

namespace Synapse
{
    public class EtoPie : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the EtoPie class.
        /// </summary>
        public EtoPie()
          : base("SnpPieChart", "SPie",
              "pie chart",
              "Synapse", "Graphics")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Size", "S", "size", GH_ParamAccess.item);
            pManager.AddNumberParameter("Numbers", "N", "numbers to represent in a pie chart", GH_ParamAccess.list, new double[] { 0.25, 0.33, 0.15, 1 - 0.15 - 0.33 - 0.25, });
            pManager.AddTextParameter("Keys", "K", "keys", GH_ParamAccess.list);
            pManager[2].Optional = true;
            pManager.AddColourParameter("Colors", "C", "regular Grasshopper colors will do\nno need to use Synapse colors", GH_ParamAccess.list);
            pManager[3].Optional = true;
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
            List<double> nums = new List<double>();
            List<string> keys = new List<string>();
            List<GH_Colour> gclrs = new List<GH_Colour>();
            DA.GetData(0, ref gobj);
            DA.GetDataList(1, nums);
            DA.GetDataList(2, keys);
            DA.GetDataList(3, gclrs);

            #region get size
            if (gobj.Value is GH_Rectangle grec)
            {
                double d = new double[] { grec.Value.X.Length, grec.Value.Y.Length }.Min();
                s = new Size((int)d, (int)d);
            }
            else if (gobj.Value is GH_Vector gvec)
            {
                double d = new double[] { gvec.Value.X, gvec.Value.Y }.Min();
                s = new Size((int)d, (int)d);
            }
            else if (gobj.Value is GH_ComplexNumber gcomp)
            {
                double d = new double[] { gcomp.Value.Real, gcomp.Value.Imaginary }.Min();
                s = new Size((int)d, (int)d);
            }
            else if (gobj.Value is GH_Point gpt)
            {
                double d = new double[] { gpt.Value.X, gpt.Value.Y }.Min();
                s = new Size((int)d, (int)d);
            }
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
                        {
                            int d = new int[] { xi, yi }.Min();
                            s = new Size(d, d);
                        }
                        else
                        {
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, " cannot parse size input string into integers");
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

            #region fill defaults
            List<Color> clrs = new List<Color>();
            if (keys.Count == 0)
                for (int i = 0; i < nums.Count; i++)
                    keys.Add(string.Format("[{0}]", i));
            else if (keys.Count != nums.Count)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, " must be same number of data and keys");
                return;
            }
            if (gclrs.Count == 0)
                for (int i = 0; i < nums.Count; i++)
                {
                    double r = Util.Rand.NextDouble() * 255;
                    double g = Util.Rand.NextDouble() * 255;
                    double b = Util.Rand.NextDouble() * 255;
                    clrs.Add(Color.FromArgb((int)r, (int)g, (int)b));
                }
            else if (gclrs.Count != nums.Count)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, " must be same number of colors and data");
                return;
            }
            else
                foreach (GH_Colour gc in gclrs)
                    clrs.Add(Color.FromArgb(gc.Value.ToArgb()));
            #endregion

            Bitmap bitmap = new Bitmap(s, PixelFormat.Format32bppRgba);
            Graphics graphics = new Graphics(bitmap);

            double[] pct = new double[nums.Count];
            for (int i = 0; i < pct.Length; i++)
                pct.SetValue(nums[i] / nums.Sum(), i);

            double start = 0;
            double sweep = pct[0] * 360;
            for (int i =0; i<pct.Length; i++)
            {
                RectangleF r = new RectangleF(s);
                graphics.FillPie(clrs[i], r, (float)start, (float)sweep);
                if (i == pct.Length - 1) break;
                start += sweep;
                sweep = pct[i + 1] * 360;
            }
            graphics.Flush();

            ImageView graph = new ImageView() { Image = bitmap, };
            ChartData pd = new ChartData(keys, ChartType.Pie) { AppdVals = pct, Colors = clrs.ToArray(), };
            DA.SetData(0, new GH_ObjectWrapper(graph));
            DA.SetData(1, new GH_ObjectWrapper(pd));
        }


        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.pie;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("440baedc-df81-4665-9f86-ba33216a052a"); }
        }
    }
}