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
    
    public partial class TRX_SURAT_TUGAS
    {
        public decimal ST_ID { get; set; }
        public Nullable<decimal> ST_PROPOSAL_ID { get; set; }
        public string ST_NO_SURAT { get; set; }
        public string ST_LAMPIRAN { get; set; }
        public Nullable<System.DateTime> ST_DATE { get; set; }
        public Nullable<decimal> ST_PROPOSAL_STATUS { get; set; }
        public Nullable<decimal> ST_VERSI { get; set; }
        public Nullable<decimal> ST_CREATE_BY { get; set; }
        public Nullable<System.DateTime> ST_CREATE_DATE { get; set; }
        public Nullable<decimal> ST_UPDATE_BY { get; set; }
        public Nullable<System.DateTime> ST_UPDATE_DATE { get; set; }
        public Nullable<decimal> ST_STATUS { get; set; }
    }
}