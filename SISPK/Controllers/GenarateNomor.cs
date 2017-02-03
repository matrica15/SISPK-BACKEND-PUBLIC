using System;
using System.Linq;
using SISPK.Models;

namespace SISPK.Controllers
{
	public class GenarateNomor
	{
		private SISPKEntities db = new SISPKEntities();

		public string GenerateNomor(VIEW_PROPOSAL DataProposal = null)
		{
			string query = "";
			string Nomor = "";
			string YearsNow = DateTime.Now.Year.ToString();

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
					Nomor = db.Database.SqlQuery<string>("" + query + "").SingleOrDefault();
				}
				else
				{
					Nomor = Convert.ToString(DataProposal.PROPOSAL_ID);
				}
				return Nomor + ":" + YearsNow;
			}
			else if (DataProposal.PROPOSAL_JENIS_PERUMUSAN == 5)
			{
				Nomor = Convert.ToString(DataProposal.PROPOSAL_TERJEMAHAN_NOMOR);
				return Nomor;
			}
			else
			{
				Nomor = Convert.ToString(DataProposal.PROPOSAL_ID);
				return Nomor + ":" + YearsNow;
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