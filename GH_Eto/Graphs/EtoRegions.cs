using System;
using System.Collections.Generic;
using System.Linq;
using Eto.Drawing;
using Eto.Forms;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

namespace Synapse
{
    public class EtoRegions : EtoPie
    {
        /// <summary>
        /// Initializes a new instance of the EtoRegions class.
        /// </summary>
        public EtoRegions()
          : base()
        {
            Name = "SnpHierarchy";
            NickName = "SHrch";
            Description = "rectangular areas showing proportions";
            Category = "Synapse";
            SubCategory = "Graphics";
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
            ChartData pd = new ChartData(keys, ChartType.Pie)
            {
                AppdVals = pct.ToArray(), // make a copy
                Colors = clrs.ToArray(),
            }; // doing this before shufflinig pct

            int[] idx = new int[nums.Count];
            for (int i = 0; i < idx.Length; i++)
                idx.SetValue(i, i);
            Array.Sort(pct, idx);
            pct = pct.Reverse().ToArray();
            idx = idx.Reverse().ToArray();
            float ex = s.Width;
            float ey = s.Height;
            float x0 = 0;
            float y0 = 0;
            double A = ex * ey;
            for (int i=0;i<pct.Length;i++)
            {
                double pp = pct[i];
                int id = idx[i];
                if (i == pct.Length - 1)
                {
                    // last rect, fill whatever is left
                    graphics.FillRectangle(clrs[id], x0, y0, s.Width-x0, s.Height-y0);
                }
                else if (ex > ey)
                {
                    double area = A * pp;
                    double rectx = area / ey;
                    graphics.FillRectangle(clrs[id], x0, y0, (float)rectx, ey);
                    ex -= (float)rectx; // remainder edge x length
                    x0 += (float)rectx; // move rect origin
                }
                else
                {
                    double area = A * pp;
                    double recty = area / ex;
                    graphics.FillRectangle(clrs[id], x0, y0, ex, (float)recty);
                    ey -= (float)recty; // remainder edge y length
                    y0 += (float)recty; // move rect origin
                }
            }
            graphics.Flush();

            ImageView graph = new ImageView() { Image = bitmap, };
            
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
                return Properties.Resources.regions;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("98cd3423-8f1e-454c-b12e-f03f45f491e1"); }
        }
    }
}