using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;


using Azure.Messaging.ServiceBus;


namespace Ricardo
{
    public static class HttpTrigger1
    {
    
    
        [FunctionName("HttpTrigger1")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]  HttpRequest req,
            [ServiceBus("queue", Connection = "AzureServiceBus")] IAsyncCollector<ServiceBusMessage> OutMessages,
            ILogger log)
        {   
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();  //Receive the body included in the http request

            
            string reqid = Guid.NewGuid().ToString();  //Generate an unique identifier to identify the request
            string hostName= string.Format("https://example.azurewebsites.net/api/Status", Environment.ExpandEnvironmentVariables("%WEBSITE_SITE_NAME%")); //Create an endpoint to verify the status
            string rqs=hostName+"/"+reqid;  //Create endpoint to check the status
          
            var message = new ServiceBusMessage(requestBody);  //Create the message to the servicebus
            message.ApplicationProperties.Add("RequestGUID", reqid);  //Add to the applicationproperties of the message the unique id
            message.ApplicationProperties.Add("RequestSubmittedAt", DateTime.Now);//Add to the applicationproperties of the message the date
            message.ApplicationProperties.Add("RequestStatusURL", rqs);//Add to the applicationproperties of the message the rqs

            await OutMessages.AddAsync(message);  //Queue the mmesage

            return new OkObjectResult($"{rqs}"); //Return successful request 

                  }
    }
}
