using System;
using System.Reflection;
using Eto.Forms;
using Eto.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using wf = System.Windows.Forms;
using GH_IO.Serialization;
using Grasshopper;

// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace Synapse
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
          : base("SynapseWindow", "SWindow",
              "main window",
              "Synapse", "Containers")
        {
        }

        internal Form EWindow = null;
        private void EWClosing(object s, CancelEventArgs e)
        {
            if (OnPingDocument() is null)
                return; // no need to re-instantiate when ghdoc is closed

            EWindow = new Form()
            {
                Height = 200,
                Width = 400,
                Title = "an Eto window",
            };
            EWindow.Closing += EWClosing;
            EWindow.GotFocus += GhDocCheck;
        }
        private void GhDocCheck(object s, EventArgs e)
        {
            if (s is Form ew)
                if (OnPingDocument() is null)
                {
                    MessageBox.Show(ew, "Grasshopper document owning this window has gone missing\nClosing soon...", "Forced Shutdown", MessageBoxType.Information);
                    ew.Close();
                }
            
        }
        internal bool ctrlfill = false;
        internal bool ctrlredo = false;

        private void OnFill(object s, EventArgs e)
        {
            ctrlfill = !ctrlfill;
            ExpireSolution(false);
        }
        private void OnRepaint(object s, EventArgs e)
        {
            ctrlredo = !ctrlredo;
            ExpireSolution(false);
        }
        public override void AppendAdditionalMenuItems(wf.ToolStripDropDown menu)
        {
            base.AppendAdditionalMenuItems(menu);
            // TODO: this uses winforms, may not work on Mac OS
            try
            {
                wf.ToolStripMenuItem i = menu.Items.Add("Fill Window", null, OnFill) as wf.ToolStripMenuItem;
                i.ToolTipText = "check to have all controls fill to the border of this window";
                i.Checked = ctrlfill;
                wf.ToolStripMenuItem m = menu.Items.Add("Repaint", null, OnRepaint) as wf.ToolStripMenuItem;
                m.ToolTipText = "check to allow instant control updates\ncontrol values not guaranteed to stay";
                m.Checked = ctrlredo;
            }
            catch { }
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Show", "S", "set on true to show window", GH_ParamAccess.item, false);
            pManager.AddTextParameter("Property", "P", "property of window to set", GH_ParamAccess.list);
            pManager[1].DataMapping = GH_DataMapping.Flatten;
            pManager[1].Optional = true;
            pManager.AddGenericParameter("Property Value", "V", "what to set to for the property", GH_ParamAccess.list);
            pManager[2].DataMapping = GH_DataMapping.Flatten;
            pManager[2].Optional = true;
            pManager.AddGenericParameter("Contents", "C", "contents to include on this window", GH_ParamAccess.list);
            pManager[3].DataMapping = GH_DataMapping.Flatten;
            pManager[3].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("All Properties", "A", "list of all accessible properties", GH_ParamAccess.list);
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
            List<GH_ObjectWrapper> contents = new List<GH_ObjectWrapper>();
            DA.GetDataList(3, contents);

            if (EWindow == null)
            {
                EWindow = new Form()
                {
                    Height = 200,
                    Width = 400,
                    Title = "an Eto window",
                };
                EWindow.Closing += EWClosing;
                EWindow.GotFocus += GhDocCheck;
            }
            else if (EWindow.Visible && !run)
            {
                EWindow.Close();
                return;
            }
            else if (EWindow.Visible && run)
            {
                if (!ctrlredo)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, " open window will not be modified unless \"Repaint\" is unchecked");
                    return;
                }
                else
                {
                    EWindow.Content = null;
                }
            }
            
            //EWindow.Owner = Instances.EtoDocumentEditor;

            for (int i = 0; i < props.Count; i++)
            {
                string n = props[i];
                object val;
                try { val = vals[i].Value; }
                catch(ArgumentOutOfRangeException)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "P, V should correspond each other");
                    return;
                }

                if (n.ToLower() == "size")
                {
                    if (val is GH_Point pt)
                    {
                        Size winsize = new Size((int)pt.Value.X, (int)pt.Value.Y);
                        EWindow.Size = winsize;
                    }
                    else if (val is GH_Vector vec)
                    {
                        Size winsize = new Size((int)vec.Value.X, (int)vec.Value.Y);
                        EWindow.Size = winsize;
                    }
                    else if (val is GH_String sstr)
                    {
                        string[] xy = sstr.Value.Split(',');
                        bool xp = int.TryParse(xy[0], out int x);
                        bool yp = int.TryParse(xy[1], out int y);
                        if (xp && yp)
                            EWindow.Size = new Size(x,y);
                    }
                    else if (val is GH_Rectangle grec)
                    {
                        int x = (int)grec.Value.X.Length;
                        int y = (int)grec.Value.Y.Length;
                        EWindow.Size = new Size(x, y);
                    }
                    else
                        try { Util.SetProp(EWindow, "Size", Util.GetGooVal(val)); }
                        catch (Exception ex){ AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message); }
                }
                else if (n.ToLower() == "title")
                {
                    Util.SetProp(EWindow, "Title", Util.GetGooVal(val).ToString());
                }
                else if (n.ToLower() == "opacity" || n.ToLower()=="transparency")
                {
                    if (val is GH_Number perct)
                        Util.SetProp(EWindow, "Opacity", perct.Value);
                    else if (val is GH_String pstr)
                    {
                        if (double.TryParse(pstr.Value, out double pc))
                            Util.SetProp(EWindow, "Opacity", pc);
                    }
                    else
                        try { Util.SetProp(EWindow, "Opacity", Util.GetGooVal(val)); }
                        catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message); }
                }
                else if (n.ToLower()=="backgroundcolor" || n.ToLower()=="color" || n.ToLower()=="background color")
                {
                    if (val is GH_Colour gclr)
                        EWindow.BackgroundColor = Color.FromArgb(gclr.Value.ToArgb());
                    else if (val is GH_String cstr)
                    {
                        if (Color.TryParse(cstr.Value, out Color clr))
                            EWindow.BackgroundColor = clr;
                    }
                    else if (val is GH_Point pt)
                    {
                        Color clr = Color.FromArgb((int)pt.Value.X, (int)pt.Value.Y, (int)pt.Value.Z);
                        EWindow.BackgroundColor = clr;
                    }
                    else if (val is GH_Vector vec)
                    {
                        Color clr = Color.FromArgb((int)vec.Value.X, (int)vec.Value.Y, (int)vec.Value.Z);
                        EWindow.BackgroundColor = clr;
                    }
                    else if (val is IEnumerable<int> nums)
                    {
                        int[] c = nums.ToArray();
                        if (c.Length == 3)
                        {
                            Color clr = Color.FromArgb(c[0], c[1], c[2]);
                            EWindow.BackgroundColor = clr;
                        }
                        else if (c.Length == 4)
                        {
                            Color clr = Color.FromArgb(c[0], c[1], c[2], c[3]);
                            EWindow.BackgroundColor = clr;
                        }
                    }
                    else if (val is Color etoclr)
                        EWindow.BackgroundColor = etoclr;
                    else
                        try { Util.SetProp(EWindow, "BackgroundColor", Util.GetGooVal(val)); }
                        catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message); }
                }
                else if (n.ToLower()=="location" || n.ToLower()=="position")
                {
                    if (val is GH_Point pt)
                        Util.SetProp(EWindow, "Location", new Eto.Drawing.Point((int)pt.Value.X, (int)pt.Value.Y));
                    else if (val is GH_Vector vec)
                        Util.SetProp(EWindow, "Location", new Eto.Drawing.Point((int)vec.Value.X, (int)vec.Value.Y));
                    else if (val is GH_String locstr)
                    {
                        if (Point3d.TryParse(locstr.Value, out Point3d rhpt))
                            Util.SetProp(EWindow, "Location", new Eto.Drawing.Point((int)rhpt.X, (int)rhpt.Y));
                    }
                    else
                        try { Util.SetProp(EWindow, "Location", Util.GetGooVal(val)); }
                        catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message); }
                }
                else if (n.ToLower() == "padding")
                {
                    if (val is GH_Point pt)
                        Util.SetProp(EWindow, "Padding", new Eto.Drawing.Padding((int)pt.Value.X, (int)pt.Value.Y));
                    else if (val is GH_Vector vec)
                        Util.SetProp(EWindow, "Padding", new Eto.Drawing.Padding((int)vec.Value.X, (int)vec.Value.Y));
                    else if (val is GH_String pstr)
                    {
                        string[] subs = pstr.Value.Split(',');
                        if (subs.Length == 2)
                        {
                            bool i0 = int.TryParse(subs[0], out int n0);
                            bool i1 = int.TryParse(subs[1], out int n1);
                            if (i0 && i1)
                                Util.SetProp(EWindow, "Padding", new Eto.Drawing.Padding(n0, n1));
                        }
                        else if (subs.Length ==4)
                        {
                            bool i0 = int.TryParse(subs[0], out int n0);
                            bool i1 = int.TryParse(subs[1], out int n1);
                            bool i2 = int.TryParse(subs[2], out int n2);
                            bool i3 = int.TryParse(subs[3], out int n3);
                            if (i0 && i1 && i2 && i3)
                                Util.SetProp(EWindow, "Padding", new Eto.Drawing.Padding(n0, n1, n2,n3));
                        }
                    }
                    else if (val is GH_Integer pad)
                        Util.SetProp(EWindow, "Padding", new Eto.Drawing.Padding(pad.Value));
                    else
                        try { Util.SetProp(EWindow, "Padding", Util.GetGooVal(val)); }
                        catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message); }
                }
                else if (n=="WindowStyle" || n=="window style")
                {
                    if (val is GH_Boolean b)
                    {
                        WindowStyle style;
                        if (b.Value) style = WindowStyle.Default;
                        else style = WindowStyle.None;
                        Util.SetProp(EWindow, "WindowStyle", style);
                    }
                    else if (val is GH_String bstr)
                    {
                        if (int.TryParse(bstr.Value, out int ws))
                            switch (ws)
                            {
                                case 0:
                                    Util.SetProp(EWindow, "WindowStyle", WindowStyle.Default);
                                    break;
                                case 1:
                                    Util.SetProp(EWindow, "WindowStyle", WindowStyle.None);
                                    break;
                                case 2:
                                    Util.SetProp(EWindow, "WindowStyle", WindowStyle.Utility);
                                    break;
                                default:
                                    break;
                            }
                    }
                    else if (val is GH_Integer inum)
                        Util.SetProp(EWindow, "WindowStyle", inum.Value);
                    else if (val is GH_Number num)
                        Util.SetProp(EWindow, "WindowStyle", (int)num.Value);
                    else
                        try { Util.SetProp(EWindow, "WindowStyle", Util.GetGooVal(val)); }
                        catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message); }
                }
                else
                    try { Util.SetProp(EWindow, n, Util.GetGooVal(val)); }
                    catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message); }
            }

            DynamicLayout bucket = new DynamicLayout();
            try
            {
                foreach (GH_ObjectWrapper ghobj in contents)
                    if (ghobj.Value is Control ctrl)
                        if (ctrlfill)
                            bucket.Add(ctrl);
                        else
                            bucket.AddAutoSized(ctrl);
            }
            catch (Exception ex)
            {
                if (ex is NullReferenceException || ex is ArgumentNullException)
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, " invalid controls detected");
                else throw ex;
            }
            Scrollable content = new Scrollable { Content = bucket, Border = BorderType.None };
            EWindow.Content = content;

            if (run)
                EWindow.Show();

            PropertyInfo[] allprops = EWindow.GetType().GetProperties();
            List<string> printouts = new List<string>();
            foreach (PropertyInfo prop in allprops)
                if (prop.CanWrite)
                    printouts.Add(prop.Name + ": " + prop.PropertyType.ToString());
            DA.SetDataList(0, printouts);
        }


        public override bool Write(GH_IWriter writer)
        {
            writer.SetBoolean("ctrlfill", ctrlfill);
            writer.SetBoolean("ctrlredo", ctrlredo);
            return base.Write(writer);
        }
        public override bool Read(GH_IReader reader)
        {
            reader.TryGetBoolean("ctrlfill", ref ctrlfill);
            reader.TryGetBoolean("ctrlredo", ref ctrlredo);
            return base.Read(reader);
        }

        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.secondary; }
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.window;
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
