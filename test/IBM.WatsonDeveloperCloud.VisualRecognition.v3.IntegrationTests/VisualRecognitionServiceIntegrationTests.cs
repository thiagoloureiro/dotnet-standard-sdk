﻿/**
* Copyright 2017 IBM Corp. All Rights Reserved.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
*      http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*
*/

using IBM.WatsonDeveloperCloud.VisualRecognition.v3.Model;
using IBM.WatsonDeveloperCloud.Http.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using IBM.WatsonDeveloperCloud.Util;

namespace IBM.WatsonDeveloperCloud.VisualRecognition.v3.IntegrationTests
{
    [TestClass]
    public class VisualRecognitionServiceIntegrationTests
    {
        VisualRecognitionService _visualRecognition;
        private static string credentials = string.Empty;
        private static string _apikey;
        private static string _endpoint;
        private string _imageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/b/bb/Kittyply_edit1.jpg/1200px-Kittyply_edit1.jpg";
        private string _faceUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/8/8d/President_Barack_Obama.jpg/220px-President_Barack_Obama.jpg";
        private string _localGiraffeFilePath = @"VisualRecognitionTestData\giraffe_to_classify.jpg";
        private string _localFaceFilePath = @"VisualRecognitionTestData\obama.jpg";
        private string _localGiraffePositiveExamplesFilePath = @"VisualRecognitionTestData\giraffe_positive_examples.zip";
        private string _giraffeClassname = "giraffe";
        private string _localTurtlePositiveExamplesFilePath = @"VisualRecognitionTestData\turtle_positive_examples.zip";
        private string _turtleClassname = "turtle";
        private string _localNegativeExamplesFilePath = @"VisualRecognitionTestData\negative_examples.zip";
        private string _createdClassifierName = "dotnet-standard-test-integration-classifier";
        private string _createdClassifierId = "";
        AutoResetEvent autoEvent = new AutoResetEvent(false);

        [TestInitialize]
        public void Setup()
        {
            if (string.IsNullOrEmpty(credentials))
            {
                try
                {
                    credentials = Utility.SimpleGet(
                        Environment.GetEnvironmentVariable("VCAP_URL"),
                        Environment.GetEnvironmentVariable("VCAP_USERNAME"),
                        Environment.GetEnvironmentVariable("VCAP_PASSWORD")).Result;
                }
                catch (Exception e)
                {
                    Console.WriteLine(string.Format("Failed to get credentials: {0}", e.Message));
                }

                Task.WaitAll();

                var vcapServices = JObject.Parse(credentials);
                _endpoint = vcapServices["visual_recognition"]["url"].Value<string>();
                _apikey = vcapServices["visual_recognition"]["api_key"].Value<string>();
            }

            _visualRecognition = new VisualRecognitionService(_apikey, _endpoint);
            _visualRecognition.Client.BaseClient.Timeout = TimeSpan.FromMinutes(60);
        }

        [TestMethod]
        public void ClassifyGetSuccess()
        {
            List<string> classifiers = new List<string>()
            {
                "default"
            };

            List<string> owners = new List<string>()
            {
                "IBM",
                "me"
            };

            var result = _visualRecognition.Classify(_imageUrl, classifiers.ToArray(), owners.ToArray());

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Images);
            Assert.IsTrue(result.Images.Count > 0);
        }

        [TestMethod]
        public void ClassifyPostSuccess()
        {
            using (FileStream fs = File.OpenRead(_localGiraffeFilePath))
            {
                var result = _visualRecognition.Classify((fs as Stream).ReadAllBytes(), Path.GetFileName(_localGiraffeFilePath), "image/jpeg");

                Assert.IsNotNull(result);
                Assert.IsNotNull(result.Images);
                Assert.IsTrue(result.Images.Count > 0);
            }
        }

