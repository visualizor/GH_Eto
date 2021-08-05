using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using Eto.Forms;
using Eto.Drawing;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using wf = System.Windows.Forms;

namespace Synapse
{
    public class EtoMaskedTB : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the EtoMaskedTB class.
        /// </summary>
        public EtoMaskedTB()
          : base("SnpMaskedTxtBox", "STBm",
              "masked text box",
              "Synapse", "Controls")
        {
        }

        protected bool ascii = false;
        protected CultureInfo cultr = CultureInfo.InvariantCulture;

        protected void OnAscii(object s, EventArgs e)
        {
            ascii = !ascii;
            ExpireSolution(true);
        }
        protected void OnCheck(object s, EventArgs e)
        {
            wf.RadioButton rb = s as wf.RadioButton;
            switch (rb.Text)
            {
                case "UI":
                    cultr = CultureInfo.CurrentUICulture;
                    break;
                case "Installed":
                    cultr = CultureInfo.InstalledUICulture;
                    break;
                case "Invariant":
                    cultr = CultureInfo.InvariantCulture;
                    break;
                default:
                    cultr = CultureInfo.CurrentCulture;
                    break;
            }
            ExpireSolution(true);
        }
        public override void AppendAdditionalMenuItems(wf.ToolStripDropDown menu)
        {
            base.AppendAdditionalMenuItems(menu);
            try
            {
                wf.ToolStripMenuItem asciionly = menu.Items.Add("ASCII only", null, OnAscii) as wf.ToolStripMenuItem;
                asciionly.Checked = ascii;

                wf.FlowLayoutPanel flowpanel = new wf.FlowLayoutPanel() { FlowDirection = wf.FlowDirection.TopDown };
                wf.RadioButton rb1 = new wf.RadioButton() { Text = "UI" };
                wf.RadioButton rb2 = new wf.RadioButton() { Text = "Installed" };
                wf.RadioButton rb3 = new wf.RadioButton() { Text = "Invariant" };
                if (cultr == CultureInfo.CurrentUICulture)
                    rb1.Checked = true;
                else if (cultr == CultureInfo.InstalledUICulture)
                    rb2.Checked = true;
                else if (cultr == CultureInfo.InvariantCulture)
                    rb3.Checked = true;
                else
                    rb3.Checked = true;
                flowpanel.Controls.AddRange(new wf.Control[] { rb1, rb2, rb3, });
                rb1.Click += OnCheck;
                rb2.Click += OnCheck;
                rb3.Click += OnCheck;

                menu.Items.Add(new wf.ToolStripControlHost(flowpanel));
            }
            catch { }
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
            pManager.AddTextParameter("Mask", "M", "text string mask\nsee R output for reference", GH_ParamAccess.item,"(000) 000 0000");
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("All Properties", "A", "list of all accessible properties", GH_ParamAccess.list);
            pManager.AddGenericParameter("Control", "C", "control to go into a container or the listener", GH_ParamAccess.item);
            pManager.AddTextParameter("AllMasks", "R", "reference list FYI", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<string> props = new List<string>();
            List<GH_ObjectWrapper> vals = new List<GH_ObjectWrapper>();
            string mask = "";
            DA.GetDataList(0, props);
            DA.GetDataList(1, vals);
            DA.GetData(2, ref mask);

            MaskedTextBox mtb = new MaskedTextBox()
            {
                ID = Guid.NewGuid().ToString(),
                Provider = new FixedMaskedTextProvider(mask, cultr, false, ascii),
            };

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

                try { Util.SetProp(mtb, n, Util.GetGooVal(val)); }
                catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message); }
            }

            DA.SetData(1, new GH_ObjectWrapper(mtb));

            PropertyInfo[] allprops = mtb.GetType().GetProperties();
            List<string> printouts = new List<string>();
            foreach (PropertyInfo prop in allprops)
                if (prop.CanWrite)
                    printouts.Add(prop.Name + ": " + prop.PropertyType.ToString());
            DA.SetDataList(0, printouts);
            DA.SetData(2, @"The mask format can consist of the following characters:
0 - Required digit from 0-9. 
9 - Optional digit or space. 
# - Optional digit, space, or sign (+/-). If blank, then it is output as a space in the Text value. 
L - Required upper or lowercase letter. 
? - Optional upper or lowercase letter. 
& - Required character. If AsciiOnly is true, then behaves like L. 
C - Optional character. If AsciiOnly is true, then behaves like ?. 
A - Required alphanumeric character. If AsciiOnly is true, then behaves like L. 
a - Optional alphanumeric. If AsciiOnly is true, then behaves like ?. 
. - Decimal placeholder based on the specified Culture for the mask. 
, - Thousands placeholder based on the specified Culture for the mask. 
: - Time separator based on the specified Culture for the mask. 
/ - Date separator based on the specified Culture for the mask. 
$ - Currency symbol based on the specified Culture for the mask. 
< - Shift all characters that follow to lower case. 
> - Shift all characters that follow to upper case. 
| - Disables a previous shift to upper or lower case. 
\ - Escape the following character into a literal. 
All other characters are treated as literal and cannot be moved or deleted.
");
        }

        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.primary | GH_Exposure.obscure; }
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.tbplus;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("1523d58a-8c62-419a-8ef4-3acfea45035b"); }
        }
    }
}