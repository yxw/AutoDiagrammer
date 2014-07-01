using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System;
using System.Reflection;
using Cinch;

namespace AutoDiagrammer
{

    public enum RepresentationType { AssemblyOrExe = 1, Namespace, Class };

    [Serializable]
    [DebuggerDisplay("{ToString()}")]
    public class AssemblyTreeViewModel : INPCBase
    {
        #region Data

        bool? isChecked = false;

        #endregion

        #region Ctor/Destructor
        public AssemblyTreeViewModel(RepresentationType nodeType, string name, SerializableVertex vertex, AssemblyTreeViewModel parent)
        {
            this.NodeType = nodeType;
            this.Name = name;
            this.Vertex = vertex;
            this.Parent = parent;
            Children = new List<AssemblyTreeViewModel>();
            Mediator.Instance.Register(this);
        }

        ~AssemblyTreeViewModel()
        {
            Mediator.Instance.Unregister(this);
        }
        #endregion

        #region Public Properties

        public RepresentationType NodeType { get; private set; }
        public List<AssemblyTreeViewModel> Children { get; private set; }
        public bool IsInitiallySelected { get; private set; }
        public string Name { get; private set; }
        public AssemblyTreeViewModel Parent { get; private set; }
        public SerializableVertex Vertex { get; private set; }

        #region IsChecked

        /// <summary>
        /// IsChecked
        /// </summary>
        static PropertyChangedEventArgs isCheckedArgs =
            ObservableHelper.CreateArgs<AssemblyTreeViewModel>(x => x.IsChecked);



        /// <summary>
        /// Gets/sets the state of the associated UI toggle (ex. CheckBox).
        /// The return value is calculated based on the check state of all
        /// child FooViewModels.  Setting this property to true or false
        /// will set all children to the same check state, and setting it 
        /// to any value will cause the parent to verify its check state.
        /// </summary>
        public bool? IsChecked
        {
            get { return isChecked; }
            set
            {
                this.SetIsChecked(value, true, true);
            }
        }

        void SetIsChecked(bool? value, bool updateChildren, bool updateParent)
        {
            if (value == isChecked)
                return;

            isChecked = value;

            if (updateChildren && isChecked.HasValue)
                this.Children.ForEach(c => c.SetIsChecked(isChecked, true, false));

            if (updateParent && Parent != null)
                Parent.VerifyCheckState();

            NotifyPropertyChanged(isCheckedArgs);

            Mediator.Instance.NotifyColleagues<bool>("TreeStateChangedMessage", true);

        }

        void VerifyCheckState()
        {
            bool? state = null;
            for (int i = 0; i < this.Children.Count; ++i)
            {
                bool? current = this.Children[i].IsChecked;
                if (i == 0)
                {
                    state = current;
                }
                else if (state != current)
                {
                    state = null;
                    break;
                }
            }
            this.SetIsChecked(state, false, true);
        }

        #endregion // IsChecked

        #endregion 

        #region Overrides
        public override string ToString()
        {
            return this.Name;
        }
        #endregion
    }
}