using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SISPK.Models;
using SISPK.Filters;
using SISPK.Helpers;
using System.Security.Cryptography;
using System.Text.RegularExpressions;


namespace SISPK.Controllers.WebServices
{
    public class SNIAPIController : Controller
    {
        //
        // GET: /SNI/
        private SISPKEntities db = new SISPKEntities();
        public String Index() { return null; }

        public ActionResult getSNIList(
            string username = "",
            string password = "",
            string tahun_sni = "",
            string keyword = "aa",
            string page = "1",
            string limit = "10"
        )
        {
            int status = 0;
            string message = "";
            page = (GetNumberOnly(page) == "0") ? "1" : GetNumberOnly(page);
            limit = (GetNumberOnly(limit) == "0") ? "10" : GetNumberOnly(limit);

            var validate = ValidateAccountParam(username, password, tahun_sni);

            //return View();

            if (validate != "")
            {
                status = 203;
                message = validate;
            }
            else
            {
                string passwordGen = GenPassword(password);
                //var UserCount = db.Database.SqlQuery<SYS_USER>("SELECT * FROM SYS_USER WHERE USER_NAME = '" + username + "' AND USER_PASSWORD = '" + passwordGen + "' AND USER_STATUS = 1").SingleOrDefault();
                if (username == "sni" && password == "sni123")
                {
                    status = 200;


                    var start = Convert.ToInt32(limit) * (Convert.ToInt32(page) - 1);
                    //var ProposalLists = db.Database.SqlQuery<VIEW_SNI>("SELECT * FROM VIEW_SNI WHERE ROWNUM <= 400");
                    var ProposalLists = db.Database.SqlQuery<VIEW_SNI>("SELECT SNI_ID,DSNI_DOC_INFO,SNI_PROPOSAL_ID,SNI_DOC_ID,SNI_NOMOR,SNI_JUDUL,SNI_STATUS,SNI_JUDUL_ENG,SNI_IS_PUBLISH,SNI_PUBLISH_START_DATE,SNI_PUBLISH_START_DATE_NAME,SNI_PUBLISH_END_DATE,SNI_PUBLISH_END_DATE_NAME,SNI_TIDAK_BERLAKU,PROPOSAL_ID,PROPOSAL_TYPE,PROPOSAL_TYPE_NAME,PROPOSAL_TAHAPAN,PROPOSAL_YEAR,PROPOSAL_KOMTEK_ID,PROPOSAL_KONSEPTOR,PROPOSAL_INSTITUSI,PROPOSAL_TIM_NAMA,PROPOSAL_TIM_ALAMAT,PROPOSAL_TIM_PHONE,PROPOSAL_TIM_EMAIL,PROPOSAL_TIM_FAX,PROPOSAL_NO_SNI_PROPOSAL,PROPOSAL_JUDUL_SNI_PROPOSAL,PROPOSAL_PNPS_CODE,PROPOSAL_JUDUL_PNPS,PROPOSAL_JUDUL_PNPS_ENG,PROPOSAL_RUANG_LINGKUP,PROPOSAL_RUANG_LINGKUP_ENG,PROPOSAL_JENIS_PERUMUSAN,PROPOSAL_JENIS_PERUMUSAN_NAME,PROPOSAL_JALUR,PROPOSAL_JALUR_NAME,PROPOSAL_JENIS_ADOPSI,PROPOSAL_JENIS_ADOPSI_NAME,PROPOSAL_METODE_ADOPSI,PROPOSAL_METODE_ADOPSI_NAME,PROPOSAL_TERJEMAHAN_SNI_ID,PROPOSAL_TERJEMAHAN_NOMOR,PROPOSAL_TERJEMAHAN_JUDUL,PROPOSAL_RALAT_SNI_ID,PROPOSAL_RALAT_NOMOR,PROPOSAL_RALAT_JUDUL,PROPOSAL_AMD_SNI_ID,PROPOSAL_AMD_NOMOR,PROPOSAL_AMD_JUDUL,PROPOSAL_IS_URGENT,PROPOSAL_IS_URGENT_NAME,PROPOSAL_PASAL,PROPOSAL_IS_HAK_PATEN,PROPOSAL_IS_HAK_PATEN_DESC,PROPOSAL_INFORMASI,PROPOSAL_TUJUAN,PROPOSAL_PROGRAM_PEMERINTAH,PROPOSAL_PIHAK_BERKEPENTINGAN,PROPOSAL_LAMPIRAN_FILE_PATH,PROPOSAL_IS_ASSIGN_KOMTEK,PROPOSAL_IS_ST_KOMTEK,PROPOSAL_IS_POLLING,PROPOSAL_POLLING_ID,PROPOSAL_IS_BATAL,PROPOSAL_ICS_NAME,PROPOSAL_ABSTRAK,PROPOSAL_ABSTRAK_ENG,PROPOSAL_CREATE_BY,PROPOSAL_CREATE_DATE,PROPOSAL_CREATE_DATE_NAME,PROPOSAL_STATUS,PROPOSAL_APPROVAL_STATUS,PROPOSAL_STATUS_NAME,PROGRESS,PROPOSAL_STATUS_PROSES,PROPOSAL_LOG_CODE,PROPOSAL_SNI_ID_OLD,KOMTEK_ID,KOMTEK_PARENT_CODE,KOMTEK_CODE,KOMTEK_BIDANG_ID,KOMTEK_INSTANSI_ID,KOMTEK_NAME,KOMTEK_SEKRETARIAT,KOMTEK_ADDRESS,KOMTEK_PHONE,KOMTEK_FAX,KOMTEK_EMAIL,KOMTEK_SK_PENETAPAN,KOMTEK_TANGGAL_PEMBENTUKAN,KOMTEK_DESCRIPTION,INSTANSI_ID,INSTANSI_CODE,INSTANSI_NAME,BIDANG_ID,BIDANG_CODE,BIDANG_NAME,BIDANG_SHORT_NAME,APPROVAL_ID,APPROVAL_TYPE,APPROVAL_TYPE_NAME,APPROVAL_REASON,APPROVAL_DATE,APPROVAL_BY,USER_FULL_NAME,APPROVAL_STATUS_SESSION,POLLING_IS_CREATE,POLLING_MONITORING_NAME,POLLING_MONITORING_TYPE,POLLING_MONITORING_JML_HARI,PROPOSAL_FULL_DATE_NAME,POLLING_ID,POLLING_PROPOSAL_ID,POLLING_TYPE,POLLING_START_DATE,POLLING_END_DATE,POLLING_VERSION,POLLING_REASON,POLLING_IS_KUORUM,POLLING_JML_PARTISIPAN,DSNI_DOC_ID,DSNI_DOC_CODE,DSNI_DOC_NAME,DSNI_DOC_DESCRIPTION,DSNI_DOC_REGULATOR,DSNI_DOC_FILE_PATH,DSNI_DOC_FILE_NAME,DSNI_DOC_FILETYPE,DSNI_DOC_LINK,DSNI_DOC_EDITABLE,SNI_SK_ID,SNI_SK_SNI_ID,SNI_SK_DOC_ID,SNI_SK_NOMOR,SNI_SK_DATE,SNI_SK_DATE_NAME,SNI_SK_STATUS,DSK_DOC_ID,DSK_DOC_CODE,DSK_DOC_NUMBER,DSK_DOC_NAME,DSK_DOC_DESCRIPTION,DSK_DOC_REGULATOR,DSK_DOC_INFO,DSK_DOC_FILE_PATH,DSK_DOC_FILE_NAME,DSK_DOC_FILETYPE,DSK_DOC_LINK,DSK_DOC_EDITABLE,SNI_MAINTENANCE_STS,SNI_CREATE_DATE,PROPOSAL_ICS_DETAIL_NAME,IS_LIMIT_DOWNLOAD,TAHUN_SNI,UMUR_SNI FROM VIEW_SNI WHERE TAHUN_SNI = " + tahun_sni);


                    //return Json(new
                    //{
                    //    status,
                    //    message,
                    //    results = ProposalLists
                    //}, JsonRequestBehavior.AllowGet);

                    var JsonResult = Json(new
                    {
                        status,
                        message,
                        results = ProposalLists
                    }, JsonRequestBehavior.AllowGet);
                    JsonResult.MaxJsonLength = int.MaxValue;
                    return JsonResult;

                }
                else
                {
                    status = 203;
                    message = "Username and Password do not match";
                }
                
            }

            string[] dummyresult = new string[0];
            return Json(new
            {
                status,
                message,
                results = dummyresult.ToArray()
            }, JsonRequestBehavior.AllowGet);
        }

        private string ValidateAccountParam(
           string username = "",
           string password = "",
           string tahun_sni = ""
       )
        {
            string result = "";
            if (username == "" && password == "" && tahun_sni == "")
            {
                result = "Username, Password and SNI Year is required";
            }
            else if (username == "")
            {
                result = "Username is required";
            }
            else if (password == "")
            {
                result = "Password is required";
            }
            else if (tahun_sni == "")
            {
                result = "SNI Year is required";
            }
            return result;
        }

        private string GenPassword(string input)
        {
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();
        }

        private string GetNumberOnly(string text)
        {
            text = Regex.Match(Regex.Match(text, @"\d+").Value, @"^\d*\.?\d+$").Value;
            text = (text == "") ? "0" : text;
            return text;
        }

    }
}
