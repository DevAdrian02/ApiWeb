using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using ApiHerramientaWeb.Modelos.CargarFacturas;
using ApiHerramientaWeb.Modelos.Welcome.Estructura;
using LumenWorks.Framework.IO.Csv;
using Microsoft.EntityFrameworkCore;
using ModeloPrincipal.Entity;

namespace ApiHerramientaWeb.Modelos
{
    public class Utils
    {
        private readonly CVGEntities _context;
        private readonly IConfiguration _configuration;

        public Utils(CVGEntities context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public void SaveAuditUsOp(Audusop tba)
        {
            _context.Audusops.Add(tba);
            _context.SaveChanges();
        }

        public void SaveLogUs(Logu tba)
        {
            _context.Logus.Add(tba);
            _context.SaveChanges();
        }

        #region Validacion Permisos
        public bool HaveAccess(string controlador, int usuario)
        {
            try
            {
                var lstMenuIni = (from relRol in _context.Relrolobjs
                                  join relUrol in _context.Relusrrols on relRol.Iderol equals relUrol.Iderol
                                  join obj in _context.Mstobjs on relRol.Ideobj equals obj.Ideobj
                                  join md in _context.Mstorigens on obj.Modobj equals md.Ideorigen
                                  where relUrol.Ideusr == usuario && md.Ideorigen == 37 && obj.Controlador == controlador
                                  select new DatosLayoutMenu.MenuPad
                                  {
                                      IDEOBJ = relRol.Ideobj,
                                      IDEOBJPAD = obj.Ideobjpad ?? 0,
                                      CODOBJ = obj.Codobj,
                                      DSCOBJ = obj.Dscobj,
                                      ICONO = obj.Icono ?? " ",
                                      URL = obj.Url ?? " ",
                                      CONTROLADOR = obj.Controlador ?? " ",
                                      ACCION = obj.Accion ?? " "
                                  }).Distinct().ToList();

                return lstMenuIni.Any();
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool HaveAccessButton(int usuario, List<int> rolesPermitidos)
        {
            try
            {
                var rolesUsuario = (from relUrol in _context.Relusrrols
                                    where relUrol.Ideusr == usuario
                                    select relUrol.Iderol).ToList();

                return rolesUsuario.Any(role => rolesPermitidos.Contains(role));
            }
            catch (Exception)
            {
                return false;
            }
        }
        #endregion

        public string FirstMayus(string text)
        {
            TextInfo txtInfo = new CultureInfo("en-US", false).TextInfo;
            return txtInfo.ToTitleCase(text.ToLower());
        }

        public DataTable ConvertCSVtoDataTable(string strFilePath)
        {
            DataTable dtResult = new DataTable("Facturas");

            try
            {
                using (var csv = new CachedCsvReader(new StreamReader(strFilePath), true))
                {
                    dtResult.Load(csv);
                }
            }
            catch (Exception)
            {
                // Handle exception
            }

            return dtResult;
        }

        public dataFact BuildRowFac(dataCsv csvRow)
        {
            dataFact facrow = new dataFact();

            try
            {
                var x = (from f in _context.Mstfacs
                         join c in _context.Mstcnts on f.Idecnt equals c.Idecnt
                         where f.Stdfac == 2 && c.Ideftocnt == csvRow.contrato && f.Flgfacimp == true && f.Flgindimp == true && f.Fchanu == null
                         orderby f.Fchini ascending
                         select new
                         {
                             c.Ideftocnt,
                             f.Srefac,
                             f.Numfisfac,
                             f.Sdofacloc,
                             c.Nomfac
                         }).Take(1).ToList();

                if (x.Any())
                {
                    foreach (var d in x)
                    {
                        facrow.ideftocnt = d.Ideftocnt;
                        facrow.srefac = d.Srefac.Trim();
                        facrow.numfisfac = d.Numfisfac;
                        facrow.nomfac = d.Nomfac;
                        facrow.sdofacloc = d.Sdofacloc;
                        facrow.pago = csvRow.monto;
                        facrow.diferencia = d.Sdofacloc - csvRow.monto;
                        facrow.numref = csvRow.referencia;
                        facrow.pagada = facrow.sdofacloc == 0;
                        facrow.process = facrow.diferencia < 0
                        ? Math.Abs(facrow.diferencia) <= int.Parse(_configuration["toleranciaPago"])
                            : facrow.diferencia == 0;
                    }
                }
                else
                {
                    var y = (from f in _context.Mstfacs
                             join c in _context.Mstcnts on f.Idecnt equals c.Idecnt
                             where f.Stdfac == 2 && c.Ideftocnt == csvRow.contrato && f.Flgfacimp == true && f.Flgindimp == true
                             orderby f.Fchini descending
                             select new
                             {
                                 c.Ideftocnt,
                                 f.Srefac,
                                 f.Numfisfac,
                                 f.Sdofacloc,
                                 c.Nomfac
                             }).Take(1).ToList();

                    if (y.Any())
                    {
                        foreach (var d in y)
                        {
                            facrow.ideftocnt = d.Ideftocnt;
                            facrow.srefac = d.Srefac.Trim();
                            facrow.numfisfac = d.Numfisfac;
                            facrow.nomfac = d.Nomfac;
                            facrow.sdofacloc = d.Sdofacloc;
                            facrow.pago = csvRow.monto;
                            facrow.diferencia = d.Sdofacloc - csvRow.monto;
                            facrow.numref = csvRow.referencia;
                            facrow.pagada = facrow.sdofacloc == 0;
                            facrow.process = facrow.diferencia < 0
                        ? Math.Abs(facrow.diferencia) <= int.Parse(_configuration["toleranciaPago"])
                                : facrow.diferencia == 0;
                        }
                    }
                    else
                    {
                        facrow.ideftocnt = csvRow.contrato;
                        facrow.srefac = "N/A";
                        facrow.numfisfac = 0;
                        facrow.nomfac = "N/A";
                        facrow.sdofacloc = 0;
                        facrow.pago = csvRow.monto;
                        facrow.diferencia = 0;
                        facrow.numref = csvRow.referencia;
                        facrow.pagada = false;
                        facrow.process = false;
                    }
                }
            }
            catch (Exception)
            {
                facrow.ideftocnt = csvRow.contrato;
                facrow.srefac = "N/A";
                facrow.numfisfac = 0;
                facrow.nomfac = "N/A";
                facrow.sdofacloc = 0;
                facrow.pago = csvRow.monto;
                facrow.diferencia = 0;
                facrow.numref = csvRow.referencia;
                facrow.pagada = false;
                facrow.process = false;
            }

            return facrow;
        }

        public class Resultado
        {
            public int codMes { get; set; }
            public bool result { get; set; }
            public string mensaje { get; set; }
        }

        public async Task<int> getIdSucursal(int cnt)
        {
            var suc = await _context.Mstcnts
                .Where(s => s.Ideftocnt == cnt)
                .Select(s => s.Idesuc)
                .FirstOrDefaultAsync();

            return suc;
        }

        /// <summary>
        /// Obtiene el código de usuario (Codusr) a partir del iduser.
        /// </summary>
        /// <param name="iduser">ID del usuario</param>
        /// <returns>El código de usuario o null si no existe</returns>
        public async Task<string?> ObtenerCodigoUsuarioPorIdAsync(int iduser)
        {
            return await _context.Mstusrs
                .Where(u => u.Ideusr == iduser)
                .Select(u => u.Codusr)
                .FirstOrDefaultAsync();
        }


    }
}
