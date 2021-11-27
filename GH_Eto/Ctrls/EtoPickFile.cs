using System;
using System.Collections.Generic;
using System.Reflection;
using Eto.Forms;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

namespace Synapse
{
    public class EtoPickFile : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the EtoPickFile class.
        /// </summary>
        public EtoPickFile()
          : base("SnpFilePicker", "SFile",
              "pick file on hard drive",
              "Synapse", "Controls")
        {
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

            FilePicker picker = new FilePicker() { ID = Guid.NewGuid().ToString(), };

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

                if (n.ToLower() == "filetype" || n.ToLower()=="extension" || n.ToLower() == "currentfilter")
                {
                    if (val is GH_String gs)
                        try
                        {
                            string[] n_e = gs.Value.Split('|');
                            string[] exts = n_e[1].Split(',');
                            picker.CurrentFilter = new FileFilter(n_e[0], exts);
                        }
                        catch (Exception ex)
                        {
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, " string cannot be interpreted");
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message);
                        }
                    else
                        try{Util.SetProp(picker, "CurrentFilter", val);}
                        catch (Exception ex){AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message);}
                }
                else if (n.ToLower() == "filterindex" || n.ToLower() == "currentfilterindex")
                {
                    if (val is GH_Integer gi)
                        try { picker.CurrentFilterIndex = gi.Value; }
                        catch { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, " filter index error"); }
                    else if (val is GH_Number gn)
                        try { picker.CurrentFilterIndex = (int)Math.Round(gn.Value, 0); }
                        catch { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, " filter index error"); }
                    else if (val is GH_Interval gitvl)
                        try { picker.CurrentFilterIndex = (int)Math.Round(gitvl.Value.Length, 0); }
                        catch { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, " filter index error"); }
                    else
                        try { Util.SetProp(picker, "CurrentFilterIndex", val); }
                        catch { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, " filter index error"); }
                }
                else if (n.ToLower() == "filters" | n.ToLower() == "extensions" | n.ToLower() == "allfiletypes")
                {
                    if (val is GH_String gs)
                        try
                        {
                            string[] lines = gs.Value.Split('\n');
                            for (int j = 0; j < lines.Length; j++)
                            {
                                string[] n_e = lines[j].Split('|');
                                string[] exts = n_e[1].Split(',');
                                picker.Filters.Add(new FileFilter(n_e[0], exts));
                            }
                        }
                        catch (Exception ex)
                        {
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, " string cannot be interpreted");
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message);
                        }
                    else if (val is FileFilter[] ffs)
                        foreach (FileFilter f in ffs)
                            picker.Filters.Add(f);
                    else if (val is FileFilter f)
                        picker.Filters.Add(f);
                    else
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, " cannot set filters; use a multiline text string like this:\n  Text|.txt\n  3D File|.3dm,.obj,.skp\n  PDF|.pdf");
                }
                else
                    try { Util.SetProp(picker, n, Util.GetGooVal(val)); }
                    catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message); }
            }

            DA.SetData(1, new GH_ObjectWrapper(picker));

            PropertyInfo[] allprops = picker.GetType().GetProperties();
            List<string> printouts = new List<string>();
            foreach (PropertyInfo prop in allprops)
                if (prop.CanWrite)
                    printouts.Add(prop.Name + ": " + prop.PropertyType.ToString());
            printouts.Add("Filters: string");
            DA.SetDataList(0, printouts);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.filepicker;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("7183ae55-a6ba-4f17-b624-aaa152b9e9c0"); }
        }
    }
}