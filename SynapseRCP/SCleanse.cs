using System;
using Rhino;
using Rhino.Commands;

namespace SynapseRCP
{
    public class SCleanse : Command
    {
        static SCleanse _instance;
        public SCleanse()
        {
            _instance = this;
        }

        ///<summary>The only instance of the SynapseCleanse command.</summary>
        public static SCleanse Instance
        {
            get { return _instance; }
        }

        public override string EnglishName
        {
            get { return "SynapseCleanse"; }
        }

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            if (SynapseRH.Instance.RemotePanel != null)
                SynapseRH.Instance.RemotePanel.Content = null;
            return Result.Success;
        }
    }
}