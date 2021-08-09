using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel.Data;
using Eto.Forms;

namespace Synapse
{
    public class EtoFileFilter : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the EtoFileFilter class.
        /// </summary>
        public EtoFileFilter()
          : base("SnpFileType", "SExt",
              "file types that can be recognized by the file picker",
              "Synapse", "Parameters")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Type Name", "N", "name of file types", GH_ParamAccess.list, "Plain Text");
            pManager[0].DataMapping = GH_DataMapping.Flatten;
            pManager[0].Optional = true;
            pManager.AddTextParameter("Extensions", "E", "file extensions", GH_ParamAccess.tree, ".txt");
            pManager[1].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("FileFilter", "F", "file type filters for the Synapse file picker", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_Structure<GH_String> exts = new GH_Structure<GH_String>();
            List<string> names = new List<string>();
            DA.GetDataList(0, names);
            DA.GetDataTree(1, out exts);

            if (names.Count != exts.Branches.Count)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "an item in the list of names should match a branch of the extensions");
                return;
            }
            else if (names.Count ==1 && exts.Branches.Count == 1)
            {
                DA.SetData(0, new FileFilter(names[0], exts.Branches[0].Select(t => t.Value).ToArray()));
            }

            FileFilter[] filters = new FileFilter[names.Count];
            for (int i = 0; i < filters.Length; i++)
            {
                GH_String[] allexts = exts.Branches[0].ToArray();
                IEnumerable<string> extstrs = allexts.Select(t => t.Value);
                filters.SetValue(new FileFilter(names[i], extstrs.ToArray()), i);
            }
            DA.SetData(0, new GH_ObjectWrapper(filters));
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("14edf74c-acca-4aac-a26f-427e2f3521ea"); }
        }
    }
}