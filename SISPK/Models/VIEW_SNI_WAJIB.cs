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
    
    public partial class VIEW_SNI_WAJIB
    {
        public decimal SNI_ID { get; set; }
        public Nullable<decimal> SNI_PROPOSAL_ID { get; set; }
        public Nullable<decimal> SNI_DOC_ID { get; set; }
        public string SNI_NOMOR { get; set; }
        public string SNI_JUDUL { get; set; }
        public Nullable<decimal> SNI_STATUS { get; set; }
        public string SNI_JUDUL_ENG { get; set; }
        public Nullable<decimal> SNI_IS_PUBLISH { get; set; }
        public Nullable<System.DateTime> SNI_PUBLISH_START_DATE { get; set; }
        public string SNI_PUBLISH_START_DATE_NAME { get; set; }
        public Nullable<System.DateTime> SNI_PUBLISH_END_DATE { get; set; }
        public string SNI_PUBLISH_END_DATE_NAME { get; set; }
        public Nullable<decimal> SNI_TIDAK_BERLAKU { get; set; }
        public decimal PROPOSAL_ID { get; set; }
        public Nullable<decimal> PROPOSAL_TYPE { get; set; }
        public string PROPOSAL_TYPE_NAME { get; set; }
        public string PROPOSAL_TAHAPAN { get; set; }
        public Nullable<decimal> PROPOSAL_YEAR { get; set; }
        public Nullable<decimal> PROPOSAL_KOMTEK_ID { get; set; }
        public string PROPOSAL_KONSEPTOR { get; set; }
        public string PROPOSAL_INSTITUSI { get; set; }
        public string PROPOSAL_TIM_NAMA { get; set; }
        public string PROPOSAL_TIM_ALAMAT { get; set; }
        public string PROPOSAL_TIM_PHONE { get; set; }
        public string PROPOSAL_TIM_EMAIL { get; set; }
        public string PROPOSAL_TIM_FAX { get; set; }
        public string PROPOSAL_NO_SNI_PROPOSAL { get; set; }
        public string PROPOSAL_JUDUL_SNI_PROPOSAL { get; set; }
        public string PROPOSAL_PNPS_CODE { get; set; }
        public string PROPOSAL_JUDUL_PNPS { get; set; }
        public string PROPOSAL_JUDUL_PNPS_ENG { get; set; }
        public string PROPOSAL_RUANG_LINGKUP { get; set; }
        public string PROPOSAL_RUANG_LINGKUP_ENG { get; set; }
        public Nullable<decimal> PROPOSAL_JENIS_PERUMUSAN { get; set; }
        public string PROPOSAL_JENIS_PERUMUSAN_NAME { get; set; }
        public Nullable<decimal> PROPOSAL_JALUR { get; set; }
        public string PROPOSAL_JALUR_NAME { get; set; }
        public Nullable<decimal> PROPOSAL_JENIS_ADOPSI { get; set; }
        public string PROPOSAL_JENIS_ADOPSI_NAME { get; set; }
        public Nullable<decimal> PROPOSAL_METODE_ADOPSI { get; set; }
        public string PROPOSAL_METODE_ADOPSI_NAME { get; set; }
        public Nullable<decimal> PROPOSAL_TERJEMAHAN_SNI_ID { get; set; }
        public string PROPOSAL_TERJEMAHAN_NOMOR { get; set; }
        public string PROPOSAL_TERJEMAHAN_JUDUL { get; set; }
        public Nullable<decimal> PROPOSAL_RALAT_SNI_ID { get; set; }
        public string PROPOSAL_RALAT_NOMOR { get; set; }
        public string PROPOSAL_RALAT_JUDUL { get; set; }
        public Nullable<decimal> PROPOSAL_AMD_SNI_ID { get; set; }
        public string PROPOSAL_AMD_NOMOR { get; set; }
        public string PROPOSAL_AMD_JUDUL { get; set; }
        public Nullable<decimal> PROPOSAL_IS_URGENT { get; set; }
        public string PROPOSAL_IS_URGENT_NAME { get; set; }
        public string PROPOSAL_PASAL { get; set; }
        public Nullable<decimal> PROPOSAL_IS_HAK_PATEN { get; set; }
        public string PROPOSAL_IS_HAK_PATEN_DESC { get; set; }
        public string PROPOSAL_INFORMASI { get; set; }
        public string PROPOSAL_TUJUAN { get; set; }
        public string PROPOSAL_PROGRAM_PEMERINTAH { get; set; }
        public string PROPOSAL_PIHAK_BERKEPENTINGAN { get; set; }
        public string PROPOSAL_LAMPIRAN_FILE_PATH { get; set; }
        public Nullable<decimal> PROPOSAL_IS_ASSIGN_KOMTEK { get; set; }
        public Nullable<decimal> PROPOSAL_IS_ST_KOMTEK { get; set; }
        public Nullable<decimal> PROPOSAL_IS_POLLING { get; set; }
        public Nullable<decimal> PROPOSAL_POLLING_ID { get; set; }
        public Nullable<decimal> PROPOSAL_IS_BATAL { get; set; }
        public string PROPOSAL_ICS_NAME { get; set; }
        public Nullable<decimal> PROPOSAL_CREATE_BY { get; set; }
        public Nullable<System.DateTime> PROPOSAL_CREATE_DATE { get; set; }
        public string PROPOSAL_CREATE_DATE_NAME { get; set; }
        public Nullable<decimal> PROPOSAL_STATUS { get; set; }
        public Nullable<decimal> PROPOSAL_APPROVAL_STATUS { get; set; }
        public string PROPOSAL_STATUS_NAME { get; set; }
        public string PROGRESS { get; set; }
        public Nullable<decimal> PROPOSAL_STATUS_PROSES { get; set; }
        public string PROPOSAL_LOG_CODE { get; set; }
        public Nullable<decimal> PROPOSAL_SNI_ID_OLD { get; set; }
        public Nullable<decimal> KOMTEK_ID { get; set; }
        public string KOMTEK_PARENT_CODE { get; set; }
        public string KOMTEK_CODE { get; set; }
        public Nullable<decimal> KOMTEK_BIDANG_ID { get; set; }
        public Nullable<decimal> KOMTEK_INSTANSI_ID { get; set; }
        public string KOMTEK_NAME { get; set; }
        public string KOMTEK_SEKRETARIAT { get; set; }
        public string KOMTEK_ADDRESS { get; set; }
        public string KOMTEK_PHONE { get; set; }
        public string KOMTEK_FAX { get; set; }
        public string KOMTEK_EMAIL { get; set; }
        public string KOMTEK_SK_PENETAPAN { get; set; }
        public Nullable<System.DateTime> KOMTEK_TANGGAL_PEMBENTUKAN { get; set; }
        public string KOMTEK_DESCRIPTION { get; set; }
        public Nullable<decimal> INSTANSI_ID { get; set; }
        public string INSTANSI_CODE { get; set; }
        public string INSTANSI_NAME { get; set; }
        public Nullable<decimal> BIDANG_ID { get; set; }
        public string BIDANG_CODE { get; set; }
        public string BIDANG_NAME { get; set; }
        public string BIDANG_SHORT_NAME { get; set; }
        public Nullable<decimal> APPROVAL_ID { get; set; }
        public Nullable<decimal> APPROVAL_TYPE { get; set; }
        public string APPROVAL_TYPE_NAME { get; set; }
        public string APPROVAL_REASON { get; set; }
        public Nullable<System.DateTime> APPROVAL_DATE { get; set; }
        public Nullable<decimal> APPROVAL_BY { get; set; }
        public string USER_FULL_NAME { get; set; }
        public Nullable<decimal> APPROVAL_STATUS_SESSION { get; set; }
        public Nullable<decimal> POLLING_IS_CREATE { get; set; }
        public string POLLING_MONITORING_NAME { get; set; }
        public string POLLING_MONITORING_TYPE { get; set; }
        public Nullable<decimal> POLLING_MONITORING_JML_HARI { get; set; }
        public string PROPOSAL_FULL_DATE_NAME { get; set; }
        public Nullable<decimal> POLLING_ID { get; set; }
        public Nullable<decimal> POLLING_PROPOSAL_ID { get; set; }
        public string POLLING_TYPE { get; set; }
        public Nullable<System.DateTime> POLLING_START_DATE { get; set; }
        public Nullable<System.DateTime> POLLING_END_DATE { get; set; }
        public Nullable<decimal> POLLING_VERSION { get; set; }
        public string POLLING_REASON { get; set; }
        public Nullable<decimal> POLLING_IS_KUORUM { get; set; }
        public Nullable<decimal> POLLING_JML_PARTISIPAN { get; set; }
        public Nullable<decimal> DSNI_DOC_ID { get; set; }
        public string DSNI_DOC_CODE { get; set; }
        public string DSNI_DOC_NAME { get; set; }
        public string DSNI_DOC_DESCRIPTION { get; set; }
        public Nullable<decimal> DSNI_DOC_REGULATOR { get; set; }
        public string DSNI_DOC_FILE_PATH { get; set; }
        public string DSNI_DOC_FILE_NAME { get; set; }
        public string DSNI_DOC_FILETYPE { get; set; }
        public string DSNI_DOC_LINK { get; set; }
        public Nullable<decimal> DSNI_DOC_EDITABLE { get; set; }
        public Nullable<decimal> SNI_SK_ID { get; set; }
        public Nullable<decimal> SNI_SK_SNI_ID { get; set; }
        public Nullable<decimal> SNI_SK_DOC_ID { get; set; }
        public string SNI_SK_NOMOR { get; set; }
        public Nullable<System.DateTime> SNI_SK_DATE { get; set; }
        public string SNI_SK_DATE_NAME { get; set; }
        public Nullable<decimal> SNI_SK_STATUS { get; set; }
        public Nullable<decimal> DSK_DOC_ID { get; set; }
        public string DSK_DOC_CODE { get; set; }
        public string DSK_DOC_NUMBER { get; set; }
        public string DSK_DOC_NAME { get; set; }
        public string DSK_DOC_DESCRIPTION { get; set; }
        public Nullable<decimal> DSK_DOC_REGULATOR { get; set; }
        public string DSK_DOC_INFO { get; set; }
        public string DSK_DOC_FILE_PATH { get; set; }
        public string DSK_DOC_FILE_NAME { get; set; }
        public string DSK_DOC_FILETYPE { get; set; }
        public string DSK_DOC_LINK { get; set; }
        public Nullable<decimal> DSK_DOC_EDITABLE { get; set; }
        public string PROPOSAL_ICS_DETAIL_NAME { get; set; }
        public Nullable<decimal> IS_LIMIT_DOWNLOAD { get; set; }
        public decimal RETEK_DETAIL_ID { get; set; }
        public Nullable<decimal> RETEK_DETAIL_RETEK_ID { get; set; }
        public Nullable<decimal> RETEK_DETAIL_SNI_ID { get; set; }
        public Nullable<decimal> RETEK_DETAIL_CREATE_BY { get; set; }
        public Nullable<System.DateTime> RETEK_DETAIL_CREATE_DATE { get; set; }
        public Nullable<decimal> RETEK_DETAIL_UPDATE_BY { get; set; }
        public Nullable<System.DateTime> RETEK_DETAIL_UPDATE_DATE { get; set; }
        public Nullable<decimal> RETEK_DETAIL_STATUS { get; set; }
        public Nullable<decimal> RETEK_DETAIL_DOCUMENT_ID { get; set; }
        public decimal RETEK_ID { get; set; }
        public string RETEK_NO_SK { get; set; }
        public string RETEK_TENTANG { get; set; }
        public Nullable<decimal> RETEK_REGULATOR { get; set; }
        public Nullable<decimal> RETEK_FILE { get; set; }
        public string RETEK_KETERANGAN { get; set; }
        public string REGULATOR_CODE { get; set; }
        public string REGULATOR_NAME { get; set; }
    }
}
