using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using sd = System.Drawing;
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
          : base("SynapsePieChart", "SPie",
              "pie chart",
              "Synapse", "Graphs")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("Size", "S", "size", GH_ParamAccess.item, 50);
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
            int s = 50;
            List<double> nums = new List<double>();
            List<string> keys = new List<string>();
            List<GH_Colour> gclrs = new List<GH_Colour>();
            DA.GetData(0, ref s);
            DA.GetDataList(1, nums);
            DA.GetDataList(2, keys);
            DA.GetDataList(3, gclrs);
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

            Bitmap bitmap = new Bitmap(new Size(s, s), PixelFormat.Format32bppRgba);
            Graphics graphics = new Graphics(bitmap);

            double[] pct = new double[nums.Count];
            for (int i = 0; i < pct.Length; i++)
                pct.SetValue(nums[i] / nums.Sum(), i);

            double start = 0;
            double sweep = pct[0] * 360;
            for (int i =0; i<pct.Length; i++)
            {
                RectangleF r = new RectangleF(new Size(s - 2, s - 2));
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