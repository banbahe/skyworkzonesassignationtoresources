using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WorkZoneLoad.Controller;
using WorkZoneLoad.Models;

using System.Net;

namespace WorkZoneLoad
{
    public enum LoggerOpcion
    {
        OK = 1,
        ERROR = 2,
        LOG = 0,
        BORRO = 3,
        DB = 4,
        RESOURCE_WORKZONE = 5,
    }

    public enum HttpStatus
    {
        OK = 200,
        BADREQUEST = 400
    }
    public class Program
    {
        public static int addworkzone { get; set; }
        private static List<WorkZone> listWorkZone = new List<WorkZone>();
        private static List<WorkZone> listWorkZoneActive = new List<WorkZone>();
        private static List<WorkZone> listWorkZoneInactive = new List<WorkZone>();

        public static List<string> list { get; set; } = new List<string>();
        public static List<Resource> listResource { get; set; }
        public static string sPath { get; set; } = @"C:\Users\inmotion\Documents\bitbucket\ofsc\skymx\code\html";
        static void Main(string[] args)
        {
            // **************************************************************
            // DOWNLOAD DB

            Console.WriteLine(" Ingrese Ubicación (Folder) en donde buscar archivos");
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine(" Por ejemplo C:\\Users\\inmotion\\Documents\\z");
            // Console.ResetColor();
            // sPath = Console.ReadLine();
            sPath = ConfigurationManager.AppSettings["filepath"];


            if (Directory.Exists(sPath))
                Console.WriteLine("Leyendo archivos CSV");
            else
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(string.Format("Ubicación no valida {0}", sPath));
                Thread.Sleep(1800);
                Console.ResetColor();
                throw new Exception(" x=> { x.id = 'error' }");
            }
            //ReadCSV(sPath);
            //// Fill Object
            //Split();
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            Console.WriteLine("Obteniendo Zonas de Trabajo disponibles " + DateTime.Now);
            // *****************************************************************
            // Get all workzone from OFSC
            WorkZoneController ctrlWorkZone = new WorkZoneController();
            listWorkZone = ctrlWorkZone.GetAll();
            Console.WriteLine("Termino de obtener Zonas de Trabajo disponibles " + listWorkZone.Count() + "  " + DateTime.Now);
            listWorkZoneActive = listWorkZone.Where(x => x.status == "active").ToList();
            Console.Clear();
            Logger("----------------------------------------------------------------------------------");
            string token = ConfigurationManager.AppSettings["execute"];
            Logger("Inicio del proceso para instancia con el token " + token);
            Logger(DateTime.Now.ToString());
            Console.WriteLine("Leyendo archivos CSV");

