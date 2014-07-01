using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Cinch;
using MEFedMVVM.ViewModelLocator;

namespace AutoDiagrammer
{
    [Serializable]
    public class SerializableMethodData
    {
        #region Ctor
        public SerializableMethodData(String methodName, String methodBodyIL)
        {
            this.MethodName = methodName;
            this.MethodBodyIL = methodBodyIL;
        }
        #endregion

        #region Public Properties

        public String MethodName { get; private set; }
        public String MethodBodyIL { get; private set; }

        #endregion
    }
}
