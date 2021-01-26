using System;
using Eto;
using Eto.Forms;
using Eto.Drawing;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

using Grasshopper.Kernel;
using Grasshopper;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel.Special;
using Grasshopper.Kernel.Parameters;
using Rhino.Geometry;

// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace GH_Eto
{
    public class EtoWindow : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public EtoWindow()
          : base("EtoWindow", "Window",
              "mian window",
              "EtoGH", "Containers")
        {
        }

        internal Form EWindow = null;
        protected void SetProp(object subject, string pname, object pval)
        {
            PropertyInfo prop = subject.GetType().GetProperty(pname);
            prop.SetValue(subject, pval);
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Show", "S", "true to show window", GH_ParamAccess.item, false);
            pManager.AddTextParameter("Property", "P", "property of window to set", GH_ParamAccess.list);
            pManager[1].DataMapping = GH_DataMapping.Flatten;
            pManager[1].Optional = true;
            pManager.AddGenericParameter("Property Value", "V", "what to set to for the property", GH_ParamAccess.list);
            pManager[2].DataMapping = GH_DataMapping.Flatten;
            pManager[2].Optional = true;
            pManager.AddGenericParameter("Contents", "C", "contents to include on this window", GH_ParamAccess.item);
            pManager[3].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool run = false;
            DA.GetData(0, ref run);
            List<string> props = new List<string>();
            bool param1 = DA.GetDataList(1, props);
            List<GH_ObjectWrapper> vals = new List<GH_ObjectWrapper>();
            bool param2 = DA.GetDataList(2, vals);
            GH_ObjectWrapper ghobj = new GH_ObjectWrapper();
            DA.GetData(3, ref ghobj);

            if (param1 && param2)
                if (props.Count!=vals.Count)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "property list and value list must match");
                    return;
                }

            if (EWindow == null)
                EWindow = new Form()
                {
                    Height = 200,
                    Width = 400,
                    Title = "an Eto window",
                };
            else if (EWindow.Visible)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "window already shown\n close first for changes");
                return;
            }

            for (int i = 0; i < props.Count; i++)
            {
                string n = props[i];
                var val = vals[i].Value;
                if (n=="Size" || n == "size")
                {
                    if (val is Size winsize)
                    {
                        SetProp(EWindow, "Size", winsize);
                    }
                    else if (val is Point3d pt)
                    {
                        winsize = new Size((int)pt.X, (int)pt.Y);
                        SetProp(EWindow, "Size", winsize);
                    }
                    else if (val is Vector3d vec)
                    {
                        winsize = new Size((int)vec.X, (int)vec.Y);
                        SetProp(EWindow, "Size", winsize);
                    }
                    else if (val is string sstr)
                    {
                        string[] xy = sstr.Split(',');
                        bool xp = int.TryParse(xy[0], out int x);
                        bool yp = int.TryParse(xy[1], out int y);
                        if (xp && yp)
                            SetProp(EWindow, "Size", new Size(x,y));
                    }
                    else
                        try { SetProp(EWindow, "Size", val); }
                        catch (Exception ex){ AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message); }
                }
                else if (n=="Title" || n == "title")
                {
                    SetProp(EWindow, "Title", val.ToString());
                }
                else if (n=="Opacity" || n == "opacity" || n=="Transparency" || n=="transparency")
                {
                    if (val is double perct)
                        SetProp(EWindow, "Opacity", perct);
                    else if (val is string pstr)
                    {
                        if (double.TryParse(pstr, out perct))
                            SetProp(EWindow, "Opacity", perct);
                    }
                    else
                        try { SetProp(EWindow, "Opacity", val); }
                        catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message); }
                }
                else if (n=="BackgroundColor" || n=="background color" || n=="backgroundcolor" || n=="backgroundColor" || n=="Color" || n == "color")
                {
                    if (val is Color clr)
                        SetProp(EWindow, "BackgroundColor", clr);
                    else if (val is string cstr)
                    {
                        if (Color.TryParse(cstr, out clr))
                            SetProp(EWindow, "BackgroundColor", clr);
                    }
                    else if (val is Point3d pt)
                    {
                        clr = Color.FromArgb((int)pt.X, (int)pt.Y, (int)pt.Z);
                        SetProp(EWindow, "BackgroundColor", clr);
                    }
                    else if (val is Vector3d vec)
                    {
                        clr = Color.FromArgb((int)vec.X, (int)vec.Y, (int)vec.Z);
                        SetProp(EWindow, "BackgroundColor", clr);
                    }
                    else if (val is IEnumerable<int> nums)
                    {
                        int[] c = nums.ToArray();
                        if (c.Length==3)
                        {
                            clr = Color.FromArgb(c[0], c[1], c[2]);
                            SetProp(EWindow, "BackgroundColor", clr);
                        }
                        else if (c.Length == 4)
                        {
                            clr = Color.FromArgb(c[0], c[1], c[2], c[3]);
                            SetProp(EWindow, "BackgroundColor", clr);
                        }
                    }
                    else
                        try { SetProp(EWindow, "BackgroundColor", val); }
                        catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message); }
                }
                else if (n=="Loaction" || n=="location" || n=="position" || n == "Position")
                {
                    if (val is Point3d pt)
                        SetProp(EWindow, "Location", new Size((int)pt.X, (int)pt.Y));
                    else if (val is Vector3d vec)
                        SetProp(EWindow, "Location", new Size((int)vec.X, (int)vec.Y));
                    else if (val is string locstr)
                    {
                        if (Point3d.TryParse(locstr, out pt))
                            SetProp(EWindow, "Location", new Size((int)pt.X, (int)pt.Y));
                    }
                    else if (val is Rhino.Geometry.Point rhpt)
                        SetProp(EWindow, "Location", new Size((int)rhpt.Location.X, (int)rhpt.Location.Y);
                    else
                        try { SetProp(EWindow, "Location", val); }
                        catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message); }
                }
                else if (n == "Padding" || n == "padding")
                {
                    if (val is Point3d pt)
                        SetProp(EWindow, "Padding", new Padding((int)pt.X, (int)pt.Y));
                    else if (val is Vector3d vec)
                        SetProp(EWindow, "Padding", new Padding((int)vec.X, (int)vec.Y));
                    else if (val is string pstr)
                    {
                        string[] subs = pstr.Split(',');
                        if (subs.Length == 2)
                        {
                            bool i0 = int.TryParse(subs[0], out int n0);
                            bool i1 = int.TryParse(subs[1], out int n1);
                            if (i0 && i1)
                                SetProp(EWindow, "Padding", new Padding(n0, n1));
                        }
                        else if (subs.Length ==4)
                        {
                            bool i0 = int.TryParse(subs[0], out int n0);
                            bool i1 = int.TryParse(subs[1], out int n1);
                            bool i2 = int.TryParse(subs[2], out int n2);
                            bool i3 = int.TryParse(subs[3], out int n3);
                            if (i0 && i1 && i2 && i3)
                                SetProp(EWindow, "Padding", new Padding(n0, n1, n2,n3));
                        }
                    }
                    else if (val is IEnumerable<int> nums)
                    {
                        int[] pd = nums.ToArray();
                        if (pd.Length==2)
                            SetProp(EWindow, "Padding", new Padding(pd[0], pd[1]));
                        else if (pd.Length==4)
                            SetProp(EWindow, "Padding", new Padding(pd[0], pd[1], pd[2], pd[3]));
                    }
                    else if (val is int pad)
                        SetProp(EWindow, "Padding", new Padding(pad));
                    else
                        try { SetProp(EWindow, "Padding", val); }
                        catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message); }
                }
                else if (n=="WindowStyle" || n=="window style")
                {

                }
            }
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("5295c8bb-d964-4605-b625-269a1672396d"); }
        }
    }
}
