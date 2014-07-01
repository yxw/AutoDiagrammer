using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace AutoDiagrammer
{
    [Serializable]
    public class TypeReflector
    {
        #region Data
        private List<MethodInfo> propGetters = new List<MethodInfo>();
        private List<MethodInfo> propSetters = new List<MethodInfo>();
        private List<Type> extraAssociations = new List<Type>();
        #endregion

        #region Ctor
        public TypeReflector(
            Type type)
        {
            this.TypeInAssembly = type;
            this.Name = type.FullName;
            this.ShortName = type.Name;

            Constructors = new List<string>();
            Fields = new List<string>();
            Properties = new List<string>();
            Interfaces = new List<string>();
            Methods = new List<SerializableMethodData>();
            Events = new List<string>();
            Associations = new HashSet<string>();
        }
        #endregion

        #region Public Methods
        public void ReflectOnType()
        {
            ReflectOutConstructors();
            ReflectOutFields();
            ReflectOutProperties();
            ReflectOutInterfaces();
            ReflectOutMethods();
            ReflectOutEvents();
        }
        #endregion

        #region Public Properties
        public Type TypeInAssembly { get; private set; }
        public String Name { get; private set; }
        public String ShortName { get; private set; }
        public List<String> Constructors { get; private set; }
        public List<String> Fields { get; private set; }
        public List<String> Properties { get; private set; }
        public List<String> Interfaces { get; private set; }
        public List<SerializableMethodData> Methods { get; private set; }
        public List<String> Events { get; private set; }
        public HashSet<String> Associations { get; private set; }
        public bool HasConstructors { get; private set; }
        public bool HasFields { get; private set; }
        public bool HasProperties { get; private set; }
        public bool HasInterfaces { get; private set; }
        public bool HasMethods { get; private set; }
        public bool HasEvents { get; private set; }
        
        public static BindingFlags RequiredBindings { get; set; }
        public static bool ShowConstructorParameters { get; set; }
        public static bool ShowFieldTypes { get; set; }
        public static bool ShowPropertyTypes { get; set; }
        public static bool ShowInterfaces { get; set; }
        public static bool ParseMethodBodyIL { get; set; }
        public static bool ShowMethodArguments { get; set; }
        public static bool ShowMethodReturnValues { get; set; }
        public static bool ShowGetMethodForProperty { get; set; }
        public static bool ShowSetMethodForProperty { get; set; }
        public static bool ShowEvents { get; set; }
        public static bool IncludeConstructorParametersAsAssociations { get; set; }
        public static bool IncludePropertyTypesAsAssociations { get; set; }
        public static bool IncludeFieldTypesAsAssociations { get; set; }
        public static bool IncludeMethodArgumentAsAssociations { get; set; }



        #endregion

        #region Private Methods
        private void ReflectOutConstructors()
        {
            //do constructors
            foreach (ConstructorInfo ci in TypeInAssembly.GetConstructors(RequiredBindings))
            {
                if (TypeInAssembly == ci.DeclaringType)
                {

                    string cDetail = TypeInAssembly.Name + "(";
                    string pDetail = "";

                    //add all the constructor param types to the associations List, so that 
                    //the association lines for this class can be obtained, and 
                    //possibly drawn on the container
                    ParameterInfo[] pif = ci.GetParameters();
                    foreach (ParameterInfo p in pif)
                    {
                        string pName = GetGenericsForType(p.ParameterType);
                        pName = LowerAndTrim(pName);

                        if (IncludeConstructorParametersAsAssociations)
                        {
string association = p.ParameterType.IsGenericType ? p.ParameterType.GetGenericTypeDefinition().FullName : p.ParameterType.FullName;

if (!Associations.Contains(association))
{
    Associations.Add(association);
}
                        }
                        pDetail = pName + " " + p.Name + ", ";
                        cDetail += pDetail;
                    }

                    if (cDetail.LastIndexOf(",") > 0)
                    {
                        cDetail = cDetail.Substring(0, cDetail.LastIndexOf(","));
                    }

                    cDetail += ")";
                    //do we want long or short field constructor displayed
                    if (ShowConstructorParameters)
                    {

                        Constructors.Add(cDetail);
                    }
                    else
                        Constructors.Add(TypeInAssembly.Name + "( )");
                }
            }

            HasConstructors = Constructors.Any();
        }

        private void ReflectOutFields()
        {
            //do fields
            foreach (FieldInfo fi in TypeInAssembly.GetFields(RequiredBindings))
            {
                if (TypeInAssembly == fi.DeclaringType)
                {
                    //add all the field types to the associations List, so that 
                    //the association lines for this class can be obtained, and 
                    //possibly drawn on the container
                    string fName = GetGenericsForType(fi.FieldType);
                    fName = LowerAndTrim(fName);
                    
                    if (IncludeFieldTypesAsAssociations)
                    {
                        string association = fi.FieldType.IsGenericType ? fi.FieldType.GetGenericTypeDefinition().FullName : fi.FieldType.FullName;

                        if (!Associations.Contains(association))
                        {
                            Associations.Add(association);
                        }
                    }

                    //do we want long or short field description displayed
                    if (ShowFieldTypes)
                        Fields.Add(fName + " " + fi.Name);
                    else
                        Fields.Add(fi.Name);
                }
            }


            HasFields = Fields.Any();
        }

        private void ReflectOutProperties()
        {
            //do properties
            foreach (PropertyInfo pi in TypeInAssembly.GetProperties(RequiredBindings))
            {
                if (TypeInAssembly == pi.DeclaringType)
                {
                    // add read method if exists
                    if (pi.CanRead) { propGetters.Add(pi.GetGetMethod(true)); }
                    // add write method if exists
                    if (pi.CanWrite) { propSetters.Add(pi.GetSetMethod(true)); }

                    string pName = GetGenericsForType(pi.PropertyType);
                    //add all the property types to the associations List, so that 
                    //the association lines for this class can be obtained, and 
                    //possibly drawn on the container
                    pName = LowerAndTrim(pName);

                    if (IncludePropertyTypesAsAssociations)
                    {
                        string association = pi.PropertyType.IsGenericType ? pi.PropertyType.GetGenericTypeDefinition().FullName : pi.PropertyType.FullName;
                        if (!Associations.Contains(association))
                        {
                            Associations.Add(association);
                        }
                    }

                    //do we want long or short property description displayed
                    if (ShowPropertyTypes)
                        Properties.Add(pName + " " + pi.Name);
                    else
                        Properties.Add(pi.Name);
                }
            }

            HasProperties = Properties.Any();
        }

        private void ReflectOutInterfaces()
        {
            //do interfaces
            if (ShowInterfaces)
            {
                Type[] tiArray = TypeInAssembly.GetInterfaces();
                foreach (Type ii in tiArray)
                {
                    Interfaces.Add(ii.Name.ToString());
                }
            }

            HasInterfaces = Interfaces.Any();
        }

        private void ReflectOutMethods()
        {
            //do methods
            foreach (MethodInfo mi in TypeInAssembly.GetMethods(RequiredBindings))
            {
                if (TypeInAssembly == mi.DeclaringType)
                {
                    string mDetail = mi.Name + "( ";
                    string pDetail = "";
                    //do we want to display method arguments, if we do create the 
                    //appopraiate string
                    if (ShowMethodArguments)
                    {
                        ParameterInfo[] pif = mi.GetParameters();
                        foreach (ParameterInfo p in pif)
                        {
                            //add all the parameter types to the associations List, so that 
                            //the association lines for this class can be obtained, and 
                            //possibly drawn on the container
                            string pName = GetGenericsForType(p.ParameterType);
                            pName = LowerAndTrim(pName);

                            if (IncludeMethodArgumentAsAssociations)
                            {
                                string association = p.ParameterType.IsGenericType ? p.ParameterType.GetGenericTypeDefinition().FullName : p.ParameterType.FullName;
                                if (!Associations.Contains(association))
                                {
                                    Associations.Add(association);
                                }
                            }

                            pDetail = pName + " " + p.Name + ", ";
                            mDetail += pDetail;
                        }
                        if (mDetail.LastIndexOf(",") > 0)
                        {
                            mDetail = mDetail.Substring(0, mDetail.LastIndexOf(","));
                        }
                    }
                    mDetail += " )";
                    //add the return type to the associations List, so that 
                    //the association lines for this class can be obtained, and 
                    //possibly drawn on the container
                    string rName = GetGenericsForType(mi.ReturnType);
                    //dont want to include void as an association type
                    if (!string.IsNullOrEmpty(rName))
                    {
                        rName = GetGenericsForType(mi.ReturnType);
                        rName = LowerAndTrim(rName);
                        string association = mi.ReturnType.IsGenericType ? mi.ReturnType.GetGenericTypeDefinition().FullName : mi.ReturnType.FullName;
                        if (!Associations.Contains(association))
                        {
                            Associations.Add(association);
                        }
                        //do we want to display method return types
                        if (ShowMethodReturnValues)
                            mDetail += " : " + rName;
                    }
                    else
                    {
                        //do we want to display method return types
                        if (ShowMethodReturnValues)
                            mDetail += " : void";
                    }

                    //work out whether this is a normal method, in which case add it
                    //or if its a property get/set method, should it be added
                    if (!ShowGetMethodForProperty && propGetters.Contains(mi))
                    {
                        /* hidden get method */
                    }
                    else if (!ShowSetMethodForProperty && propSetters.Contains(mi))
                    {
                        /* hidden set method */
                    }
                    else
                    {
                        if (ParseMethodBodyIL)
                            Methods.Add(new SerializableMethodData(mDetail, ReadMethodBodyAndAddAssociations(mi)));
                        else
                            Methods.Add(new SerializableMethodData(mDetail, ""));

                    }
                }
            }
            HasMethods = Methods.Any();
        }

        /// <summary>
        /// Code in this method does the following
        /// 1. Read the methodbodyIL string
        /// 2. Look at all ILInstructions and look for new objects being 
        ///    created inside the method, and add as Association
        /// 3. Finally return the method body IL for the diagram to use
        /// </summary>
        private String ReadMethodBodyAndAddAssociations(MethodInfo mi)
        {


            String ilBody = "";

            try
            {
                if (mi == null)
                    return "";

                if (mi.GetMethodBody() == null)
                    return "";

                MethodBodyReader mr = new MethodBodyReader(mi);

                foreach (ILInstruction instruction in mr.Instructions)
                {
                    if (instruction.Code.Name.ToLower().Equals("newobj"))
                    {
                        dynamic operandType = instruction.Operand;
                        String association = operandType.DeclaringType.FullName;
                        if (!Associations.Contains(association))
                        {
                            Associations.Add(association);
                        }
                    }
                }
                ilBody = mr.GetBodyCode();
                return ilBody;
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        private void ReflectOutEvents()
        {
            //do events
            foreach (EventInfo ei in TypeInAssembly.GetEvents(RequiredBindings))
            {
                if (TypeInAssembly == ei.DeclaringType)
                {
                    //add all the event types to the associations List, so that 
                    //the association lines for this class can be obtained, and 
                    //possibly drawn on the container
                    string eName = GetGenericsForType(ei.EventHandlerType);
                    eName = LowerAndTrim(eName);
                    string association = ei.EventHandlerType.IsGenericType ? ei.EventHandlerType.GetGenericTypeDefinition().FullName : ei.EventHandlerType.FullName;
                    if (!Associations.Contains(association))
                    {
                        Associations.Add(association);
                    }
                    //do we want long or short event description displayed
                    if (ShowEvents)
                        Events.Add(eName + " " + ei.Name);
                    else
                        Events.Add(ei.Name);
                }
            }

            HasEvents = Events.Any();
        }

        /// <summary>
        /// Takes an input string and lower cases it and trims 
        /// it. Taking all chars after a "." (if one exists", 
        /// then returns it
        /// </summary>
        /// <param name="sIn">the input string</param>
        /// <returns>the input string lower cased and trimmed</returns>
        private string LowerAndTrim(string sIn)
        {
            string s = sIn;
            //s = s.ToLower();
            if (s.IndexOf(".") > 0)
            {
                s = s.Substring(s.LastIndexOf(".") + 1);
            }
            return s;
        }

        /// <summary>
        /// Returs a string which is the name of the type in its full
        /// format. If its not a generic type, then the name of the
        /// t input parameter is simply returned, if however it is
        /// a generic method say a List of ints then the appropraite string
        /// will be retrurned
        /// </summary>
        /// <param name="t">The Type to check for generics</param>
        /// <returns>a string representing the type</returns>
        private string GetGenericsForType(Type t)
        {
            string name = "";
            if (!t.GetType().IsGenericType)
            {
                //see if there is a ' char, which there is for
                //generic types
                int idx = t.Name.IndexOfAny(new char[] { '`', '\'' });
                if (idx >= 0)
                {
                    name = t.Name.Substring(0, idx);
                    //get the generic arguments
                    Type[] genTypes = t.GetGenericArguments();
                    foreach (var genType in genTypes)
                    {
                        var type = genType.IsGenericType ? genType.GetGenericTypeDefinition() : genType;
                        if (type.FullName != null)
                            Associations.Add(type.FullName);
                    }

                    //and build the list of types for the result string
                    if (genTypes.Length == 1)
                    {
                        name += "<" + GetGenericsForType(genTypes[0]) + ">";
                    }
                    else
                    {
                        name += "<";
                        foreach (Type gt in genTypes)
                        {
                            name += GetGenericsForType(gt) + ", ";
                        }
                        if (name.LastIndexOf(",") > 0)
                        {
                            name = name.Substring(0, name.LastIndexOf(","));
                        }
                        name += ">";
                    }
                }
                else
                {
                    name = t.Name;
                }
                return name;
            }
            else
            {
                return t.Name;
            }
        }
        #endregion
    }
}
