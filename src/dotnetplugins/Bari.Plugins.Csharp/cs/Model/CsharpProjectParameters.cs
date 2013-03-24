﻿using System;
using System.Globalization;
using System.Linq;
using System.Xml;
using Bari.Core.Model;

namespace Bari.Plugins.Csharp.Model
{
    public class CsharpProjectParameters: IProjectParameters
    {
        public uint? BaseAddress { get; set; }
        public bool Checked { get; set; }
        public string CodePage { get; set; }
        public DebugLevel Debug { get; set; }
        public string[] Defines { get; set; }
        public bool DelaySign { get; set; }
        public string DocOutput { get; set; }
        public uint? FileAlign { get; set; }
        public bool HighEntropyVirtualAddressSpace { get; set; }
        public string KeyContainer { get; set; }
        public string KeyFile { get; set; }
        public CsharpLanguageVersion LanguageVersion { get; set; }
        public string MainClass { get; set; }
        public bool NoStdLib { get; set; }
        public int[] SuppressedWarnings { get; set; }
        public bool NoWin32Manifest { get; set; }
        public bool Optimize { get; set; }
        public CLRPlatform Platform { get; set; }
        public string PreferredUILang { get; set; }
        public string SubsystemVersion { get; set; }
        public bool Unsafe { get; set; }
        public WarningLevel WarningLevel { get; set; }
        public bool AllWarningsAsError { get; set; }
        public int[] SpecificWarningsAsError { get; set; }

        public CsharpProjectParameters()
        {
            BaseAddress = null;
            Checked = false;
            CodePage = null;
            Debug = DebugLevel.Full;
            Defines = new[] {"DEBUG"};
            DelaySign = false;
            DocOutput = null;
            FileAlign = null;
            HighEntropyVirtualAddressSpace = false;
            KeyContainer = null;
            KeyFile = null;
            LanguageVersion = CsharpLanguageVersion.V3;
            MainClass = null;
            NoStdLib = false;
            SuppressedWarnings = null;
            NoWin32Manifest = false;
            Optimize = false;
            Platform = CLRPlatform.AnyCPU;
            PreferredUILang = null;
            SubsystemVersion = null;
            Unsafe = false;
            WarningLevel = WarningLevel.All;
            AllWarningsAsError = false;
            SpecificWarningsAsError = null;
        }

        public void ToCsprojProperties(XmlWriter writer)
        {
            if (BaseAddress.HasValue)
                writer.WriteElementString("BaseAddress", "0x"+BaseAddress.Value.ToString("X", CultureInfo.InvariantCulture));
            writer.WriteElementString("CheckForOverflowUnderflow", XmlConvert.ToString(Checked));
            if (CodePage != null)
                writer.WriteElementString("CodePage", CodePage);
            writer.WriteElementString("DebugType", Debug.ToString().ToLowerInvariant());
            if (Defines != null)
                writer.WriteElementString("DefineConstants", string.Join(";", Defines));
            writer.WriteElementString("DelaySign", XmlConvert.ToString(DelaySign));
            if (DocOutput != null)
                writer.WriteElementString("DocumentationFile", DocOutput);
            if (FileAlign.HasValue)
                writer.WriteElementString("FileAlignment", XmlConvert.ToString(FileAlign.Value));
            writer.WriteElementString("HighEntropyVA", XmlConvert.ToString(HighEntropyVirtualAddressSpace));
            if (KeyContainer != null)
                writer.WriteElementString("KeyContainerName", KeyContainer);
            if (KeyFile != null)
                writer.WriteElementString("KeyOriginatorFile", KeyFile);
            writer.WriteElementString("LangVersion", ToParameter(LanguageVersion));
            if (MainClass != null)
                writer.WriteElementString("StartupObject", MainClass);
            writer.WriteElementString("NoCompilerStandardLib", XmlConvert.ToString(NoStdLib));
            if (SuppressedWarnings != null)
                writer.WriteElementString("NoWarn", 
                    String.Join(";", SuppressedWarnings.Select(warn => warn.ToString(CultureInfo.InvariantCulture))));
            writer.WriteElementString("NoWin32Manifest", XmlConvert.ToString(NoWin32Manifest));
            writer.WriteElementString("Optimize", XmlConvert.ToString(Optimize));
            writer.WriteElementString("PlatformTarget", Platform.ToString().ToLowerInvariant());
            if (PreferredUILang != null)
                writer.WriteElementString("PreferredUILang", PreferredUILang);
            if (SubsystemVersion != null)
                writer.WriteElementString("SubsystemVersion", SubsystemVersion);
            writer.WriteElementString("AllowUnsafeBlocks", XmlConvert.ToString(Unsafe));
            writer.WriteElementString("WarningLevel", XmlConvert.ToString((int)WarningLevel));
            writer.WriteElementString("TreatWarningsAsErrors", XmlConvert.ToString(AllWarningsAsError));
            if (SpecificWarningsAsError != null)
                writer.WriteElementString("WarningsAsErrors",
                                          String.Join(";", SpecificWarningsAsError.Select(warn => warn.ToString(CultureInfo.InvariantCulture))));
        }

        private string ToParameter(CsharpLanguageVersion languageVersion)
        {
            switch (languageVersion)
            {
                case CsharpLanguageVersion.Default:
                    return "default";
                case CsharpLanguageVersion.ISO1:
                    return "ISO-1";
                case CsharpLanguageVersion.ISO2:
                    return "ISO-2";
                case CsharpLanguageVersion.V3:
                    return "3";
                default:
                    throw new ArgumentOutOfRangeException("languageVersion");
            }
        }
    }
}