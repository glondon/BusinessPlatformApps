﻿using Microsoft.Deployment.Common.ActionModel;
using Microsoft.Deployment.Common.Actions;
using Microsoft.Deployment.Common.Helpers;
using Newtonsoft.Json.Linq;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Deployment.Actions.AzureCustom.ActivityLogs
{
    [Export(typeof(IAction))]
    public class SetStreamAnalyticsJSONFunction : BaseAction
    {
        // Updates the default query provided by a Stream Analytics job
        public override async Task<ActionResponse> ExecuteActionAsync(ActionRequest request)
        {
            string token = request.DataStore.GetJson("AzureToken", "access_token");
            string subscription = request.DataStore.GetJson("SelectedSubscription", "SubscriptionId");
            string resourceGroup = request.DataStore.GetValue("SelectedResourceGroup");
            string jobName = request.DataStore.GetValue("SAJob");
            string apiVersion = "2015-10-01";
            string funcName = "ConvertBlobToJSON";
            string uri = $"https://management.azure.com/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.StreamAnalytics/streamingjobs/{jobName}/functions/{funcName}?api-version={apiVersion}";
            string script = "function main(InputJSON) {\n    return JSON.parse(InputJSON);\n}";
            string body = $"{{\"properties\": {{\"type\": \"Scalar\", \"properties\": {{\"inputs\": [{{\"dataType\": \"any\"}}],\"output\": {{\"dataType\": \"nvarchar(max)\"}}, \"binding\": {{\"type\": \"Microsoft.StreamAnalytics/JavascriptUdf\", \"properties\": {{\"script\":\"{script}\" }}}}}}}}}}";
            AzureHttpClient ahc = new AzureHttpClient(token, subscription);
            HttpResponseMessage response = await ahc.ExecuteGenericRequestWithHeaderAsync(HttpMethod.Put, uri, body);
            return response.IsSuccessStatusCode ? new ActionResponse(ActionStatus.Success) : new ActionResponse(ActionStatus.Failure);
        }
    }
}
