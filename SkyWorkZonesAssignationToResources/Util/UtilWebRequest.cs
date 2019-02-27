using RestSharp;
using System.Configuration;
using WorkZoneLoad.Models;

namespace WorkZoneLoad
{
    public enum enumMethod
    {
        GET,
        POST,
        DELETE,
        PUT,
        PATCH
    }

    public enum enumCountrys
    {
        CR,
        GT,
        HN,
        MX,
        NI,
        PA,
        SV
    }


    public class UtilWebRequest
    {
        // DEV
        // const string token = "Basic YThhM2VjMmIzNjQxNTkzYTM5MjVlNjQ5MGJjMjg0ZGFlNjhmMDk5MzZAc2t5LW14Mi50ZXN0OjIyOTc5NTkwZjI0OTEwYTY4NDgwZjQ4MzcyYzFhOGE4NmVlNWM3OTdjMjI5Y2U5MjE1ZmFkZmNlN2FhZjE1YzY=";

        // PROD
        // const string token = "Basic YThhM2VjMmIzNjQxNTkzYTM5MjVlNjQ5MGJjMjg0ZGFlNjhmMDk5MzZAc2t5LW14OjAwZTVlZTRkNmI2ODc0NTQ0NDI3ZDZlMGM3ZDg1YWY4ZDdiNDQzM2E4MzE1ZGQ3NzllYWEzM2QyYmM4NGY0NDk=";
        public static ResponseOFSC SendWayAsync(string endpoint, enumMethod enumMethod, string data)
        {
            var client = new RestClient("https://api.etadirect.com/" + endpoint);
            string token = ConfigurationManager.AppSettings["execute"];

            RestRequest request = new RestRequest();

            switch (enumMethod.ToString())
            {
                case "PUT":
                    request.Method = Method.PUT;
                    break;

                case "POST":
                    request.Method = Method.POST;
                    break;
                case "PATCH":
                    request.Method = Method.PATCH;
                    break;
                case "GET":
                    request.Method = Method.GET;
                    break;
                case "DELETE":
                    request.Method = Method.DELETE;
                    break;
                default:
                    break;
            }

            // request.AddHeader("Cache-Control", "no-cache");
            request.AddHeader("Authorization", token);
            request.AddHeader("Content-Type", "application/json");
            if (!string.IsNullOrEmpty(data))
                request.AddParameter("", data, ParameterType.RequestBody);

            IRestResponse response = client.Execute(request);

            ResponseOFSC responseOFSC = new ResponseOFSC();
            responseOFSC.Content = response.Content;
            responseOFSC.statusCode = (int)response.StatusCode;
            responseOFSC.ErrorMessage = response.ErrorMessage;
            return responseOFSC;
        }

    }
}
