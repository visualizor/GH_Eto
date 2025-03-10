﻿using System;
using System.Collections.Generic;
using System.Reflection;
using Eto.Forms;
using Eto.Drawing;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using wf = System.Windows.Forms;
using GH_IO.Serialization;

namespace Synapse.Containers
{
    public class EtoTabView : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the EtoTabView class.
        /// </summary>
        public EtoTabView()
          : base("SnpTabbedView", "STabs",
              "tabbed view",
              "Synapse", "Containers")
        {
        }

        protected bool stretchy = false;

        protected void OnStretch(object s, EventArgs e)
        {
            stretchy = !stretchy;
            ExpireSolution(true);
        }

        public override void AppendAdditionalMenuItems(wf.ToolStripDropDown menu)
        {
            base.AppendAdditionalMenuItems(menu);
            try
            {
                menu.Items.Add(stretchy ? "Fix control sizes" : "Flex control sizes", null, OnStretch);
                wf.ToolStripMenuItem click = menu.Items.Add("List Properties", null, Util.OnListProps) as wf.ToolStripMenuItem;
                click.ToolTipText = "put all properties of this control in a check list";
            }
            catch (NullReferenceException) { }

            Util.ListPropLoc = Attributes.Pivot;
            TabControl dummy = new TabControl();
            Util.ListPropType = dummy.GetType();
            dummy.Dispose();
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Property", "P", "property to set", GH_ParamAccess.list);
            pManager[0].DataMapping = GH_DataMapping.Flatten;
            pManager[0].Optional = true;
            pManager.AddGenericParameter("Property Value", "V", "values for the property", GH_ParamAccess.list);
            pManager[1].DataMapping = GH_DataMapping.Flatten;
            pManager[1].Optional = true;
            pManager.AddGenericParameter("Controls", "C", "controls to go onto each tabs\nuse a container if there are more than one controls on a tab", GH_ParamAccess.list);
            pManager[2].DataMapping = GH_DataMapping.Flatten;
            pManager[2].Optional = true;
            pManager.AddTextParameter("TabName", "N", "the name of tabs the controls go onto\nthis should match list order with \"C\"", GH_ParamAccess.list);
            pManager[3].DataMapping = GH_DataMapping.Flatten;
            pManager[3].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("All Properties", "A", "list of all accessible properties", GH_ParamAccess.list);
            pManager.AddGenericParameter("Control", "C", "container control that houses other controls", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<string> props = new List<string>();
            List<GH_ObjectWrapper> vals = new List<GH_ObjectWrapper>();
            List<GH_ObjectWrapper> ctrls = new List<GH_ObjectWrapper>();
            List<string> tabs = new List<string>();
            DA.GetDataList(0, props);
            DA.GetDataList(1, vals);
            DA.GetDataList(2, ctrls);
            DA.GetDataList(3, tabs);
            Message = stretchy ? "ResizeCtrl" : "FixedCtrl";


            TabControl tabview = new TabControl() { ID = Guid.NewGuid().ToString(), };
            for (int i = 0; i < tabs.Count; i++)
            {
                TabPage tp = new TabPage() { Text = tabs[i], };
                try
                {
                    if (ctrls[i].Value is Control ctrl)
                    {
                        DynamicLayout layout = new DynamicLayout();
                        layout.AddAutoSized(ctrl,xscale:stretchy,yscale:stretchy);
                        tp.Content = layout;
                    }
                }
                catch (Exception ex)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, ex.Message);
                }
                tabview.Pages.Add(tp);
            }

            for (int i = 0; i < props.Count; i++)
            {
                string n = props[i];
                object val;
                try { val = vals[i].Value; }
                catch (ArgumentOutOfRangeException)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "P, V should correspond each other");
                    return;
                }

                if (n.ToLower() == "backgroundcolor" || n.ToLower() == "color" || n.ToLower() == "background color")
                {
                    if (val is GH_Colour gclr)
                        tabview.BackgroundColor = Color.FromArgb(gclr.Value.ToArgb());
                    else if (val is GH_String cstr)
                    {
                        if (Color.TryParse(cstr.Value, out Color clr))
                            tabview.BackgroundColor = clr;
                    }
                    else if (val is GH_Point pt)
                    {
                        Color clr = Color.FromArgb((int)pt.Value.X, (int)pt.Value.Y, (int)pt.Value.Z);
                        tabview.BackgroundColor = clr;
                    }
                    else if (val is GH_Vector vec)
                    {
                        Color clr = Color.FromArgb((int)vec.Value.X, (int)vec.Value.Y, (int)vec.Value.Z);
                        tabview.BackgroundColor = clr;
                    }
                    else if (val is Color etoclr)
                        tabview.BackgroundColor = etoclr;
                    else
                        try { Util.SetProp(tabview, "BackgroundColor", Util.GetGooVal(val)); }
                        catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message); }
                }
                else
                    try { Util.SetProp(tabview, n, Util.GetGooVal(val)); }
                    catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message); }
            }

            DA.SetData(1, new GH_ObjectWrapper(tabview));

            PropertyInfo[] allprops = tabview.GetType().GetProperties();
            List<string> printouts = new List<string>();
            foreach (PropertyInfo prop in allprops)
                if (prop.CanWrite)
                    printouts.Add(prop.Name + ": " + prop.PropertyType.ToString());
            DA.SetDataList(0, printouts);
        }

        public override bool Write(GH_IWriter writer)
        {
            writer.SetBoolean("stretchy", stretchy);
            return base.Write(writer);
        }
        public override bool Read(GH_IReader reader)
        {
            reader.TryGetBoolean("stretchy", ref stretchy);
            return base.Read(reader);
        }


        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.tabbed;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("2A8ADE80-3E75-47C4-9BFF-009C2111BB99"); }
        }
    }
}