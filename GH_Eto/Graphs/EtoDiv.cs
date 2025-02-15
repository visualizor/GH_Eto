using System;
using System.Collections.Generic;
using System.Reflection;
using wf = System.Windows.Forms;
using GH_IO.Serialization;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.UI.Controls;
using Eto.Forms;

namespace Synapse
{
    public class EtoDiv : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the Divider class.
        /// </summary>
        public EtoDiv()
          : base("SnpDivider", "SDiv",
              "Divider line for organizing controls",
              "Synapse", "Graphics")
        {
        }

        protected bool labeled = true;

        public override void AppendAdditionalMenuItems(wf.ToolStripDropDown menu)
        {
            base.AppendAdditionalMenuItems(menu);
            try
            {
                wf.ToolStripMenuItem l = menu.Items.Add("With Label", null, OnLabeled) as wf.ToolStripMenuItem;
                l.Checked = labeled;
                l.ToolTipText = "Not all properties are shared between labeled and line-only versions";
            }
            catch (Exception) { }
        }

        public void OnLabeled(object s, EventArgs e)
        {
            labeled = !labeled;
            ExpireSolution(true);
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
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("All Properties", "A", "list of all accessible properties", GH_ParamAccess.list);
            pManager.AddGenericParameter("Control", "C", "control to go into a container or the listener", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<string> props = new List<string>();
            List<GH_ObjectWrapper> vals = new List<GH_ObjectWrapper>();
            DA.GetDataList(0, props);
            DA.GetDataList(1, vals);
            Message = labeled ? "Labeled" : "";

            Control div;
            if (labeled)
                div = new LabelSeparator();
            else
                div = new Divider();

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

                try { Util.SetProp(div, n, Util.GetGooVal(val)); }
                catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message); }
            }

            DA.SetData(1, new GH_ObjectWrapper(div));

            PropertyInfo[] allprops = div.GetType().GetProperties();
            List<string> printouts = new List<string>();
            foreach (PropertyInfo prop in allprops)
                if (prop.CanWrite)
                    printouts.Add(prop.Name + ": " + prop.PropertyType.ToString());
            DA.SetDataList(0, printouts);
        }

        public override bool Write(GH_IWriter writer)
        {
            writer.SetBoolean("labeled", labeled);
            return base.Write(writer);
        }
        public override bool Read(GH_IReader reader)
        {
            reader.TryGetBoolean("labeled", ref labeled);
            return base.Read(reader);
        }

        public override GH_Exposure Exposure => GH_Exposure.secondary;


        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.div;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("1CDC351F-85FE-4ED2-AC79-0DF5633B8095"); }
        }
    }
}