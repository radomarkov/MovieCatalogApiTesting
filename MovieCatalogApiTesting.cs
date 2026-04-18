using System;
using System.Text.Json;
using System.Net;
using RestSharp;
using RestSharp.Authenticators;
using MovieCatalogApiTesting.Models;

namespace MovieCatalogApiTesting
{
    [TestFixture]
    public class MovieCatalogApiTests
    {
        private RestClient client;
        private static string lastCreatedMovie;
        private const string baseUrl = "http://144.91.123.158:5000/api";
        private const string usernameLogin = "tester@example.com";
        private const string passwordLogin = "tester123";
        private const string accessToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJKd3RTZXJ2aWNlQWNjZXNzVG9rZW4iLCJqdGkiOiIyODgwMTZiMC1jMzAxLTRiNzgtYmU4OS1mYmU2NjM0ZjI3MWYiLCJpYXQiOiIwNC8xOC8yMDI2IDA2OjE4OjExIiwiVXNlcklkIjoiMWFhMWUyZmQtNjk2My00OWZkLTYxZDQtMDhkZTc2OTcxYWI5IiwiRW1haWwiOiJ0ZXN0ZXJAZXhhbXBsZS5jb20iLCJVc2VyTmFtZSI6InRlc3RlciIsImV4cCI6MTc3NjUxNDY5MSwiaXNzIjoiTW92aWVDYXRhbG9nX0FwcF9Tb2Z0VW5pIiwiYXVkIjoiTW92aWVDYXRhbG9nX1dlYkFQSV9Tb2Z0VW5pIn0.06DN2Aa2JokizkzVUECsBDF0nTzxrQopIFFpidgoqcQ";

        [OneTimeSetUp]
        public void Setup()
        {
            string JwtToken;

            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                JwtToken = accessToken;
            }
            else
            {
                JwtToken = GetNewJwtToken(usernameLogin, passwordLogin);
            }
            var options = new RestClientOptions(baseUrl)
            {
                Authenticator = new JwtAuthenticator(JwtToken)
            };
            this.client = new RestClient(options);
        }
        private string GetNewJwtToken(string email, string password)
        {
            var newClient = new RestClient(baseUrl);
            var newRequest = new RestRequest("/User/Authentication", Method.Post);
            newRequest.AddJsonBody(new { email, password });
            var newResponse = newClient.Execute(newRequest);
            if(newResponse.StatusCode == HttpStatusCode.OK)
            {
                var content = JsonSerializer.Deserialize<JsonElement>(newResponse.Content);
                var token = content.GetProperty("token").GetString();
                if (string.IsNullOrWhiteSpace(token))
                {
                    throw new InvalidOperationException("Failed to retrieve token");
                }
                return token;
            }
            else
            {
                throw new InvalidOperationException("Failed to authenticate");
            }
        }

        [Order(1)]
        [Test]
        public void CreateNewMovie_WithNeededFields_ShouldSucceed()
        {
            var newMovie = new MovieDTO
            {
                Title = "Test Movie",
                Description = "This is a test movie"
            };
            var request = new RestRequest("/Movie/Create", Method.Post);
            request.AddJsonBody(newMovie);
            var response = this.client.Execute(request);
            var responseData = JsonSerializer.Deserialize<JsonElement>(response.Content);
            var responseMsg = responseData.GetProperty("msg").GetString();
            var movieJson = responseData.GetProperty("movie").GetRawText();
            var responseContent = JsonSerializer.Deserialize<MovieDTO>(movieJson);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(responseContent, Is.Not.Null);
            Assert.That(responseContent.Id, Is.Not.Null.And.Not.Empty);
            Assert.That(responseMsg, Is.EqualTo("Movie created successfully!"));
            lastCreatedMovie = responseContent?.Id;
            //Console.WriteLine(lastCreatedMovie);
        }

        [Order(2)]
        [Test]
        public void EditCreatedMovieWithNeededFields_ShouldSucceed()
        {
            var editedMovie = new MovieDTO
            {
                Title = "Edited Movie",
                Description = "Edited description"
            };
            var request = new RestRequest("/Movie/Edit", Method.Put);
            request.AddQueryParameter("movieId", lastCreatedMovie);
            request.AddJsonBody(editedMovie);
            var response = this.client.Execute(request);
            var responseData = JsonSerializer.Deserialize<MovieDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(responseData.Msg, Is.EqualTo("Movie edited successfully!"));
        }

        [Order(3)]
        [Test]
        public void GetAllMovies_ShouldSucceed()
        {
            var request = new RestRequest("/Catalog/All", Method.Get);
            var response = this.client.Execute(request);
            var responseData = JsonSerializer.Deserialize<List<ApiResponseDTO>>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(responseData, Is.Not.Empty, "Response data was empty");
        }

        [Order(4)]
        [Test]
        public void DeleteCreatedMovie_ShouldSucceed()
        {
            var request = new RestRequest("/Movie/Delete", Method.Delete);
            request.AddQueryParameter("movieId", lastCreatedMovie);
            var response = this.client.Execute(request);
            var responseData = JsonSerializer.Deserialize<MovieDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(responseData.Msg, Is.EqualTo("Movie deleted successfully!"));
        }

        [Order(5)]
        [Test]
        public void CreateNewMovie_WithoutNeededFields_ShouldFail()
        {
            var newMovie = new MovieDTO
            {
                Title = "Test Movie",
                Description = ""
            };
            var request = new RestRequest("/Movie/Create", Method.Post);
            request.AddJsonBody(newMovie);
            var response = this.client.Execute(request);
            var responseData = JsonSerializer.Deserialize<JsonElement>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Order(6)]
        [Test]
        public void EditNonExistingMovie_ShouldFail()
        {
            string nonExistingMovieId = "0000-0000-0000-0000";
            var editedMovie = new MovieDTO
            {
                Title = "Edited Movie",
                Description = "Edited description"
            };
            var request = new RestRequest("/Movie/Edit", Method.Put);
            request.AddQueryParameter("movieId", nonExistingMovieId);
            request.AddJsonBody(editedMovie);
            var response = this.client.Execute(request);
            var responseData = JsonSerializer.Deserialize<MovieDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(responseData.Msg, Is.EqualTo("Unable to edit the movie! Check the movieId parameter or user verification!"));
        }

        [Order(7)]
        [Test]
        public void DeleteNonExistingMovie_ShouldFail()
        {
            string nonExistingMovieId = "0000-0000-0000-0000";
            var request = new RestRequest("/Movie/Delete", Method.Delete);
            request.AddQueryParameter("movieId", nonExistingMovieId);
            var response = this.client.Execute(request);
            var responseData = JsonSerializer.Deserialize<MovieDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(responseData.Msg, Is.EqualTo("Unable to delete the movie! Check the movieId parameter or user verification!"));
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            this.client.Dispose();
        }
    }
}