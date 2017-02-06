using System;
using System.Linq;
using SISPK.Models;

namespace SISPK.Controllers
{
	public class GenarateNomor
	{
		
		public struct ParNomor
		{
			public string nomor;
			public string nomorThn;
		}

		private SISPKEntities db = new SISPKEntities();

		//public string GenerateNomor(VIEW_PROPOSAL DataProposal = null)
		//{
		//	string query = "";
		//	string NomorQuery = "";
		//	string Nomor = "";
		//	string YearsNow = DateTime.Now.Year.ToString();
		//	var DataLogbook = (from lb in db.MASTER_LOGBOOK select lb).SingleOrDefault();
		//	string forbiddenNomor = DataLogbook.LOGBOOK_FORBIDDEN_NOMOR;
		//	string[] fNomor = forbiddenNomor.Split(',');
		//	string lastLogNomor = Convert.ToString(Convert.ToInt32(DataLogbook.LOGBOOK_NOMOR) + 1);


		//	if (DataProposal.PROPOSAL_JENIS_PERUMUSAN == 1 || DataProposal.PROPOSAL_JENIS_PERUMUSAN == 2)
		//	{
		//		if (DataProposal.PROPOSAL_JALUR == 2 && DataProposal.PROPOSAL_JENIS_ADOPSI == 1)
		//		{
		//			query = @"SELECT
		//						*
		//					FROM
		//						(
		//							SELECT
		//								REGEXP_SUBSTR (
		//									TP.PROPOSAL_ADOPSI_NOMOR_JUDUL,
		//									'[^:]+',
		//									1,
		//									1
		//								) AS nomor
		//							FROM
		//								TRX_PROPOSAL_ADOPSI TP
		//							WHERE
		//								TP.PROPOSAL_ADOPSI_PROPOSAL_ID = " + DataProposal.PROPOSAL_ID + @"
		//							AND TP.PROPOSAL_ADOPSI_NOMOR_JUDUL LIKE '%:%'
		//							ORDER BY
		//								TP.PROPOSAL_ADOPSI_ID ASC
		//						)
		//					WHERE
		//						ROWNUM = 1";
		//			NomorQuery = db.Database.SqlQuery<string>("" + query + "").SingleOrDefault();
		//			if (NomorQuery != null)
		//			{
		//				Nomor = NomorQuery;

		//			}
		//			else
		//			{
		//				foreach (string Res in fNomor)
		//				{
		//					if (Convert.ToInt32(Res) >= Convert.ToInt32(lastLogNomor))
		//					{
		//						if (Convert.ToInt32(Res) != Convert.ToInt32(lastLogNomor))
		//						{
		//							Nomor = lastLogNomor;
		//							break;
		//						}
		//						else
		//						{
		//							lastLogNomor = Convert.ToString(Convert.ToInt32(lastLogNomor) + 1);
		//						}
		//					}


		//				}
		//			}

		//		}
		//		else
		//		{
		//			foreach (string Res in fNomor)
		//			{
		//				if (Convert.ToInt32(Res) >= Convert.ToInt32(lastLogNomor))
		//				{
		//					if (Convert.ToInt32(Res) != Convert.ToInt32(lastLogNomor))
		//					{
		//						Nomor = lastLogNomor;
		//						break;
		//					}
		//					else
		//					{
		//						lastLogNomor = Convert.ToString(Convert.ToInt32(lastLogNomor) + 1);
		//					}
		//				}


		//			}
		//			//Nomor = Convert.ToString(DataProposal.PROPOSAL_ID);
		//		}
		//		return Nomor + ":" + YearsNow;
		//	}
		//	else if (DataProposal.PROPOSAL_JENIS_PERUMUSAN == 5)
		//	{
		//		Nomor = Convert.ToString(DataProposal.PROPOSAL_TERJEMAHAN_NOMOR);
		//		return Nomor;
		//	}
		//	else
		//	{
		//		foreach (string Res in fNomor)
		//		{
		//			if (Convert.ToInt32(Res) >= Convert.ToInt32(lastLogNomor))
		//			{
		//				if (Convert.ToInt32(Res) != Convert.ToInt32(lastLogNomor))
		//				{
		//					Nomor = lastLogNomor;
		//					break;
		//				}
		//				else
		//				{
		//					lastLogNomor = Convert.ToString(Convert.ToInt32(lastLogNomor) + 1);
		//				}
		//			}


		//		}
		//		return Nomor + ":" + YearsNow;
		//	}


		//}

		public void UpdateNomorLog(string nomor, int id)
		{
			db.Database.ExecuteSqlCommand("UPDATE MASTER_LOGBOOK SET LOGBOOK_NOMOR = '"+ nomor + "', LOGBOOK_PROPOSAL_ID = "+ id +" WHERE LOGBOOK_ID = 1");
		}

