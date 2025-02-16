using System;
using System.Collections.Generic;
using wdrw = System.Drawing;
using Eto.Forms;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Grasshopper.Kernel.Types;

namespace Synapse
{
    public class EtoBars_ : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the EtoBars_ class.
        /// </summary>
        public EtoBars_()
          : base("SnpHistogram", "SnpHisto",
              "experiment with pair init",
              "Synapse", "Graphics")
        { //this object is to test pair initialization!
        }

        public override void AddedToDocument(GH_Document document)
        {
            base.AddedToDocument(document);
            var lochere = Attributes.Pivot;
            wdrw.PointF loc = new wdrw.PointF(lochere.X-150, lochere.Y);
            Guid cid = new Guid("0644169d-4da6-41c7-88c0-ff0eaa9663e4");
            //guid can be hard coded. just look for it in the comp to be pair-init'd
            GH_Component addedcomp = Util.CtrlCanvasInit(loc, cid) as GH_Component;
            Params.Input[0].AddSource(addedcomp.Params.Output[0]);
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Graph", "C","histogram graph object", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_ObjectWrapper gobj = null;
            DA.GetData(0, ref gobj);
            if (gobj.Value is ImageView img)
                AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "ok");
        }


        public override GH_Exposure Exposure => GH_Exposure.hidden;

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.bars;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("E245FF19-A3D7-46C7-A0EF-A31AA447A94D"); }
        }
    }
}