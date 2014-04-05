// -------------------------------------------------------------------------------------------------
// wiki-browser.cs 0.0.1
//
// KSP wiki reader plugin.
// Copyright (C) 2014 Iván Atienza
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see http://www.gnu.org/licenses/. 
// 
// Email: mecagoenbush at gmail dot com
// Freenode: hashashin
//
// -------------------------------------------------------------------------------------------------

using HtmlAgilityPack;
using System;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

namespace wikitext
{
    [KSPAddon(KSPAddon.Startup.EveryScene, false)]
    public class wikitext : MonoBehaviour
    {
        private Vector2 _scrollViewVector = Vector2.zero;
        private Rect _websize;
        private string _text = "";
        private string _url = "";
        private string _keybind;
        private bool _visible = false;
        private string _version;
        private string _versionlastrun;
        private IButton _button;
        private string _tooltipon = "Hide wiki-browser";
        private string _tooltipoff = "Show wiki-browser";
        private string _btexture_on = "wiki-text/Textures/icon_on";
        private string _btexture_off = "wiki-text/Textures/icon_off";
        private int selGridInt = 0;
        private string[] selStrings = new string[] { "Kerbin", "Jool", "Eve", "Mun","Kerbol"
            , "Gilly", "Minmus", "Duna", "Ike", "Dres", "Laythe", "Vall", "Tylo", "Bop", "Pol"
            , "Eeloo", "Controls", "Tutorial:Action_group", "Calendar", "Basic_SSTO_Design"};

        void Awake()
        {
            LoadVersion();
            VersionCheck();
            LoadSettings();
            CheckDefaults();
        }

        void Start()
        {
            if (ToolbarManager.ToolbarAvailable)
            {
                _button = ToolbarManager.Instance.add("wiki-browser", "toggle");
                _button.TexturePath = _btexture_off;
                _button.ToolTip = _tooltipoff;

                _button.OnClick += ((e) =>
                {
                    Toggle();
                });
            }
        }

        void Update()
        {
            if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(_keybind))
            {
                Toggle();
            }
        }

        void OnDestroy()
        {
            SaveSettings();
            if (_button != null)
            {
                _button.Destroy();
            }
        }


        void OnGUI()
        {
            if (_visible)
            {
                _websize = GUI.Window(GUIUtility.GetControlID(0, FocusType.Passive), _websize, webWindow, "KSP-Wiki browser");
            }
        }

        void webWindow(int windowID)
        {
            _scrollViewVector = GUI.BeginScrollView(new Rect(0f, 15f, 835f, 600f), _scrollViewVector, new Rect(0f, 0f, 805f, 13000f));
            _text = GUI.TextArea(new Rect(3f, 0f, 820f, 13000f), _text);
            GUI.EndScrollView();
            if (GUI.Button(new Rect(2f, 2f, 13f, 13f), "X"))
            {
                Toggle();
            }
            GUI.Label(new Rect(5f, 615f, 350f, 20f), _url);
            if (GUI.Button(new Rect(355f, 615f, 80f, 20f), "Load"))
            {
                Load();
            }
            GUI.BeginGroup(new Rect(3f, 620f, 1000f, 1000f));
            selGridInt = GUILayout.SelectionGrid(selGridInt, selStrings, 4);
            GUI.EndGroup();
            GUI.DragWindow();
        }

        private void Load()
        {
            _url = "http://wiki.kerbalspaceprogram.com/wiki/" + selStrings[selGridInt];
            var web = new HtmlWeb();
            var doc = web.Load(_url);
            var html = doc.DocumentNode.SelectSingleNode("//*[@id='bodyContent']").InnerText;
            string regex = @"(<.+?>|&.+?;)|<!--.+?-->";
            _text = Regex.Replace(html, regex, String.Empty).Trim();
        }

        private void LoadSettings()
        {
            KSPLog.print("[wiki-browser.dll] Loading Config...");
            KSP.IO.PluginConfiguration configfile = KSP.IO.PluginConfiguration.CreateForType<wikitext>();
            configfile.load();

            _websize = configfile.GetValue<Rect>("windowpos");
            _keybind = configfile.GetValue<string>("keybind");
            _versionlastrun = configfile.GetValue<string>("version");
            KSPLog.print("[wiki-browser.dll] Config Loaded Successfully");
        }
        
        private void SaveSettings()
        {
            KSPLog.print("[wiki-browser.dll] Saving Config...");
            KSP.IO.PluginConfiguration configfile = KSP.IO.PluginConfiguration.CreateForType<wikitext>();

            configfile.SetValue("windowpos", _websize);
            configfile.SetValue("keybind", _keybind);
            configfile.SetValue("version", _version);

            configfile.save();
            KSPLog.print("[wiki-browser.dll] Config Saved ");
        }

        private void CheckDefaults()
        {
            if (_websize == new Rect(0, 0, 0, 0))
            {
                _websize = new Rect(20, 20, 840, 770);
            }
            if (_keybind == null)
            {
                _keybind = "w";
            }
        }

        private void Toggle()
        {
            if (_visible == true)
            {
                _visible = false;
                _button.TexturePath = _btexture_off;
                _button.ToolTip = _tooltipoff;
            }
            else
            {
                _visible = true;
                _button.TexturePath = _btexture_on;
                _button.ToolTip = _tooltipon;
            }
        }

        private void VersionCheck()
        {
            _version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            KSPLog.print("wiki-browser.dll version: " + _version);
            if ((_version != _versionlastrun) && (KSP.IO.File.Exists<wikitext>("config.xml")))
            {
                KSP.IO.File.Delete<wikitext>("config.xml");
            }
#if DEBUG
            KSP.IO.File.Delete<wikitext>("config.xml");
#endif
        }

        private void LoadVersion()
        {
            KSP.IO.PluginConfiguration configfile = KSP.IO.PluginConfiguration.CreateForType<wikitext>();
            configfile.load();
            _versionlastrun = configfile.GetValue<string>("version");
        }
    }
}
