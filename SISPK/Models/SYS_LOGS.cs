//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SISPK.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class SYS_LOGS
    {
        public long LOG_ID { get; set; }
        public string LOG_CODE { get; set; }
        public string LOG_USER { get; set; }
        public Nullable<short> LOG_USER_TYPE { get; set; }
        public Nullable<long> LOG_MENU_ID { get; set; }
        public Nullable<short> LOG_ACTION { get; set; }
        public string LOG_OBJECT { get; set; }
        public Nullable<System.DateTime> LOG_DATE { get; set; }
    }
}
