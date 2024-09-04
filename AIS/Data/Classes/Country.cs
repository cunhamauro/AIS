using System;

namespace AIS.Data.Classes
{
    public class Country
    {
        #region Properties

        public Name Name { get; set; }

        public Flags Flags { get; set; }

        #endregion
    }

    #region Subclasses

    public class Name
    {
        public string Common { get; set; }
    }

    public class Flags
    {
        public string Png { get; set; }
    }

    #endregion
}
