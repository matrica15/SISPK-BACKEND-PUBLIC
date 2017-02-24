using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SISPK.Models;
using SISPK.Filters;
using SISPK.Helpers;
using System.IO;
using Aspose.Words;
using Aspose.Words.Lists;


namespace SISPK.Controllers.Pengajuan
{
	[Auth(RoleTipe = 1)]
	public class PengesahanController : Controller
	{
		private SISPKEntities db = new SISPKEntities();
		private GenarateNomor gn = new GenarateNomor();

		public ActionResult Index()
		{
			return View();
		}
		[Auth(RoleTipe = 5)]
		public ActionResult Approval(int id = 0)
		{
			//var DataProposal = db.Database.SqlQuery<VIEW_PROPOSAL>("SELECT * FROM VIEW_PROPOSAL WHERE PROPOSAL_ID = " + id).SingleOrDefault();
			var DataProposal = (from proposal in db.VIEW_PROPOSAL where proposal.PROPOSAL_ID == id select proposal).SingleOrDefault();
			var AcuanNormatif = (from an in db.VIEW_PROPOSAL_REF where an.PROPOSAL_REF_TYPE == 1 && an.PROPOSAL_REF_PROPOSAL_ID == id orderby an.PROPOSAL_REF_ID ascending select an).ToList();
			var AcuanNonNormatif = (from an in db.VIEW_PROPOSAL_REF where an.PROPOSAL_REF_TYPE == 2 && an.PROPOSAL_REF_PROPOSAL_ID == id orderby an.PROPOSAL_REF_ID ascending select an).ToList();
			var Bibliografi = (from an in db.VIEW_PROPOSAL_REF where an.PROPOSAL_REF_TYPE == 3 && an.PROPOSAL_REF_PROPOSAL_ID == id orderby an.PROPOSAL_REF_ID ascending select an).ToList();
			var ICS = (from an in db.VIEW_PROPOSAL_ICS where an.PROPOSAL_ICS_REF_PROPOSAL_ID == id orderby an.ICS_CODE ascending select an).ToList();
			var AdopsiList = (from an in db.TRX_PROPOSAL_ADOPSI where an.PROPOSAL_ADOPSI_PROPOSAL_ID == id orderby an.PROPOSAL_ADOPSI_NOMOR_JUDUL ascending select an).ToList();
			var RevisiList = db.Database.SqlQuery<VIEW_SNI_SELECT>("SELECT BB.* FROM TRX_PROPOSAL_REV AA INNER JOIN VIEW_SNI_SELECT BB ON AA.PROPOSAL_REV_MERIVISI_ID = BB.ID WHERE AA.PROPOSAL_REV_PROPOSAL_ID = '" + id + "' ORDER BY AA.PROPOSAL_REV_ID ASC").ToList();
			var Lampiran = db.Database.SqlQuery<VIEW_DOCUMENTS>("SELECT * FROM VIEW_DOCUMENTS WHERE DOC_FOLDER_ID = 10 AND DOC_RELATED_ID = " + id + " AND DOC_RELATED_TYPE = 30").FirstOrDefault();
			var Bukti = db.Database.SqlQuery<VIEW_DOCUMENTS>("SELECT * FROM VIEW_DOCUMENTS WHERE DOC_FOLDER_ID = 10 AND DOC_RELATED_ID = " + id + " AND DOC_RELATED_TYPE = 29").FirstOrDefault();
			var Surat = db.Database.SqlQuery<VIEW_DOCUMENTS>("SELECT * FROM VIEW_DOCUMENTS WHERE DOC_FOLDER_ID = 10 AND DOC_RELATED_ID = " + id + " AND DOC_RELATED_TYPE = 32").FirstOrDefault();
			var Outline = db.Database.SqlQuery<VIEW_DOCUMENTS>("SELECT * FROM VIEW_DOCUMENTS WHERE DOC_FOLDER_ID = 10 AND DOC_RELATED_ID = " + id + " AND DOC_RELATED_TYPE = 36").FirstOrDefault();
			var ListKomtek = (from komtek in db.MASTER_KOMITE_TEKNIS where komtek.KOMTEK_STATUS == 1 orderby komtek.KOMTEK_CODE ascending select komtek).ToList();
			ViewData["ListKomtek"] = ListKomtek;
			ViewData["DataProposal"] = DataProposal;
			ViewData["AcuanNormatif"] = AcuanNormatif;
			ViewData["AcuanNonNormatif"] = AcuanNonNormatif;
			ViewData["Bibliografi"] = Bibliografi;
			ViewData["ICS"] = ICS;
			ViewData["AdopsiList"] = AdopsiList;
			ViewData["RevisiList"] = RevisiList;
			ViewData["Lampiran"] = Lampiran;
			ViewData["Bukti"] = Bukti;
			ViewData["Surat"] = Surat;
			ViewData["Outline"] = Outline;

			var Dokumen = db.Database.SqlQuery<VIEW_DOCUMENTS>("SELECT * FROM VIEW_DOCUMENTS WHERE DOC_STATUS = 1 AND DOC_RELATED_ID = " + id).ToList();
			ViewData["Dokumen"] = Dokumen;
			string SearchName = DataProposal.PROPOSAL_JUDUL_PNPS;
			string[] Name = SearchName.Split(' ');
			string QueryRefLain = "SELECT * FROM VIEW_DOCUMENTS WHERE DOC_STATUS = 1 AND (DOC_RELATED_ID <> " + id + " OR DOC_RELATED_ID IS NULL) AND ( ";
			//string lastItem = Name.Last();
			int lastNameIndex = Name.Length;
			int count = 1;
			foreach (string Res in Name)
			{
				
				//if (!object.ReferenceEquals(Res, lastItem))
				//{
				//	QueryRefLain += " DOC_NAME_LOWER LIKE '%" + Res.ToLower() + "%' OR ";
				//}
				//else
				//{
				//	QueryRefLain += " DOC_NAME_LOWER LIKE '%" + Res.ToLower() + "%' )";
				//}
				if (count != lastNameIndex)
				{
					QueryRefLain += " DOC_NAME_LOWER LIKE '%" + Res.ToLower() + "%' OR ";
				}
				else
				{
					QueryRefLain += " DOC_NAME_LOWER LIKE '%" + Res.ToLower() + "%' )";
				}
				count++;
			}
			var RefLain = db.Database.SqlQuery<VIEW_DOCUMENTS>(QueryRefLain).ToList();
			ViewData["RefLain"] = RefLain;

			return View();
		}

