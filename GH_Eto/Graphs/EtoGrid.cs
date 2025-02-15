using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Eto.Forms;
using Eto.Drawing;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Parameters;
using Grasshopper;

namespace Synapse
{
    public class EtoGrid : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the EtoGridView class.
        /// </summary>
        public EtoGrid()
          : base("SnpGridView", "SGV",
              "shows data in a tabular format",
              "Synapse", "Graphics")
        {
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
            pManager.AddTextParameter("Data", "D", "data to display\nuse a tree, each branch a row, first branch the headers\nmake sure each branch matches first branch's list length", GH_ParamAccess.tree);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("All Properties", "A", "list of all accessible properties", GH_ParamAccess.list);
            pManager.AddGenericParameter("Control", "C", "control to go into a container or the listener", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<string> props = new List<string>();
            List<GH_ObjectWrapper> vals = new List<GH_ObjectWrapper>();
            DA.GetDataList(0, props);
            DA.GetDataList(1, vals);

            DA.GetDataTree(2, out GH_Structure<GH_String> ghtree);

            //to text, skip header row
            List<string[]> ds = new List<string[]>();
            for (int bi = 1; bi < ghtree.Branches.Count; bi++)
            {
                string[] t = ghtree.Branches[bi].Select(i => i.Value).ToArray();
                ds.Add(t);
            }
            // instantiate gridview
            GridView gv = new GridView { DataStore = ds.ToArray(), };
            for (int hi = 0; hi < ghtree.Branches[0].Count; hi++)
            {
                GridColumn gc = new GridColumn
                {
                    HeaderText = ghtree.Branches[0][hi].Value,
                    DataCell = new TextBoxCell(hi),
                };

                gv.Columns.Add(gc);

            }

            DA.SetData(1, new GH_ObjectWrapper(gv));
        }


        protected void SortDS(GridView g, int coli)
        {
            if (!(g.DataStore is string[][] d) || d.Length == 0)
                return;

            string[][] sorted = d.OrderBy(r => r[coli]).ToArray();

            g.DataStore = sorted;
            for (int ri = 0; ri < sorted.Length; ri++)
                g.ReloadData(ri);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.gridview;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("cb41ea70-ba6f-40e6-97e1-60f285036f6c"); }
        }
    }
}