using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkZoneLoad.Models;

namespace WorkZoneLoad.Controller
{
    public class WorkZoneController
    {
        //const DateTime dateTimeStart = DateTime.Now;
        public bool Add(string resource, string workZone)
        {
            DateTime dateTimeStart = DateTime.Now;
            DateTime dateTimeEnd = dateTimeStart.AddYears(1);

            bool flag = false;
            try
            {
                string json = @"{'workZone': '#','startDate': '@','endDate': '&','ratio': 100, 'recurEvery' : 1 , 'recurrence': 'daily'}";
                json = json.Replace("#", workZone);
                json = json.Replace("@", dateTimeStart.ToString("yyyy-MM-dd"));
                json = json.Replace("&", dateTimeEnd.ToString("yyyy-MM-dd"));

                JObject rss = JObject.Parse(json);
                ResponseOFSC result = UtilWebRequest.SendWayAsync(string.Format("rest/ofscCore/v1/resources/{0}/workZones", resource),
                                                                  enumMethod.POST,
                                                                  rss.ToString(Formatting.None));

                if (result.statusCode >= 200 && result.statusCode <= 300)
                {
                    Program.Logger(string.Format("Recurso {0}      zona de trabajo {1}      mensaje: se agrego correctamente", resource, workZone), LoggerOpcion.OK);
                    flag = true;
                }
                else
                {
                    Console.WriteLine(string.Format("* No se asigno zona;Recurso {0}      zona de trabajo {1}      mensaje: {2}|{3}", resource, workZone, result.Content, result.ErrorMessage));
                    Program.Logger(string.Format("Recurso {0}      zona de trabajo {1}      mensaje: {2}|{3}", resource, workZone, result.Content, result.ErrorMessage), LoggerOpcion.ERROR);
                    flag = false;
                }
            }
            catch (Exception ex)
            {
                Program.Logger("Error en la clase.metodo WorkZoneController.Add " + ex.Message);
            }
            return flag;
        }

