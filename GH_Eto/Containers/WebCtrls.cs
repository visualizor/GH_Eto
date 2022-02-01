using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Grasshopper.Kernel;
using Grasshopper;
using Grasshopper.Kernel.Types;
using Eto.Forms;
using Eto.Drawing;

namespace Synapse
{
    public class WebCtrls : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the EtoWebCtrls class.
        /// </summary>
        public WebCtrls()
          : base("SnpWebCtrls", "WebCtrls",
              "a web form that contains web controls styled with HTML and CSS",
              "Synapse", "Containers")
        {
        }

        /// <summary>
        /// default html string
        /// </summary>
        protected string DHtml
        {
            get
            {
                return @"
<!DOCTYPE html>
<html>
    <head>
        <meta charset='UTF-8'/>
        <title>Synapse Web Form</title>
    </head>
    <body>
        <p> Parameter </p>
        <input type='range' min='0' max='100' value='50' class='slider' id='num_param'>
    </body>

    <style>
        p {font-family: monospace;}
    </style>
    <script>
        onslider = function()
        {
            window.location = 'synapse:paramslider';
        }
        document.getElementById('num_param').onchange = onslider
    </script>
</html>
";
            }
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("HTML", "H", "html texts", GH_ParamAccess.item, DHtml);
            pManager.AddTextParameter("ElementIDs", "E", "element IDs of controls in the html", GH_ParamAccess.list, new string[] { "num_param", });
            pManager[1].DataMapping = GH_DataMapping.Flatten;

            pManager.AddTextParameter("Property", "P", "property to set", GH_ParamAccess.list);
            pManager[2].DataMapping = GH_DataMapping.Flatten;
            pManager[2].Optional = true;
            pManager.AddGenericParameter("Property Value", "V", "values for the property", GH_ParamAccess.list);
            pManager[3].DataMapping = GH_DataMapping.Flatten;
            pManager[3].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("All Properties", "A", "list of all accessible properties", GH_ParamAccess.list);
            pManager.AddGenericParameter("WebForm", "C", "WebView control", GH_ParamAccess.item);
            pManager.AddTextParameter("ControlValues", "Q", "queried values from controls on the web form", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string html = "";
            List<string> ids = new List<string>();
            DA.GetData(0, ref html);
            DA.GetDataList(1, ids);
            List<string> props = new List<string>();
            List<GH_ObjectWrapper> vals = new List<GH_ObjectWrapper>();
            DA.GetDataList(2, props);
            DA.GetDataList(3, vals);

            WebForm wv = new WebForm() { ID = Guid.NewGuid().ToString(), Html = html,};
            wv.LoadHtml(wv.Html);

            DA.SetData(1, new GH_ObjectWrapper(wv));
            
            PropertyInfo[] allprops = typeof(Control).GetProperties();
            List<string> printouts = new List<string>();
            foreach (PropertyInfo prop in allprops)
                if (prop.CanWrite)
                    printouts.Add(prop.Name + ": " + prop.PropertyType.ToString());
            DA.SetDataList(0, printouts);
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
            get { return new Guid("35fcb4c3-6347-4112-ba7c-222112de41b9"); }
        }
    }
}