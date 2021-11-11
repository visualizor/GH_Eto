using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Grasshopper.Kernel;

namespace Synapse
{
    public class ReSolve : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the ReSolve class.
        /// </summary>
        public ReSolve()
          : base("ReSolve", "ReSolve",
              "Re-solve an upstream GH component\nlike Trigger, but controlled by input parameters\nuse all flatten inputs",
              "Synapse", "Parameters")
        {
            Hidden = true;
        }

        protected bool run = false;
        protected Dictionary<Guid, bool> srcstats = new Dictionary<Guid, bool>();

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Target", "T", "Target GH component\nlink here with any one of the target's outputs\nactual data passed herein is wholy ignored", GH_ParamAccess.list);
            pManager[0].WireDisplay = GH_ParamWireDisplay.faint;
            pManager[0].Optional = true;
            pManager[0].DataMapping = GH_DataMapping.Flatten;
            pManager.AddIntegerParameter("Interval", "P", "Time interval (ms) between solutions\nmust be a single value\nmultiples are ignored except first item", GH_ParamAccess.list, 500);
            pManager[1].DataMapping = GH_DataMapping.Flatten;
            pManager.AddBooleanParameter("Active", "A", "set to true to start ticking\nmust be a single value\nmultiples are ignored except first item", GH_ParamAccess.list, false);
            pManager[2].DataMapping = GH_DataMapping.Flatten;
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
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<int> itvls = new List<int>();
            List<bool> actives = new List<bool>();
            DA.GetDataList(1, itvls);
            DA.GetDataList(2, actives);
            run = actives[0];
            IList<IGH_Param> srcs = Params.Input[0].Sources;

            if (!run)
            {
                srcstats.Clear();
                return;
            }
            else
            {
                for (int i = 0; i < srcs.Count; i++)
                {
                    int itvl = i < itvls.Count ? itvls[i] : itvls.Last();
                    IGH_DocumentObject ghcomp = srcs[i].Attributes.GetTopLevel.DocObject;
                    if (!srcstats.TryGetValue(ghcomp.InstanceGuid, out bool running))
                    {
                        srcstats.Add(ghcomp.InstanceGuid, true);
                        Task.Run(() => PeriodicReSolve(ghcomp, itvl));
                    }
                    else if (running)
                        continue;
                    else
                    {
                        srcstats[ghcomp.InstanceGuid] = true;
                        Task.Run(() => PeriodicReSolve(ghcomp, itvl));
                    }

                }
            }
        }

        protected async void PeriodicReSolve(IGH_DocumentObject c, int i)
        {
            await Task.Delay(i);
            object locker = new object();
            OnPingDocument().ScheduleSolution(1, (doc) => c.ExpireSolution(false));
            lock (locker)
                srcstats[c.InstanceGuid] = false;
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.resolve;
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
            get { return new Guid("405936e1-a685-4981-a38d-3f828f15abf0"); }
        }
    }
}