		public ActionResult Detail(int id = 0)
		{
			//var DataProposal = (from proposal in db.VIEW_PROPOSAL where proposal.PROPOSAL_ID == id select proposal).SingleOrDefault();
			var DataProposal = db.Database.SqlQuery<VIEW_PROPOSAL>("SELECT * FROM VIEW_PROPOSAL WHERE PROPOSAL_ID = " + id).SingleOrDefault();
			var AcuanNormatif = (from an in db.VIEW_PROPOSAL_REF where an.PROPOSAL_REF_TYPE == 1 && an.PROPOSAL_REF_PROPOSAL_ID == id orderby an.PROPOSAL_REF_ID ascending select an).ToList();
			var AcuanNonNormatif = (from an in db.VIEW_PROPOSAL_REF where an.PROPOSAL_REF_TYPE == 2 && an.PROPOSAL_REF_PROPOSAL_ID == id orderby an.PROPOSAL_REF_ID ascending select an).ToList();
			var Bibliografi = (from an in db.VIEW_PROPOSAL_REF where an.PROPOSAL_REF_TYPE == 3 && an.PROPOSAL_REF_PROPOSAL_ID == id orderby an.PROPOSAL_REF_ID ascending select an).ToList();
			var ICS = (from an in db.VIEW_PROPOSAL_ICS where an.PROPOSAL_ICS_REF_PROPOSAL_ID == id orderby an.ICS_CODE ascending select an).ToList();
			var AdopsiList = (from an in db.TRX_PROPOSAL_ADOPSI where an.PROPOSAL_ADOPSI_PROPOSAL_ID == id orderby an.PROPOSAL_ADOPSI_NOMOR_JUDUL ascending select an).ToList();
			var RevisiList = db.Database.SqlQuery<VIEW_SNI_SELECT>("SELECT BB.* FROM TRX_PROPOSAL_REV AA INNER JOIN VIEW_SNI_SELECT BB ON AA.PROPOSAL_REV_MERIVISI_ID = BB.ID WHERE AA.PROPOSAL_REV_PROPOSAL_ID = '" + id + "' ORDER BY AA.PROPOSAL_REV_ID ASC").ToList();
			var Lampiran = db.Database.SqlQuery<VIEW_DOCUMENTS>("SELECT * FROM VIEW_DOCUMENTS WHERE DOC_FOLDER_ID = 10 AND DOC_RELATED_ID = " + id + " AND DOC_RELATED_TYPE = 30").FirstOrDefault();
			var Bukti = db.Database.SqlQuery<VIEW_DOCUMENTS>("SELECT * FROM VIEW_DOCUMENTS WHERE DOC_FOLDER_ID = 10 AND DOC_RELATED_ID = " + id + " AND DOC_RELATED_TYPE = 29").FirstOrDefault();
			var Surat = db.Database.SqlQuery<VIEW_DOCUMENTS>("SELECT * FROM VIEW_DOCUMENTS WHERE DOC_FOLDER_ID = 10 AND DOC_RELATED_ID = " + id + " AND DOC_RELATED_TYPE = 32").FirstOrDefault();
			var Outline = db.Database.SqlQuery<VIEW_DOCUMENTS>("SELECT * FROM VIEW_DOCUMENTS WHERE DOC_FOLDER_ID = 10 AND DOC_RELATED_ID = " + id + " AND DOC_RELATED_TYPE = 36").FirstOrDefault();
			ViewData["DataProposal"] = DataProposal;
			ViewData["AcuanNormatif"] = AcuanNormatif;
			ViewData["AcuanNonNormatif"] = AcuanNonNormatif;
			ViewData["Bibliografi"] = Bibliografi;
			ViewData["ICS"] = ICS;
			ViewData["AdopsiList"] = AdopsiList;
			ViewData["RevisiList"] = RevisiList;
			ViewData["Lampiran"] = Lampiran;
			ViewData["Bukti"] = Bukti;
			ViewData["Surat"] = Surat;
			ViewData["Outline"] = Outline;
			return View();
		}
		[HttpPost]
		public ActionResult Approval(int PROPOSAL_ID = 0, int APPROVAL_TYPE = 0, string PROPOSAL_ST_KOM_NO = "", string PROPOSAL_ST_KOM_LAMPIRAN = "-", string PROPOSAL_ST_KOM_DATE = "", string PROPOSAL_ST_KOM_TIME = "", string APPROVAL_REASON = "", int[] PROPOSAL_ICS_REF_ICS_ID = null, int PROPOSAL_KOMTEK_ID = 0, int PROPOSAL_KOMTEK_ID_OLD = 0)
		{
			var LOGCODE = MixHelper.GetLogCode();
			//int LASTID = MixHelper.GetSequence("TRX_SURAT_TUGAS");
			var DATENOW = MixHelper.ConvertDateNow();
			var join_tgl_surat = (PROPOSAL_ST_KOM_DATE + " " + PROPOSAL_ST_KOM_TIME);
			String ST_DATE = "TO_DATE('" + PROPOSAL_ST_KOM_DATE + "', 'yyyy-mm-dd hh24:mi:ss')";
			var USER_ID = Convert.ToInt32(Session["USER_ID"]);
			var TGL_SEKARANG = DateTime.Now.ToString("yyyyMMddHHmmss");
			
			if (APPROVAL_TYPE == 1)
			{                
				var Data = db.Database.SqlQuery<VIEW_PROPOSAL>("SELECT * FROM VIEW_PROPOSAL WHERE PROPOSAL_ID = " + PROPOSAL_ID).SingleOrDefault();
				var DataKomtekCek = db.Database.SqlQuery<VIEW_ANGGOTA>("SELECT * FROM VIEW_ANGGOTA WHERE KOMTEK_ANGGOTA_KOMTEK_ID = " + Data.KOMTEK_ID + " AND JABATAN = 'Ketua' AND KOMTEK_ANGGOTA_STATUS = 1").SingleOrDefault();

				if (DataKomtekCek == null)
				{
					TempData["Notifikasi"] = 2;
					TempData["NotifikasiText"] = "Data Ketua Komtek Tidak Ditemukan";

					return RedirectToAction("Approval/" + PROPOSAL_ID);
				}
				Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/PNPS/" + Data.PROPOSAL_PNPS_CODE));
				string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/PNPS/" + Data.PROPOSAL_PNPS_CODE + "/");

				string dataDir = Server.MapPath("~/Format/Laporan/");
				Stream stream = System.IO.File.OpenRead(dataDir + "FORMULIR_PENUGASAN_KOMITE_TEKNIS.docx");

				Aspose.Words.Document doc = new Aspose.Words.Document(stream);
				stream.Close();
				if (Data != null)
				{
					var DataKomtek = db.Database.SqlQuery<VIEW_ANGGOTA>("SELECT * FROM VIEW_ANGGOTA WHERE KOMTEK_ANGGOTA_KOMTEK_ID = " + Data.KOMTEK_ID + " AND JABATAN = 'Ketua' AND KOMTEK_ANGGOTA_STATUS = 1").SingleOrDefault();

					var dt = Convert.ToDateTime(Data.ST_DATE);
					string time = dt.ToString("hh:mm tt");
					doc.Range.Replace("FullTanggalPenugasan", ConvertTanggal(Convert.ToDateTime(DateTime.Now), "full"), false, true);
					doc.Range.Replace("HariPenugasan", ConvertTanggal(Convert.ToDateTime(Data.ST_DATE), "namahari"), false, true);
					doc.Range.Replace("TanggalPenugasan", ConvertTanggal(Convert.ToDateTime(Data.ST_DATE), "full"), false, true);
					doc.Range.Replace("NomorPenugasan", "", false, true);
					doc.Range.Replace("LampiranPenugasan", "", false, true);
					doc.Range.Replace("NamaKepalaPPS", "Sumartini Maksum", false, true);
					doc.Range.Replace("NIPKepalaPPS", "Nip : 19561014 198107 2 001", false, true);
					doc.Range.Replace("NamaKetuaKomiteTeknis", DataKomtek.KOMTEK_ANGGOTA_NAMA, false, true);
					doc.Range.Replace("KodeKomtek", Data.KOMTEK_CODE, false, true);
					doc.Range.Replace("NamaKomtek", Data.KOMTEK_NAME, false, true);
					doc.Range.Replace("WaktuPenugasan", time, false, true);
					doc.Range.Replace("TahunPenugasan", ConvertTanggal(Convert.ToDateTime(Data.PROPOSAL_CREATE_DATE), "tahun"), false, true);
				}
				doc.Save(@"" + path + "SURAT_PERSETUJUAN_PNPS_" + Data.PROPOSAL_PNPS_CODE + "_" + TGL_SEKARANG + ".docx", Aspose.Words.SaveFormat.Docx);
				doc.Save(@"" + path + "SURAT_PERSETUJUAN_PNPS_" + Data.PROPOSAL_PNPS_CODE + "_" + TGL_SEKARANG + ".pdf", Aspose.Words.SaveFormat.Pdf);
				doc.Save(@"" + path + "SURAT_PERSETUJUAN_PNPS_" + Data.PROPOSAL_PNPS_CODE + "_" + TGL_SEKARANG + ".xml");
				var LOGCODE_ST = MixHelper.GetLogCode();
				int LASTID_ST = MixHelper.GetSequence("TRX_DOCUMENTS");
				var FNAME_TANGGAPAN_MTPS = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
				var FVALUE_TANGGAPAN_MTPS = "'" + LASTID_ST + "', " +
							"'11', " +
							"'2', " +
							"'" + PROPOSAL_ID + "', " +
							"'" + "(" + Data.PROPOSAL_PNPS_CODE + ") Surat Persetujuan PNPS" + "', " +
							"'Surat Persetujuan PNPS dengan kode PNPS : " + Data.PROPOSAL_PNPS_CODE + "', " +
							"'" + "/Upload/Dokumen/RANCANGAN_SNI/PNPS/" + Data.PROPOSAL_PNPS_CODE + "/" + "', " +
							"'" + "SURAT_PERSETUJUAN_PNPS_" + Data.PROPOSAL_PNPS_CODE + "_" + TGL_SEKARANG + "" + "', " +
							"'DOCX', " +
							"'0', " +
							"'" + USER_ID + "', " +
							DATENOW + "," +
							"'1', " +
							"'" + LOGCODE_ST + "'";
				db.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_TANGGAPAN_MTPS + ") VALUES (" + FVALUE_TANGGAPAN_MTPS.Replace("''", "NULL") + ")");
				String objekTanggapan = FVALUE_TANGGAPAN_MTPS.Replace("'", "-");
				MixHelper.InsertLog(LOGCODE_ST, objekTanggapan, 1);

				//if (Data.PROPOSAL_JENIS_PERUMUSAN == 2)
				//{
				//    db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL SET  PROPOSAL_KOMTEK_ID=" + PROPOSAL_KOMTEK_ID + ",PROPOSAL_STATUS = 10, PROPOSAL_STATUS_PROSES = 1, PROPOSAL_UPDATE_DATE = " + DATENOW + ", PROPOSAL_UPDATE_BY = " + USER_ID + " WHERE PROPOSAL_ID = " + PROPOSAL_ID);
				//    var PROPOSAL_LOG_CODE = db.Database.SqlQuery<string>("SELECT PROPOSAL_LOG_CODE FROM TRX_PROPOSAL WHERE PROPOSAL_ID = " + PROPOSAL_ID).SingleOrDefault();
				//    String objek1 = "UPDATE TRX_PROPOSAL SET PROPOSAL_STATUS = 10, PROPOSAL_STATUS_PROSES = 1, PROPOSAL_UPDATE_DATE = " + DATENOW + ", PROPOSAL_UPDATE_BY = " + USER_ID + " WHERE PROPOSAL_ID = " + PROPOSAL_ID;
				//    MixHelper.InsertLog(PROPOSAL_LOG_CODE, objek1.Replace("'", "-"), 2);
				//}
				//else 
				if (Data.PROPOSAL_JENIS_PERUMUSAN == 3)
				{
					db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL SET  PROPOSAL_KOMTEK_ID=" + PROPOSAL_KOMTEK_ID + ",PROPOSAL_STATUS = 10, PROPOSAL_STATUS_PROSES = 1, PROPOSAL_UPDATE_DATE = " + DATENOW + ", PROPOSAL_UPDATE_BY = " + USER_ID + " WHERE PROPOSAL_ID = " + PROPOSAL_ID);
					var PROPOSAL_LOG_CODE = db.Database.SqlQuery<string>("SELECT PROPOSAL_LOG_CODE FROM TRX_PROPOSAL WHERE PROPOSAL_ID = " + PROPOSAL_ID).SingleOrDefault();
					String objek1 = "UPDATE TRX_PROPOSAL SET PROPOSAL_STATUS = 10, PROPOSAL_STATUS_PROSES = 1, PROPOSAL_UPDATE_DATE = " + DATENOW + ", PROPOSAL_UPDATE_BY = " + USER_ID + " WHERE PROPOSAL_ID = " + PROPOSAL_ID;
					MixHelper.InsertLog(PROPOSAL_LOG_CODE, objek1.Replace("'", "-"), 2);
				}
				else {
					db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL SET PROPOSAL_KOMTEK_ID=" + PROPOSAL_KOMTEK_ID + ",PROPOSAL_STATUS = 4, PROPOSAL_STATUS_PROSES = 0, PROPOSAL_UPDATE_DATE = " + DATENOW + ", PROPOSAL_UPDATE_BY = " + USER_ID + " WHERE PROPOSAL_ID = " + PROPOSAL_ID);
					var PROPOSAL_LOG_CODE = db.Database.SqlQuery<string>("SELECT PROPOSAL_LOG_CODE FROM TRX_PROPOSAL WHERE PROPOSAL_ID = " + PROPOSAL_ID).SingleOrDefault();
					String objek1 = "UPDATE TRX_PROPOSAL SET PROPOSAL_STATUS = 4, PROPOSAL_STATUS_PROSES = 0, PROPOSAL_UPDATE_DATE = " + DATENOW + ", PROPOSAL_UPDATE_BY = " + USER_ID + " WHERE PROPOSAL_ID = " + PROPOSAL_ID;
					MixHelper.InsertLog(PROPOSAL_LOG_CODE, objek1.Replace("'", "-"), 2);
				}

				

				int APPROVAL_ID = MixHelper.GetSequence("TRX_PROPOSAL_APPROVAL");
				db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL_APPROVAL SET APPROVAL_STATUS = 0 WHERE APPROVAL_PROPOSAL_ID = " + PROPOSAL_ID);
				db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_APPROVAL (APPROVAL_ID,APPROVAL_PROPOSAL_ID,APPROVAL_TYPE,APPROVAL_DATE,APPROVAL_BY,APPROVAL_STATUS,APPROVAL_STATUS_PROPOSAL,APPROVAL_STATUS_SESSION) VALUES (" + APPROVAL_ID + "," + PROPOSAL_ID + ",1," + DATENOW + "," + USER_ID + ",1,3,1)");
				db.Database.ExecuteSqlCommand("UPDATE TRX_MONITORING SET MONITORING_TGL_APP_PNPS = " + DATENOW + ", MONITORING_HASIL_APP_PNPS = 1, MONITORING_NO_SRT_APP_PUB_PNPS = '" + PROPOSAL_ST_KOM_NO + "', MONITORING_TGL_APP_PUB_PNPS = " + ST_DATE + " WHERE MONITORING_PROPOSAL_ID = " + PROPOSAL_ID);
			}
			else
			{
				db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL SET PROPOSAL_KOMTEK_ID=" + PROPOSAL_KOMTEK_ID + ", PROPOSAL_STATUS_PROSES = 2, PROPOSAL_UPDATE_DATE = " + DATENOW + ", PROPOSAL_UPDATE_BY = " + USER_ID + " WHERE PROPOSAL_ID = " + PROPOSAL_ID);
				var PROPOSAL_LOG_CODE = db.Database.SqlQuery<string>("SELECT PROPOSAL_LOG_CODE FROM TRX_PROPOSAL WHERE PROPOSAL_ID = " + PROPOSAL_ID).SingleOrDefault();
				String objek1 = "UPDATE TRX_PROPOSAL SET PROPOSAL_KOMTEK_ID=" + PROPOSAL_KOMTEK_ID + ", PROPOSAL_STATUS_PROSES = 2, PROPOSAL_UPDATE_DATE = " + DATENOW + ", PROPOSAL_UPDATE_BY = " + USER_ID + " WHERE PROPOSAL_ID = " + PROPOSAL_ID;
				MixHelper.InsertLog(PROPOSAL_LOG_CODE, objek1.Replace("'", "-"), 2);

				int APPROVAL_ID = MixHelper.GetSequence("TRX_PROPOSAL_APPROVAL");
				db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL_APPROVAL SET APPROVAL_STATUS = 0 WHERE APPROVAL_PROPOSAL_ID = " + PROPOSAL_ID);
				db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_APPROVAL (APPROVAL_ID,APPROVAL_PROPOSAL_ID,APPROVAL_REASON,APPROVAL_TYPE,APPROVAL_DATE,APPROVAL_BY,APPROVAL_STATUS,APPROVAL_STATUS_PROPOSAL,APPROVAL_STATUS_SESSION) VALUES (" + APPROVAL_ID + "," + PROPOSAL_ID + ",'" + APPROVAL_REASON + "',0," + DATENOW + "," + USER_ID + ",1,3,1)");
				db.Database.ExecuteSqlCommand("UPDATE TRX_MONITORING SET MONITORING_TGL_APP_PNPS = " + DATENOW + ", MONITORING_HASIL_APP_PNPS = 0, MONITORING_NO_SRT_APP_PUB_PNPS = '" + PROPOSAL_ST_KOM_NO + "', MONITORING_TGL_APP_PUB_PNPS = " + ST_DATE + " WHERE MONITORING_PROPOSAL_ID = " + PROPOSAL_ID);
			}

