/**
* Copyright 2018 IBM Corp. All Rights Reserved.
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

using System.Collections.Generic;
using Newtonsoft.Json;

namespace IBM.WatsonDeveloperCloud.SpeechToText.v1.Model
{
    /// <summary>
    /// SpeechRecognitionResult.
    /// </summary>
    public class SpeechRecognitionResult : BaseModel
    {
        /// <summary>
        /// An indication of whether the transcription results are final. If `true`, the results for this utterance are not updated further; no additional results are sent for a `result_index` once its results are indicated as final.
        /// </summary>
        /// <value>An indication of whether the transcription results are final. If `true`, the results for this utterance are not updated further; no additional results are sent for a `result_index` once its results are indicated as final.</value>
        [JsonProperty("final", NullValueHandling = NullValueHandling.Ignore)]
        public bool? FinalResults { get; set; }
        /// <summary>
        /// An array of alternative transcripts. The `alternatives` array can include additional requested output such as word confidence or timestamps.
        /// </summary>
        /// <value>An array of alternative transcripts. The `alternatives` array can include additional requested output such as word confidence or timestamps.</value>
        [JsonProperty("alternatives", NullValueHandling = NullValueHandling.Ignore)]
        public List<SpeechRecognitionAlternative> Alternatives { get; set; }
        /// <summary>
        /// A dictionary (or associative array) whose keys are the strings specified for `keywords` if both that parameter and `keywords_threshold` are specified. A keyword for which no matches are found is omitted from the array. You can spot a maximum of 1000 keywords. The array is omitted if no keywords are found.
        /// </summary>
        /// <value>A dictionary (or associative array) whose keys are the strings specified for `keywords` if both that parameter and `keywords_threshold` are specified. A keyword for which no matches are found is omitted from the array. You can spot a maximum of 1000 keywords. The array is omitted if no keywords are found.</value>
        [JsonProperty("keywords_result", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, List<KeywordResult>> KeywordsResult { get; set; }
        /// <summary>
        /// An array of alternative hypotheses found for words of the input audio if a `word_alternatives_threshold` is specified.
        /// </summary>
        /// <value>An array of alternative hypotheses found for words of the input audio if a `word_alternatives_threshold` is specified.</value>
        [JsonProperty("word_alternatives", NullValueHandling = NullValueHandling.Ignore)]
        public List<WordAlternativeResults> WordAlternatives { get; set; }
    }

}
