using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OrbItProcs;

namespace OrbItProcs
{
    //[System.AttributeUsage(System.AttributeTargets.Field |
    //                       System.AttributeTargets.Property)
    //]
    //public class DoNotInspect : System.Attribute { }


    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class InspectMethod : System.Attribute { }

    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class CopyNodeProperty : System.Attribute { }
    
    public enum UserLevel
    {
        User = 0,
        Advanced = 1,
        Developer = 2,
        Debug = 3,
        Never = 99
    }
 

    [System.AttributeUsage(System.AttributeTargets.Field |
                           System.AttributeTargets.Property|
                           System.AttributeTargets.Class)]
    public class Info : System.Attribute {
        

        public UserLevel userLevel;
        public string summary="";
        public mtypes compType;
        public Info(UserLevel userLevel)
        {
            this.userLevel = userLevel;
            
        }

        public Info(UserLevel userLevel, string summary)
        {
            this.userLevel = userLevel;
            this.summary = summary;

            if (userLevel == UserLevel.User || userLevel == UserLevel.Advanced)
                if (summary.Length < 5) throw new NotImplementedException("Please properly Document any fields/properties that are meant to be used by users");
        }
        public Info(UserLevel userLevel, string summary, mtypes compType)
        {
            this.userLevel = userLevel;
            this.summary = summary;
            this.compType = compType;

            if (userLevel == UserLevel.User || userLevel == UserLevel.Advanced)
                if (summary.Length < 5) throw new NotImplementedException("Please properly Document any fields/properties that are meant to be used by users");
        }
    }
    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class Clickable : System.Attribute
    {
    }
}
