namespace Cake.GitLabAPICI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Cake.GitLabAPICI.GitLabTypes;
    using Cake.GitLabAPICI.Responses;
    using Cake.Core;
    using Cake.Core.Annotations;
    using Newtonsoft.Json;

    using LogLevel = Cake.Core.Diagnostics.LogLevel;
    using Verbosity = Cake.Core.Diagnostics.Verbosity;

    public static class GitLabJobRunner
    {
        private static HttpClient _httpClient;

        [CakeMethodAlias]
        public static long ExecuteGitLabPipeline(this ICakeContext context, string gitLabUri, string gitLabToken, int projectId,
            string branch, string triggerToken)
        {
            Pipeline pipeline = null;
            try
            {
                context?.Log.Write(Verbosity.Normal, LogLevel.Information, $"Executing GitLab pipeline for project id '{projectId}' on branch '{branch}'");

                CreateHttpClient(gitLabUri, gitLabToken);

                pipeline = CreateNewPipelineAsync(projectId, branch, triggerToken).GetAwaiter().GetResult();
                if (!WaitForPipelineToFinishAsync(projectId, pipeline, context).GetAwaiter().GetResult())
                {
                    throw new Exception("Pipeline execution failed!");
                }

                context?.Log.Write(Verbosity.Normal, LogLevel.Information, $"Finished executing GitLab pipeline for project id '{projectId}' on branch '{branch}'");
            }
            catch (Exception e)
            {
                context?.Log.Write(Verbosity.Normal, LogLevel.Error, e.ToString());
                throw;
            }

            return pipeline?.Id ?? 0;
        }
            
        [CakeMethodAlias]
        public static void ExecuteGitLabJob(this ICakeContext context, string gitLabUri, string gitLabToken, int projectId,
            string branch, string triggerToken, long pipelineId, string jobName)
        {
            try
            {
                context?.Log.Write(Verbosity.Normal, LogLevel.Information, $"Executing GitLab job '{jobName}' for project id '{projectId}' on branch '{branch}' for pipeline id '{pipelineId}'");

                CreateHttpClient(gitLabUri, gitLabToken);

                var jobs = GetJobsForAPipelineAsync(projectId, pipelineId).GetAwaiter().GetResult();
                var jobToExecute = jobs.FirstOrDefault(job => job.Name.ToLower().Equals(jobName));
                if (jobToExecute == null)
                {
                    throw new Exception($"No job found with name '{jobName}' on pipeline with id {pipelineId}");
                }

                PlayJobAsync(projectId, jobToExecute.Id).GetAwaiter().GetResult();
                if (!WaitForJobToFinishAsync(projectId, jobToExecute.Id, context).GetAwaiter().GetResult())
                {
                    throw new Exception("Job failed!");
                }

                context?.Log.Write(Verbosity.Normal, LogLevel.Information, $"Finished executing GitLab job '{jobName}' for project id '{projectId}' on branch '{branch}' for pipeline id '{pipelineId}'");
            }
            catch (Exception e)
            {
                context?.Log.Write(Verbosity.Normal, LogLevel.Error, e.ToString());
                throw;
            }
        }

        private static async Task<bool> WaitForPipelineToFinishAsync(int projectId, Pipeline pipeline, ICakeContext context)
        {
            var pipelineIsRunning = true;
            while (pipelineIsRunning)
            {
                await Task.Delay(15000);
                Console.WriteLine("Checking pipeline status...");
                pipeline = await GetPipelineAsync(projectId, pipeline.Id);

                switch (pipeline.Status)
                {
                    case "success":
                        pipelineIsRunning = false;
                        context?.Log.Write(Verbosity.Normal, LogLevel.Information, "Pipeline succeeded");
                        break;

                    case "failed":
                        context?.Log.Write(Verbosity.Normal, LogLevel.Information, "Pipeline failed");
                        return false;

                    case "canceled":
                        context?.Log.Write(Verbosity.Normal, LogLevel.Information, "Pipeline was cancelled");
                        return false;

                    case "skipped":
                        pipelineIsRunning = false;
                        context?.Log.Write(Verbosity.Normal, LogLevel.Information, "Pipeline was skipped");
                        break;

                    default:
                        continue;
                }
            }

            return true;
        }

        private static async Task<bool> WaitForJobToFinishAsync(int projectId, long jobId, ICakeContext context)
        {
            var jobIsRunning = true;
            while (jobIsRunning)
            {
                await Task.Delay(15000);
                Console.WriteLine("Checking job status...");
                var job = await GetJobAsync(projectId, jobId);

                switch (job.Status)
                {
                    case "success":
                        jobIsRunning = false;
                        context?.Log.Write(Verbosity.Normal, LogLevel.Information, "Job succeeded");
                        break;

                    case "failed":
                        context?.Log.Write(Verbosity.Normal, LogLevel.Information, "Job failed");
                        return false;

                    case "canceled":
                        context?.Log.Write(Verbosity.Normal, LogLevel.Information, "Job was cancelled");
                        return false;

                    case "skipped":
                        jobIsRunning = false;
                        context?.Log.Write(Verbosity.Normal, LogLevel.Information, "Job was skipped");
                        break;

                    default:
                        continue;
                }
            }

            return true;
        }

        private static void CreateHttpClient(string gitLabUri, string gitLabToken)
        {
            _httpClient = new HttpClient { BaseAddress = new Uri(gitLabUri) };
            _httpClient.DefaultRequestHeaders.Add("PRIVATE-TOKEN", gitLabToken);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
        }

        private static async Task<Pipeline> CreateNewPipelineAsync(int projectId, string branch, string triggerToken)
        {
            Uri uri = new Uri($"{_httpClient.BaseAddress}/projects/{projectId}/trigger/pipeline");
            var form = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("ref", branch),
                new KeyValuePair<string, string>("token", triggerToken),
            };

            Console.WriteLine($"Starting pipeline on branch '{branch}' using project id {projectId}");
            HttpResponseMessage response = await _httpClient.PostAsync(uri, new FormUrlEncodedContent(form));

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to create pipeline on branch '{branch}' using project id {projectId}.{Environment.NewLine} Response status code is '{response.StatusCode}' and reason is {response.ReasonPhrase}'");
            }

            var responseString = await response.Content.ReadAsStringAsync();
            var pipeline = JsonConvert.DeserializeObject<TriggerPipelineResponse>(responseString);

            return new Pipeline
            {
                Id = pipeline.Id,
                Ref = pipeline.Ref,
                Sha = pipeline.Sha,
                Status = pipeline.Status
            };
        }

        private static async Task<Pipeline> GetPipelineAsync(int projectId, long pipelineId)
        {
            Uri uri = new Uri($"{_httpClient.BaseAddress}/projects/{projectId}/pipelines/{pipelineId}");
            HttpResponseMessage response = await _httpClient.GetAsync(uri);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to get pipeline with id {pipelineId} from project with id {projectId}.{Environment.NewLine} Response status code is '{response.StatusCode}' and reason is {response.ReasonPhrase}'");
            }

            var responseString = await response.Content.ReadAsStringAsync();
            var pipeline = JsonConvert.DeserializeObject<TriggerPipelineResponse>(responseString);

            return new Pipeline
            {
                Id = pipeline.Id,
                Ref = pipeline.Ref,
                Sha = pipeline.Sha,
                Status = pipeline.Status
            };
        }

        private static async Task<IEnumerable<Job>> GetJobsForAPipelineAsync(int projectId, long pipelineId)
        {
            var uri = new Uri($"{_httpClient.BaseAddress}/projects/{projectId}/pipelines/{pipelineId}/jobs");
            var response = await _httpClient.GetAsync(uri);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to get jobs from pipeline with id {pipelineId} from project with id {projectId}.{Environment.NewLine} Response status code is '{response.StatusCode}' and reason is {response.ReasonPhrase}'");
            }

            var responseString = await response.Content.ReadAsStringAsync();
            var jobs = JsonConvert.DeserializeObject<IEnumerable<Job>>(responseString);

            return jobs;
        }

        private static async Task PlayJobAsync(int projectId, int jobId)
        {
            Uri uri = new Uri($"{_httpClient.BaseAddress}/projects/{projectId}/jobs/{jobId}/play");
            var response = await _httpClient.PostAsync(uri, new StringContent(string.Empty));
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to start job with id {jobId} on project with id {projectId}.{Environment.NewLine} Response status code is '{response.StatusCode}' and reason is {response.ReasonPhrase}'");
            }
        }

        private static async Task<Job> GetJobAsync(int projectId, long jobId)
        {
            Uri uri = new Uri($"{_httpClient.BaseAddress}/projects/{projectId}/jobs/{jobId}");
            var response = await _httpClient.GetAsync(uri);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to get job with id {jobId} on project with id {projectId}.{Environment.NewLine} Response status code is '{response.StatusCode}' and reason is {response.ReasonPhrase}'");
            }

            var responseString = await response.Content.ReadAsStringAsync();
            var job = JsonConvert.DeserializeObject<Job>(responseString);

            return job;
        }
    }
}
