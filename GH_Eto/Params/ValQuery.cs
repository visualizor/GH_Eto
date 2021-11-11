using System;
using System.Collections.Generic;
using System.Linq;
using Eto.Forms;
using Eto.Drawing;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel.Data;
using Rhino.Geometry;

namespace Synapse
{
    public class ValQuery : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the Listener class.
        /// </summary>
        public ValQuery()
          : base("SnpValueQuery", "SValQ",
              "queries a Synapse component for its value",
              "Synapse", "Parameters")
        {
        }

        private Guid[] srcs; // gh comp linked to this
        private bool listening = false;  // this isn't used anywhere!!
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
            else if (ctrl is LatentTB tb)
            {
                listenees.Add(tb.ID);
                if (tb.Live)
                    tb.TextChanged += OnCtrl;
                else
                    tb.KeyUp += OnTBEnter;
            }
            else if (ctrl is CheckBox cb)
            {
                listenees.Add(cb.ID);
                cb.CheckedChanged += OnCtrl;
            }
            else if (ctrl is Slider sl)
            {
                listenees.Add(sl.ID);
                sl.ValueChanged += OnCtrl;
            }
            else if (ctrl is NumericStepper numsteps)
            {
                listenees.Add(numsteps.ID);
                numsteps.ValueChanged += OnCtrl;
            }
            else if (ctrl is RadioButtonList rblist)
            {
                listenees.Add(rblist.ID);
                rblist.SelectedValueChanged += OnCtrl;
            }
            else if (ctrl is Label lb) { }
            else if (ctrl is DropDown dd)
            {
                if (dd is ComboBox combo)
                {
                    listenees.Add(combo.ID);
                    combo.SelectedIndexChanged += OnCtrl;
                    combo.TextChanged += OnCtrl;
                }
                else
                {
                    listenees.Add(dd.ID);
                    dd.SelectedIndexChanged += OnCtrl;
                }
            }
            else if (ctrl is ComboSlider csl)
            {
                listenees.Add(csl.ID);
                if (csl.Live)
                    csl.slider.ValueChanged += OnCtrl;
                else
                    csl.slider.MouseUp += OnCtrl;
            }
            else if (ctrl is MaskedTextBox mtb)
            {
                listenees.Add(mtb.ID);
                mtb.TextChanged += OnCtrl;
            }
            else if (ctrl is TextArea multi)
            {
                listenees.Add(multi.ID);
                multi.TextChanged += OnCtrl;
            }
            else if (ctrl is ListBox lbx)
            {
                listenees.Add(lbx.ID);
                lbx.SelectedIndexChanged += OnCtrl;
            }
            else if (ctrl is TrackPad tp)
            {
                listenees.Add(tp.ID);
                tp.MouseUp += OnCtrl;
            }
            else if (ctrl is ComboDomSl combodom)
            {
                listenees.Add(combodom.ID);
                combodom.DomSl.PropertyChanged += OnCtrl;
            }
            else if (ctrl is FilePicker fp)
            {
                listenees.Add(fp.ID);
                fp.FilePathChanged += OnCtrl;
            }
            else if (ctrl is ColorPicker cp)
            {
                listenees.Add(cp.ID);
                cp.ValueChanged += OnCtrl;
            }
            else
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, " unrecognized control detected");
        }
        private GH_ObjectWrapper GetCtrlValue(Control ctrl)
        {
            if (ctrl is Button btn)
                return new GH_ObjectWrapper(btnpress[btn.ID]);
            else if (ctrl is TextBox tb)
                return new GH_ObjectWrapper(tb.Text);
            else if (ctrl is CheckBox cb)
                return new GH_ObjectWrapper(cb.Checked);
            else if (ctrl is Slider sl)
                return new GH_ObjectWrapper(sl.Value);
            else if (ctrl is NumericStepper numsteps)
                return new GH_ObjectWrapper(numsteps.Value);
            else if (ctrl is RadioButtonList rblist)
                return new GH_ObjectWrapper(rblist.SelectedIndex);
            else if (ctrl is DropDown dd)
                if (dd is ComboBox combo)
                    return new GH_ObjectWrapper(combo.Text);
                else
                    return new GH_ObjectWrapper(dd.SelectedIndex);
            else if (ctrl is ComboSlider csl)
                return new GH_ObjectWrapper(csl.val);
            else if (ctrl is MaskedTextBox mtb)
                return new GH_ObjectWrapper(mtb.Text);
            else if (ctrl is TextArea multi)
                return new GH_ObjectWrapper(multi.Text);
            else if (ctrl is ListBox lbx)
                return new GH_ObjectWrapper(lbx.SelectedIndex);
            else if (ctrl is TrackPad tp)
                return new GH_ObjectWrapper(tp.Position.ToString());
            else if (ctrl is ComboDomSl combodom)
                return new GH_ObjectWrapper(new Interval(combodom.DomSl.Lower, combodom.DomSl.Upper));
            else if (ctrl is FilePicker fp)
                return new GH_ObjectWrapper(fp.FilePath);
            else if (ctrl is ColorPicker cp)
                return new GH_ObjectWrapper(cp.Value.ToString());
            else
                return new GH_ObjectWrapper();
        }

        #region expire solutions
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
        public void OnTBEnter(object s, KeyEventArgs e)
        {
            if (e.Key == Keys.Enter)
                ExpireSolution(true);
        }
        protected void OnCtrl(object s, EventArgs e)
        {
            ExpireSolution(true);
        }
        #endregion

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Synapse Object", "C", "control object to query", GH_ParamAccess.tree);
            pManager[0].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Value", "V", "the value passed from the control(s)", GH_ParamAccess.tree);
        }

        /// <summary>
        /// clear listenees and reset btn press states when wired to a different component
        /// </summary>
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
            srcs = Params.Input[0].Sources.Select(s => s.InstanceGuid).ToArray();
            base.BeforeSolveInstance();
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
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, " No Synapse connected");
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
                    if (obj.Value is Control ctrl)
                    {
                        if (ctrl.ID == Guid.Empty.ToString() || ctrl.ID == string.Empty || ctrl.ID==null)
                        {
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, string.Format(" a {0} isn't initialized properly with a valid ID\n this is a code error; contact dev", ctrl.GetType()));
                            listening = true;
                        }
                        else if (!listenees.Contains(ctrl.ID))
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
                        listening = false; // not an Eto control
                    }
                }
                
            }
            DA.SetDataTree(0, outputs);
        }

        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.secondary; }
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