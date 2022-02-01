using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.DocObjects;
using Rhino.Commands;
using Grasshopper.Kernel.Parameters;
using Rhino.Input;

namespace Synapse
{
    public class RHSelect : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the RHSelect class.
        /// </summary>
        public RHSelect()
          : base("RHSelect", "Select",
              "trigger a selection from Rhino viewport by mouse click",
              "Synapse", "Parameters")
        {
            run = false;
            running = false;
            otype = ObjectType.AnyObject;
            picked = null;
        }

        protected bool run;
        protected bool running;
        protected ObjectType otype;
        protected ObjRef[] picked;

        protected async void Trigger(string txt = "select objects for Synapse", bool nothing = true)
        {
            await Task.Delay(1);
            running = true;
            run = false;
            if(RhinoGet.GetMultipleObjects(txt, nothing, otype, out picked) == Result.Success)
                OnPingDocument().ScheduleSolution(1, (doc) => ExpireSolution(false));
            running = false;
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Trigger", "T", "flip to true and then false to trigger selection", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("OnTrue", "B", "true to trigger selection whenver T is true\nfalse to act like a button", GH_ParamAccess.item, false);
            pManager.AddIntegerParameter("Mode", "M", "mode of selection", GH_ParamAccess.item, -1);
            Param_Integer pint = pManager[2] as Param_Integer;
            pint.AddNamedValue("Anything", -1);
            pint.AddNamedValue("Point", 1);
            pint.AddNamedValue("Curve", 4);
            pint.AddNamedValue("Brep", 16);
            pint.AddNamedValue("Mesh", 32);
            pint.AddNamedValue("SubD", 262144);
            pint.AddNamedValue("Block", 2048);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("GUID", "I", "guid of selected", GH_ParamAccess.list);
            pManager.AddGeometryParameter("Object", "O", "selected objects", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            int ot = -1;
            bool click = false;
            bool ontrue = false;
            DA.GetData(0, ref click);
            DA.GetData(1, ref ontrue);
            DA.GetData(2, ref ot);
            otype = ot == -1 ? ObjectType.AnyObject : (ObjectType)ot;

            if (click && !run)
                run = true;
            else if (!click && run && !running)
                Trigger();
            else if (click && run && ontrue && !running)
                Trigger();
            else
            {
                run = false;
                running = false;
            }

            if (picked != null)
            {
                DA.SetDataList(0, picked.Select(i => i.ObjectId.ToString()));
                DA.SetDataList(1, picked.Select(i => i.Geometry()));
            }
            
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.select;
            }
        }

        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.secondary; }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("6021cb96-4644-40ad-b68a-8650fb96a911"); }
        }
    }
}