using Castle.Core.Logging;
using CodeSignal.Models;
using GraphQL.Client;
using GraphQL.Common.Request;
using GraphQL.Common.Response;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace CodeSignal.Client
{
    public class Client
    {
        private readonly Options options;
        private readonly GraphQLClient client;
        private readonly ILogger logger;

        private const string GetTest = @"
query GetTest($id: ID!) 
{ 
    companyTest(id: $id) 
    { id, title, introMessage, outroMessage, duration } 
}";

        private const string GetTests = @"
query GetTests($id: ID!) 
{   
    companyTestSessions(companyTestId: $id)
    { id, testTaker { email }, invitationUrl }
}";

        private const string CreateTest = @"
mutation CreateTest($id: ID!, $token: String!) 
{   
    createCompanyTestSession(sessionFields: 
    { 
        candidateFirstName: $token, 
        candidateLastName: $token, 
        candidateEmail: $token, 
        testId: $id
    }) 
    { id, invitationUrl }
}";

        public Client(ILogger logger, Options options)
        {
            this.options = options;
            this.logger = logger;
            logger.Debug($"Initialising GraphQL Client on {options.Endpoint}");
            client = new GraphQLClient(options.Endpoint);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", this.options.Key);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            logger.Debug("Initialisation complete");
        }

        public async Task<Assessment> GetAssessment(string id)
        {
            logger.Debug($"Getting assessment with Id: {id}");
            var response = await PostGraphQL(GetTest, new { id });
            return CheckAndGetObject<Assessment>(response);
        }

        public async Task<Link> GetAssessmentLink(string id, string token)
        {
            logger.Debug($"Getting user with token: {token} an invitation link for assessment with Id: {id}");
            var existingLink = CheckAndGetObject<IList<Session>>(await PostGraphQL(GetTests, new { id }))
                .FirstOrDefault(session => session.TestTaker.Email.Contains(token));

            if (existingLink != null)
            {
                return existingLink as Link;
            }

            logger.Debug($"Creating an invitation link for assessment with Id: {id} for user with token: {token}");
            return CheckAndGetObject<Link>(await PostGraphQL(CreateTest, new { id, token }));
        }

        private void CheckAndThrowException(GraphQLError[] errors)
        {
            if (errors?.Length > 0)
            {
                foreach (var error in errors)
                {
                    logger.Error(error.Message);
                }
                var errorMessages = errors.Aggregate(string.Empty, (output, error) => $"{output}{error.Message}\n");
                throw new Exception(errorMessages);
            }
        }

        private T GetFromJObject<T>(dynamic dynamicObject)
        {
            logger.Debug($"Deserializing Response");
            var data = (dynamicObject as JObject);
            if (data == null || data.First == null || data.First.First == null)
            {
                const string errorMessage = "Failed to resolve dynamic object";
                logger.Error(errorMessage);
                throw new Exception(errorMessage);
            }
            var output = data.First.First.ToObject<T>();
            logger.Debug($"Deserialization Complete");
            return output;
        }

        private T CheckAndGetObject<T>(GraphQLResponse response)
        {
            logger.Debug("Response Recieved");
            CheckAndThrowException(response.Errors);
            return GetFromJObject<T>(response.Data);
        }

        private async Task<GraphQLResponse> PostGraphQL(string query, dynamic variables)
        {
            logger.Debug("Posting GraphQL Request");
            var request = new GraphQLRequest
            {
                Query = query,
                Variables = variables
            };
            return await client.PostAsync(request);
        }
    }
}