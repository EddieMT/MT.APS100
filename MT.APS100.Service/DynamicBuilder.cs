using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;

namespace MT.APS100.Service
{
    public class DynamicBuilder
    {
        private FieldBuilder fieldbuilder = null;
        private PropertyBuilder GetterBuilder = null;
        private MethodBuilder GetPropBuilder = null;
        private MethodBuilder SetPropBuilder = null;
        private ILGenerator getGenerator = null;
        private ILGenerator setGenerator = null;
        private TypeBuilder typebuilder = null;
        private ModuleBuilder modulebuilder = null;
        private AssemblyBuilder assemblybuilder = null;
        private AssemblyName assemblyname = null;
        private int x = 0;
        private string fieldname = string.Empty;
        private string property = string.Empty;
        private string getter = string.Empty;
        private string setter = string.Empty;

        //public Type BuildDynamicTypeWithProperties(string _ObjectName, string[] _FieldName, string[] _PropName, string[] _Getter, string[] _Setter)
        public Type BuildDynamicTypeWithProperties(string _ObjectName, List<string> _items)
        {
            string[] _FieldName = CreateFields(_items);
            string[] _PropName = CreateProperties(_items);
            string[] _Getter = CreateGetter(_items);
            string[] _Setter = CreateSetter(_items);
            //Fernando -- Using Reflection to generate an assembly with objects at run time.
            //This is necessary to create dynamically based classes populated from an external file at runtime such as JSON

            //Begin This Section uses Reflection to generate an assembly at run time
            //if (!System.IO.File.Exists(@"C:\Repos\Merlin\MT\MT\bin\Debug\MerlinMagic.dll"))
            //{
            AppDomain domain = Thread.GetDomain();
            assemblyname = new AssemblyName();
            assemblyname.Name = "MerlinMagic";
            assemblybuilder = domain.DefineDynamicAssembly(assemblyname, AssemblyBuilderAccess.RunAndSave);
            modulebuilder = assemblybuilder.DefineDynamicModule(assemblyname.Name, assemblyname.Name + ".dll");
            typebuilder = modulebuilder.DefineType(_ObjectName, TypeAttributes.Public);

            //}

            //End

            //Begin Creates the object at run time

            //End
            //Begin loop through the properties,fields,getters and setters generating proper IL Code
            do
            {
                string fieldname = GetField(_FieldName, x);
                string property = GetProperty(_PropName, x);
                string getter = GetGetter(_Getter, x);
                string setter = GetSetter(_Setter, x);

                MethodAttributes getSetAttributes = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig;
                fieldbuilder = typebuilder.DefineField(fieldname, typeof(string), FieldAttributes.Private);
                GetterBuilder = typebuilder.DefineProperty(property, PropertyAttributes.HasDefault, typeof(string), null);
                GetPropBuilder = typebuilder.DefineMethod(getter, getSetAttributes, typeof(string), Type.EmptyTypes);
                SetPropBuilder = typebuilder.DefineMethod(setter, getSetAttributes, null, new Type[] { typeof(string) });
                getGenerator = GetPropBuilder.GetILGenerator();
                getGenerator.Emit(OpCodes.Ldarg_0);
                getGenerator.Emit(OpCodes.Ldfld, fieldbuilder);
                getGenerator.Emit(OpCodes.Ret);
                GetterBuilder.SetGetMethod(GetPropBuilder);
                setGenerator = SetPropBuilder.GetILGenerator();
                setGenerator = SetPropBuilder.GetILGenerator();
                setGenerator.Emit(OpCodes.Ldarg_0);
                setGenerator.Emit(OpCodes.Ldarg_1);
                setGenerator.Emit(OpCodes.Stfld, fieldbuilder);
                setGenerator.Emit(OpCodes.Ret);
                GetterBuilder.SetSetMethod(SetPropBuilder);

                x++;
            } while (x < _FieldName.Length);

            Type retval = typebuilder.CreateType();
            assemblybuilder.Save(assemblyname.Name + ".dll");
            return retval;
        }

        public string[] CreateFields(List<string> items)
        {
            int x = 0;
            string[] fields = new string[items.Count];
            foreach (string _item in items)
            {
                fields[x] = _item.ToLower();
                x++;
            }
            return fields;
        }

        public string[] CreateProperties(List<string> items)
        {
            int x = 0;
            string[] properties = new string[items.Count];
            foreach (string _item in items)
            {
                properties[x] = _item.First().ToString().ToUpper() + string.Join("", _item.Skip(1));
                x++;
            }
            return properties;
        }

        public string[] CreateGetter(List<string> items)
        {
            int x = 0;
            string[] getter = new string[items.Count];
            foreach (string _item in items)
            {
                getter[x] = "Get_" + _item;
                x++;
            }
            return getter;
        }

        public string[] CreateSetter(List<string> items)
        {
            int x = 0;
            string[] setter = new string[items.Count];
            foreach (string _item in items)
            {
                setter[x] = "Set_" + _item;
                x++;
            }
            return setter;
        }

        public string GetField(string[] _fields, int x)
        {
            return _fields[x];
        }

        public string GetProperty(string[] _properties, int x)
        {
            return _properties[x];
        }

        public string GetGetter(string[] _getter, int x)
        {
            return _getter[x];
        }

        public string GetSetter(string[] _setter, int x)
        {
            return _setter[x];
        }
    }
}