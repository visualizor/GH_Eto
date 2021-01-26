using System;
using System.Drawing;
using Grasshopper.Kernel;
using Grasshopper;

namespace GH_Eto
{
    public class GH_EtoInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "EtoGH";
            }
        }
        public override Bitmap Icon
        {
            get
            {
                //Return a 24x24 pixel bitmap to represent this GHA library.
                return Properties.Resources.cat_icon;
            }
        }
        public override string Description
        {
            get
            {
                //Return a short string describing the purpose of this GHA library.
                return "make Eto accessible in Grasshopper visual scripting environment";
            }
        }
        public override Guid Id
        {
            get
            {
                return new Guid("22fa92be-2a51-41b5-a1dd-685eb5c0ccd4");
            }
        }

        public override string AuthorName
        {
            get
            {
                //Return a string identifying you or your company.
                return "Will Wang";
            }
        }
        public override string AuthorContact
        {
            get
            {
                //Return a string representing your preferred contact details.
                return @"will.wang6@gmail.com";
            }
        }
    }

    public class EDG_GHCategoryIcon : GH_AssemblyPriority
    {
        public override GH_LoadingInstruction PriorityLoad()
        {
            Instances.ComponentServer.AddCategoryIcon("Synapse", Properties.Resources.cat_icon);
            Instances.ComponentServer.AddCategorySymbolName("Synapse", 'S');
            return GH_LoadingInstruction.Proceed;
        }
    }
}
