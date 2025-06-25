﻿using System;
using System.IO;
using System.Management.Automation;
using SvRooij.ContentPrep;
namespace SvR.ContentPrep.Cmdlet
{
    /// <summary>
    /// <para type="synopsis">Create a new IntuneWin package</para>
    /// <para type="description">This is a re-implementation of the IntuneWinAppUtil.exe tool, it's not feature complete use at your own risk.</para>
    /// <para type="link" uri="https://github.com/svrooij/ContentPrep/blob/main/src/SvR.ContentPrep.Cmdlet/README.md">Documentation</para> 
    /// </summary>
    /// <example>
    /// <code>
    /// New-IntuneWinPackage -SourcePath "C:\Temp\Source" -SetupFile "C:\Temp\Source\setup.exe" -DestinationPath "C:\Temp\Destination"
    /// </code>
    /// </example>
    [Cmdlet(VerbsCommon.New, "IntuneWinPackage", HelpUri = "https://github.com/svrooij/ContentPrep/blob/main/src/SvR.ContentPrep.Cmdlet/README.md")]
    public class NewIntuneWinPackageCommand : PSCmdlet
    {
        /// <summary>
        /// <para type="description">Directory containing all the installation files</para>
        /// </summary>
        [Parameter(
            Mandatory = true,
            Position = 0,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The directory containing all the installation files")]
        public string SourcePath { get; set; }

        /// <summary>
        /// <para type="description">The main setupfile in the source directory</para>
        /// </summary>
        [Parameter(
            Mandatory = true,
            Position = 1,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The main setupfile in the source directory")]
        public string SetupFile { get; set; }

        /// <summary>
        /// <para type="description">Destination folder</para>
        /// </summary>
        [Parameter(
            Mandatory = true,
            Position = 2,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Destination folder")]
        public string DestinationPath { get; set; }

        private Packager packager;
        private PowerShellLogger<Packager> logger;

        /// <summary>
        /// 
        /// </summary>
        protected override void BeginProcessing()
        {
            WriteVerbose("Begin creating package");
            logger = new PowerShellLogger<Packager>(this);
            packager = new Packager(logger);
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void ProcessRecord()
        {
            try
            {
                /// Trimming unwanted spaces
                SourcePath = SourcePath?.Trim();
                DestinationPath = DestinationPath?.Trim();
                SetupFile = SetupFile?.Trim();
                
                if (!Path.IsPathRooted(SourcePath))
                {
                    SourcePath = Path.Combine(Environment.CurrentDirectory, SourcePath);
                }
                if (!Path.IsPathRooted(DestinationPath))
                {
                    DestinationPath = Path.Combine(Environment.CurrentDirectory, DestinationPath);
                }

                if (DestinationPath.StartsWith(SourcePath, StringComparison.OrdinalIgnoreCase))
                    throw new Exception("DestinationPath can't be a subfolder of SourcePath");

                if (!Path.IsPathRooted(SetupFile))
                {
                    /// SetupFile = Path.Combine(SourcePath, SetupFile); /// Allow full paths but get filename.
                    SetupFile = Path.Combine(SourcePath, Path.GetFileName(SetupFile));
                }
                else if (!SetupFile.StartsWith(SourcePath, StringComparison.OrdinalIgnoreCase))
                {
                    throw new Exception("SetupFile can't be outside of SourcePath");
                }

                if (!Directory.Exists(DestinationPath))
                {
                    WriteVerbose($"Creating destination folder {DestinationPath}");
                    Directory.CreateDirectory(DestinationPath);
                }
                WriteVerbose($"Trying to create package for {SetupFile}");
                ThreadAffinitiveSynchronizationContext.RunSynchronized(() =>
                    packager.CreatePackage(SourcePath, SetupFile, DestinationPath)
                );
            }
            catch (System.Exception ex)
            {
                WriteError(new ErrorRecord(ex, "1", ErrorCategory.InvalidOperation, null));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void EndProcessing()
        {
            packager = null;
            logger = null;
        }
    }
}
