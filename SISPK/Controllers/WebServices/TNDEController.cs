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
    public class TNDEController : Controller
    {
        private SISPKEntities db = new SISPKEntities();
        public String Index() { return null; }

        public ActionResult getProposalLists(
            string username = "",
            string password = "",
            string keyword = "",
            string page = "1",
            string limit = "10"
        )
        {
            int status = 0;
            string message = "";
            page = (GetNumberOnly(page) == "0") ? "1" : GetNumberOnly(page);
            limit = (GetNumberOnly(limit) == "0") ? "10" : GetNumberOnly(limit);

            var validate = ValidateAccountParam(username, password);

            if (validate != "")
            {
                status = 203;
                message = validate;
            }
            else
            {
                string passwordGen = GenPassword(password);
                var UserCount = db.Database.SqlQuery<SYS_USER>("SELECT * FROM SYS_USER WHERE USER_NAME = '" + username + "' AND USER_PASSWORD = '" + passwordGen + "' AND USER_STATUS = 1").SingleOrDefault();
                if (UserCount != null)
                {
                    status = 200;

                    var start = Convert.ToInt32(limit)*(Convert.ToInt32(page)-1);
                    var ProposalLists = db.Database.SqlQuery<WSTNDEProposalList>("SELECT PROPOSAL_ID AS ID, PROPOSAL_JUDUL_SNI_PROPOSAL AS TITLE FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM TRX_PROPOSAL WHERE PROPOSAL_STATUS = 11 AND PROPOSAL_STATUS_PROSES = 1 AND UPPER(PROPOSAL_JUDUL_SNI_PROPOSAL) LIKE '%" + keyword.ToUpper() + "%') T1 WHERE ROWNUM <= " + Convert.ToString(Convert.ToInt32(limit) + start) + ") WHERE ROWNUMBER > " + Convert.ToString(start));
                    return Json(new
                    {
                        status,
                        message,
                        results = ProposalLists
                    }, JsonRequestBehavior.AllowGet);
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

        public ActionResult getKomtekMembers(
            string username = "",
            string password = "",
            string proposal_id = ""
        )
        {
            int status = 0;
            string message = "";
            proposal_id = (GetNumberOnly(proposal_id) == "0") ? "1" : GetNumberOnly(proposal_id);

            var validate = ValidateAccountParam(username, password);

            if (validate != "")
            {
                status = 203;
                message = validate;
            }
            else
            {
                string passwordGen = GenPassword(password);
                var UserCount = db.Database.SqlQuery<SYS_USER>("SELECT * FROM SYS_USER WHERE USER_NAME = '" + username + "' AND USER_PASSWORD = '" + passwordGen + "' AND USER_STATUS = 1").SingleOrDefault();
                if (UserCount != null)
                {
                    status = 200;
                    var ProposalData = db.Database.SqlQuery<TRX_PROPOSAL>("SELECT * FROM TRX_PROPOSAL WHERE PROPOSAL_STATUS = 2 AND PROPOSAL_STATUS_PROSES = 1 AND PROPOSAL_ID = " + proposal_id).SingleOrDefault();
                    if (ProposalData != null)
                    {
                        var KomtekLists = db.Database.SqlQuery<WSTNDEKomtekList>("SELECT KOMTEK_ANGGOTA_KODE AS CODE, KOMTEK_ANGGOTA_NAMA AS NAME, KOMTEK_ANGGOTA_TELP AS PHONE, KOMTEK_ANGGOTA_FAX AS FAX, KOMTEK_ANGGOTA_EMAIL AS EMAIL FROM MASTER_KOMTEK_ANGGOTA WHERE KOMTEK_ANGGOTA_KOMTEK_ID = " + ProposalData.PROPOSAL_KOMTEK_ID);
                        /*while (KomtekLists)
                        {
                            var values = new List<object>();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                values.Add(reader.GetValue(i));
                            }
                            yield return values.ToArray();
                        }
                        /*var result = from list in KomtekLists
                                     select new string[] 
                        {
                             Convert.ToString(list.PROPOSAL_ID),
                             Convert.ToString(list.PROPOSAL_JUDUL_SNI_PROPOSAL)
                        };*/
                        return Json(new
                        {
                            status,
                            message,
                            results = KomtekLists
                        }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        status = 204;
                        message = "Proposal data not found";
                    }
                    
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
            string password = ""
        )
        {
            string result = "";
            if (username == "" && password == "")
            {
                result = "Username and Password is required";
            }
            else if (username == "")
            {
                result = "Username is required";
            }
            else if (password == "")
            {
                result = "Password is required";
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
