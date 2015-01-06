using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace OrbItProcs
{
    public class FPInfo
    {
        private FieldInfo _fieldInfo;
        public FieldInfo fieldInfo { get { return _fieldInfo; } set { _fieldInfo = value; } }

        private PropertyInfo _propertyInfo;
        public PropertyInfo propertyInfo { get { return _propertyInfo; } set { _propertyInfo = value; } }

        public string DeclaringTypeName { get; set; }

        public string Name { get; set; }
        public Type FPType
        {
            get
            {
                if (propertyInfo != null)
                {
                    return propertyInfo.PropertyType;
                }
                else if (fieldInfo != null)
                {
                    return fieldInfo.FieldType;
                }
                return null;
            }
            set { }
        }
        public FPInfo (FieldInfo fieldInfo)
        {
            this.fieldInfo = fieldInfo;
            this.DeclaringTypeName = this.fieldInfo.DeclaringType.ToString();
            Name = fieldInfo.Name;
        }
        public FPInfo (PropertyInfo propertyInfo)
        {
            this.propertyInfo = propertyInfo;
            this.DeclaringTypeName = this.propertyInfo.DeclaringType.ToString();
            Name = propertyInfo.Name;
        }
        public FPInfo(FieldInfo fieldInfo, PropertyInfo propertyInfo) //for copying component use
        {
            this.propertyInfo = propertyInfo;
            this.fieldInfo = fieldInfo;
            if (propertyInfo != null) 
            {
                this.DeclaringTypeName = this.propertyInfo.DeclaringType.ToString();
                Name = propertyInfo.Name;
            }
            else if (fieldInfo != null)
            {
                this.DeclaringTypeName = this.fieldInfo.DeclaringType.ToString();
                Name = fieldInfo.Name;
            }
            else Name = "error_Name_1";
        }
        public FPInfo(FPInfo old) //for copying component use
        {
            this.propertyInfo = old.propertyInfo;
            this.fieldInfo = old.fieldInfo;

            if (propertyInfo != null)
            {
                Name = propertyInfo.Name;
                DeclaringTypeName = propertyInfo.DeclaringType.ToString();
            }
            else if (fieldInfo != null)
            {
                Name = fieldInfo.Name;
                DeclaringTypeName = fieldInfo.DeclaringType.ToString();
            }
            else if (old.DeclaringTypeName != null)
            {
                PropertyInfo pi = Type.GetType(old.DeclaringTypeName).GetProperty(old.Name);
                if (pi != null)
                {
                    this.propertyInfo = pi;
                    Name = old.Name;
                    return;
                }
                FieldInfo fi = Type.GetType(old.DeclaringTypeName).GetField(old.Name);
                if (fi != null)
                {
                    this.fieldInfo = fi;
                    Name = old.Name;
                    return;
                }
            }
            else Name = "error_Name_2";
        }
        public object GetValue(object obj)
        {
            if (propertyInfo != null)
            {
                return propertyInfo.GetValue(obj, null);
            }
            else if (fieldInfo != null)
            {
                return fieldInfo.GetValue(obj);
            }
            return null;
        }
        public void SetValue(object obj, object value)
        {
            if (propertyInfo != null)
            {
                propertyInfo.SetValue(obj, value, null);
            }
            else if (fieldInfo != null)
            {
                if (fieldInfo.IsLiteral) return;
                fieldInfo.SetValue(obj, value);
            }
        }
    }
}
