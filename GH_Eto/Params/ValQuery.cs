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

        private bool live = true; // set to false to not expire downstream
        private Guid[] srcs; // gh comp linked to this
        private bool listening = false;  // this isn't affecting anything
        private List<string> listenees = new List<string>(); // eto comp this listens to
        private Dictionary<string, bool> btnpress = new Dictionary<string, bool>(); // button press state

        private void Relisten(Control ctrl)
        {
            if (ctrl is ToggleButton tbtn)
            {
                listenees.Add(tbtn.ID);
                tbtn.CheckedChanged += OnCtrl;
            }
            else if (ctrl is Button btn)
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
                    tb.KeyUp += OnEnterUp;
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
                {
                    csl.slider.MouseUp += OnCtrl;
                    csl.slider.KeyUp += OnEnterUp;
                    csl.UserInputClosed += OnCtrl;
                }
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
            else if (ctrl is WebForm webf)
            {
                listenees.Add(webf.ID);
                webf.DocumentLoading += OnWebView;
            }
            else
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, " unrecognized control detected");
        }
        private GH_ObjectWrapper[] GetCtrlValue(Control ctrl)
        {
            List<GH_ObjectWrapper> val_out = new List<GH_ObjectWrapper>();
            if (!ctrl.Enabled) return new GH_ObjectWrapper[]{ };

            if (ctrl is ToggleButton tbtn)
                val_out.Add(new GH_ObjectWrapper(tbtn.Checked));
            else if (ctrl is Button btn)
                val_out.Add(new GH_ObjectWrapper(btnpress[btn.ID]));
            else if (ctrl is TextBox tb)
                val_out.Add(new GH_ObjectWrapper(tb.Text));
            else if (ctrl is CheckBox cb)
                val_out.Add(new GH_ObjectWrapper(cb.Checked));
            else if (ctrl is Slider sl)
                val_out.Add(new GH_ObjectWrapper(sl.Value));
            else if (ctrl is NumericStepper numsteps)
                val_out.Add(new GH_ObjectWrapper(numsteps.Value));
            else if (ctrl is RadioButtonList rblist)
                val_out.Add(new GH_ObjectWrapper(rblist.SelectedIndex));
            else if (ctrl is DropDown dd)
                if (dd is ComboBox combo)
                    val_out.Add(new GH_ObjectWrapper(combo.Text));
                else
                    val_out.Add(new GH_ObjectWrapper(dd.SelectedIndex));
            else if (ctrl is ComboSlider csl)
                val_out.Add(new GH_ObjectWrapper(csl.val));
            else if (ctrl is MaskedTextBox mtb)
                val_out.Add(new GH_ObjectWrapper(mtb.Text));
            else if (ctrl is TextArea multi)
                val_out.Add(new GH_ObjectWrapper(multi.Text));
            else if (ctrl is ListBox lbx)
                val_out.Add(new GH_ObjectWrapper(lbx.SelectedIndex));
            else if (ctrl is TrackPad tp)
                val_out.Add(new GH_ObjectWrapper(tp.Position.ToString()));
            else if (ctrl is ComboDomSl combodom)
                val_out.Add(new GH_ObjectWrapper(
                    new Interval(combodom.DomSl.Lower, combodom.DomSl.Upper)
                    ));
            else if (ctrl is FilePicker fp)
                val_out.Add(new GH_ObjectWrapper(fp.FilePath));
            else if (ctrl is ColorPicker cp)
                val_out.Add(new GH_ObjectWrapper(cp.Value.ToString()));
            else if (ctrl is WebForm webf)
            {
                GH_ObjectWrapper[] vs = new GH_ObjectWrapper[webf.OrderedKeys.Count];
                for (int i = 0; i < vs.Length; i++)
                    vs.SetValue(
                        new GH_ObjectWrapper(webf.CtrlVals[webf.OrderedKeys[i]]),
                        i);
                val_out.AddRange(vs);
            }
            else
                val_out.Add(new GH_ObjectWrapper());

            return val_out.ToArray();
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
        public void OnEnterUp(object s, KeyEventArgs e)
        {
            if (e.Key == Keys.Enter)
                ExpireSolution(true);
        }
        protected void OnCtrl(object s, EventArgs e)
        {
            ExpireSolution(true);
        }
        protected void OnWebView(object s, WebViewLoadingEventArgs e)
        {
            string js = @"
var el = document.getElementById('{0}');
try{{ return el.{1};}}
catch (er) {{return 'ERROR getting element';}}
"; // double curly brackets for string format method
            if (s is WebForm wv)
                if (e.Uri.Scheme == "synapse")
                {
                    foreach (string prm in wv.CtrlVals.Keys.ToArray())
                    {
                        if (e.Uri.LocalPath != prm)
                            continue;
                        string ptype = wv.ExecuteScript(string.Format(js, prm, "type"));
                        string v = "";
                        switch (ptype)
                        {
                            case "checkbox":
                                v = wv.ExecuteScript(string.Format(js, prm,"checked"));
                                break;
                            case "range":
                                v = wv.ExecuteScript(string.Format(js, prm, "value"));
                                break;
                            case "button":
                                if (e.Uri.Query == "?press")
                                    btnpress[prm] = true;
                                else if (e.Uri.Query == "?lift")
                                    btnpress[prm] = false;
                                else
                                    break; //exit early
                                v = btnpress[prm].ToString();
                                break;
                            case "text":
                                v = wv.ExecuteScript(string.Format(js, prm, "value"));
                                break;
                            case "select-one":
                                v = wv.ExecuteScript(string.Format(js, prm, "value"));
                                break;
                            case "radio":
                                v = wv.ExecuteScript(string.Format(@"var el = document.getElementById('{0}');
var els = document.getElementsByName(el.name);
var valstr = '';
for (let i=0;i<els.length;i++){{
	if (els[i].checked){{
		valstr = els[i].value;
	}}
}}
return valstr;", prm));
                                break;
                            default:
                                break;
                        }
                        wv.CtrlVals[prm] = v;
                    }

                    e.Cancel = true;
                }
            ExpireSolution(true); //safe, we're in event handler so not expiring during a solution
        }
        #endregion

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Synapse Object", "C", "control object to query", GH_ParamAccess.tree);
            pManager[0].Optional = true;
            pManager.AddBooleanParameter("Update", "T", "an update trigger: helpful when you only want values to update with a click of a button\nleave this on true to be live update", GH_ParamAccess.item, true);
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
            DA.GetData(1, ref live);
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
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, string.Format(" a {0} isn't initialized properly with a valid ID\n contact dev", ctrl.GetType()));
                            listening = true;
                        }
                        else if (!listenees.Contains(ctrl.ID))
                        {
                            Relisten(ctrl);
                            listening = true;
                            outputs.AppendRange(GetCtrlValue(ctrl), pth);
                        }
                        else if (!listening)
                        {
                            listening = true;
                            outputs.AppendRange(GetCtrlValue(ctrl), pth);
                        }
                        else
                        {
                            outputs.AppendRange(GetCtrlValue(ctrl), pth);
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

        protected override void ExpireDownStreamObjects()
        {
            if (live)
                base.ExpireDownStreamObjects();
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
                return Properties.Resources.query;
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