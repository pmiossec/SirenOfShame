using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SirenOfShame.Lib.Settings;

namespace TfsRestServices
{
    public class TfsRestService
    {
        public async Task<List<TfsRestBuildDefinition>> GetBuildDefinitions(string url, string username, string password)
        {
            var projects = await GetProjects(url, username, password);
            return projects.Select(i => new TfsRestBuildDefinition(i)).ToList();
        }

        public async Task<TfsJsonBuildDefinition[]> GetProjects(string url, string username, string password, IEnumerable<string> buildDefinitions = null)
        {
            HttpClient httpClient = new HttpClient();
            var byteArray = Encoding.ASCII.GetBytes(username + ":" + password);
            var betterUrl = url + (url.EndsWith("/") ? "" : "/");
            httpClient.BaseAddress = new Uri(betterUrl);
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            string queryParams = buildDefinitions == null ? "" : "&definitions=" + string.Join(",", buildDefinitions);
            var buildDefinitionsStr = await httpClient.GetStringAsync("_apis/build/builds?api-version=2.0" + queryParams);
            var jsonWrapper = JsonConvert.DeserializeObject<TfsJsonWrapper>(buildDefinitionsStr);
            return jsonWrapper.Value;
        }

        public async Task<IEnumerable<TfsRestBuildStatus>> GetBuildsStatuses(CiEntryPointSetting ciEntryPointSetting, BuildDefinitionSetting[] watchedBuildDefinitions)
        {
            var buildIds = watchedBuildDefinitions.Select(i => i.Id);
            var projects = await GetProjects(ciEntryPointSetting.Url, ciEntryPointSetting.UserName, ciEntryPointSetting.GetPassword(), buildIds);
            return projects.Select(i => new TfsRestBuildStatus(i));
        }
    }
}