            ReadCSV(sPath);
            // Fill Object
            Split();
            Console.WriteLine("Se asignaran zonas de trabajo a un total de " + listResource.Count + " recursos");
            // int addworkzone = 0;
            int replaceworkzone = 0;
            foreach (var resource in listResource)
            {
                // Obtiene zonas de trabajo de recurso
                WorkZoneController workZoneController = new WorkZoneController();
                ResourceController resourceController = new ResourceController();
                Console.Clear();
                Console.WriteLine("---------------------------------------------------");
                Console.WriteLine(" Recurso " + resource.externalId);

                // Check if exist resource
                string tmpContent = string.Empty;
                if (!resourceController.Exist(resource.externalId, out tmpContent))
                {
                    Program.Logger(string.Format(";Resource:{0};WorkZone:;Message:Recurso no existe|{1};", resource, tmpContent), LoggerOpcion.ERROR);
                    continue;
                }

                // List<WorkZone> listWorkZonesCurrent = new List<WorkZone>();
                List<string> listWorkZonesCSV = workZoneController.Ranges(resource.workZone);

                if (listWorkZonesCSV.Count > 0)
                {
                    List<WorkZone> listworkZone = new List<WorkZone>();
                    List<string> listTmpWorkZoneCSV = new List<string>();

                    foreach (var item in listWorkZonesCSV)
                    {
                        if (listWorkZoneActive.Exists(x => x.workZoneLabel == item))
                            listTmpWorkZoneCSV.Add(item);
                        else
                            continue;
                    }

                    var addItems = listTmpWorkZoneCSV;

                    // delete item
                    if (resource.workZone.replace)
                    {
                        Console.WriteLine("Reemplazando zonas de trabajo del recurso {0} ", resource.externalId);
                        var deleteItems = workZoneController.Get(resource.externalId);

                        foreach (var itemZipCode in deleteItems)
                        {
                            if (workZoneController.Delete(itemZipCode, resource.externalId))
                            {
                                replaceworkzone = replaceworkzone + 1;
                                Console.Clear();
                                Console.WriteLine(string.Format("* Reemplazando cobertura del recurso {0}", resource.externalId));
                            }
                        }
                    }

                    // add item
                    Console.WriteLine("Agregando zonas de trabajo del recurso {0} ", resource.externalId);
                    foreach (string itemWorkZoneAdd in addItems)
                        workZoneController.Add(resource.externalId, itemWorkZoneAdd);

                    workZoneController.LogWorkzonesOK(resource.externalId);

                }
            }
            // multitask 
            stopwatch.Stop();
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Ha terminado de asignar a {0} recursos ", listResource.Count);
            Console.WriteLine("Total de asignaciones realizadas {0} ", addworkzone);
            Console.WriteLine("Total de asignaciones reemplazadas {0} ", replaceworkzone);
            Console.WriteLine("Total Minutos " + stopwatch.Elapsed.TotalMinutes);
            Console.WriteLine(" Tiempo aproximado por petición es de : " + (stopwatch.Elapsed.TotalSeconds / addworkzone).ToString() + " segundos");
            Logger(" Zonas Agregadas: " + addworkzone);
            Logger(" Minutos: " + stopwatch.Elapsed.TotalMinutes.ToString());
            Logger(" Tiempo aproximado por petición es de : " + (stopwatch.Elapsed.TotalSeconds / addworkzone).ToString() + " segundos");
            Logger(DateTime.Now.ToString());
            Logger(" End ");
            Logger("----------------------------------------------------------------------------------");
            Console.ReadLine();
        }
        static Resource FillObject(string[] aItems)
        {
            Resource objResource = new Resource();
            WorkZoneController workZone = new WorkZoneController();
            objResource.workZone = new WorkZone();
            try
            {
                objResource.parentId = aItems[0].Trim();
                objResource.externalId = aItems[1].Trim();
                objResource.resourceType = aItems[3].Trim();

                if (aItems.Count() >= 25)
                {
                    // AGREGAR
                    if (string.IsNullOrEmpty(aItems[23]) || aItems[23].ToString().ToUpper() == "AGREGAR")
                        objResource.workZone.replace = false;
                    else
                        // REEMPLAZAR
                        objResource.workZone.replace = true;

                    if (string.IsNullOrEmpty(aItems[24]))
                        return null;
                    else
                    {
                        objResource.resource_workzones = aItems[25];

                        switch (aItems[24])
                        {
                            case "MX":
                                objResource.resource_workzones = aItems[25];
                                break;

                            case "CR":
                                objResource.workZone.country = "CR";
                                objResource.resource_workzones = aItems[25];
                                break;

                            case "GT":
                                objResource.workZone.country = "GT";
                                objResource.resource_workzones = aItems[25];
                                break;

                            case "HN":
                                objResource.workZone.country = "HN";
                                objResource.resource_workzones = aItems[25];
                                break;

                            case "NI":
                                objResource.workZone.country = "NI";
                                objResource.resource_workzones = aItems[25];
                                break;

                            case "PA":
                                objResource.workZone.country = "PA";
                                objResource.resource_workzones = aItems[25];
                                break;

                            case "SV":
                                objResource.workZone.country = "SV";
                                objResource.resource_workzones = aItems[25];
                                break;

                            default:
                                objResource.resource_workzones = aItems[25];
                                break;
                        }
                    }
                }

                else
                    return null;

                objResource.workZone.source = objResource.resource_workzones;
                objResource.workZone.id = workZone.Ranges(objResource.workZone);

            }
            catch (Exception ex)
            {
                string text = string.Concat("* dont marred item parent id {0} and external id {1} no working but continue process, details: {2}", objResource.externalId, objResource.parentId, ex.Message);
                Console.WriteLine(text);
                Logger(text, LoggerOpcion.ERROR);
                return null;
            }
            return objResource;
        }
        private static void ReadCSV(string path)
        {
            Console.Clear();
            var files = Directory.GetFiles(path, "*.csv");

            foreach (var item in files)
            {
                try
                {
                    CSVController objCSVController = new CSVController();
                    objCSVController.source = @item;

                    Task<List<string>> task = objCSVController.LinesFile();
                    task.Wait();
                    var result = task.Result;
                    list.AddRange(result);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(string.Format("Error  al leer el archivo {0} : Exepción :{1}", item, ex.Message));
                    Logger(string.Format("Error  al leer el archivo {0} : Exepción :{1}", item, ex.Message));
                    throw;
                }
            }
        }