		public ParNomor GenerateNomor(VIEW_PROPOSAL DataProposal = null)
		{
			string query = "";
			string NomorQuery = "";
			string Nomor = "";
			string NomorThn = "";
			string YearsNow = DateTime.Now.Year.ToString();
			var DataLogbook = (from lb in db.MASTER_LOGBOOK select lb).SingleOrDefault();
			string forbiddenNomor = DataLogbook.LOGBOOK_FORBIDDEN_NOMOR;
			string[] fNomor = forbiddenNomor.Split(',');
			string lastLogNomor = Convert.ToString(Convert.ToInt32(DataLogbook.LOGBOOK_NOMOR) + 1);


			if (DataProposal.PROPOSAL_JENIS_PERUMUSAN == 1 || DataProposal.PROPOSAL_JENIS_PERUMUSAN == 2)
			{
				if (DataProposal.PROPOSAL_JALUR == 2 && DataProposal.PROPOSAL_JENIS_ADOPSI == 1)
				{
					query = @"SELECT
								*
							FROM
								(
									SELECT
										REGEXP_SUBSTR (
											TP.PROPOSAL_ADOPSI_NOMOR_JUDUL,
											'[^:]+',
											1,
											1
										) AS nomor
									FROM
										TRX_PROPOSAL_ADOPSI TP
									WHERE
										TP.PROPOSAL_ADOPSI_PROPOSAL_ID = " + DataProposal.PROPOSAL_ID + @"
									AND TP.PROPOSAL_ADOPSI_NOMOR_JUDUL LIKE '%:%'
									ORDER BY
										TP.PROPOSAL_ADOPSI_ID ASC
								)
							WHERE
								ROWNUM = 1";
					NomorQuery = db.Database.SqlQuery<string>("" + query + "").SingleOrDefault();
					if (NomorQuery != null)
					{
						NomorThn = NomorQuery;
						Nomor = Convert.ToString(Convert.ToInt32(lastLogNomor) - 1);


					}
					else
					{
						foreach (string Res in fNomor)
						{
							if (Convert.ToInt32(Res) >= Convert.ToInt32(lastLogNomor))
							{
								if (Convert.ToInt32(Res) != Convert.ToInt32(lastLogNomor))
								{
									break;
								}
								else
								{
									lastLogNomor = Convert.ToString(Convert.ToInt32(lastLogNomor) + 1);
								}
							}


						}
						Nomor = lastLogNomor;
						NomorThn = lastLogNomor;
					}

				}
				else
				{
					foreach (string Res in fNomor)
					{
						if (Convert.ToInt32(Res) >= Convert.ToInt32(lastLogNomor))
						{
							if (Convert.ToInt32(Res) != Convert.ToInt32(lastLogNomor))
							{
								break;
							}
							else
							{
								lastLogNomor = Convert.ToString(Convert.ToInt32(lastLogNomor) + 1);
							}
						}


					}
					Nomor = lastLogNomor;
					NomorThn = lastLogNomor;
					//Nomor = Convert.ToString(DataProposal.PROPOSAL_ID);
				}
				var result = new ParNomor
				{
					nomor = Nomor,
					nomorThn = NomorThn + ":" + YearsNow
				};
				return result;
				//return Nomor + ":" + YearsNow;
			}
			else if (DataProposal.PROPOSAL_JENIS_PERUMUSAN == 5)
			{
				NomorThn = Convert.ToString(DataProposal.PROPOSAL_TERJEMAHAN_NOMOR);
				Nomor = Convert.ToString(Convert.ToInt32(lastLogNomor) - 1);
				var result = new ParNomor
				{
					nomor = Nomor,
					nomorThn = NomorThn
				};
				return result;
				//return Nomor;
			}
			else
			{
				foreach (string Res in fNomor)
				{
					if (Convert.ToInt32(Res) >= Convert.ToInt32(lastLogNomor))
					{
						if (Convert.ToInt32(Res) != Convert.ToInt32(lastLogNomor))
						{
							
							break;
						}
						else
						{
							lastLogNomor = Convert.ToString(Convert.ToInt32(lastLogNomor) + 1);
						}
					}


				}
				Nomor = lastLogNomor;
				NomorThn = lastLogNomor;
				var result = new ParNomor
				{
					nomor = Nomor,
					nomorThn = NomorThn + ":" + YearsNow
				};
				return result;
				//return Nomor + ":" + YearsNow;
			}


		}

		public string GenerateKodePNPS(int PROPOSAL_KOMTEK_ID = 0)
		{
			string strQuery = @"SELECT
								CAST (
									TO_CHAR (SYSDATE, 'YYYY') || '.' || KOMTEK_CODE || '.' || (
										SELECT
											CAST (
												(CASE
												WHEN (TO_CHAR (SYSDATE, 'YYYY') = REGEXP_SUBSTR (NP.MAXIMUM, '[^/.]+', 1, 1) OR NP.MAXIMUM IS NOT NULL) THEN
													CASE
													WHEN LENGTH (REGEXP_SUBSTR (NP.MAXIMUM, '[^/.]+', 1, 3) + 1) = 1 THEN
														'0' || CAST (REGEXP_SUBSTR (NP.MAXIMUM, '[^/.]+', 1, 3) + 1 AS VARCHAR2 (255))
													ELSE
														CAST (REGEXP_SUBSTR (NP.MAXIMUM, '[^/.]+', 1, 3) + 1 AS VARCHAR2 (255))
													END
												ELSE
													'01'
												END
												) AS VARCHAR2 (255)
											) NOMOR_URUT
										FROM (
											SELECT
												MAX (TP.PROPOSAL_PNPS_CODE) AS MAXIMUM
											FROM
												TRX_PROPOSAL TP
											WHERE
												TP.PROPOSAL_KOMTEK_ID = " + PROPOSAL_KOMTEK_ID + @"

											AND REGEXP_SUBSTR (TP.PROPOSAL_PNPS_CODE, '[^/.]+', 1, 1 ) = TO_CHAR (SYSDATE, 'YYYY')
										) NP
									) AS VARCHAR2 (255)
								) AS PNPSCODE
							FROM
								MASTER_KOMITE_TEKNIS
							WHERE
								KOMTEK_ID = " + PROPOSAL_KOMTEK_ID;
			var PROPOSAL_PNPS_CODE = db.Database.SqlQuery<string>(strQuery).SingleOrDefault();

			return PROPOSAL_PNPS_CODE;
		}
	}
}