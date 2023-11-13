using GESI.CORE.BO;
using GESI.CORE.DAL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace APIImportacionComprobantes.BLL
{
    public class LogSucesosAPI
    {

        /// <summary>
        /// Registra los errores que surgieron en la importacion via API.
        /// </summary>
        /// <param name="strDescripcionError"></param>
        public static void LoguearErrores(String strDescripcionError, int EmpresaID)
        {
            int i = 0;
            int max_intentos = 3;
            do
            {
                try
                {
                    using (StreamWriter mylogs = File.AppendText(System.IO.Directory.GetCurrentDirectory() + "\\ImportacionCobranzasAPI_" + EmpresaID + ".log"))
                    {
                        mylogs.WriteLine(DateTime.Now.ToString() + "|" + strDescripcionError);
                        mylogs.Close();
                        // Si la grabación es exitosa, establecer la variable i a 2.
                        i = max_intentos;
                    }
                }
                catch (Exception ex)
                {
                    // Si se produce un error, intenta escribirlo de nuevo.
                    i++;
                    System.Threading.Thread.Sleep(50);
                }
                // Esperar 50 ms antes de incrementar la variable.

            } while (i < max_intentos);

        }

        /// <summary>
        /// Loguea los errores producidos por en la importacion de pedidos
        /// </summary>
        /// <param name="strDescripcionError"></param>
        public static void LoguearErroresPedidos(String strDescripcionError,int EmpresaID)
        {

            int i = 0;
            int max_intentos = 3;
            do
            {
                try
                {
                    using (StreamWriter mylogs = File.AppendText(System.IO.Directory.GetCurrentDirectory() + "\\ImportacionPedidosAPI_" + EmpresaID + ".log"))
                    {
                        mylogs.WriteLine(DateTime.Now.ToString() + "|" + strDescripcionError);
                        mylogs.Close();
                        // Si la grabación es exitosa, establecer la variable i a 2.
                        i = max_intentos;
                    }
                }
                catch (Exception ex)
                {
                    // Si se produce un error, intenta escribirlo de nuevo.
                    i++;
                    System.Threading.Thread.Sleep(50);
                }
                // Esperar 50 ms antes de incrementar la variable.

            } while (i < max_intentos);
            

        }


        /// <summary>
        /// Registra las operacione exitosas en la API
        /// </summary>
        /// <param name="strDescripcionError"></param>
        public static void LoguearExitososPedidos(String strDescripcionError)
        {
            int i = 0;
            int max_intentos = 3;
            do
            {
                try
                {
                    using (StreamWriter mylogs = File.AppendText(System.IO.Directory.GetCurrentDirectory() + "\\ImportacionExitosasPedidos.log"))
                    {
                        mylogs.WriteLine(DateTime.Now.ToString() + "|" + strDescripcionError);
                        mylogs.Close();
                        // Si la grabación es exitosa, establecer la variable i a 2.
                        i = max_intentos;
                    }
                }
                catch (Exception ex)
                {
                    // Si se produce un error, intenta escribirlo de nuevo.
                    i++;
                    System.Threading.Thread.Sleep(50);
                }
                // Esperar 50 ms antes de incrementar la variable.

            } while (i < max_intentos);

        }


        /// <summary>
        /// Registra las operaciones exitosas en la API
        /// </summary>
        /// <param name="strDescripcionError"></param>
        public static void LoguearExitosos(String strDescripcionError)
        {

            int i = 0;
            int max_intentos = 3;
            do
            {
                try
                {
                    using (StreamWriter mylogs = File.AppendText(System.IO.Directory.GetCurrentDirectory() + "\\ImportacionExitosas.log"))
                    {
                        mylogs.WriteLine(DateTime.Now.ToString() + "|" + strDescripcionError);
                        mylogs.Close();
                        // Si la grabación es exitosa, establecer la variable i a 2.
                        i = max_intentos;
                    }
                }
                catch (Exception ex)
                {
                    // Si se produce un error, intenta escribirlo de nuevo.
                    i++;
                    System.Threading.Thread.Sleep(50);
                }
                // Esperar 50 ms antes de incrementar la variable.

            } while (i < max_intentos);
        }


    }
}