        [TestMethod]
        public void DetectFacesGetSuccess()
        {
            var result = _visualRecognition.DetectFaces(_faceUrl);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Images);
            Assert.IsTrue(result.Images.Count > 0);
        }

        [TestMethod]
        public void DetectFacesPostSuccess()
        {
            using (FileStream fs = File.OpenRead(_localFaceFilePath))
            {
                var result = _visualRecognition.DetectFaces((fs as Stream).ReadAllBytes(), Path.GetFileName(_localFaceFilePath), "image/jpeg");

                Assert.IsNotNull(result);
                Assert.IsNotNull(result.Images);
                Assert.IsTrue(result.Images.Count > 0);
            }
        }

        [TestMethod]
        public void GetClassifiersBriefSuccess()
        {
            var results = _visualRecognition.GetClassifiersBrief();

            Assert.IsNotNull(results);
        }

        [TestMethod]
        public void GetClassifiersVerboseSuccess()
        {
            _visualRecognition = new VisualRecognitionService(_apikey, _endpoint);
            var results = _visualRecognition.GetClassifiersVerbose();

            Assert.IsNotNull(results);
        }

        [TestMethod]
        public void CreateClassifierSuccess()
        {
            var createClassifierResponse = CreateClassifier();
            _createdClassifierId = createClassifierResponse.ClassifierId;

            Assert.IsNotNull(createClassifierResponse);
        }

        [TestMethod]
        public void UpdateClassifierSuccess()
        {
            var createClassifierResponse = CreateClassifier();

            //  Wait for classifier to finish training
            var isClassifierReady = IsClassifierReady(_createdClassifierId);
            autoEvent.WaitOne();

            var updateClassiferResponse = UpdateClassifier();

            Assert.IsNotNull(updateClassiferResponse);
        }

        [TestMethod]
        public void DeleteClassifierSuccess()
        {
            var createClassifierResponse = CreateClassifier();
            var deleteClassifierResponse = DeleteClassifier(createClassifierResponse.ClassifierId);

            Assert.IsNotNull(deleteClassifierResponse);
        }

        [TestCleanup]
        public void Teardown()
        {
            if (!string.IsNullOrEmpty(_createdClassifierId))
                DeleteClassifier(_createdClassifierId);
        }

        #region CreateClassifier
        public GetClassifiersPerClassifierVerbose CreateClassifier()
        {
            using (FileStream positiveExamplesStream = File.OpenRead(_localGiraffePositiveExamplesFilePath), negativeExamplesStream = File.OpenRead(_localNegativeExamplesFilePath))
            {
                Dictionary<string, byte[]> positiveExamples = new Dictionary<string, byte[]>();
                positiveExamples.Add(_giraffeClassname, positiveExamplesStream.ReadAllBytes());

                var result = _visualRecognition.CreateClassifier(_createdClassifierName, positiveExamples, negativeExamplesStream.ReadAllBytes());

                _createdClassifierId = result.ClassifierId;
                Console.WriteLine(string.Format("Created classifier {0}", _createdClassifierId));

                return result;
            }
        }
        #endregion

        #region UpdateClassifier
        public GetClassifiersPerClassifierVerbose UpdateClassifier()
        {
            using (FileStream positiveExamplesStream = File.OpenRead(_localTurtlePositiveExamplesFilePath))
            {
                Dictionary<string, byte[]> positiveExamples = new Dictionary<string, byte[]>();
                positiveExamples.Add(_turtleClassname, positiveExamplesStream.ReadAllBytes());

                var result = _visualRecognition.UpdateClassifier(_createdClassifierId, positiveExamples);
                Console.WriteLine(string.Format("Updated classifier {0}", _createdClassifierId));

                return result;
            }
        }
        #endregion

        #region DeleteClassifier
        public object DeleteClassifier(string classifierId)
        {
            #region Delay
            Delay(_delayTime);
            #endregion

            var result = _visualRecognition.DeleteClassifier(classifierId);
            _createdClassifierId = null;
            Console.WriteLine(string.Format("Deleted classifier {0}", classifierId));

            return result;
        }
        #endregion

        #region IsClassifierReady
        private bool IsClassifierReady(string classifierId)
        {
            var getClassifierResponse = _visualRecognition.GetClassifier(classifierId);

            string status = getClassifierResponse.Status.ToLower();
            Console.WriteLine(string.Format("Classifier status is {0}", status));

            if (status == "ready")
                autoEvent.Set();
            else
            {
                Task.Factory.StartNew(() =>
                {
                    System.Threading.Thread.Sleep(5000);
                    IsClassifierReady(classifierId);
                });
            }

            return getClassifierResponse.Status.ToLower() == "ready";
        }
        #endregion

        #region Delay
        //  Introducing a delay because of a known issue with Visual Recognition where newly created classifiers 
        //  will disappear without being deleted if a delete is attempted less than ~10 seconds after creation.
        private int _delayTime = 15000;
        private void Delay(int delayTime)
        {
            Console.WriteLine(string.Format("Delaying for {0} ms", delayTime));
            Thread.Sleep(delayTime);
        }
        #endregion
    }
}
