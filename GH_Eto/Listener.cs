using System;
using System.Collections.Generic;
using System.Linq;
using Eto.Forms;
using Eto.Drawing;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel.Data;

namespace Synapse
{
    public class Listener : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the Listener class.
        /// </summary>
        public Listener()
          : base("Listener", "Value",
              "listen to a Synapse component",
              "Synapse", "Parameters")
        {
        }

        private Guid[] srcs; // gh comp linked to this
        private bool listening = false;
        private List<string> listenees = new List<string>(); // eto comp this listens to
        private Dictionary<string, bool> btnpress = new Dictionary<string, bool>(); // button press state

        private void Relisten(Control ctrl)
        {
            if (ctrl is Button btn)
            {
                listenees.Add(btn.ID);
                btnpress[btn.ID] = false;
                btn.MouseDown += OnBtnDn;
                btn.MouseUp += OnBtnUp;
            }
            else if (ctrl is TextBox tb)
            {
                listenees.Add(tb.ID);
                tb.TextChanged += OnTBText;
            }
            //TODO: insert more control types
        }
        private GH_ObjectWrapper GetCtrlValue(Control ctrl)
        {
            if (ctrl is Button btn)
                return new GH_ObjectWrapper(btnpress[btn.ID]);
            else if (ctrl is TextBox tb)
                return new GH_ObjectWrapper(tb.Text);
            
            return new GH_ObjectWrapper();
        }

        public void OnBtnDn(object s, EventArgs e)
        {
            if (s is Button btn)
                btnpress[btn.ID] = true;
            else
                return;
            ExpireSolution(true);
        }
        public void OnBtnUp(object s, EventArgs e)
        {
            if (s is Button btn)
                btnpress[btn.ID] = false;
            else
                return;
            ExpireSolution(true);
        }
        public void OnTBText(object s, EventArgs e)
        {
            ExpireSolution(true);
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Synapse Object", "S", "what to listen to", GH_ParamAccess.tree);
            pManager[0].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Value", "V", "the value heard", GH_ParamAccess.tree);
        }

        protected override void BeforeSolveInstance()
        {
            if (srcs is null) srcs = new Guid[] { };
            foreach (var src in Params.Input[0].Sources)
                if (!srcs.Contains(src.InstanceGuid))
                {
                    listenees.Clear();
                    btnpress.Clear();
                    listening = false;
                    break;
                }
            base.BeforeSolveInstance();
            srcs = Params.Input[0].Sources.Select(s => s.InstanceGuid).ToArray();
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            DA.GetDataTree(0, out GH_Structure<IGH_Goo> objtree);
            if (objtree.IsEmpty)
            {
                listenees.Clear();
                btnpress.Clear();
                listening = false;
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, " No Eto components listened");
                return;
            }

            GH_Structure<GH_ObjectWrapper> outputs = new GH_Structure<GH_ObjectWrapper>();
            for (int bi = 0; bi<objtree.Branches.Count; bi++)
            {
                List<IGH_Goo> objs = objtree.Branches[bi];
                GH_Path pth = objtree.Paths[bi];
                for (int ii=0; ii < objs.Count; ii++)
                {
                    GH_ObjectWrapper obj = objs[ii] as GH_ObjectWrapper;
                    if (obj.Value is Container cont)
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Synapse containers cannot be listened for values\n You can listen to controls inside them\n Try go upstream");
                        continue;
                    }
                    else if (obj.Value is Control ctrl)
                    {
                        if (!listenees.Contains(ctrl.ID))
                        {
                            Relisten(ctrl);
                            listening = true;
                        }
                        else if (!listening)
                        {
                            listening = true;
                        }
                        else
                        {
                            outputs.Append(GetCtrlValue(ctrl), pth);
                        }
                    }
                    else
                    {
                        listening = false;
                    }
                }
                
            }
            DA.SetDataTree(0, outputs);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.listen;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("f283eb4d-4481-428f-88ab-ceeffa900d29"); }
        }
    }
}