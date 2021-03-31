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
          : base("SynapseOverride", "SetVal",
              "override control values\nthis lets you set the primary value of a control without repainting the entire window",
              "Synapse", "Parameters")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Control", "C", "controls to set value for", GH_ParamAccess.list);
            pManager.AddGenericParameter("Value", "V", "value - depending on control type, this input is automatically parsed as best as it can", GH_ParamAccess.list);
            pManager[0].DataMapping = GH_DataMapping.Flatten;
            pManager[1].DataMapping = GH_DataMapping.Flatten;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBooleanParameter("Success", "S", "whether the value was set successfully", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<GH_ObjectWrapper> v = new List<GH_ObjectWrapper>();
            List<GH_ObjectWrapper> c = new List<GH_ObjectWrapper>();
            DA.GetDataList(0, c);
            DA.GetDataList(1, v);
            bool[] r = new bool[c.Count];

            for(int i = 0; i < c.Count; i++)
            {
                if (c[i].Value is Control ctrl)
                {
                    GH_ObjectWrapper v_wrapped;
                    try { v_wrapped = v[i]; }
                    catch (Exception ex)
                    {
                        if (ex is IndexOutOfRangeException || ex is ArgumentOutOfRangeException)
                            v_wrapped = v.Last();
                        else
                            throw ex;
                    }

                    if (ctrl is TextBox tb)
                        if (v_wrapped.Value is GH_String gstr)
                        {
                            tb.Text = gstr.Value;
                            r.SetValue(true, i);
                        }
                        else
                            r.SetValue(false, i);
                    else if (ctrl is Button btn)
                        if (v_wrapped.Value is GH_String gstr)
                        {
                            btn.Text = gstr.Value;
                            r.SetValue(true, i);
                        }
                        else
                            r.SetValue(false, i);
                    else if (ctrl is Label lbl)
                        if (v_wrapped.Value is GH_String gstr)
                        {
                            lbl.Text = gstr.Value;
                            r.SetValue(true, i);
                        }
                        else
                            r.SetValue(false, i);
                    else if (ctrl is CheckBox cb)
                        if (v_wrapped.Value is GH_Boolean gb)
                        {
                            cb.Checked = gb.Value;
                            r.SetValue(true, i);
                        }
                        else
                            r.SetValue(false, i);
                    else if (ctrl is ComboSlider csl)
                        if (v_wrapped.Value is GH_Integer gint)
                        {
                            csl.slider.Value = (int)(gint.Value / csl.coef);
                            r.SetValue(true, i);
                        }
                        else if (v_wrapped.Value is GH_Number gn)
                        {
                            csl.slider.Value = (int)(gn.Value / csl.coef);
                            r.SetValue(true, i);
                        }
                        else
                            r.SetValue(false, i);
                    else
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "not a control that can be set");
                        r.SetValue(false, i);
                    }
                        
                }
                else
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "not a valid control");
                    r.SetValue(false, i);
                }
            }

            DA.SetDataList(0, r);
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
            get { return new Guid("bb66300f-c96e-4253-9c8a-a9faa341c052"); }
        }
    }
}