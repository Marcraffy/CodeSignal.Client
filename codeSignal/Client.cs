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
mutation CreateTest($id: ID!, $token: String) 
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

        public Client(Options options)
        {
            this.options = options;
            client = new GraphQLClient(options.Endpoint);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", this.options.Key);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<Assessment> GetAssessment(string id)
        {
            var response = await client.PostAsync(new GraphQLRequest
            {
                Query = GetTest,
                Variables = new { id }
            });

            return CheckAndGetObject<Assessment>(response);
        }

        public async Task<Link> GetAssessmentLink(string id, string token)
        {
            var response = await client.PostAsync(new GraphQLRequest
            {
                Query = GetTests,
                Variables = new { id }
            });

            var sessions = CheckAndGetObject<IList<Session>>(response);
            var existingLink = sessions.FirstOrDefault(session => session.TestTaker.Email.Contains(token));
            if (existingLink != null)
            {
                return existingLink as Link;
            }

            response = await client.PostAsync(new GraphQLRequest
            {
                Query = CreateTest,
                Variables = new { id, token }
            });

            return CheckAndGetObject<Link>(response);
        }

        private void CheckAndThrowException(GraphQLError[] errors)
        {
            if (errors?.Length > 0)
            {
                throw new Exception(errors.Aggregate(string.Empty, (acc, val) => acc + val + "\n"));
            }
        }

        private T GetFromJObject<T>(dynamic dynamicObject)
        {
            var data = (dynamicObject as JObject);
            if (data == null || data.First == null || data.First.First == null)
            {
                throw new Exception("Failed to resolve dynamic object");
            }
            return data.First.First.ToObject<T>();
        }

        private T CheckAndGetObject<T>(GraphQLResponse response)
        {
            CheckAndThrowException(response.Errors);
            return GetFromJObject<T>(response.Data);
        }
    }
}