        public bool Create(WorkZone workZone)
        {
            try
            {
                // create request object
                dynamic objWorkZone = new JObject();
                objWorkZone.workZoneLabel = workZone.workZoneLabel;
                objWorkZone.status = workZone.status;
                objWorkZone.travelArea = workZone.travelArea;
                objWorkZone.workZoneName = workZone.workZoneName;

                JArray jArray = new JArray();
                jArray.Add(workZone.label.FirstOrDefault());
                objWorkZone.keys = jArray;

                var result = UtilWebRequest.SendWayAsync("rest/ofscMetadata/v1/workZones",
                                                         enumMethod.POST,
                                                         objWorkZone.ToString());
                if (result.statusCode == 201)
                    return true;
                if (result.statusCode == 409)
                {
                    var result2 = UtilWebRequest.SendWayAsync("rest/ofscMetadata/v1/workZones/" + workZone.workZoneLabel,
                                                       enumMethod.PUT,
                                                       objWorkZone.ToString());
                    if (result2.statusCode == 200 || result2.statusCode == 201)
                        return true;
                    else
                        return false;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public bool Exist(WorkZone workZone)
        {
            try
            {
                // check exist
                var result = UtilWebRequest.SendWayAsync("rest/ofscMetadata/v1/workZones/" + workZone.label.FirstOrDefault(),
                                            enumMethod.GET,
                                            string.Empty);

                if (result.statusCode == 200)
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Concat("Error Excepction {0} details {1} ", ex.Message, ex.InnerException.Message));
                return false;
            }
        }

        // public async Task<bool> ExistAsync(string zipCode)
        public bool ExistAsync(string zipCode)
        {
            bool flag = false;
            try
            {
                var result = UtilWebRequest.SendWayAsync(string.Format("https://api-codigos-postales.herokuapp.com/v2/codigo_postal/{0}", zipCode), enumMethod.GET, null);
                dynamic results = JsonConvert.DeserializeObject<dynamic>(result.Content);

                var municipio = results.municipio;

                if (string.IsNullOrEmpty(municipio.Value))
                    flag = false;
                else
                    flag = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return flag;
        }

        public List<string> Ranges(WorkZone workZone)
        {
            workZone.id = new List<string>();
            try
            {
                string[] aWorkZonetmp = workZone.source.Split('|');

                foreach (var item in aWorkZonetmp)
                {
                    if (string.IsNullOrEmpty(item))
                        continue;

                    string tmp = item;

                    if (item.Contains(","))
                        tmp = item.Substring(item.IndexOf(',') + 1);

                    string[] aZipCode = tmp.Split('-');

                    if (aZipCode.Count() > 0)
                    {
                        int major = int.Parse(aZipCode[1]);
                        int minor = int.Parse(aZipCode[0]);

                        for (int i = minor; i <= major; i++)
                        {
                            string tmpzipcode = i.ToString().PadLeft(5, '0');
                            tmpzipcode = string.Concat(workZone.country, tmpzipcode);
                            workZone.id.Add(tmpzipcode);
                        }
                    }
                }
                List<string> tmpB = new List<string>();
                tmpB.AddRange(workZone.id.Distinct());
                workZone.id.Clear();
                workZone.id.AddRange(tmpB);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                // throw new Exception(ex.Message + " " + ex.InnerException);
            }

            return workZone.id;
        }

        public bool Set(WorkZone workZone)
        {
            try
            {
                // create request object
                dynamic objWorkZone = new JObject();
                objWorkZone.workZoneLabel = workZone.workZoneLabel;
                objWorkZone.status = workZone.status;
                objWorkZone.travelArea = workZone.travelArea;
                objWorkZone.workZoneName = workZone.workZoneName;
                JArray jArray = new JArray();
                jArray.Add(workZone.label.FirstOrDefault());
                objWorkZone.keys = jArray;

                var result = UtilWebRequest.SendWayAsync("rest/ofscMetadata/v1/workZones/" + workZone.workZoneLabel,
                                         enumMethod.PUT,
                                         objWorkZone.ToString());
                if (result.statusCode == 200)
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Concat("la zona {0} fallo {1} detalles {2}", workZone.workZoneLabel, ex.Message, ex.InnerException));
                return false;

            }
        }

        //  public bool Delete(WorkZone workZone, string resourceId)
        public bool Delete(WorkZone workZone, string resourceId)
        {
            bool flag = false;
            try
            {
                var result = UtilWebRequest.SendWayAsync(string.Format("/rest/ofscCore/v1/resources/{0}/workZones/{1}", resourceId, workZone.workZoneItemId), enumMethod.DELETE, string.Empty);

                if (result.statusCode >= 200 && result.statusCode <= 300)
                {
                    Program.Logger(string.Format("Recurso {0}      zona de trabajo {1}      mensaje: se borro correctamente", resourceId, workZone.workZone), LoggerOpcion.BORRO);
                    flag = true;
                }
                else
                {
                    Program.Logger(string.Format("Recurso {0}      zona de trabajo {1}      mensaje: {2}|{3}", resourceId, workZone.workZone, result.Content, result.ErrorMessage), LoggerOpcion.ERROR);
                    flag = false;
                }
            }
            catch (Exception ex)
            {
                Program.Logger("bool Delete " + ex.Message);
            }
            return flag;
        }

        public List<WorkZone> Get(string externalid)
        {
            List<WorkZone> listWorkZone = new List<WorkZone>();

            var result = UtilWebRequest.SendWayAsync("rest/ofscCore/v1/resources/" + externalid + "/workZones",
                                       enumMethod.GET,
                                       externalid);

            if (result.statusCode == 200)
            {
                JObject o = JObject.Parse(result.Content);
                var aitems = o["items"];
                foreach (var item in aitems)
                {
                    try
                    {
                        Console.Clear();
                        Console.WriteLine(item["workZoneItemId"].ToString());
                        WorkZone workZone = new WorkZone();
                        workZone.workZoneItemId = item["workZoneItemId"].ToString();
                        workZone.workZone = item["workZone"].ToString();
                        workZone.startDate = item["startDate"].ToString();
                        workZone.endDate = item["endDate"] == null ? DateTime.Now.AddYears(-1) : DateTime.Parse(item["endDate"].ToString());
                        listWorkZone.Add(workZone);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("* Details " + ex.Message);
                        Program.Logger("List<WorkZone> Get(string externalid) " + ex.Message);
                    }
                }
            }

            return listWorkZone;
        }

        public List<WorkZone> GetAll()
        {
            List<WorkZone> listWorkZone = new List<WorkZone>();
            List<WorkZone> listWorkZoneActive = new List<WorkZone>();
            List<WorkZone> listWorkZoneInactive = new List<WorkZone>();

            try
            {
                ResponseOFSC result = UtilWebRequest.SendWayAsync("rest/ofscMetadata/v1/workZones?limit=1&offset=0",
                                                         enumMethod.GET,
                                                         string.Empty);
                if (result.statusCode == 200)
                {
                    JObject o = JObject.Parse(result.Content);
                    int tmpLimit = 500;
                    double totalItems = (int)o["totalResults"];
                    double iteration = totalItems / tmpLimit;

                    iteration = Math.Ceiling(iteration);
                    //Console.WriteLine("Total de zonas de trabajo disponibles " + totalItems);
                    //Console.WriteLine("Total de iteraciones " + iteration);
                    //iteration = 54;
                    for (int i = 0; i < iteration; i++)
                    {
                        Console.Clear();
                        Console.WriteLine("Total de zonas de trabajo disponibles " + totalItems);
                        Console.WriteLine("Total de iteraciones " + iteration + " de 1000");
                        Console.WriteLine(string.Format("{0} %", (i * tmpLimit) / totalItems));

                        // int ilimit = 1000;
                        int offset = i == 0 ? 0 : (tmpLimit * i);
                        string endpoint = string.Format("rest/ofscMetadata/v1/workZones?limit={0}&offset={1}", tmpLimit, offset);
                        ResponseOFSC resultDynamic = UtilWebRequest.SendWayAsync(endpoint, enumMethod.GET, string.Empty);

                        JObject items = JObject.Parse(resultDynamic.Content);

                        List<WorkZone> listWorkZoneTmp = JsonConvert.DeserializeObject<List<WorkZone>>(items["items"].ToString());
                        listWorkZone.AddRange(listWorkZoneTmp);


                    }
                }
            }
            catch (Exception ex)
            {
                Program.Logger("List<WorkZone> GetAll(string externalid) " + ex.Message);
            }

            return listWorkZone;
        }
    }
}
