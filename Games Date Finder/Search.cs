using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Xml;
using System.Windows.Forms;
using System.ComponentModel;

namespace Games_Date_Finder
{
    class Search
    {


        // Replace the following string with the AppId you received from the
        // Bing Developer Center.
        const string AppId = "ECF5759080C7CC51CF61208714514F392BAC1EF2";
        public String m_description;
        public List<String> links;
        public List<String> titles;
        WebBrowser m_web;

        public WebBrowser Form
        {
            get { return m_web; }
            set { m_web = value; }
        }
        public string GameName;
        public string machine;

        public Search()
        {

        }

        public string Searchit()
        {

            HttpWebRequest request = BuildRequest();
            titles = new List<string>();
            links = new List<string>();
            m_description = " ";
            try
            {
                // Send the request; display the response.
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                DisplayResponse(response);


                //Variables mevas
                List<KeyValuePair<String, KeyValuePair<int, int>>> results = new List<KeyValuePair<String, KeyValuePair<int, int>>>(); //Key=any Value= posicio   --> Posicions  de las datas
                bool gameNameFounded = false;
                bool[] gameNameCheck = new bool[GameName.Length];
                KeyValuePair<int, int> GameNamePosition = new KeyValuePair<int, int>();
                string str = m_description;


                //Busquem la data
                for (int i = 0; i < str.Length; i++)
                {
                    char c = str[i];

                    if (i == str.Length - 3) break;
                    if (Char.IsDigit(c))
                    {
                        char[] strArray = new char[4];

                        strArray[0] = str[i];
                        strArray[1] = str[i + 1];
                        strArray[2] = str[i + 2];
                        strArray[3] = str[i + 3];

                        if (strArray[0] != '1') continue;
                        if (strArray[1] != '9') continue;
                        if (strArray[2] != '7' && strArray[2] != '8' && strArray[2] != '9') continue;
                        if (Char.IsDigit(strArray[3]) == false) continue;


                        if (i < 0)
                            if (Char.IsDigit(str[i - 1]) == true) continue;

                        if (i + 4 < strArray.Length)
                            if (Char.IsDigit(str[i + 4]) == true) continue;

                        //Afegim el resultat

                        results.Add(new KeyValuePair<String, KeyValuePair<int, int>>(new String(strArray), new KeyValuePair<int, int>(i - strArray.Length, i)));

                    }

                    //Busquem la posicio on apareix el nom del joc per primer cop
                    else if (gameNameFounded == false)
                    {
                        byte pos = 0;
                        while (gameNameCheck[pos] == true)
                        {
                            //L'hem trobat
                            if (pos == gameNameCheck.Length - 1)
                            {
                                gameNameFounded = true;
                                GameNamePosition = new KeyValuePair<int, int>(i - GameName.Length, i);
                                gameNameCheck = new bool[GameName.Length];
                            }
                            pos++;
                        }

                        if (c == GameName[pos]) gameNameCheck[pos] = true;
                        else gameNameCheck = new bool[GameName.Length];
                    }

                }

                //Si no hem trobat el nom del joc i no hem trobat dates es retorna null
                if (gameNameFounded == false)
                    if (results.Count < 1)
                        return null;
                    else
                        return results[0].Key;

                //Busquem el any mes porpe (distancia mes curta) al nom del joc
                else
                {
                    int minDistance = Int32.MaxValue;
                    string res = null;
                    foreach (var pv in results)
                    {
                        int bigValue;
                        int smallvalue;
                        int currentDistance;

                        if (pv.Value.Key > GameNamePosition.Key)
                        {
                            bigValue = pv.Value.Key;
                            smallvalue = GameNamePosition.Key;
                        }
                        else
                        {
                            smallvalue = pv.Value.Key;
                            bigValue = GameNamePosition.Key;
                        }

                        currentDistance = bigValue - smallvalue;
                        if (currentDistance < minDistance)
                        {
                            minDistance = currentDistance;
                            res = pv.Key;
                        }
                    }

                    return res;
                }
            }
            catch (WebException ex)
            {
                // An exception occurred while accessing the network.
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        HttpWebRequest BuildRequest()
        {
            string requestString = "http://api.bing.net/xml.aspx?"

                // Common request fields (required)
                + "AppId=" + AppId
                + "&Query=" + machine + " " + "\"" + GameName + "\"" + " RELEASE DATE"
                + "&Sources=Web"

                // Common request fields (optional)
                + "&Version=2.0"
                + "&Market=en-us"
                + "&Adult=Moderate"
                + "&Options=EnableHighlighting"

                // Web-specific request fields (optional)
                + "&Web.Count=10"
                + "&Web.Offset=0"
                + "&Web.Options=DisableHostCollapsing+DisableQueryAlterations";


            // Create and initialize the request.
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(
                requestString);

            return request;
        }

        void DisplayResponse(HttpWebResponse response)
        {
            // Load the response into an XmlDocument.
            XmlDocument document = new XmlDocument();
            document.Load(response.GetResponseStream());

            // Add the default namespace to the namespace manager.
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(
                document.NameTable);
            nsmgr.AddNamespace(
                "api",
                "http://schemas.microsoft.com/LiveSearch/2008/04/XML/element");

            XmlNodeList errors = document.DocumentElement.SelectNodes(
                "./api:Errors/api:Error",
                nsmgr);

            if (errors.Count > 0)
            {
                // There are errors in the response. Display error details.
                DisplayErrors(errors);
            }
            else
            {
                // There were no errors in the response. Display the
                // Web results.
                DisplayResults(document.DocumentElement, nsmgr);
            }
        }

        void DisplayResults(XmlNode root, XmlNamespaceManager nsmgr)
        {
            // Add the Web SourceType namespace to the namespace manager.
            nsmgr.AddNamespace(
                "web",
                "http://schemas.microsoft.com/LiveSearch/2008/04/XML/web");

            XmlNode web = root.SelectSingleNode("./web:Web", nsmgr);
            XmlNodeList results = web.SelectNodes(
                "./web:Results/web:WebResult",
                nsmgr);

            string version = root.SelectSingleNode("./@Version", nsmgr).InnerText;
            string searchTerms = root.SelectSingleNode(
                "./api:Query/api:SearchTerms",
                nsmgr).InnerText;
            int offset;
            int.TryParse(
                web.SelectSingleNode("./web:Offset", nsmgr).InnerText,
                out offset);
            int total;
            int.TryParse(
                web.SelectSingleNode("./web:Total", nsmgr).InnerText,
                out total);

            // Display the results header.



            // Display the Web results.
            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            foreach (XmlNode result in results)
            {


                titles.Add(result.SelectSingleNode("./web:Title", nsmgr).InnerText);
                links.Add(result.SelectSingleNode("./web:Url", nsmgr).InnerText);
                string webData = String.Empty;
                string url = result.SelectSingleNode("./web:Url", nsmgr).InnerText;

               







                //string webData = System.Text.Encoding.UTF8.GetString(raw);



                //descriptions.Add(result.SelectSingleNode("./web:DisplayUrl", nsmgr).InnerText);

                //builder.Length = 0;
                //builder.AppendLine(
                //    result.SelectSingleNode("./web:Title", nsmgr).InnerText);
                //builder.AppendLine(
                //    result.SelectSingleNode("./web:Url", nsmgr).InnerText);
                //builder.AppendLine(
                //    result.SelectSingleNode("./web:Description", nsmgr).InnerText);
                //builder.Append("Last Crawled: ");
                //builder.AppendLine(
                //    result.SelectSingleNode("./web:DateTime", nsmgr).InnerText);

                //DisplayTextWithHighlighting(builder.ToString());
                //Console.WriteLine();
            }
        }

        void DisplayTextWithHighlighting(string text)
        {
            // Write text to the standard output stream, changing the console
            // foreground color as highlighting characters are encountered.
            foreach (char c in text.ToCharArray())
            {
                if (c == '\uE000')
                {
                    // If the current character is the begin highlighting
                    // character (U+E000), change the console foreground color
                    // to green.
                    Console.ForegroundColor = ConsoleColor.Green;
                }
                else if (c == '\uE001')
                {
                    // If the current character is the end highlighting
                    // character (U+E001), revert the console foreground color
                    // to gray.
                    Console.ForegroundColor = ConsoleColor.Gray;
                }
                else
                {
                    Console.Write(c);
                }
            }
        }

        void DisplayErrors(XmlNodeList errors)
        {
            // Iterate over the list of errors and display error details.
            Console.WriteLine("Errors:");
            Console.WriteLine();
            foreach (XmlNode error in errors)
            {
                foreach (XmlNode detail in error.ChildNodes)
                {
                    Console.WriteLine(detail.Name + ": " + detail.InnerText);
                }

                Console.WriteLine();
            }
        }

    }
}