			var PROPOSAL_PNPS_CODE = gn.GenerateKodePNPS(PROPOSAL_KOMTEK_ID);
			//var PROPOSAL_PNPS_CODE = db.Database.SqlQuery<string>("SELECT CAST(TO_CHAR (SYSDATE, 'YYYY') || '.' || KOMTEK_CODE || '.' || ( SELECT CAST ( ( CASE WHEN LENGTH (COUNT(PROPOSAL_ID) + 1) = 1 THEN '0' || CAST ( COUNT (PROPOSAL_ID) + 1 AS VARCHAR2 (255) ) ELSE CAST ( COUNT (PROPOSAL_ID) + 1 AS VARCHAR2 (255) ) END ) AS VARCHAR2 (255) ) PNPSCODE FROM TRX_PROPOSAL WHERE PROPOSAL_KOMTEK_ID = KOMTEK_ID ) AS VARCHAR2(255)) AS PNPSCODE FROM MASTER_KOMITE_TEKNIS WHERE KOMTEK_ID = " + PROPOSAL_KOMTEK_ID).SingleOrDefault();
			db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL SET PROPOSAL_KOMTEK_ID=" + PROPOSAL_KOMTEK_ID + ",PROPOSAL_PNPS_CODE = '" + PROPOSAL_PNPS_CODE + "', PROPOSAL_UPDATE_DATE = " + DATENOW + ", PROPOSAL_UPDATE_BY = " + USER_ID + " WHERE PROPOSAL_ID = " + PROPOSAL_ID);

			var DataProposal = db.Database.SqlQuery<VIEW_PROPOSAL>("SELECT * FROM VIEW_PROPOSAL WHERE PROPOSAL_ID = " + PROPOSAL_ID).SingleOrDefault();
			//var DataProposal = (from proposal in db.VIEW_PROPOSAL where proposal.PROPOSAL_ID == PROPOSAL_ID select proposal).SingleOrDefault();
			var PROPOSAL_PNPS_CODE_FIXER = DataProposal.PROPOSAL_PNPS_CODE;
			HttpPostedFileBase file2 = Request.Files["RISALAH_RAPAT"];

			if (file2.ContentLength > 0)
			{
				int LASTID_DOC = MixHelper.GetSequence("TRX_DOCUMENTS");
				Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/MTPS/" + PROPOSAL_PNPS_CODE_FIXER));
				string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/MTPS/" + PROPOSAL_PNPS_CODE_FIXER + "/");
				Stream stremdokumen = file2.InputStream;
				byte[] appData = new byte[file2.ContentLength + 1];
				stremdokumen.Read(appData, 0, file2.ContentLength);
				string Extension = Path.GetExtension(file2.FileName);
				if (Extension.ToLower() == ".pdf")
				{
					Aspose.Pdf.Document pdf = new Aspose.Pdf.Document(stremdokumen);
					string filePathpdf = path + "RISALAH_RAPAT_PUBLIKASI_USULAN_PNPS_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + ".pdf";
					string filePathxml = path + "RISALAH_RAPAT_PUBLIKASI_USULAN_PNPS_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + ".xml";
					pdf.Save(@"" + filePathpdf, Aspose.Pdf.SaveFormat.Pdf);
					pdf.Save(@"" + filePathxml);
					var LOGCODE_TANGGAPAN_MTPS = MixHelper.GetLogCode();
					var FNAME_TANGGAPAN_MTPS = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
					var FVALUE_TANGGAPAN_MTPS = "'" + LASTID_DOC + "', " +
								"'10', " +
								"'33', " +
								"'" + PROPOSAL_ID + "', " +
								"'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Risalah Rapat Publikasi Usulan PNPS" + "', " +
								"'Risalah Rapat Publikasi Usulan PNPS dengan Judul PNPS : " + DataProposal.PROPOSAL_JUDUL_PNPS + "', " +
								"'" + "/Upload/Dokumen/RANCANGAN_SNI/MTPS/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
								"'" + "RISALAH_RAPAT_PUBLIKASI_USULAN_PNPS_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + "" + "', " +
								"'" + Extension.ToUpper().Replace(".", "") + "', " +
								"'0', " +
								"'" + USER_ID + "', " +
								DATENOW + "," +
								"'1', " +
								"'" + LOGCODE_TANGGAPAN_MTPS + "'";
					db.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_TANGGAPAN_MTPS + ") VALUES (" + FVALUE_TANGGAPAN_MTPS.Replace("''", "NULL") + ")");
					String objekTanggapan = FVALUE_TANGGAPAN_MTPS.Replace("'", "-");
					MixHelper.InsertLog(LOGCODE_TANGGAPAN_MTPS, objekTanggapan, 1);
				}
			}
			if (PROPOSAL_ICS_REF_ICS_ID != null)
			{
				foreach (var i in PROPOSAL_ICS_REF_ICS_ID)
				{
					int PROPOSAL_ICS_REF_ID = MixHelper.GetSequence("TRX_PROPOSAL_ICS_REF");
					db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_ICS_REF (PROPOSAL_ICS_REF_ID,PROPOSAL_ICS_REF_PROPOSAL_ID,PROPOSAL_ICS_REF_ICS_ID)VALUES(" + PROPOSAL_ICS_REF_ID + "," + PROPOSAL_ID + "," + i + ")");
				}
			}

