using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Parameters;
using Eto.Drawing;
using Eto.Forms;

namespace Synapse.Params
{
    public class EtoIcon : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the EtoIcon class.
        /// </summary>
        public EtoIcon()
          : base("SnpIcon", "SIcon",
              "image icon that can be used by Synapse controls",
              "Synapse", "Parameters")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("File Location", "F", "file location for the image", GH_ParamAccess.item);
            pManager.AddGenericParameter("Size", "S", "optional size to fit image\nirrelevant if T is set to bitmap", GH_ParamAccess.item);
            pManager[1].Optional = true;
            pManager.AddIntegerParameter("Format", "T", "use the image for display(0), as icon(1) or simply bitmap type(2)", GH_ParamAccess.item, 0);
            Param_Integer pint = pManager[2] as Param_Integer;
            pint.AddNamedValue("Display", 0);
            pint.AddNamedValue("Icon", 1);
            pint.AddNamedValue("Bitmap", 2);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Image Icon", "I", "image icon", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string pth = "";
            GH_ObjectWrapper sizeobj = new GH_ObjectWrapper();
            int t = 0;
            DA.GetData(0, ref pth);
            bool issized = DA.GetData(1, ref sizeobj);
            DA.GetData(2, ref t);

            if (t>2 || t < 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Format(T) index must be 0, 1 or 2");
                return;
            }
            Bitmap bitmap = new Bitmap(pth);
            Icon icon = new Icon(pth);

            switch (t)
            {
                case 0:
                    if (!issized)
                        DA.SetData(0, new GH_ObjectWrapper((Control)bitmap));
                    else
                    {
                        if (sizeobj.Value is Size es)
                            DA.SetData(0, new GH_ObjectWrapper((Control)bitmap.WithSize(es)));
                        else if (sizeobj.Value is GH_ComplexNumber ri)
                            DA.SetData(0, new GH_ObjectWrapper((Control)bitmap.WithSize(new Size((int)ri.Value.Real, (int)ri.Value.Imaginary))));
                        else if (sizeobj.Value is GH_Vector gv)
                            DA.SetData(0, new GH_ObjectWrapper((Control)bitmap.WithSize(new Size((int)gv.Value.X, (int)gv.Value.Y))));
                        else if (sizeobj.Value is GH_Point gp)
                            DA.SetData(0, new GH_ObjectWrapper((Control)bitmap.WithSize(new Size((int)gp.Value.X, (int)gp.Value.Y))));
                        else if (sizeobj.Value is GH_Rectangle grec)
                            DA.SetData(0, new GH_ObjectWrapper((Control)bitmap.WithSize(new Size((int)grec.Value.X.Length, (int)grec.Value.Y.Length))));
                        else
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, " check size input data type");
                    }
                    break;
                case 1:
                    if (!issized)
                        DA.SetData(0, new GH_ObjectWrapper(icon));
                    else
                    {
                        if (sizeobj.Value is Size es)
                            DA.SetData(0, new GH_ObjectWrapper(icon.WithSize(es)));
                        else if (sizeobj.Value is GH_ComplexNumber ri)
                            DA.SetData(0, new GH_ObjectWrapper(icon.WithSize(new Size((int)ri.Value.Real, (int)ri.Value.Imaginary))));
                        else if (sizeobj.Value is GH_Vector gv)
                            DA.SetData(0, new GH_ObjectWrapper(icon.WithSize(new Size((int)gv.Value.X, (int)gv.Value.Y))));
                        else if (sizeobj.Value is GH_Point gp)
                            DA.SetData(0, new GH_ObjectWrapper(icon.WithSize(new Size((int)gp.Value.X, (int)gp.Value.Y))));
                        else if (sizeobj.Value is GH_Rectangle grec)
                            DA.SetData(0, new GH_ObjectWrapper(icon.WithSize(new Size((int)grec.Value.X.Length, (int)grec.Value.Y.Length))));
                        else
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, " check size input data type");
                    }
                    break;
                case 2:
                    DA.SetData(0, new GH_ObjectWrapper(bitmap));
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get { return Properties.Resources.etoimg;}
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("3a863a33-e0f9-4a81-bebb-76346790aef3"); }
        }
    }
}