using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Cinch;
using MEFedMVVM.ViewModelLocator;

namespace AutoDiagrammer
{
    public class MethodData
    {
        #region Data
        private IUIVisualizerService uiVisualizerService;
        #endregion

        #region Ctor
        public MethodData(String methodName, String methodBodyIL)
        {
            this.MethodName = methodName;
            this.MethodBodyIL = methodBodyIL;

            ShowMethodBodyCommand = new SimpleCommand<Object, Object>(CanExecuteShowMethodBodyCommand, ExecuteShowMethodBodyCommand);

            uiVisualizerService =
                    ViewModelRepository.Instance.Resolver.Container.GetExport<IUIVisualizerService>().Value;

        }
        #endregion

        #region Public Properties

        public ICommand ShowMethodBodyCommand { get; private set; }

        public String MethodName { get; private set; }
        public String MethodBodyIL { get; private set; }

        #endregion

        #region Command Handlers

        private bool CanExecuteShowMethodBodyCommand(Object parameter)
        {
            return !String.IsNullOrEmpty(MethodBodyIL);
        }

        private void ExecuteShowMethodBodyCommand(Object parameter)
        {
            uiVisualizerService.ShowDialog("MethodBodyILPopup", this);
        }

        #endregion
    }
}
