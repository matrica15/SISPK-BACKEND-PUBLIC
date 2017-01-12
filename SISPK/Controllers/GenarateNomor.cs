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
			}
			else if (DataProposal.PROPOSAL_JENIS_PERUMUSAN == 5)
			{
				Nomor = Convert.ToString(DataProposal.PROPOSAL_TERJEMAHAN_NOMOR);
			}
			else
			{
				Nomor = Convert.ToString(DataProposal.PROPOSAL_ID);
			}

			return Nomor + ":" + YearsNow;
		}
	}
}