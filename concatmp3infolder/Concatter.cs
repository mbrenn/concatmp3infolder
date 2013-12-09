using System;
using System.IO;
using System.Diagnostics;
using System.Linq;

namespace concatmp3infolder
{
	/// <summary>
	/// Performs the concattenation of all files
	/// </summary>
	public class Concatter
	{
		/// <summary>
		/// Defines the path, whose MP3s shall be concattenated
		/// </summary>
		private string path;
		
		/// <summary>
		/// Stores the file names to be concattenated
		/// </summary>Sto
		private string[] fileNames;
		
		private const string finalMp3 = "final.mp3";
		
		private const string tempMp3 = "temp.mp3";
		
		private const string tempConcatMp3 = "tempConcat.mp3";
		
		public Concatter (string path = null)
		{
			if (path == null) {
				path = Directory.GetCurrentDirectory ();
			}
			
			this.path = path;
			if ( !Directory.Exists (path) )
			{
				throw new InvalidOperationException ( "Path does not exist");
			}
		}
		
		public void PerformConcat ()
		{
			var directory = new DirectoryInfo (path);
			var fileNames = directory.GetFiles ("*.mp3");
			
			this.fileNames = fileNames
				.Select (x => x.Name)
				.Where (x => x != finalMp3 && x != tempConcatMp3 && x != tempMp3)
				.OrderBy (x => x)
					.ToArray ();
			
			Console.WriteLine ("{0} MP3 files found", this.fileNames.Length);
			
			this.PrepareConcat ();
			
			if (this.fileNames.Length == 0) {
				Console.WriteLine ("No MP3 found");
				return;
			}
			
			if (this.fileNames.Length == 1) {
				Console.WriteLine ("One mp3: " + this.fileNames.First ());
				File.Copy (
					Path.Combine (this.path, this.fileNames.First ()),
					Path.Combine (this.path, finalMp3));
			}
			else for (var n = 0; n < this.fileNames.Length; n++) {
				// Ok, do the concat thing
				var currentDirectory = Directory.GetCurrentDirectory ();
				
				Directory.SetCurrentDirectory (this.path);
				
				if (n == 0) {
					File.Copy (this.fileNames [n], finalMp3);
				} else {
					
					if (File.Exists (tempConcatMp3)) {
						File.Delete (tempConcatMp3);
					}
					
					if (File.Exists (finalMp3)) {
						File.Move (finalMp3, tempConcatMp3);
					}
					File.Copy (this.fileNames [n], tempMp3);
					var commandLine = string.Format (
						"-i \"concat:{0}|{1}\" -acodec copy {2}",
						tempConcatMp3,
						tempMp3,
						finalMp3);
					
					var process = Process.Start ("ffmpeg", commandLine);
					process.WaitForExit ();
					
					File.Delete (tempMp3);
					
					if (File.Exists (tempConcatMp3)) {
						File.Delete (tempConcatMp3);
					}
				}
				
				Directory.SetCurrentDirectory (currentDirectory);
			}
			
			Console.WriteLine ("Concattenated: ");
			foreach (var file in this.fileNames) {
				Console.WriteLine ("- " + file);
			}
		}
		
		/// <summary>
		/// Removes the temporary file, if existing
		/// </summary>
		private void PrepareConcat ()
		{	
			var temporaryName = Path.Combine (this.path, finalMp3);
			if (File.Exists (temporaryName)) {
				File.Delete (temporaryName);
			}
			
			temporaryName = Path.Combine (this.path, tempMp3);
			if (File.Exists (temporaryName)) {
				File.Delete (temporaryName);
			}
						
			temporaryName = Path.Combine (this.path, tempConcatMp3);
			if (File.Exists (temporaryName)) {
				File.Delete (temporaryName);
			}
		}
	}
}