        static void Split()
        {
            listResource = new List<Resource>();
            foreach (var item in list)
            {
                CSVController objCSVController = new CSVController();
                Resource objResource = new Resource();
                string[] result = objCSVController.SplitBy(item, ';');
                objResource = FillObject(result);
                if (objResource != null)
                {
                    //   objResource.resource_workzones
                    if (!string.IsNullOrEmpty(objResource.resource_workzones))
                        listResource.Add(objResource);
                }
            }
        }
        static string WorkZoneQueue(List<WorkZone> listworkZone)
        {
            int good = 0;
            int bad = 0;
            int limitTemp = 0;

            foreach (var item in listworkZone)
            {
                limitTemp++;
                var flag = WorkZoneMain(item);
                if (flag)
                    good++;
                else
                {
                    Logger(string.Format("workzone {0}|{1}|{2}|{3}|{4}|", item.workZoneLabel, item.status, item.travelArea, item.workZoneName, item.label.FirstOrDefault()));
                    bad++;
                }

                if (limitTemp == 1000)
                {
                    Thread.Sleep(1000);
                    limitTemp = 0;
                }
            }

            return string.Concat(listworkZone.Count, ",", good, ",", bad);
        }
        static bool WorkZoneMain(WorkZone workZone)
        {
            bool flag = false;
            WorkZoneController ctrlworkZone = new WorkZoneController();
            var checkExist = ctrlworkZone.Exist(workZone);
            if (checkExist)
                flag = ctrlworkZone.Set(workZone);
            else
                flag = ctrlworkZone.Create(workZone);

            return flag;
        }

        static void WorkZoneList(string externalId, List<string> list)
        {
            WorkZoneController workZoneController = new WorkZoneController();
            foreach (var item in list)
                workZoneController.Add(externalId, item);
        }



        public static void Logger(String lines, LoggerOpcion loggerOpcion = LoggerOpcion.LOG)
        {
            string temppath = string.Empty;
            try
            {
                switch ((int)loggerOpcion)
                {
                    case 1:
                        temppath = @sPath + "\\log_ok.txt";
                        break;
                    case 2:
                        temppath = @sPath + "\\log_error.txt";
                        break;
                    case 3:
                        temppath = @sPath + "\\log_reemplar.txt";
                        break;
                    case 4:
                        temppath = Directory.GetCurrentDirectory() + "\\db.json";
                        break;
                    case 5:
                        temppath = @sPath + "\\log_recurso_zonas_trabajo_ok.txt";
                        break;
                    case 6:
                        temppath = @sPath + "\\log_recurso_zonas_trabajo_error.txt";
                        break;
                    default:
                        temppath = @sPath + "\\log.txt";
                        break;
                }

                System.IO.StreamWriter file = new System.IO.StreamWriter(temppath, true);
                // file.WriteLine((int)loggerOpcion == 4 ? lines : DateTime.Now + " : " + lines);
                file.WriteLine(lines);
                file.Close();
            }
            catch
            {
                Thread.Sleep(800);
                Logger(lines, loggerOpcion);
            }
        }

    }
}

// end 