			string pathnya = Server.MapPath("~/Upload/Dokumen/SK_PNPS/");
			HttpPostedFileBase file_sk = Request.Files["SK_PNPS"];
			var upload = Request.Files["SK_PNPS"];
			var file_name_sk = "";
			var filePath_sk = "";
			var fileExtension_sk = "";
			if (upload.ContentLength > 0)
			{
				//Check whether Directory (Folder) exists.
				if (!Directory.Exists(pathnya))
				{
					//If Directory (Folder) does not exists. Create it.
					Directory.CreateDirectory(pathnya);
				}
				string lampiranregulasipath = file_sk.FileName;
				if (lampiranregulasipath.Trim() != "")
				{
					var Data = db.Database.SqlQuery<VIEW_PROPOSAL>("SELECT * FROM VIEW_PROPOSAL WHERE PROPOSAL_ID = " + PROPOSAL_ID).SingleOrDefault();
					lampiranregulasipath = Path.GetFileNameWithoutExtension(file_sk.FileName);
					fileExtension_sk = Path.GetExtension(file_sk.FileName);
					file_name_sk = "SK_PNPS_" + Data.PROPOSAL_PNPS_CODE + "_" + PROPOSAL_ID  + "_" + TGL_SEKARANG + fileExtension_sk;
					filePath_sk = pathnya + file_name_sk.Replace(" ", "_");
					file_sk.SaveAs(filePath_sk);

					var LOGCODE_ST = MixHelper.GetLogCode();
					int LASTID_ST = MixHelper.GetSequence("TRX_DOCUMENTS");
					var FNAME_TANGGAPAN_MTPS = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
					var FVALUE_TANGGAPAN_MTPS = "'" + LASTID_ST + "', " +
								"'11', " +
								"'99', " +
								"'" + PROPOSAL_ID + "', " +
								"'" + "(" + Data.PROPOSAL_PNPS_CODE + ") SK PNPS" + "', " +
								"'SK PNPS dengan kode PNPS : " + Data.PROPOSAL_PNPS_CODE + "', " +
								"'" + "/Upload/Dokumen/SK_PNPS/" + "', " +
								"'" + "SK_PNPS_" + Data.PROPOSAL_PNPS_CODE + "_" + PROPOSAL_ID + "_" + TGL_SEKARANG + "', " +
								"'pdf', " +
								"'0', " +
								"'" + USER_ID + "', " +
								DATENOW + "," +
								"'1', " +
								"'" + LOGCODE_ST + "'";
					db.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_TANGGAPAN_MTPS + ") VALUES (" + FVALUE_TANGGAPAN_MTPS.Replace("''", "NULL") + ")");
					String objekTanggapan = FVALUE_TANGGAPAN_MTPS.Replace("'", "-");
					MixHelper.InsertLog(LOGCODE_ST, objekTanggapan, 1);
				}
			}

			TempData["Notifikasi"] = 1;
			TempData["NotifikasiText"] = "Data Berhasil Disimpan";

			return RedirectToAction("Index");
		}
		[HttpPost]
		public ActionResult ApprovalSave(int PROPOSAL_ID = 0)
		{
			var UserId = Session["USER_ID"];
			var logcode = db.Database.SqlQuery<String>("SELECT PROPOSAL_LOG_CODE FROM TRX_PROPOSAL WHERE PROPOSAL_ID = " + PROPOSAL_ID).SingleOrDefault();
			var datenow = MixHelper.ConvertDateNow();
			var year_now = DateTime.Now.Year;

			db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL SET PROPOSAL_STATUS = 2, PROPOSAL_STATUS_PROSES = 0, PROPOSAL_UPDATE_DATE = " + datenow + ", PROPOSAL_UPDATE_BY = " + UserId + " WHERE PROPOSAL_ID = " + PROPOSAL_ID);

			String objek = "PROPOSAL_STATUS = 2, PROPOSAL_UPDATE_DATE = " + datenow + ", PROPOSAL_UPDATE_BY = " + UserId;
			MixHelper.InsertLog(logcode, objek.Replace("'", "-"), 2);

			int lastid = MixHelper.GetSequence("TRX_PROPOSAL_APPROVAL");
			db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL_APPROVAL SET APPROVAL_STATUS = 0 WHERE APPROVAL_PROPOSAL_ID = " + PROPOSAL_ID);
			db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_APPROVAL (APPROVAL_ID,APPROVAL_PROPOSAL_ID,APPROVAL_TYPE,APPROVAL_DATE,APPROVAL_BY,APPROVAL_STATUS,APPROVAL_STATUS_PROPOSAL,APPROVAL_STATUS_SESSION) VALUES (" + lastid + "," + PROPOSAL_ID + ",1," + datenow + "," + UserId + ",1,1,1)");

			TempData["Notifikasi"] = 1;
			TempData["NotifikasiText"] = "Data Berhasil Disimpan";
			return Json(new { result = true }, JsonRequestBehavior.AllowGet);
			//return RedirectToAction("Index");
		}
		public ActionResult DataPengesahan(DataTables param, int id = 0)
		{
			var IsDiterima = id;
			var default_order = "PROPOSAL_CREATE_DATE";
			var limit = 10;
			var BIDANG_ID = Convert.ToInt32(Session["BIDANG_ID"]);

			List<string> order_field = new List<string>();
			order_field.Add("PROPOSAL_CREATE_DATE");
			order_field.Add("PROPOSAL_PNPS_CODE");
			order_field.Add("PROPOSAL_TYPE_NAME");
			order_field.Add("PROPOSAL_JENIS_PERUMUSAN_NAME");
			order_field.Add("PROPOSAL_JUDUL_PNPS");
			order_field.Add("PROPOSAL_TAHAPAN");
			order_field.Add("PROPOSAL_IS_URGENT_NAME");
			order_field.Add("PROPOSAL_STATUS_NAME");

			string order_key = (param.iSortCol_0 == "0") ? "0" : param.iSortCol_0;
			string order = (param.iSortCol_0 == "0") ? default_order : order_field[Convert.ToInt32(order_key)];
			string sort = (param.sSortDir_0 == "") ? "desc" : param.sSortDir_0;
			string search = (param.sSearch == "") ? "" : param.sSearch;

			limit = (param.iDisplayLength == 0) ? limit : param.iDisplayLength;
			var start = (param.iDisplayStart == 0) ? 0 : param.iDisplayStart;


			string where_clause = "";
			if (IsDiterima == 0)
			{
				where_clause += "PROPOSAL_STATUS_PROSES = 1 AND PROPOSAL_STATUS = 3 " + ((BIDANG_ID != 0) ? "AND KOMTEK_BIDANG_ID IN (" + BIDANG_ID + ",0)" : "");
			}
			else
			{
				where_clause += "PROPOSAL_STATUS > 3 " + ((BIDANG_ID != 0) ? "AND KOMTEK_BIDANG_ID IN (" + BIDANG_ID + ",0)" : "");
			}
			string search_clause = "";
			if (search != "")
			{
				if (where_clause != "")
				{
					search_clause += " AND ";
				}
				search_clause += "(";
				var i = 1;
				foreach (var fields in order_field)
				{
					if (fields != "")
					{
						search_clause += "LOWER(" + fields + ")  LIKE LOWER('%" + search + "%')";
						if (i < order_field.Count())
						{
							search_clause += " OR ";
						}
					}
					i++;
				}
				search_clause += " OR LOWER(PROPOSAL_CREATE_DATE_NAME) LIKE LOWER('%" + search + "%') OR LOWER(KOMTEK_NAME) LIKE LOWER('%" + search + "%') OR LOWER(KOMTEK_CODE) LIKE LOWER('%" + search + "%'))";
			}

			string inject_clause_count = "";
			string inject_clause_select = "";
			if (where_clause != "" || search_clause != "")
			{
				inject_clause_count = "WHERE " + where_clause + " " + search_clause;
				inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_PROPOSAL WHERE " + where_clause + " AND (PROPOSAL_IS_BATAL <> '1' OR PROPOSAL_IS_BATAL IS NULL) " + search_clause + " ORDER BY " + order + " " + sort + ") T1 WHERE ROWNUM <= " + Convert.ToString(limit + start) + ") WHERE ROWNUMBER > " + Convert.ToString(start);
			}
			var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_PROPOSAL " + inject_clause_count);
			var SelectedData = db.Database.SqlQuery<VIEW_PROPOSAL>(inject_clause_select);

			var result = from list in SelectedData
						 select new string[] 
			{ 
				Convert.ToString("<center>"+list.PROPOSAL_CREATE_DATE_NAME+"</center>"),
				Convert.ToString(list.KOMTEK_CODE+" "+list.KOMTEK_NAME),
				Convert.ToString(list.PROPOSAL_TYPE_NAME),
				Convert.ToString(list.PROPOSAL_JENIS_PERUMUSAN_NAME),
				Convert.ToString("<span class='judul_"+list.PROPOSAL_ID+"'>"+list.PROPOSAL_JUDUL_PNPS+"</span>"),
				Convert.ToString("<center>"+list.PROPOSAL_IS_URGENT_NAME+"</center>"),
				Convert.ToString("<center>"+list.PROPOSAL_TAHAPAN+"</center>"),
				Convert.ToString("<center>"+list.PROPOSAL_STATUS_NAME+"</center>"),
				Convert.ToString("<center><a href='/Pengajuan/Pengesahan/Detail/"+list.PROPOSAL_ID+"' class='btn blue btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Lihat'><i class='action fa fa-file-text-o'></i></a><a href='/Pengajuan/Pengesahan/update/"+list.PROPOSAL_ID+"' class='btn btn-warning btn-sm action tooltips btn_update' data-container='body' data-placement='top' data-original-title='Ubah'><i class='action fa fa-edit'></i></a>"+((list.PROPOSAL_STATUS == 3 && list.PROPOSAL_STATUS_PROSES == 1)?"<a href='/Pengajuan/Pengesahan/Approval/"+list.PROPOSAL_ID+"' class='btn purple btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Pengesahan Usulan'><i class='action fa fa-check'></i></a>":"")+"<a href='javascript:void(0)' onclick='cetak_usulan("+list.PROPOSAL_ID+")' class='btn green btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Cetak'><i class='action fa fa-print'></i></a></center>"),
			};
			return Json(new
			{
				sEcho = param.sEcho,
				iTotalRecords = CountData,
				iTotalDisplayRecords = CountData,
				aaData = result.ToArray()
			}, JsonRequestBehavior.AllowGet);

			//return Content(inject_clause_select);
		}
		public string ConvertTanggal(DateTime tanggal, string tipe = "")
		{
			var res = "";
			var AngkaBulan = tanggal.ToString("MM");
			var NamaHariEng = tanggal.ToString("dddd");
			var AngkaHari = tanggal.ToString("dd");
			var Tahun = tanggal.ToString("yyyy");
			var Bulan = "";
			var Hari = "";
			switch (NamaHariEng)
			{
				case "Sunday":
					Hari = "Minggu";
					break;
				case "Monday":
					Hari = "Senin";
					break;
				case "Tuesday":
					Hari = "Selasa";
					break;
				case "Wednesday":
					Hari = "Rabu";
					break;
				case "Thursday":
					Hari = "Kamis";
					break;
				case "Friday":
					Hari = "Jumat";
					break;
				case "Saturday":
					Hari = "Sabtu";
					break;
				default:
					return "";
			}
			switch (AngkaBulan)
			{
				case "01":
					Bulan = "Januari";
					break;
				case "02":
					Bulan = "Februari";
					break;
				case "03":
					Bulan = "Maret";
					break;
				case "04":
					Bulan = "April";
					break;
				case "05":
					Bulan = "Mei";
					break;
				case "06":
					Bulan = "Juni";
					break;
				case "07":
					Bulan = "Juli";
					break;
				case "08":
					Bulan = "Agustus";
					break;
				case "09":
					Bulan = "September";
					break;
				case "10":
					Bulan = "Oktober";
					break;
				case "11":
					Bulan = "November";
					break;
				case "12":
					Bulan = "Desember";
					break;
				default:
					return "";
			}
			if (tipe == "full")
			{
				res = AngkaHari + " " + Bulan + " " + Tahun;
			}
			else if (tipe == "namabulan")
			{
				res = Bulan;
			}
			else if (tipe == "angkabulan")
			{
				res = AngkaBulan;
			}
			else if (tipe == "tahun")
			{
				res = Tahun;
			}
			else if (tipe == "namahari")
			{
				res = Hari;
			}
			else if (tipe == "angkahari")
			{
				res = AngkaHari;
			}
			return res;
		}

		public ActionResult Update(int id = 0)
		{
			var DataProposal = db.Database.SqlQuery<VIEW_PROPOSAL>("SELECT * FROM VIEW_PROPOSAL WHERE PROPOSAL_ID = " + id).SingleOrDefault();
			var DataKomtek = (from komtek in db.MASTER_KOMITE_TEKNIS where komtek.KOMTEK_STATUS == 1 && komtek.KOMTEK_ID == DataProposal.KOMTEK_ID select komtek).SingleOrDefault();
			var AcuanNormatif = (from an in db.VIEW_PROPOSAL_REF where an.PROPOSAL_REF_TYPE == 1 && an.PROPOSAL_REF_PROPOSAL_ID == id orderby an.PROPOSAL_REF_ID ascending select an).ToList();
			var AcuanNonNormatif = (from an in db.VIEW_PROPOSAL_REF where an.PROPOSAL_REF_TYPE == 2 && an.PROPOSAL_REF_PROPOSAL_ID == id orderby an.PROPOSAL_REF_ID ascending select an).ToList();
			var Bibliografi = (from an in db.VIEW_PROPOSAL_REF where an.PROPOSAL_REF_TYPE == 3 && an.PROPOSAL_REF_PROPOSAL_ID == id orderby an.PROPOSAL_REF_ID ascending select an).ToList();
			var ICS = (from an in db.VIEW_PROPOSAL_ICS where an.PROPOSAL_ICS_REF_PROPOSAL_ID == id orderby an.ICS_CODE ascending select an).ToList();
			var ListKomtek = (from komtek in db.MASTER_KOMITE_TEKNIS where komtek.KOMTEK_STATUS == 1 orderby komtek.KOMTEK_CODE ascending select komtek).ToList();
			var AdopsiList = (from an in db.TRX_PROPOSAL_ADOPSI where an.PROPOSAL_ADOPSI_PROPOSAL_ID == id orderby an.PROPOSAL_ADOPSI_NOMOR_JUDUL ascending select an).ToList();
			var RevisiList = db.Database.SqlQuery<VIEW_SNI_SELECT>("SELECT BB.* FROM TRX_PROPOSAL_REV AA INNER JOIN VIEW_SNI_SELECT BB ON AA.PROPOSAL_REV_MERIVISI_ID = BB.ID WHERE AA.PROPOSAL_REV_PROPOSAL_ID = '" + id + "' ORDER BY AA.PROPOSAL_REV_ID ASC").ToList();
			var Lampiran = db.Database.SqlQuery<VIEW_DOCUMENTS>("SELECT * FROM VIEW_DOCUMENTS WHERE DOC_FOLDER_ID = 10 AND DOC_RELATED_ID = " + id + " AND DOC_RELATED_TYPE = 30").FirstOrDefault();
			var Bukti = db.Database.SqlQuery<VIEW_DOCUMENTS>("SELECT * FROM VIEW_DOCUMENTS WHERE DOC_FOLDER_ID = 10 AND DOC_RELATED_ID = " + id + " AND DOC_RELATED_TYPE = 29").FirstOrDefault();
			var Surat = db.Database.SqlQuery<VIEW_DOCUMENTS>("SELECT * FROM VIEW_DOCUMENTS WHERE DOC_FOLDER_ID = 10 AND DOC_RELATED_ID = " + id + " AND DOC_RELATED_TYPE = 32").FirstOrDefault();
			var Outline = db.Database.SqlQuery<VIEW_DOCUMENTS>("SELECT * FROM VIEW_DOCUMENTS WHERE DOC_FOLDER_ID = 10 AND DOC_RELATED_ID = " + id + " AND DOC_RELATED_TYPE = 36").FirstOrDefault();

			ViewData["DataProposal"] = DataProposal;
			ViewData["AcuanNormatif"] = AcuanNormatif;
			ViewData["AcuanNonNormatif"] = AcuanNonNormatif;
			ViewData["Bibliografi"] = Bibliografi;
			ViewData["ICS"] = ICS;
			ViewData["Komtek"] = DataKomtek;
			ViewData["ListKomtek"] = ListKomtek;
			ViewData["AdopsiList"] = AdopsiList;
			ViewData["RevisiList"] = RevisiList;
			ViewData["Lampiran"] = Lampiran;
			ViewData["Bukti"] = Bukti;
			ViewData["Surat"] = Surat;
			ViewData["Outline"] = Outline;
			return View();
		}

		[HttpPost]
		public ActionResult Update(TRX_PROPOSAL INPUT, int[] PROPOSAL_REV_MERIVISI_ID, string[] PROPOSAL_ADOPSI_NOMOR_JUDUL, int[] PROPOSAL_REF_SNI_ID, string[] PROPOSAL_REF_NON_SNI, string[] BIBLIOGRAFI, string[] PROPOSAL_LPK_ID, string[] PROPOSAL_RETEK_ID)
		{
			var ID = Convert.ToInt32(INPUT.PROPOSAL_ID);
			var USER_ID = Convert.ToInt32(Session["USER_ID"]);
			var LOGCODE = MixHelper.GetLogCode();
			int LASTID = MixHelper.GetSequence("TRX_PROPOSAL");
			var DATENOW = MixHelper.ConvertDateNow();
			var DataProposal = db.Database.SqlQuery<VIEW_PROPOSAL>("SELECT * FROM VIEW_PROPOSAL WHERE PROPOSAL_ID = " + INPUT.PROPOSAL_ID).SingleOrDefault();
			var PROPOSAL_LPK_ID_CONVERT = "";
			var PROPOSAL_RETEK_ID_CONVERT = "";
			if (PROPOSAL_LPK_ID == null)
			{
				PROPOSAL_LPK_ID_CONVERT = null;
			}
			else
			{
				PROPOSAL_LPK_ID_CONVERT = string.Join(";", PROPOSAL_LPK_ID);
			}

			if (PROPOSAL_RETEK_ID == null)
			{
				PROPOSAL_RETEK_ID_CONVERT = null;
			}
			else
			{
				PROPOSAL_RETEK_ID_CONVERT = string.Join(";", PROPOSAL_RETEK_ID);
			}
			//var PROPOSAL_LPK_ID_CONVERT = string.Join(";", PROPOSAL_LPK_ID);
			//var PROPOSAL_RETEK_ID_CONVERT = string.Join(";", PROPOSAL_RETEK_ID);
			PROPOSAL_LPK_ID_CONVERT = ((PROPOSAL_LPK_ID_CONVERT == null) ? "" : PROPOSAL_LPK_ID_CONVERT);
			PROPOSAL_RETEK_ID_CONVERT = ((PROPOSAL_RETEK_ID_CONVERT == null) ? "" : PROPOSAL_RETEK_ID_CONVERT);

			string pathnya = Server.MapPath("~/Upload/Dokumen/HAK_PATEN/");
			HttpPostedFileBase file_paten = Request.Files["PROPOSAL_HAK_PATEN_LOCATION"];
			var file_name_paten = "";
			var filePath_paten = "";
			var fileExtension_paten = "";
			if (file_paten != null)
			{
				//Check whether Directory (Folder) exists.
				if (!Directory.Exists(pathnya))
				{
					//If Directory (Folder) does not exists. Create it.
					Directory.CreateDirectory(pathnya);
				}
				string lampiranregulasipath = file_paten.FileName;
				if (lampiranregulasipath.Trim() != "")
				{
					lampiranregulasipath = Path.GetFileNameWithoutExtension(file_paten.FileName);
					fileExtension_paten = Path.GetExtension(file_paten.FileName);
					file_name_paten = "HAK_PATEN_ID_PROPOSAL_" + ID + fileExtension_paten;
					filePath_paten = pathnya + file_name_paten.Replace(" ", "_");
					file_paten.SaveAs(filePath_paten);
				}
			}
			var Dir = Convert.ToString("/Upload/Dokumen/HAK_PATEN/");
			var fupdate = "";
			if (INPUT.PROPOSAL_HAK_PATEN_LOCATION != null)
			{
				fupdate = "UPDATE TRX_PROPOSAL SET PROPOSAL_KOMTEK_ID = '" + INPUT.PROPOSAL_KOMTEK_ID + "'," +
						"PROPOSAL_KONSEPTOR = '" + INPUT.PROPOSAL_KONSEPTOR + "'," +
						"PROPOSAL_INSTITUSI = '" + INPUT.PROPOSAL_INSTITUSI + "'," +
						"PROPOSAL_JUDUL_PNPS = '" + INPUT.PROPOSAL_JUDUL_PNPS + "'," +
						"PROPOSAL_LPK_ID = '" + PROPOSAL_LPK_ID_CONVERT + "'," +
						"PROPOSAL_RETEK_ID = '" + PROPOSAL_RETEK_ID_CONVERT + "'," +
						"PROPOSAL_RUANG_LINGKUP = '" + INPUT.PROPOSAL_RUANG_LINGKUP + "'," +
						"PROPOSAL_JENIS_PERUMUSAN = '" + INPUT.PROPOSAL_JENIS_PERUMUSAN + "'," +
						"PROPOSAL_JALUR = '" + INPUT.PROPOSAL_JALUR + "'," +
						"PROPOSAL_JENIS_ADOPSI = '" + INPUT.PROPOSAL_JENIS_ADOPSI + "'," +
						"PROPOSAL_METODE_ADOPSI = '" + INPUT.PROPOSAL_METODE_ADOPSI + "'," +
						"PROPOSAL_TERJEMAHAN_SNI_ID = '" + INPUT.PROPOSAL_TERJEMAHAN_SNI_ID + "'," +
						"PROPOSAL_RALAT_SNI_ID = '" + INPUT.PROPOSAL_RALAT_SNI_ID + "'," +
						"PROPOSAL_AMD_SNI_ID = '" + INPUT.PROPOSAL_AMD_SNI_ID + "'," +
						"PROPOSAL_IS_URGENT = '" + INPUT.PROPOSAL_IS_URGENT + "'," +
						"PROPOSAL_PASAL = '" + INPUT.PROPOSAL_PASAL + "'," +
						"PROPOSAL_IS_HAK_PATEN = '" + INPUT.PROPOSAL_IS_HAK_PATEN + "'," +
						"PROPOSAL_IS_HAK_PATEN_DESC = '" + INPUT.PROPOSAL_IS_HAK_PATEN_DESC + "'," +
						"PROPOSAL_INFORMASI = '" + INPUT.PROPOSAL_INFORMASI + "'," +
						"PROPOSAL_TUJUAN = '" + INPUT.PROPOSAL_TUJUAN + "'," +
						"PROPOSAL_PROGRAM_PEMERINTAH = '" + INPUT.PROPOSAL_PROGRAM_PEMERINTAH + "'," +
						"PROPOSAL_PIHAK_BERKEPENTINGAN = '" + INPUT.PROPOSAL_PIHAK_BERKEPENTINGAN + "'," +
						"PROPOSAL_MANFAAT_PENERAPAN = '" + INPUT.PROPOSAL_MANFAAT_PENERAPAN + "'," +
						"PROPOSAL_IS_ORG_MENDUKUNG = '" + INPUT.PROPOSAL_IS_ORG_MENDUKUNG + "'," +
						"PROPOSAL_IS_DUPLIKASI_DESC = '" + INPUT.PROPOSAL_IS_DUPLIKASI_DESC + "'," +
						"PROPOSAL_UPDATE_BY = '" + USER_ID + "'," +
						"PROPOSAL_UPDATE_DATE = " + DATENOW + "," +
						"PROPOSAL_HAK_PATEN_LOCATION = '" + Dir + "'," +
						"PROPOSAL_HAK_PATEN_NAME = '" + file_name_paten.Replace(" ", "_") + "'," +
						"PROPOSAL_STATUS_PROSES = " + ((DataProposal.APPROVAL_TYPE == 0) ? 1 : DataProposal.PROPOSAL_STATUS_PROSES) + "," +
						"PROPOSAL_LOG_CODE = '" + LOGCODE + "' WHERE PROPOSAL_ID = " + INPUT.PROPOSAL_ID;
			}
			else
			{
				fupdate = "UPDATE TRX_PROPOSAL SET PROPOSAL_KOMTEK_ID = '" + INPUT.PROPOSAL_KOMTEK_ID + "'," +
						"PROPOSAL_KONSEPTOR = '" + INPUT.PROPOSAL_KONSEPTOR + "'," +
						"PROPOSAL_INSTITUSI = '" + INPUT.PROPOSAL_INSTITUSI + "'," +
						"PROPOSAL_JUDUL_PNPS = '" + INPUT.PROPOSAL_JUDUL_PNPS + "'," +
						"PROPOSAL_LPK_ID = '" + PROPOSAL_LPK_ID_CONVERT + "'," +
						"PROPOSAL_RETEK_ID = '" + PROPOSAL_RETEK_ID_CONVERT + "'," +
						"PROPOSAL_RUANG_LINGKUP = '" + INPUT.PROPOSAL_RUANG_LINGKUP + "'," +
						"PROPOSAL_JENIS_PERUMUSAN = '" + INPUT.PROPOSAL_JENIS_PERUMUSAN + "'," +
						"PROPOSAL_JALUR = '" + INPUT.PROPOSAL_JALUR + "'," +
						"PROPOSAL_JENIS_ADOPSI = '" + INPUT.PROPOSAL_JENIS_ADOPSI + "'," +
						"PROPOSAL_METODE_ADOPSI = '" + INPUT.PROPOSAL_METODE_ADOPSI + "'," +
						"PROPOSAL_TERJEMAHAN_SNI_ID = '" + INPUT.PROPOSAL_TERJEMAHAN_SNI_ID + "'," +
						"PROPOSAL_RALAT_SNI_ID = '" + INPUT.PROPOSAL_RALAT_SNI_ID + "'," +
						"PROPOSAL_AMD_SNI_ID = '" + INPUT.PROPOSAL_AMD_SNI_ID + "'," +
						"PROPOSAL_IS_URGENT = '" + INPUT.PROPOSAL_IS_URGENT + "'," +
						"PROPOSAL_PASAL = '" + INPUT.PROPOSAL_PASAL + "'," +
						"PROPOSAL_IS_HAK_PATEN = '" + INPUT.PROPOSAL_IS_HAK_PATEN + "'," +
						"PROPOSAL_IS_HAK_PATEN_DESC = '" + INPUT.PROPOSAL_IS_HAK_PATEN_DESC + "'," +
						"PROPOSAL_INFORMASI = '" + INPUT.PROPOSAL_INFORMASI + "'," +
						"PROPOSAL_TUJUAN = '" + INPUT.PROPOSAL_TUJUAN + "'," +
						"PROPOSAL_PROGRAM_PEMERINTAH = '" + INPUT.PROPOSAL_PROGRAM_PEMERINTAH + "'," +
						"PROPOSAL_PIHAK_BERKEPENTINGAN = '" + INPUT.PROPOSAL_PIHAK_BERKEPENTINGAN + "'," +
						"PROPOSAL_MANFAAT_PENERAPAN = '" + INPUT.PROPOSAL_MANFAAT_PENERAPAN + "'," +
						"PROPOSAL_IS_ORG_MENDUKUNG = '" + INPUT.PROPOSAL_IS_ORG_MENDUKUNG + "'," +
						"PROPOSAL_IS_DUPLIKASI_DESC = '" + INPUT.PROPOSAL_IS_DUPLIKASI_DESC + "'," +
						"PROPOSAL_UPDATE_BY = '" + USER_ID + "'," +
						"PROPOSAL_UPDATE_DATE = " + DATENOW + "," +
						"PROPOSAL_STATUS_PROSES = " + ((DataProposal.APPROVAL_TYPE == 0) ? 1 : DataProposal.PROPOSAL_STATUS_PROSES) + "," +
						"PROPOSAL_LOG_CODE = '" + LOGCODE + "' WHERE PROPOSAL_ID = " + INPUT.PROPOSAL_ID;
			}


			db.Database.ExecuteSqlCommand(fupdate);

			var tester = fupdate;
			//return Content(tester);
			if (PROPOSAL_REV_MERIVISI_ID != null)
			{
				db.Database.ExecuteSqlCommand("DELETE TRX_PROPOSAL_REV WHERE PROPOSAL_REV_PROPOSAL_ID = " + INPUT.PROPOSAL_ID);
				foreach (var PROPOSAL_REV_MERIVISI_ID_VAL in PROPOSAL_REV_MERIVISI_ID)
				{
					var PROPOSAL_REV_ID = MixHelper.GetSequence("TRX_PROPOSAL_REV");
					db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_REV (PROPOSAL_REV_ID,PROPOSAL_REV_PROPOSAL_ID,PROPOSAL_REV_MERIVISI_ID) VALUES (" + PROPOSAL_REV_ID + "," + INPUT.PROPOSAL_ID + "," + PROPOSAL_REV_MERIVISI_ID_VAL + ")");
				}
			}
			if (PROPOSAL_ADOPSI_NOMOR_JUDUL != null)
			{
				db.Database.ExecuteSqlCommand("DELETE TRX_PROPOSAL_ADOPSI WHERE PROPOSAL_ADOPSI_PROPOSAL_ID = " + INPUT.PROPOSAL_ID);
				foreach (var PROPOSAL_ADOPSI_NOMOR_JUDUL_VAL in PROPOSAL_ADOPSI_NOMOR_JUDUL)
				{
					var PROPOSAL_ADOPSI_ID = MixHelper.GetSequence("TRX_PROPOSAL_ADOPSI");
					db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_ADOPSI (PROPOSAL_ADOPSI_ID,PROPOSAL_ADOPSI_PROPOSAL_ID,PROPOSAL_ADOPSI_NOMOR_JUDUL) VALUES (" + PROPOSAL_ADOPSI_ID + "," + INPUT.PROPOSAL_ID + ",'" + PROPOSAL_ADOPSI_NOMOR_JUDUL_VAL + "')");
				}
			}

			if (PROPOSAL_REF_SNI_ID != null)
			{
				db.Database.ExecuteSqlCommand("DELETE TRX_PROPOSAL_REFERENCE WHERE PROPOSAL_REF_TYPE = 1 AND PROPOSAL_REF_PROPOSAL_ID = " + INPUT.PROPOSAL_ID);
				foreach (var SNI_ID in PROPOSAL_REF_SNI_ID)
				{
					var PROPOSAL_REF_ID = MixHelper.GetSequence("TRX_PROPOSAL_REFERENCE");
					db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_REFERENCE (PROPOSAL_REF_ID,PROPOSAL_REF_PROPOSAL_ID,PROPOSAL_REF_TYPE,PROPOSAL_REF_SNI_ID) VALUES (" + PROPOSAL_REF_ID + "," + INPUT.PROPOSAL_ID + ",1," + SNI_ID + ")");
				}
			}
			if (PROPOSAL_REF_NON_SNI != null)
			{
				db.Database.ExecuteSqlCommand("DELETE TRX_PROPOSAL_REFERENCE WHERE PROPOSAL_REF_TYPE = 2 AND PROPOSAL_REF_PROPOSAL_ID = " + INPUT.PROPOSAL_ID);
				foreach (var DATA_NON_SNI_VAL in PROPOSAL_REF_NON_SNI)
				{
					var PROPOSAL_REF_ID = MixHelper.GetSequence("TRX_PROPOSAL_REFERENCE");
					var CEK_PROPOSAL_REF_NON_SNI = db.Database.SqlQuery<MASTER_ACUAN_NON_SNI>("SELECT * FROM MASTER_ACUAN_NON_SNI WHERE ACUAN_NON_SNI_STATUS = 1 AND LOWER(ACUAN_NON_SNI_JUDUL) = '" + DATA_NON_SNI_VAL.ToLower() + "'").SingleOrDefault();
					if (CEK_PROPOSAL_REF_NON_SNI != null)
					{

						db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_REFERENCE (PROPOSAL_REF_ID,PROPOSAL_REF_PROPOSAL_ID,PROPOSAL_REF_TYPE,PROPOSAL_REF_SNI_ID,PROPOSAL_REF_EXT_JUDUL) VALUES (" + PROPOSAL_REF_ID + "," + INPUT.PROPOSAL_ID + ",2,'" + CEK_PROPOSAL_REF_NON_SNI.ACUAN_NON_SNI_ID + "','" + DATA_NON_SNI_VAL + "')");
					}
					else
					{
						db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_REFERENCE (PROPOSAL_REF_ID,PROPOSAL_REF_PROPOSAL_ID,PROPOSAL_REF_TYPE,PROPOSAL_REF_EXT_JUDUL) VALUES (" + PROPOSAL_REF_ID + "," + INPUT.PROPOSAL_ID + ",2,'" + DATA_NON_SNI_VAL + "')");
					}
				}
			}
			if (BIBLIOGRAFI != null)
			{
				db.Database.ExecuteSqlCommand("DELETE TRX_PROPOSAL_REFERENCE WHERE PROPOSAL_REF_TYPE = 3 AND PROPOSAL_REF_PROPOSAL_ID = " + INPUT.PROPOSAL_ID);
				foreach (var BIBLIOGRAFI_VAL in BIBLIOGRAFI)
				{
					var PROPOSAL_REF_ID = MixHelper.GetSequence("TRX_PROPOSAL_REFERENCE");
					//db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_REFERENCE (PROPOSAL_REF_ID,PROPOSAL_REF_PROPOSAL_ID,PROPOSAL_REF_TYPE,PROPOSAL_REF_SNI_ID) VALUES (" + PROPOSAL_REF_ID + "," + INPUT.PROPOSAL_ID + ",3,'" + BIBLIOGRAFI_VAL + "')");
					var CEK_BIBLIOGRAFI = db.Database.SqlQuery<MASTER_BIBLIOGRAFI>("SELECT * FROM MASTER_BIBLIOGRAFI WHERE BIBLIOGRAFI_STATUS = 1 AND LOWER(BIBLIOGRAFI_JUDUL) = '" + BIBLIOGRAFI_VAL.ToLower() + "'").SingleOrDefault();
					if (CEK_BIBLIOGRAFI != null)
					{
						db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_REFERENCE (PROPOSAL_REF_ID,PROPOSAL_REF_PROPOSAL_ID,PROPOSAL_REF_TYPE,PROPOSAL_REF_SNI_ID,PROPOSAL_REF_EXT_JUDUL) VALUES (" + PROPOSAL_REF_ID + "," + INPUT.PROPOSAL_ID + ",3,'" + CEK_BIBLIOGRAFI.BIBLIOGRAFI_ID + "','" + BIBLIOGRAFI_VAL + "')");
					}
					else
					{
						db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_REFERENCE (PROPOSAL_REF_ID,PROPOSAL_REF_PROPOSAL_ID,PROPOSAL_REF_TYPE,PROPOSAL_REF_EXT_JUDUL) VALUES (" + PROPOSAL_REF_ID + "," + INPUT.PROPOSAL_ID + ",3,'" + BIBLIOGRAFI_VAL + "')");
					}
				}
			}


			var PROPOSAL_PNPS_CODE_FIXER = DataProposal.PROPOSAL_CODE;
			var PROPOSAL_ID = INPUT.PROPOSAL_ID;
			var TGL_SEKARANG = DateTime.Now.ToString("yyyyMMddHHmmss");
			if (INPUT.PROPOSAL_IS_ORG_MENDUKUNG == 1)
			{
				HttpPostedFileBase file = Request.Files["PROPOSAL_DUKUNGAN_FILE_PATH"];
				if (file.ContentLength > 0)
				{
					db.Database.ExecuteSqlCommand("UPDATE TRX_DOCUMENTS SET DOC_STATUS = 0 WHERE DOC_FOLDER_ID = 10 AND DOC_RELATED_ID = " + PROPOSAL_ID + " AND DOC_RELATED_TYPE = 29");
					int LASTID_DOC = MixHelper.GetSequence("TRX_DOCUMENTS");
					Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/MTPS/" + PROPOSAL_PNPS_CODE_FIXER));
					string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/MTPS/" + PROPOSAL_PNPS_CODE_FIXER + "/");
					Stream stremdokumen = file.InputStream;
					byte[] appData = new byte[file.ContentLength + 1];
					stremdokumen.Read(appData, 0, file.ContentLength);
					string Extension = Path.GetExtension(file.FileName);
					if (Extension.ToLower() == ".pdf")
					{
						Aspose.Pdf.Document pdf = new Aspose.Pdf.Document(stremdokumen);
						string filePathpdf = path + "BUKTI_DUKUNGAN_USULAN_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + ".pdf";
						string filePathxml = path + "BUKTI_DUKUNGAN_USULAN_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + ".xml";
						pdf.Save(@"" + filePathpdf, Aspose.Pdf.SaveFormat.Pdf);
						pdf.Save(@"" + filePathxml);
						var LOGCODE_TANGGAPAN_MTPS = MixHelper.GetLogCode();
						var FNAME_TANGGAPAN_MTPS = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
						var FVALUE_TANGGAPAN_MTPS = "'" + LASTID_DOC + "', " +
									"'10', " +
									"'29', " +
									"'" + PROPOSAL_ID + "', " +
									"'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Bukti Dukungan Usulan" + "', " +
									"'Bukti Dukungan Usulan dengan Judul PNPS : " + DataProposal.PROPOSAL_JUDUL_PNPS + "', " +
									"'" + "/Upload/Dokumen/RANCANGAN_SNI/MTPS/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
									"'" + "BUKTI_DUKUNGAN_USULAN_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + "" + "', " +
									"'" + Extension.ToUpper().Replace(".", "") + "', " +
									"'0', " +
									"'" + USER_ID + "', " +
									DATENOW + "," +
									"'1', " +
									"'" + LOGCODE_TANGGAPAN_MTPS + "'";
						db.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_TANGGAPAN_MTPS + ") VALUES (" + FVALUE_TANGGAPAN_MTPS.Replace("''", "NULL") + ")");
						String objekTanggapan = FVALUE_TANGGAPAN_MTPS.Replace("'", "-");
						MixHelper.InsertLog(LOGCODE_TANGGAPAN_MTPS, objekTanggapan, 1);
					}
				}
			}
			HttpPostedFileBase file2 = Request.Files["PROPOSAL_LAMPIRAN_FILE_PATH"];
			if (file2.ContentLength > 0)
			{
				db.Database.ExecuteSqlCommand("UPDATE TRX_DOCUMENTS SET DOC_STATUS = 0 WHERE DOC_FOLDER_ID = 10 AND DOC_RELATED_ID = " + PROPOSAL_ID + " AND DOC_RELATED_TYPE = 30");
				int LASTID_DOC = MixHelper.GetSequence("TRX_DOCUMENTS");
				Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/MTPS/" + PROPOSAL_PNPS_CODE_FIXER));
				string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/MTPS/" + PROPOSAL_PNPS_CODE_FIXER + "/");
				Stream stremdokumen = file2.InputStream;
				byte[] appData = new byte[file2.ContentLength + 1];
				stremdokumen.Read(appData, 0, file2.ContentLength);
				string Extension = Path.GetExtension(file2.FileName);
				if (Extension.ToLower() == ".pdf")
				{
					Aspose.Pdf.Document pdf = new Aspose.Pdf.Document(stremdokumen);
					string filePathpdf = path + "LAMPIRAN_PENDUKUNG_USULAN_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + ".pdf";
					string filePathxml = path + "LAMPIRAN_PENDUKUNG_USULAN_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + ".xml";
					pdf.Save(@"" + filePathpdf, Aspose.Pdf.SaveFormat.Pdf);
					pdf.Save(@"" + filePathxml);
					var LOGCODE_TANGGAPAN_MTPS = MixHelper.GetLogCode();
					var FNAME_TANGGAPAN_MTPS = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
					var FVALUE_TANGGAPAN_MTPS = "'" + LASTID_DOC + "', " +
								"'10', " +
								"'30', " +
								"'" + PROPOSAL_ID + "', " +
								"'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Lampiran Pendukung Usulan" + "', " +
								"'Lampiran Pendukung Usulan dengan Judul PNPS : " + DataProposal.PROPOSAL_JUDUL_PNPS + "', " +
								"'" + "/Upload/Dokumen/RANCANGAN_SNI/MTPS/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
								"'" + "LAMPIRAN_PENDUKUNG_USULAN_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + "" + "', " +
								"'" + Extension.ToUpper().Replace(".", "") + "', " +
								"'0', " +
								"'" + USER_ID + "', " +
								DATENOW + "," +
								"'1', " +
								"'" + LOGCODE_TANGGAPAN_MTPS + "'";
					db.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_TANGGAPAN_MTPS + ") VALUES (" + FVALUE_TANGGAPAN_MTPS.Replace("''", "NULL") + ")");
					String objekTanggapan = FVALUE_TANGGAPAN_MTPS.Replace("'", "-");
					MixHelper.InsertLog(LOGCODE_TANGGAPAN_MTPS, objekTanggapan, 1);
				}
			}
			HttpPostedFileBase file3 = Request.Files["PROPOSAL_SURAT_PENGAJUAN_PNPS"];
			if (file3.ContentLength > 0)
			{
				db.Database.ExecuteSqlCommand("UPDATE TRX_DOCUMENTS SET DOC_STATUS = 0 WHERE DOC_FOLDER_ID = 10 AND DOC_RELATED_ID = " + PROPOSAL_ID + " AND DOC_RELATED_TYPE = 32");
				int LASTID_DOC = MixHelper.GetSequence("TRX_DOCUMENTS");
				Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/MTPS/" + PROPOSAL_PNPS_CODE_FIXER));
				string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/MTPS/" + PROPOSAL_PNPS_CODE_FIXER + "/");
				Stream stremdokumen = file3.InputStream;
				byte[] appData = new byte[file3.ContentLength + 1];
				stremdokumen.Read(appData, 0, file3.ContentLength);
				string Extension = Path.GetExtension(file3.FileName);
				if (Extension.ToLower() == ".pdf")
				{
					Aspose.Pdf.Document pdf = new Aspose.Pdf.Document(stremdokumen);
					string filePathpdf = path + "SURAT_PENGAJUAN_PNPS_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + ".pdf";
					string filePathxml = path + "SURAT_PENGAJUAN_PNPS_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + ".xml";
					pdf.Save(@"" + filePathpdf, Aspose.Pdf.SaveFormat.Pdf);
					pdf.Save(@"" + filePathxml);
					var LOGCODE_TANGGAPAN_MTPS = MixHelper.GetLogCode();
					var FNAME_TANGGAPAN_MTPS = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
					var FVALUE_TANGGAPAN_MTPS = "'" + LASTID_DOC + "', " +
								"'10', " +
								"'32', " +
								"'" + PROPOSAL_ID + "', " +
								"'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Lampiran Surat Pengajuan PNPS" + "', " +
								"'Lampiran Surat Pengajuan PNPS dengan Judul PNPS : " + DataProposal.PROPOSAL_JUDUL_PNPS + "', " +
								"'" + "/Upload/Dokumen/RANCANGAN_SNI/MTPS/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
								"'" + "SURAT_PENGAJUAN_PNPS_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + "" + "', " +
								"'" + Extension.ToUpper().Replace(".", "") + "', " +
								"'0', " +
								"'" + USER_ID + "', " +
								DATENOW + "," +
								"'1', " +
								"'" + LOGCODE_TANGGAPAN_MTPS + "'";
					db.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_TANGGAPAN_MTPS + ") VALUES (" + FVALUE_TANGGAPAN_MTPS.Replace("''", "NULL") + ")");
					String objekTanggapan = FVALUE_TANGGAPAN_MTPS.Replace("'", "-");
					MixHelper.InsertLog(LOGCODE_TANGGAPAN_MTPS, objekTanggapan, 1);
				}
			}

			HttpPostedFileBase file4 = Request.Files["PROPOSAL_OUTLINE_RSNI"];
			if (file4.ContentLength > 0)
			{
				db.Database.ExecuteSqlCommand("UPDATE TRX_DOCUMENTS SET DOC_STATUS = 0 WHERE DOC_FOLDER_ID = 10 AND DOC_RELATED_ID = " + PROPOSAL_ID + " AND DOC_RELATED_TYPE = 36");
				int LASTID_DOC = MixHelper.GetSequence("TRX_DOCUMENTS");
				Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/MTPS/" + PROPOSAL_PNPS_CODE_FIXER));
				string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/MTPS/" + PROPOSAL_PNPS_CODE_FIXER + "/");
				Stream stremdokumen = file4.InputStream;
				byte[] appData = new byte[file4.ContentLength + 1];
				stremdokumen.Read(appData, 0, file4.ContentLength);
				string Extension = Path.GetExtension(file4.FileName);
				if (Extension.ToLower() == ".pdf")
				{
					Aspose.Pdf.Document pdf = new Aspose.Pdf.Document(stremdokumen);
					string filePathpdf = path + "LAMPIRAN_OUTLINE_RSNI_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + ".pdf";
					string filePathxml = path + "LAMPIRAN_OUTLINE_RSNI_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + ".xml";
					pdf.Save(@"" + filePathpdf, Aspose.Pdf.SaveFormat.Pdf);
					pdf.Save(@"" + filePathxml);
					var LOGCODE_TANGGAPAN_MTPS = MixHelper.GetLogCode();
					var FNAME_TANGGAPAN_MTPS = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
					var FVALUE_TANGGAPAN_MTPS = "'" + LASTID_DOC + "', " +
								"'10', " +
								"'36', " +
								"'" + PROPOSAL_ID + "', " +
								"'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Lampiran Outline RSNI" + "', " +
								"'Lampiran Outline RSNI dengan Judul PNPS : " + DataProposal.PROPOSAL_JUDUL_PNPS + "', " +
								"'" + "/Upload/Dokumen/RANCANGAN_SNI/MTPS/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
								"'" + "LAMPIRAN_OUTLINE_RSNI_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + "" + "', " +
								"'" + Extension.ToUpper().Replace(".", "") + "', " +
								"'0', " +
								"'" + USER_ID + "', " +
								DATENOW + "," +
								"'1', " +
								"'" + LOGCODE_TANGGAPAN_MTPS + "'";
					db.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_TANGGAPAN_MTPS + ") VALUES (" + FVALUE_TANGGAPAN_MTPS.Replace("''", "NULL") + ")");
					String objekTanggapan = FVALUE_TANGGAPAN_MTPS.Replace("'", "-");
					MixHelper.InsertLog(LOGCODE_TANGGAPAN_MTPS, objekTanggapan, 1);
				}
			}

			if (DataProposal.APPROVAL_TYPE == 0)
			{
				db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL_APPROVAL SET APPROVAL_STATUS = 0 WHERE APPROVAL_PROPOSAL_ID = " + PROPOSAL_ID + " AND APPROVAL_STATUS_PROPOSAL = 0 AND APPROVAL_TYPE = 0");
			}

			//return Json(new { tester,INPUT, PROPOSAL_REV_MERIVISI_ID, PROPOSAL_ADOPSI_NOMOR_JUDUL, PROPOSAL_REF_SNI_ID, PROPOSAL_REF_NON_SNI, BIBLIOGRAFI }, JsonRequestBehavior.AllowGet);
			String objek = fupdate.Replace("'", "-");
			MixHelper.InsertLog(LOGCODE, objek, 2);

			TempData["Notifikasi"] = 1;
			TempData["NotifikasiText"] = "Data Berhasil Disimpan";
			return RedirectToAction("Index");
		}
	}
}
