using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Metadata;
using Windows.Storage;

/*
 * Copyright 2015 Laurent Ellerbach (http://blogs.msdn.com/laurelle)
 *
 * Please refer to the details here:
 *
 *     http://opensource.org/licenses/ms-pl
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 * 
 * Please note that most of this code is ported from an old .Net Microframework
 * development. It may be far from what you can do using most recent C# features
 */
namespace IoTCoreHelpers
{

    public sealed class Param
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public static char ParamStart { get { return '?'; } }
        public static char ParamEqual { get { return '='; } }
        public static char ParamSeparator { get { return '&'; } }
        public static List<Param> decryptParam(string Parameters)
        {
            List<Param> retParams = new List<Param>();
            int i = Parameters.IndexOf(ParamStart);
            String strtocut = Parameters;
            if (i >= 0)
                strtocut = Parameters.Substring(i + 1);
            var strcut = strtocut.Split(ParamSeparator);
            if (strcut != null)
            {
                for (int inc = 0; inc < strcut.Length; inc++)
                {
                    var strgood = strcut[inc].Split(ParamEqual);
                    if (strgood.Length == 2)
                    {
                        retParams.Add( new Param { Name = strgood[0], Value = strgood[1] });
                    }
                }

            }
            return retParams;
        }
        public static Param[] decryptParamArray(string Parameters)
        {
            return decryptParam(Parameters).ToArray();
        }

        
        /// <summary>
        /// Search for a specific parameter, all lowercase and return the value as a boolean
        /// </summary>
        /// <param name="Params">a list of Param</param>
        /// <param name="NameToSeach">a Name value to search</param>
        /// <returns></returns>
        public static bool CheckConvertBool(List<Param> Params, string NameToSeach)
        {
            Param Paramret = null;
            bool retval = false;
            Paramret = Params.Find(p => p.Name.ToLower().IndexOf(NameToSeach) == 0);
            try
            {
                if (Paramret != null)
                    if (Convert.ToByte(Paramret.Value) == 1)
                        retval = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception converting parameter: {ex.Message}");
            }
            return retval;
        }
        /// <summary>
        /// Search for a specific parameter, all lowercase and return the value as a boolean
        /// </summary>
        /// <param name="Params">a list of Param</param>
        /// <param name="NameToSeach">a Name value to search</param>
        /// <returns></returns>
        public static byte CheckConvertByte(List<Param> Params, string NameToSeach)
        {
            Param Paramret = null;
            byte retval = byte.MaxValue;
            Paramret = Params.Find(p => p.Name.ToLower().IndexOf(NameToSeach) == 0);
            try
            {
                retval = Convert.ToByte(Paramret.Value);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception converting parameter: {ex.Message}");
            }
            return retval;
        }
    }
}
