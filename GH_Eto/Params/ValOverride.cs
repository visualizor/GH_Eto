using System;
using System.Collections.Generic;
using Eto.Forms;
using System.Linq;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

namespace Synapse
{
    public class ValOverride : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the ValOverride class.
        /// </summary>
        public ValOverride()
          : base("SnpOverride", "SSetVal",
              "override control values without re-instantiating the entire window",
              "Synapse", "Parameters")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Property", "P", "Property names of the control to alter", GH_ParamAccess.list);
            pManager.AddGenericParameter("Value", "V", "Values to set as the properties\nMatch P list\nEnforce data types - refer to A outputs on components", GH_ParamAccess.list);
            pManager.AddGenericParameter("Control","S","Any Synapse component\nMostly for controls", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBooleanParameter("Success", "S", "whether the value was set successfully\nfailure typically caused by wrong data type i.e. feeding an integer type into a text type", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<GH_ObjectWrapper> v = new List<GH_ObjectWrapper>();
            List<string> p = new List<string>();
            GH_ObjectWrapper gobj = null;
            DA.GetDataList(0, p);
            DA.GetDataList(1, v);
            DA.GetData(2, ref gobj);
            bool[] r = new bool[p.Count];

            if (gobj.Value is Control ctrl)
            {
                for (int i = 0; i < p.Count; i++)
                {
                    string n = p[i];
                    object val;
                    try { val = v[i].Value; }
                    catch (ArgumentOutOfRangeException)
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "P, V should correspond each other");
                        return;
                    }

                    try 
                    { 
                        Util.SetProp(ctrl, n, Util.GetGooVal(val));
                        r.SetValue(true, i);
                    }
                    catch (Exception ex) 
                    { 
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message); 
                        r.SetValue(false, i);
                    }
                }
                DA.SetDataList(0, r);
            }
            else
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "object not recognized as a valid Synapse component");
                return;
            }

            
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources._override;
            }
        }

        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.secondary | GH_Exposure.obscure; }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("BC63AD12-A0C4-4DC2-AB35-8A57B2C6F722"); }
        }
    }
}